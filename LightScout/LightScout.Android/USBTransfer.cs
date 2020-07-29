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
    public class USBTransfer : USBCommunication
    {
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
        public void USBCallBack(IAsyncResult result)
        {

        }
    }
}