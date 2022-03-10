using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Newtonsoft.Json;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(LightScout.Droid.PeripheralManagement))]
namespace LightScout.Droid
{
    public enum BluetoothLogType
    {
        Successful,
        Unsuccessful,
        Aborted,
        ElevatedEvent
    }
    public class BluetoothDataLog
    {
        public DateTime timestamp { get; set; }
        public BluetoothLogType bluetoothLogType { get; set; }
        public string extraData { get; set; }
    }
    public class QueueItemOut
    {
        public string communicationId;
        public string messageLeft;
        public string fullMessage;
        public DateTime startedAt;
    }
    public class QueueItemIn
    {
        public string communicationId;
        public string latestHeader;
        public int numMessages;
        public string latestData;
        public bool isEnded;
        public string deviceId;
        public string protocolIn;
        public string protocolOut;
        public int offset;
    }
    public class NotifyingDevice
    {
        public BluetoothDevice Device;
        public BluetoothGattCharacteristic Characteristic;
    }
    
    public sealed class ServerManagement
    {
        public static StorageManager storageManager = StorageManager.Instance;
        private static readonly object l1 = new object();
        public static ServerManagement instance = null;
        public static ServerManagement Instance
        {
            get
            {
                lock (l1)
                {
                    if (instance == null)
                    {
                        instance = new ServerManagement();
                    }
                    return instance;
                }

            }
        }

        public static BluetoothGattServer Server;
        public static List<QueueItemOut> QueueOut = new List<QueueItemOut>();
        public static List<QueueItemIn> QueueIn = new List<QueueItemIn>();
        public static int CurrentRequestId;
        public static NotifyingDevice CurrentNotificationTo;
    }
    public class GattServerCallback : BluetoothGattServerCallback
    {
        
