using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;
using LightScout.Models;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(LightScout.Droid.USBTransfer))]
namespace LightScout.Droid
{

    public class USBContext
    {
        public static Socket socket;
        public static int phase;
        public static bool ready;
        public static bool startScan;
    }
    public class USBTransfer : USBCommunication
    {
        UsbManager manager;

        public void SendData(string rawstring)
        {

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            //MessagingCenter.Send<object, int>(this, "USBResponse", 2);
            //var tabletid = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).TabletIdentifier;
            int nummessagesreceived = 0;
            Task.Run(() =>
            {
                while (true)
                {
                    if (socket.Connected)
                    {
                        byte[] givenBytes = new byte[5000];
                        try
                        {
                            int numbytessent = socket.Receive(givenBytes);
                            if (numbytessent <= 0) continue;
                            nummessagesreceived++;
                            byte[] finalBytes = givenBytes.Take(numbytessent).ToArray();
                            var stringmessage = Encoding.ASCII.GetString(finalBytes);
                            if (nummessagesreceived >= 2)
                            {
                                socket.Disconnect(false);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            socket.Connect(new IPEndPoint(IPAddress.Loopback, 6000));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Computer not correctly connected!");
                        }
                    }
                    
                    



                }
            });
            Task.Run(() =>
            {
                while (true)
                {
                    if (socket.Connected)
                    {
                        socket.BeginSend(Encoding.ASCII.GetBytes("Test"), 0, Encoding.ASCII.GetBytes("Test").Length, SocketFlags.None, (ars) =>
                        {
                            socket.BeginSend(Encoding.ASCII.GetBytes("Test"), 0, Encoding.ASCII.GetBytes("Test").Length, SocketFlags.None, USBCallBack, socket);
                        }, socket);
                        Console.WriteLine("Socket Bound");
                        break;
                    }
                    
                }
            });
            

            


        }

        public bool CheckForConnection()
        {
            try
            {
                USBContext.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                USBContext.socket.Connect("localhost", 6001);
                
                return true;
            }catch(Exception e)
            {
                USBContext.socket = null;
                USBContext.ready = false;
                return false;
            }
            
        }

        public void StartChecking()
        {

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!USBContext.ready)
                {
                    var res = CheckForConnection();
                    Console.WriteLine("CONNECTION STATUS: " + res);
                    if (res)
                    {
                        USBContext.ready = true;
                        USBContext.phase = 0;
                    }
                    return true;
                }
                TransferProcess();

                return true;
            });
            
        }

        public byte[] ReceiveAll(Socket socket)
        {
            var buffer = new List<byte>();

            while (socket.Available > 0)
            {
                var currByte = new Byte[1];
                var byteCounter = socket.Receive(currByte, currByte.Length, SocketFlags.None);

                if (byteCounter.Equals(1))
                {
                    buffer.Add(currByte[0]);
                }
            }

            return buffer.ToArray();
        }


        public int dullConnections = 0;

        public void TransferProcess()
        {
            var firstRes = ReceiveAll(USBContext.socket);
            var stringOfRes = System.Text.Encoding.UTF8.GetString(firstRes, 0, firstRes.Length);

            try
            {

                var gPhase = Int32.Parse(stringOfRes.Split(":")[0]);
                USBContext.phase = gPhase;

                var sendBack = "";

                switch (gPhase)
                {
                    case 0:
                        Console.WriteLine("USB Wants DeviceID, giving...");
                        sendBack = "0:" + ApplicationDataHandler.CurrentApplicationData.DeviceId.ToString();
                        
                        break;
                    case 1:
                        Console.WriteLine("Request: " + stringOfRes);

                        var controlReq = stringOfRes.Substring(2).Split("*-*")[0];
                        var responseReq = stringOfRes.Substring(2).Split("*-*")[1];
                        var data = stringOfRes.Substring(2).Split("*-*")[2];

                        var item = new QueueItemIn() { protocolIn = controlReq.ToUpper(), protocolOut = responseReq.ToUpper(), latestData = data };
                        // do actions
                        sendBack = "1:";
                        switch (item.protocolIn)
                        {
                            case "0000":
                                Console.WriteLine("(0000) Doing nothing");
                                break;
                            case "0998":
                                Console.WriteLine("(0998) Ping from the central!");
                                break;
                            case "0999":
                                Console.WriteLine("(0999) Data from central: " + item.protocolIn);
                                break;
                            case "A001":
                                Console.WriteLine("(a001) Removing everything...");
                                ApplicationDataHandler.Instance.ClearAllData(true, ApplicationDataHandler.ClearDataType.Data);

                                break;
                            case "A002":
                                Console.WriteLine("(a001) Removing everything...");
                                ApplicationDataHandler.Instance.ClearAllData(true, ApplicationDataHandler.ClearDataType.Entries);

                                break;
                            case "A003":
                                Console.WriteLine("(a001) Removing everything...");
                                ApplicationDataHandler.Instance.ClearAllData(true, ApplicationDataHandler.ClearDataType.All);

                                break;
                            case "A101":
                                Console.WriteLine("(a101) This should lock tablet");
                                ApplicationDataHandler.CurrentApplicationData.Locked = true;
                                ApplicationDataHandler.CurrentApplicationData.LockedMessage = item.latestData;
                                ApplicationDataHandler.Instance.SaveAppData();
                                break;
                            case "A102":
                                Console.WriteLine("(a102) This should unlock tablet");
                                ApplicationDataHandler.CurrentApplicationData.Locked = false;
                                ApplicationDataHandler.CurrentApplicationData.LockedMessage = "";
                                ApplicationDataHandler.Instance.SaveAppData();
                                // unlock tablet here
                                break;
                            case "A111":
                                Console.WriteLine("(a111) Sending dialog box");
                                MessagingCenter.Send("MasterPage", "DialogBox", item.latestData);
                                break;
                            case "A201":
                                Console.WriteLine("(a201) Compiling diagnostic data");
                                ApplicationDataHandler.Instance.CompileDiagnostics();

                                break;
                            case "A202":
                                Console.WriteLine("(a202) Compiling diagnostic data and waiting");
                                ApplicationDataHandler.Instance.CompileDiagnostics();
                                break;
                            case "A301":
                                Console.WriteLine("(a301) Force competition schema");
                                ApplicationDataHandler.CurrentApplicationData.RestrictMatches = true;
                                ApplicationDataHandler.Instance.SaveAppData();
                                break;
                            case "A302":
                                Console.WriteLine("(a302) Don't force competition schema");
                                ApplicationDataHandler.CurrentApplicationData.RestrictMatches = false;
                                ApplicationDataHandler.Instance.SaveAppData();
                                break;
                            case "A401":
                                Console.WriteLine("(a401) Add competition");
                                ApplicationDataHandler.Competitions.Add(JsonConvert.DeserializeObject<Competition>(item.latestData));
                                // need to add link in classes!
                                ApplicationDataHandler.Instance.SaveCompetitions();
                                break;
                            case "A402":
                                Console.WriteLine("(a402) Update competition");
                                var objToUpdate = JsonConvert.DeserializeObject<Competition>(item.latestData);
                                ApplicationDataHandler.Competitions.Single(s => s.Id == objToUpdate.Id).Name = objToUpdate.Name;
                                ApplicationDataHandler.Competitions.Single(s => s.Id == objToUpdate.Id).Location = objToUpdate.Location;
                                ApplicationDataHandler.Competitions.Single(s => s.Id == objToUpdate.Id).StartsAt = objToUpdate.StartsAt;
                                ApplicationDataHandler.Competitions.Single(s => s.Id == objToUpdate.Id).AllowedSchemas = objToUpdate.AllowedSchemas;
                                ApplicationDataHandler.Competitions.Single(s => s.Id == objToUpdate.Id).DateSpan = objToUpdate.DateSpan;
                                ApplicationDataHandler.Instance.SaveCompetitions();
                                break;
                            case "A701":
                                Console.WriteLine("(a701) Lockdown mode");
                                // N/A At the Moment

                                break;
                            case "A702":
                                Console.WriteLine("(a702) Don't lockdown mode");
                                // N/A At the Moment
                                break;
                            case "A711":
                                Console.WriteLine("(a711) Set admin code");
                                ApplicationDataHandler.CurrentApplicationData.AdminCode = item.latestData;
                                ApplicationDataHandler.Instance.SaveAppData();
                                break;
                            case "A801":
                                Console.WriteLine("(a801) Enable logging mode");
                                ApplicationDataHandler.CurrentApplicationData.Logging = true;
                                ApplicationDataHandler.Instance.SaveAppData();
                                break;
                            case "A802":
                                Console.WriteLine("(a802) Disable logging mode");
                                ApplicationDataHandler.CurrentApplicationData.Logging = false;
                                ApplicationDataHandler.Instance.SaveAppData();
                                break;
                            case "A803":
                                Console.WriteLine("(a803) Remove all logs");
                                ApplicationDataHandler.Logs.Clear();
                                ApplicationDataHandler.Instance.SaveLogs();
                                break;
                            case "A811":
                                Console.WriteLine("(a811) Set emergency medical information");

                                break;
                            case "A901":
                                Console.WriteLine("(a901) Enable debugging mode");
                                ApplicationDataHandler.CurrentApplicationData.Debugging = true;
                                ApplicationDataHandler.Instance.SaveAppData();
                                break;
                            case "A902":
                                Console.WriteLine("(a902) Disable debugging mode");
                                ApplicationDataHandler.CurrentApplicationData.Debugging = false;
                                ApplicationDataHandler.Instance.SaveAppData();
                                break;
                            case "C101":
                                Console.WriteLine("(c101) Add schema");
                                ApplicationDataHandler.Schemas.Add(JsonConvert.DeserializeObject<Schema>(item.latestData));
                                ApplicationDataHandler.Instance.SaveSchemas();
                                break;
                            case "C102":
                                Console.WriteLine("(c102) Remove schema");
                                var schemaId = item.latestData;
                                ApplicationDataHandler.Schemas.Remove(ApplicationDataHandler.Schemas.Single(s => s.Id == schemaId));
                                ApplicationDataHandler.Instance.SaveSchemas();
                                break;
                            case "C103":
                                Console.WriteLine("(c103) Update schema");
                                var objToUpdate2 = JsonConvert.DeserializeObject<Schema>(item.latestData);
                                ApplicationDataHandler.Schemas.Single(s => s.Id == objToUpdate2.Id).Name = objToUpdate2.Name;
                                ApplicationDataHandler.Schemas.Single(s => s.Id == objToUpdate2.Id).JSONData = objToUpdate2.JSONData;
                                ApplicationDataHandler.Schemas.Single(s => s.Id == objToUpdate2.Id).GotAt = DateTime.Now;
                                ApplicationDataHandler.Instance.SaveSchemas();
                                break;
                            case "C104":
                                Console.WriteLine("(c104) Force update schema");
                                var objToForceUpdate = JsonConvert.DeserializeObject<Schema>(item.latestData);
                                ApplicationDataHandler.Schemas.Single(s => s.Id == objToForceUpdate.Id).Name = objToForceUpdate.Name;
                                ApplicationDataHandler.Schemas.Single(s => s.Id == objToForceUpdate.Id).JSONData = objToForceUpdate.JSONData;
                                ApplicationDataHandler.Schemas.Single(s => s.Id == objToForceUpdate.Id).GotAt = DateTime.Now;
                                ApplicationDataHandler.Instance.SaveSchemas();
                                break;
                            case "C201":
                                Console.WriteLine("(c201) Add match");
                                ApplicationDataHandler.AllEntries.Add(JsonConvert.DeserializeObject<DataEntry>(item.latestData));
                                MessagingCenter.Send("MasterPage", "MatchesChanged", "hola");
                                ApplicationDataHandler.Instance.SaveMatches();

                                break;
                            case "C202":
                                Console.WriteLine("(c202) Remove match");
                                ApplicationDataHandler.AllEntries.Remove(ApplicationDataHandler.AllEntries.Single(d => d.Id == item.latestData));
                                MessagingCenter.Send("MasterPage", "MatchesChanged", "hola");
                                ApplicationDataHandler.Instance.SaveMatches();
                                break;
                            case "C203":
                                Console.WriteLine("(c203) Force remove match");
                                ApplicationDataHandler.AllEntries.Remove(ApplicationDataHandler.AllEntries.Single(d => d.Id == item.latestData));
                                MessagingCenter.Send("MasterPage", "MatchesChanged", "hola");
                                ApplicationDataHandler.Instance.SaveMatches();
                                break;
                            case "C204":
                                Console.WriteLine("(c204) Update match");
                                var matchToUpdate = JsonConvert.DeserializeObject<DataEntry>(item.latestData);
                                if (!ApplicationDataHandler.AllEntries.Single(d => d.Id == matchToUpdate.Id).Completed)
                                {
                                    var doThis = ApplicationDataHandler.AllEntries.Single(d => d.Id == matchToUpdate.Id);
                                    doThis.AssistedBy = matchToUpdate.AssistedBy;
                                    doThis.Audited = matchToUpdate.Audited;
                                    doThis.Competition = matchToUpdate.Competition;
                                    doThis.Completed = matchToUpdate.Completed;
                                    doThis.Data = matchToUpdate.Data;
                                    doThis.LastEdited = matchToUpdate.LastEdited;
                                    doThis.Number = matchToUpdate.Number;
                                    doThis.Position = matchToUpdate.Position;
                                    doThis.Schema = matchToUpdate.Schema;
                                    doThis.TeamIdentifier = matchToUpdate.TeamIdentifier;
                                    doThis.TeamName = matchToUpdate.TeamName;
                                }
                                MessagingCenter.Send("MasterPage", "MatchesChanged", "hola");
                                ApplicationDataHandler.Instance.SaveMatches();
                                break;
                            case "C205":
                                Console.WriteLine("(c205) Force update match");
                                var matchToForceUpdate = JsonConvert.DeserializeObject<DataEntry>(item.latestData);
                                var doThisOne = ApplicationDataHandler.AllEntries.Single(d => d.Id == matchToForceUpdate.Id);
                                doThisOne.AssistedBy = matchToForceUpdate.AssistedBy;
                                doThisOne.Audited = matchToForceUpdate.Audited;
                                doThisOne.Competition = matchToForceUpdate.Competition;
                                doThisOne.Completed = matchToForceUpdate.Completed;
                                doThisOne.Data = matchToForceUpdate.Data;
                                doThisOne.LastEdited = matchToForceUpdate.LastEdited;
                                doThisOne.Number = matchToForceUpdate.Number;
                                doThisOne.Position = matchToForceUpdate.Position;
                                doThisOne.Schema = matchToForceUpdate.Schema;
                                doThisOne.TeamIdentifier = matchToForceUpdate.TeamIdentifier;
                                doThisOne.TeamName = matchToForceUpdate.TeamName;
                                MessagingCenter.Send("MasterPage", "MatchesChanged", "hola");
                                ApplicationDataHandler.Instance.SaveMatches();
                                break;

                            case "C211":
                                Console.WriteLine("(c211) Add matches");
                                var listOfMatches = JsonConvert.DeserializeObject<List<DataEntry>>(item.latestData);
                                foreach (var match in listOfMatches)
                                {
                                    if (ApplicationDataHandler.AllEntries.Find(el => el.Id == match.Id) == null)
                                    {
                                        ApplicationDataHandler.AllEntries.Add(match);
                                    }
                                }
                                ApplicationDataHandler.Instance.SaveMatches();
                                MessagingCenter.Send("MasterPage", "MatchesChanged", "hola");
                                break;
                            case "C212":
                                Console.WriteLine("(c212) Remove matches");
                                var listOfIds = JsonConvert.DeserializeObject<List<string>>(item.latestData);
                                foreach (var id in listOfIds)
                                {
                                    ApplicationDataHandler.AllEntries.Remove(ApplicationDataHandler.AllEntries.Single(d => d.Id == id));
                                }
                                MessagingCenter.Send("MasterPage", "MatchesChanged", "hola");
                                ApplicationDataHandler.Instance.SaveMatches();
                                break;
                            case "C301":
                                Console.WriteLine("(c301) Change forced schema");
                                ApplicationDataHandler.CurrentApplicationData.CurrentCompetition = item.latestData;

                                break;
                            case "C401":
                                try
                                {
                                    Console.WriteLine("(c401) Add user");
                                    ApplicationDataHandler.Users.Add(JsonConvert.DeserializeObject<Scouter>(item.latestData));
                                    ApplicationDataHandler.Instance.SaveUsers();
                                    MessagingCenter.Send("MasterPage", "UsersChanged", "hola");
                                }
                                catch (Exception e)
                                {
                                    MessagingCenter.Send("MasterPage", "DialogBox", e.ToString());
                                }
                                MessagingCenter.Send("MasterPage", "DialogBox", "DONE ADDING USER");

                                break;
                            case "C402":
                                Console.WriteLine("(c402) Remove user");
                                ApplicationDataHandler.Users.Remove(ApplicationDataHandler.Users.Single(d => d.Id == item.latestData));
                                ApplicationDataHandler.Instance.SaveUsers();
                                MessagingCenter.Send("MasterPage", "UsersChanged", "hola");
                                break;
                            case "C403":
                                Console.WriteLine("(c403) Update user");
                                var userToUpdate = JsonConvert.DeserializeObject<Scouter>(item.latestData);
                                var doThisUser = ApplicationDataHandler.Users.Single(d => d.Id == userToUpdate.Id);
                                doThisUser.Name = userToUpdate.Name;
                                doThisUser.Score = userToUpdate.Score;
                                doThisUser.Banned = userToUpdate.Banned;
                                ApplicationDataHandler.Instance.SaveUsers();
                                MessagingCenter.Send("MasterPage", "UsersChanged", "hola");
                                break;
                            case "C411":
                                Console.WriteLine("(c411) Add users");
                                ApplicationDataHandler.Users.AddRange(JsonConvert.DeserializeObject<List<Scouter>>(item.latestData));
                                ApplicationDataHandler.Instance.SaveUsers();
                                MessagingCenter.Send("MasterPage", "UsersChanged", "hola");
                                break;
                            case "C412":
                                Console.WriteLine("(c412) Update users");
                                var listOfUsersToUpdate = JsonConvert.DeserializeObject<List<Scouter>>(item.latestData);
                                foreach (var user in listOfUsersToUpdate)
                                {
                                    var doThisUserToo = ApplicationDataHandler.Users.Single(d => d.Id == user.Id);
                                    doThisUserToo.Name = user.Name;
                                    doThisUserToo.Score = user.Score;
                                    doThisUserToo.Banned = user.Banned;
                                }
                                ApplicationDataHandler.Instance.SaveUsers();
                                MessagingCenter.Send("MasterPage", "UsersChanged", "hola");
                                break;
                            case "C421":
                                Console.WriteLine("(c421) Remove all users");
                                ApplicationDataHandler.Users.Clear();
                                ApplicationDataHandler.Instance.SaveUsers();
                                MessagingCenter.Send("MasterPage", "UsersChanged", "hola");
                                break;
                            case "C431":
                                Console.WriteLine("(c431) Change announcement");
                                ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.Data = item.latestData;
                                ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.GotAt = DateTime.Now;
                                ApplicationDataHandler.Instance.SaveAppData();
                                MessagingCenter.Send("MasterPage", "AnnouncementChanged", "hola");

                                break;
                            case "C501":
                                Console.WriteLine("(c501) Create backup");
                                ApplicationDataHandler.Instance.SetBackup();
                                break;
                            case "C502":
                                Console.WriteLine("(c502) Load backup");
                                ApplicationDataHandler.Instance.GetBackup();
                                MessagingCenter.Send("MasterPage", "MatchesChanged", "hola");
                                break;
                            case "C701":
                                Console.WriteLine("(c701) Enable competition security mode");
                                ApplicationDataHandler.CurrentApplicationData.SecurityMode = true;
                                break;
                            case "C702":
                                Console.WriteLine("(c702) Disable competition security mode");
                                ApplicationDataHandler.CurrentApplicationData.SecurityMode = false;
                                break;
                            case "C711":
                                Console.WriteLine("(c711) Set competition security mode key");
                                ApplicationDataHandler.CurrentApplicationData.SecurityKey = item.latestData;
                                break;
                        }
                        // next send back what told to
                        switch (item.protocolOut.ToLower())
                        {
                            case "0000":
                                Console.WriteLine("(0000 >) Doing nothing");
                                sendBack += "Pong!";
                                break;
                            case "0001":
                                // allow queue to do something here
                                break;
                            case "0998":
                                sendBack += "Pong!";
                                break;
                            case "0999":
                                sendBack += "I'm a data packet";
                                break;
                            case "a101":
                                sendBack += ApplicationDataHandler.CurrentApplicationData.Locked.ToString();
                                //UpdateNotification(, item.protocolIn, item.protocolOut);
                                break;
                            case "a111":
                                sendBack += "Nothing :>";
                                //UpdateNotification("Nothing :>", item.protocolIn, item.protocolOut);
                                break;
                            case "a201":
                                List<string> Schemas = new List<string>();
                                foreach (var schema in ApplicationDataHandler.Schemas)
                                {
                                    Schemas.Add(schema.Id);
                                }

                                sendBack += Newtonsoft.Json.JsonConvert.SerializeObject(new DiagnosticReport()
                                {
                                    BatteryLevel = .9f,
                                    InternetConnectivity = false,
                                    NumberOfMatchesStored = ApplicationDataHandler.AllEntries.Count,
                                    SchemasIncluded = Schemas
                                });
                                break;
                            case "a202":
                                sendBack += ":[";
                                break;
                            case "a301":
                                sendBack += ApplicationDataHandler.CurrentApplicationData.RestrictMatches.ToString();
                                break;
                            case "a701":
                                sendBack += "Not implemented!";
                                // Probably won't be implemented at this state
                                break;
                            case "a801":
                                sendBack += JsonConvert.SerializeObject(ApplicationDataHandler.Logs);
                                break;
                            case "a811":
                                sendBack += "Not implemented!";
                                break;
                            case "a901":
                                sendBack += ApplicationDataHandler.CurrentApplicationData.Debugging.ToString();
                                break;
                            case "c101":

                                sendBack += JsonConvert.SerializeObject(ApplicationDataHandler.Schemas);
                                break;
                            case "c201":
                                List<DataEntry> listWithEmptyData = new List<DataEntry>();
                                foreach (var match in ApplicationDataHandler.AllEntries)
                                {
                                    listWithEmptyData.Add(new DataEntry() { Number = match.Number, Competition = match.Competition, Completed = match.Completed, TeamIdentifier = match.TeamIdentifier, Schema = match.Schema, Id = match.Id, Position = match.Position });
                                }
                                sendBack += JsonConvert.SerializeObject(listWithEmptyData);
                                break;
                            case "c202":
                                sendBack += JsonConvert.SerializeObject(ApplicationDataHandler.AwaitingSubmission);
                                ApplicationDataHandler.AwaitingSubmission.Clear();
                                ApplicationDataHandler.Instance.SaveMatches();
                                break;
                            case "c203":
                                sendBack += JsonConvert.SerializeObject(ApplicationDataHandler.AllEntries);
                                break;
                            case "c301":
                                sendBack += ApplicationDataHandler.CurrentApplicationData.CurrentCompetition.ToString();
                                break;
                            case "c401":
                                sendBack += JsonConvert.SerializeObject(ApplicationDataHandler.Users);
                                break;
                            case "c501":
                                //var backup = ApplicationDataHandler.Instance.GetBackupWithoutApplying().Result;
                                //UpdateNotification(backup.CreatedAt.ToLongDateString(), item.protocolIn, item.protocolOut);
                                break;
                            case "c701":
                                sendBack += ApplicationDataHandler.CurrentApplicationData.SecurityMode.ToString();
                                break;
                        }


                        // put handler here
                        
                        break;
                    case 2:
                        Console.WriteLine("Closing Socket");
                        USBContext.socket.Close();
                        USBContext.socket = null;
                        USBContext.ready = false;

                        break;
                    default:
                        sendBack = "9:INVALID";
                        Console.WriteLine("Invalid Phase");
                        break;
                }
                dullConnections = 0;
                USBContext.socket.Send(System.Text.Encoding.UTF8.GetBytes(sendBack), SocketFlags.None);


            }
            catch(Exception e)
            {
                dullConnections += 1;
                Console.WriteLine("Invalid Message");
            }


            if (USBContext.ready && (!USBContext.socket.Connected || dullConnections > 10))
            {
                USBContext.socket.Close();
                USBContext.socket = null;
                USBContext.ready = false;
            }
        }

        public void USBCallBack(IAsyncResult result)
        {

        }

        
    }
}