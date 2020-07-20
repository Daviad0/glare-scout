using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Foundation;
using LightScout.Models;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(LightScout.iOS.USBTransfer))]
namespace LightScout.iOS
{
    public class USBTransfer : USBCommunication
    {
        public void SendData(string rawstring)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 862));

            socket.Listen(100);
            socket.BeginSend(Encoding.ASCII.GetBytes(rawstring), 0, Encoding.ASCII.GetBytes(rawstring).Length, SocketFlags.None, USBCallBack, new LSConfiguration());
        }
        public void USBCallBack(IAsyncResult result)
        {

        }
    }
}