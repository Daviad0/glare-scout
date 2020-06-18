using LightScout.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LightScout
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private static bool[] ControlPanel = new bool[2];
        private static bool Balanced;
        private static int BluetoothDevices = 0;
        public MainPage()
        {
            InitializeComponent();
            ControlPanel[0] = false;
            ControlPanel[1] = false;
        }

        private void FTCShow_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FTCMain());
        }

        private void FRCShow_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FRCMain());
        }
        private async void CheckBluetooth(object sender, EventArgs e)
        {
            BindingContext = new BluetoothDeviceViewModel();
            /*var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            adapter.DeviceDiscovered += async (s, a) =>
            {

                BluetoothDevices++;
                if (a.Device.Name != null)
                {
                    if (a.Device.Name == "David's MacBook Air")
                    {
                        BluetoothList.Text = BluetoothList.Text + " ID = " + a.Device.Id.ToString();
                    }
                    Console.WriteLine("Name: " + a.Device.Name.ToString() + ", ID: " + a.Device.Id.ToString());
                    Console.WriteLine("16fd1a9b-f36f-7eab-66b2-499bf4dbb0f2");
                    if (a.Device.Id.ToString() == "16fd1a9b-f36f-7eab-66b2-499bf4dbb0f2")
                    {
                        Console.WriteLine("Attempting to connect to: " + a.Device.Name.ToString());
                        await adapter.ConnectToDeviceAsync(a.Device);

                    }

                }


            };
            adapter.DeviceConnected += (s, a) =>
            {
                Console.WriteLine("Connected to: " + a.Device.Name.ToString());
            };
            adapter.DeviceConnectionLost += (s, a) =>
            {
                Console.WriteLine("Lost connection to: " + a.Device.Name.ToString());
            };
            adapter.DeviceDisconnected += (s, a) =>
            {
                Console.WriteLine("Lost connection to: " + a.Device.Name.ToString());
            };
            await adapter.StartScanningForDevicesAsync();*/
        }
        private void AddDevice(object s, DeviceEventArgs a)
        {
           
        }
    }
}
