using LightScout.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
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
        private static IBluetoothLE ble = CrossBluetoothLE.Current;
        private static IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        private static IDevice deviceIWant;
        private static bool Balanced;
        private static int BluetoothDevices = 0;
        private static ObservableCollection<IDevice> Devices = new ObservableCollection<IDevice>();
        public MainPage()
        {
            InitializeComponent();
            ControlPanel[0] = false;
            ControlPanel[1] = false;
            adapter.DeviceDiscovered += async (s, a) =>
            {

                BluetoothDevices++;
                if (a.Device.Name != null)
                {
                    Devices.Add(a.Device);
                }
                listofdevices.ItemsSource = Devices;

            };
            adapter.DeviceConnected += async (s, a) =>
            {
                Console.WriteLine("Connected to: " + a.Device.Name.ToString());
                BluetoothFind.Text = "Connected to: " + a.Device.Name.ToString();
                deviceIWant = a.Device;
                var services = await deviceIWant.GetServicesAsync();
                Console.WriteLine("Current Services: " + services.ToString());
            };
            adapter.DeviceConnectionLost += (s, a) =>
            {
                Console.WriteLine("Lost connection to: " + a.Device.Name.ToString());
                BluetoothFind.Text = "Disconnected from: " + a.Device.Name.ToString();
            };
            adapter.DeviceDisconnected += (s, a) =>
            {
                Console.WriteLine("Lost connection to: " + a.Device.Name.ToString());
                BluetoothFind.Text = "Disconnected from: " + a.Device.Name.ToString();
            };
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
            
            Devices.Clear();
            
            await adapter.StartScanningForDevicesAsync();
            
        }

        private async void listofdevices_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            IDevice selectedDevice = e.Item as IDevice;
            
            if(deviceIWant != null)
            {
                await adapter.DisconnectDeviceAsync(deviceIWant);
                await adapter.ConnectToDeviceAsync(selectedDevice);
            }
            else
            {
                await adapter.ConnectToDeviceAsync(selectedDevice);
            }
        }
    }
}
