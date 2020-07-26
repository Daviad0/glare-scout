using System;
using System.Collections.Generic;
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
        IUsbSerialPort port;

        SerialInputOutputManager serialIoManager;

        UsbManager usbManager;
        public void SendData(string data)
        {
            var usbList = usbManager.DeviceList;
            foreach(var item in usbList)
            {
                Console.WriteLine(item.Key);
            }
        }
    }
}