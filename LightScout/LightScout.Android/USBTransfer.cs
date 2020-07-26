using System;
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
            var usbManager = (UsbManager)Application.Context.GetSystemService(Context.UsbService);
            var devices = usbManager.DeviceList;
            var access = usbManager.GetAccessoryList();
            foreach(var dv in devices)
            {
                Console.WriteLine(dv.Key);
            }
            foreach (var a in access)
            {
                Console.WriteLine("Found");
            }
        }
    }
}