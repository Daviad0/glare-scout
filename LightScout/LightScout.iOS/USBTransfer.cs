using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Foundation;
using LightScout.Models;
using Newtonsoft.Json;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(LightScout.iOS.USBTransfer))]
namespace LightScout.iOS
{
    public class USBTransfer : USBCommunication
    {
        public void SendData(string rawstring)
        {

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Loopback, 862));
                //MessagingCenter.Send<object, int>(this, "USBResponse", 1);
                socket.Listen(100);
                socket.BeginAccept((ar) =>
                {
                    //MessagingCenter.Send<object, int>(this, "USBResponse", 2);

                    var connectionAttempt = (Socket)ar.AsyncState;
                    var connectedSocket = connectionAttempt.EndAccept(ar);
                    var tabletid = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).TabletIdentifier;
                    connectedSocket.BeginSend(Encoding.ASCII.GetBytes(tabletid + ":S:" + rawstring), 0, Encoding.ASCII.GetBytes(tabletid + ":S:" + rawstring).Length, SocketFlags.None, (ars) =>
                    {
                        connectedSocket.BeginSend(Encoding.ASCII.GetBytes(tabletid + ":B:" + Battery.ChargeLevel.ToString()), 0, Encoding.ASCII.GetBytes(tabletid + ":B:" + Battery.ChargeLevel.ToString()).Length, SocketFlags.None, USBCallBack, connectedSocket);

                    }, connectedSocket);


                    socket.Close();
                    //MessagingCenter.Send<object, int>(this, "USBResponse", 3);

                }, socket);
            }catch(Exception ex)
            {
                //MessagingCenter.Send<object, int>(this, "USBResponse", -1);
            }

            /*byte[] gottenBytes = new byte[200];
            socket.BeginReceive(gottenBytes, 0, gottenBytes.Length, SocketFlags.None, (ars) =>
            {
                Console.WriteLine("We did it gamers");

            }, socket);*/

        }
        public void USBCallBack(IAsyncResult result)
        {

        }
    }
}