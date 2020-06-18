using LightScout.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private static ObservableCollection<string> Devices = new ObservableCollection<string>();
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
            //BindingContext = new BluetoothDeviceViewModel();
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            Devices.Clear();
            adapter.DeviceDiscovered += async (s, a) =>
            {

                BluetoothDevices++;
                if (a.Device.Name != null)
                {
                    Devices.Add(a.Device.Name);
                }
                listofdevices.ItemsSource = Devices;

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
            await adapter.StartScanningForDevicesAsync();
        }
        private void AddDevice(object s, DeviceEventArgs a)
        {
           
        }
    }
}
