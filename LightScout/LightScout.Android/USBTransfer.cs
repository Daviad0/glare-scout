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

                        // put handler here
                        sendBack = "1:" + "DATA";
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