﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;

[assembly: Xamarin.Forms.Dependency(typeof(LightScout.Droid.USBTransfer))]
namespace LightScout.Droid
{
    public class USBTransfer : USBCommunication
    {
        public void SendData(string data)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach(var port in ports)
            {
                Console.WriteLine(port);
            }
        }
    }
}