        private ServerManagement ServerManagement = new ServerManagement();
        public override void OnConnectionStateChange(BluetoothDevice device, [GeneratedEnum] ProfileState status, [GeneratedEnum] ProfileState newState)
        {
            
            base.OnConnectionStateChange(device, status, newState);
        }
        public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic)
        {
            Console.WriteLine("Thingy read");
            // need to somehow pass server here
            ServerManagement.Server.SendResponse(device, requestId, GattStatus.Success, offset, Encoding.ASCII.GetBytes("Hello, nothing has been set!"));

            base.OnCharacteristicReadRequest(device, requestId, offset, characteristic);
        }
        public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            Console.WriteLine("Detecting if proper message");
            try
            {
                ServerManagement.CurrentRequestId = requestId;
                var header = BitConverter.ToString(value.Take(16).ToArray()).Replace("-", string.Empty);
                var communicationId = header.Substring(24, 8);
                var deviceId = header.Substring(4, 6);
                var protocolIn = header.Substring(10, 4);
                var protocolOut = header.Substring(14, 4);
                var messageNumber = int.Parse(header.Substring(20, 4));
                Console.WriteLine(messageNumber);
                var isEnded = header.Substring(19, 1) == "A" ? false : true;
                var expectingResponse = header.Substring(18, 1) == "1" ? true : false;
                var data = Encoding.ASCII.GetString(value.Skip(16).ToArray());
                Console.WriteLine("Data: " + data);
                // 2 things are essentially writing at once, and I don't know why. It disrupts the flow
                // assume for now that it just wants to send a test value
                
                ServerManagement.Server.SendResponse(device, requestId, GattStatus.Success, offset, Encoding.ASCII.GetBytes("Test").ToArray());
                if (ServerManagement.QueueIn.Exists(item => item.communicationId == communicationId))
                {
                    // use existing device
                    ServerManagement.QueueIn.Single(item => item.communicationId == communicationId).latestHeader = header;
                    ServerManagement.QueueIn.Single(item => item.communicationId == communicationId).latestData = (ServerManagement.QueueIn.Single(item => item.communicationId == communicationId).latestData == null ? "" : ServerManagement.QueueIn.Single(item => item.communicationId == communicationId).latestData) + data;
                    ServerManagement.QueueIn.Single(item => item.communicationId == communicationId).isEnded = isEnded;
                    ServerManagement.QueueIn.Single(item => item.communicationId == communicationId).numMessages += 1;
                    ServerManagement.QueueIn.Single(item => item.communicationId == communicationId).offset = offset;
                }
                else
                {
                    ServerManagement.QueueIn.Add(new QueueItemIn() { communicationId = communicationId, deviceId = deviceId, protocolIn = protocolIn, protocolOut = protocolOut, latestData = data, latestHeader = header, isEnded = isEnded, numMessages = 1, offset = offset });
                    
                }
                if (ServerManagement.CurrentNotificationTo != null && device.Address == ServerManagement.CurrentNotificationTo.Device.Address)
                {
                    CheckIfFinished(ServerManagement.QueueIn.Single(item => item.communicationId == communicationId));
                }
                else
                {
                    // cannot be notified of changes, so I must send back an error!
                    ServerManagement.Server.SendResponse(device, requestId, GattStatus.RequestNotSupported, offset, Encoding.ASCII.GetBytes("Comm ID updated!").ToArray());
                }
                //check if device is currently subscribed





                ServerManagement.Server.SendResponse(device, requestId, GattStatus.Success, offset, Encoding.ASCII.GetBytes("Comm ID updated!").ToArray());
            }
            catch(Exception e)
            {
                ServerManagement.Server.SendResponse(device, requestId, GattStatus.Failure, offset, Encoding.ASCII.GetBytes("Comm ID updated!").ToArray());
                Console.WriteLine("Detected BAD Write Request!");
                Console.WriteLine(e);
            }
            
            
            //characteristic.SetValue(value);
            base.OnCharacteristicWriteRequest(device, requestId, characteristic, preparedWrite, responseNeeded, offset, value);
        }

        public async void NextMessageTrigger(QueueItemIn item)
        {
            var headerString = (862).ToString("0000") + item.deviceId + item.protocolIn + item.protocolOut + "1" + "a" + item.numMessages.ToString("0000") + item.communicationId;
            var finalByteArray = StringToByteArray(headerString).Concat(Encoding.ASCII.GetBytes("Get to work ya freeloader!").ToArray()).ToArray();
            ServerManagement.CurrentNotificationTo.Characteristic.SetValue(finalByteArray);
            ServerManagement.Server.NotifyCharacteristicChanged(ServerManagement.CurrentNotificationTo.Device, ServerManagement.CurrentNotificationTo.Characteristic, false);
        }
        public async void UpdateNotification(string data, string protocolIn, string protocolOut)
        {
            var communicationId = GenerateRandomHexString();
            var encodedMessage = Encoding.ASCII.GetBytes(data);
            var numMessages = (int)Math.Ceiling((float)Encoding.ASCII.GetBytes(data).Length / (float)200);
            for(int m = 0; m < numMessages; m++)
            {
                var headerString = (862).ToString("0000") + "123456" + protocolIn + protocolOut + "0" + (m + 1 == numMessages ? "e" : "a") + (m + 1).ToString("0000") + communicationId;
                var finalByteArray = StringToByteArray(headerString).Concat(encodedMessage.Skip(m * 200).ToArray().Take(200)).ToArray();
                ServerManagement.CurrentNotificationTo.Characteristic.SetValue(finalByteArray);
                ServerManagement.Server.NotifyCharacteristicChanged(ServerManagement.CurrentNotificationTo.Device, ServerManagement.CurrentNotificationTo.Characteristic, false);
                
                Console.WriteLine(m.ToString() + " message notified");
                await Task.Delay(500);
            }
        }
        public override void OnDescriptorWriteRequest(BluetoothDevice device, int requestId, BluetoothGattDescriptor descriptor, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            Console.WriteLine("Notify yay!");
            ServerManagement.CurrentNotificationTo = new NotifyingDevice() { Characteristic = descriptor.Characteristic, Device = device };
            ServerManagement.Server.SendResponse(device, requestId, GattStatus.Success, offset, value);
            base.OnDescriptorWriteRequest(device, requestId, descriptor, preparedWrite, responseNeeded, offset, value);
        }
        public override void OnDescriptorReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor)
        {
            var alreadyExistingUUID = UUID.FromString("0000A404-0000-1000-8000-00805F9B34FB");
            if (descriptor.Uuid.ToString() == alreadyExistingUUID.ToString())
            {
                // getting tablet unique ID
                ServerManagement.Server.SendResponse(device, requestId, GattStatus.Success, offset, Encoding.ASCII.GetBytes("1234567890ab"));
            }
            else
            {
                ServerManagement.Server.SendResponse(device, requestId, GattStatus.Success, offset, Encoding.ASCII.GetBytes("uwu"));
                
            }
            //ServerManagement.CurrentNotificationTo = new NotifyingDevice() { Characteristic = descriptor.Characteristic, Device = device };
            base.OnDescriptorReadRequest(device, requestId, offset, descriptor);
        }
        /*public override void OnNotificationSent(BluetoothDevice device, [GeneratedEnum] GattStatus status)
        {
            ServerManagement.Server.SendResponse(device, ServerManagement.CurrentRequestId, GattStatus.Success, Encoding.ASCII.GetBytes("AAA").ToArray().Length, Encoding.ASCII.GetBytes("AAA").ToArray());
            base.OnNotificationSent(device, status);
        }*/
        public void CheckIfFinished(QueueItemIn item)
        {
            if (item.isEnded)
            {
                // first do what was told to do
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
                        ApplicationDataHandler.Instance.ClearAllData(true);
                        
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
                        if(!ApplicationDataHandler.AllEntries.Single(d => d.Id == matchToUpdate.Id).Completed)
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
                        foreach(var match in listOfMatches)
                        {
                            if(ApplicationDataHandler.AllEntries.Find(el => el.Id == match.Id) == null)
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
                        foreach(var id in listOfIds)
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
                        catch(Exception e)
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
                        foreach(var user in listOfUsersToUpdate)
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
                        UpdateNotification("Pong!", item.protocolIn, item.protocolOut);
                        break;
                    case "0001":
                        // allow queue to do something here
                        break;
                    case "0998":
                        UpdateNotification("Pong!", item.protocolIn, item.protocolOut);
                        break;
                    case "0999":
                        UpdateNotification("I'm a data packet!", item.protocolIn, item.protocolOut);
                        break;
                    case "a101":
                        UpdateNotification(ApplicationDataHandler.CurrentApplicationData.Locked.ToString(), item.protocolIn, item.protocolOut);
                        break;
                    case "a111":
                        UpdateNotification("Nothing :>", item.protocolIn, item.protocolOut);
                        break;
                    case "a201":
                        List<string> Schemas = new List<string>();
                        foreach(var schema in ApplicationDataHandler.Schemas)
                        {
                            Schemas.Add(schema.Id);
                        }
                        UpdateNotification(Newtonsoft.Json.JsonConvert.SerializeObject(new DiagnosticReport() { 
                            BatteryLevel = .9f,
                            InternetConnectivity = false,
                            NumberOfMatchesStored = ApplicationDataHandler.AllEntries.Count,
                            SchemasIncluded = Schemas
                        }), item.protocolIn, item.protocolOut);
                        break;
                    case "a202":
                        UpdateNotification(":[", item.protocolIn, item.protocolOut);
                        break;
                    case "a301":
                        UpdateNotification(ApplicationDataHandler.CurrentApplicationData.RestrictMatches.ToString(), item.protocolIn, item.protocolOut);
                        break;
                    case "a701":
                        UpdateNotification("Not implemented!", item.protocolIn, item.protocolOut);
                        // Probably won't be implemented at this state
                        break;
                    case "a801":
                        UpdateNotification(JsonConvert.SerializeObject(ApplicationDataHandler.Logs), item.protocolIn, item.protocolOut);
                        break;
                    case "a811":
                        UpdateNotification("Not implemented!", item.protocolIn, item.protocolOut);
                        break;
                    case "a901":
                        UpdateNotification(ApplicationDataHandler.CurrentApplicationData.Debugging.ToString(), item.protocolIn, item.protocolOut);
                        break;
                    case "c101":
                        
                        UpdateNotification(JsonConvert.SerializeObject(ApplicationDataHandler.Schemas), item.protocolIn, item.protocolOut);
                        break;
                    case "c201":
                        List<DataEntry> listWithEmptyData = new List<DataEntry>();
                        foreach (var match in ApplicationDataHandler.AllEntries)
                        {
                            listWithEmptyData.Add(new DataEntry() { Number = match.Number, Competition = match.Competition, Completed = match.Completed, TeamIdentifier = match.TeamIdentifier, Schema = match.Schema, Id = match.Id, Position = match.Position });
                        }
                        UpdateNotification(JsonConvert.SerializeObject(listWithEmptyData), item.protocolIn, item.protocolOut);
                        break;
                    case "c202":
                        UpdateNotification(JsonConvert.SerializeObject(ApplicationDataHandler.AwaitingSubmission), item.protocolIn, item.protocolOut);
                        ApplicationDataHandler.AwaitingSubmission.Clear();
                        ApplicationDataHandler.Instance.SaveMatches();
                        break;
                    case "c203":
                        UpdateNotification(JsonConvert.SerializeObject(ApplicationDataHandler.AllEntries), item.protocolIn, item.protocolOut);
                        break;
                    case "c301":
                        UpdateNotification(ApplicationDataHandler.CurrentApplicationData.CurrentCompetition.ToString(), item.protocolIn, item.protocolOut);
                        break;
                    case "c401":
                        UpdateNotification(JsonConvert.SerializeObject(ApplicationDataHandler.Users), item.protocolIn, item.protocolOut);
                        break;
                    case "c501":
                        //var backup = ApplicationDataHandler.Instance.GetBackupWithoutApplying().Result;
                        //UpdateNotification(backup.CreatedAt.ToLongDateString(), item.protocolIn, item.protocolOut);
                        break;
                    case "c701":
                        UpdateNotification(ApplicationDataHandler.CurrentApplicationData.SecurityMode.ToString(), item.protocolIn, item.protocolOut);
                        break;
                }
                var removed = ServerManagement.QueueIn.Remove(ServerManagement.QueueIn.Single(el => el.communicationId == item.communicationId));
                //Console.WriteLine("Should be cleansed");
                var method = ServerManagement.Server.GetType().GetMethod("refresh");
                if(method != null)
                {
                    method.Invoke(ServerManagement.Server, new object[] { });
                }
            }
            else
            {
                NextMessageTrigger(item);
            }
            

        }
        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static string GenerateRandomHexString()
        {
            var characters = "0 1 2 3 4 5 6 7 8 9 a b c d e f";
            var randomGen = new System.Random();
            var finalString = "";
            for (var i = 0; i < 8; i++)
            {
                finalString = finalString + characters.Split(' ')[randomGen.Next(0, 16)];
            }
            return finalString;
        }
    }
    public class AdvertiserCallback : AdvertiseCallback
    {
        public async override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            Console.WriteLine(settingsInEffect.IsConnectable);
            AddToBluetoothLog("", BluetoothLogType.Successful);
            base.OnStartSuccess(settingsInEffect);
        }
        public async override void OnStartFailure([GeneratedEnum] AdvertiseFailure errorCode)
        {
            Console.WriteLine(errorCode.ToString());
            AddToBluetoothLog("", BluetoothLogType.Unsuccessful);
            base.OnStartFailure(errorCode);
        }
        public async void AddToBluetoothLog(string extraData, BluetoothLogType bluetoothLogType)
        {
            var existingData = await ServerManagement.storageManager.GetData("bluetooth_log");
            if (existingData == "")
            {
                var emptyList = new List<BluetoothDataLog>() { new BluetoothDataLog() { timestamp = DateTime.Now, bluetoothLogType = bluetoothLogType, extraData = extraData } };
                existingData = Newtonsoft.Json.JsonConvert.SerializeObject(emptyList);
            }
            else
            {
                var existingList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BluetoothDataLog>>(existingData);
                var newItem = new BluetoothDataLog() { extraData = extraData, bluetoothLogType = bluetoothLogType, timestamp = DateTime.Now };
                existingList.Add(newItem);
                existingData = Newtonsoft.Json.JsonConvert.SerializeObject(existingList);
            }
            Console.WriteLine(await ServerManagement.storageManager.SetData("bluetooth_log", existingData));
        }
    }


    class PeripheralManagement : BLEPeripheral
    {
        private ServerManagement serverManagement = new ServerManagement();
        private AdvertiseCallback callback;
        public void StartAdvertising(string serviceUUID, string serviceName)
        {
            AdvertiseSettings settings = new AdvertiseSettings.Builder().SetConnectable(true).Build();

            BluetoothAdapter.DefaultAdapter.SetName("GP-" + ApplicationDataHandler.CurrentApplicationData.DeviceId);

            ParcelUuid parcelUuid = new ParcelUuid(UUID.FromString("00000862-0000-1000-8000-00805f9b34fb"));
            AdvertiseData data = new AdvertiseData.Builder().AddServiceUuid(parcelUuid).SetIncludeDeviceName(true).Build();

            this.callback = new AdvertiserCallback();
            BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser.StartAdvertising(settings, data, callback);

            BluetoothGattCharacteristic chara = new BluetoothGattCharacteristic(
                UUID.FromString("00000001-0000-1000-8000-00805f9b34fb"),
                GattProperty.Read | GattProperty.Write | GattProperty.Notify,
                GattPermission.Read | GattPermission.Write
            );
            BluetoothGattDescriptor uniqueIdDesc = new BluetoothGattDescriptor(UUID.FromString("0000a404-0000-1000-8000-00805F9B34FB"), GattDescriptorPermission.Read);
            BluetoothGattDescriptor notifdesc = new BluetoothGattDescriptor(UUID.FromString("00002902-0000-1000-8000-00805F9B34FB"), GattDescriptorPermission.Write | GattDescriptorPermission.Read);
            chara.AddDescriptor(uniqueIdDesc);
            chara.AddDescriptor(notifdesc);
            BluetoothGattService service = new BluetoothGattService(
                UUID.FromString("00000862-0000-1000-8000-00805f9b34fb"),
                GattServiceType.Primary
            );
            service.AddCharacteristic(chara);

            BluetoothManager manager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Context.BluetoothService);
            BluetoothGattServer server = manager.OpenGattServer(Android.App.Application.Context, new GattServerCallback());
            //server.GetType().GetMethod()
            ServerManagement.Server = server;
            server.AddService(service);
        }
    }
}