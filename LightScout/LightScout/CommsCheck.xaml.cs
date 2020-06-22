using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LightScout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommsCheck : ContentPage
    {
        private static IBluetoothLE ble = CrossBluetoothLE.Current;
        private static IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        private static IDevice deviceIWant;
        private static ObservableCollection<IDevice> Devices = new ObservableCollection<IDevice>();
        public CommsCheck()
        {
            InitializeComponent();
            adapter.DeviceDiscovered += async (s, a) =>
            {
                
                if (a.Device.Name != null)
                {
                    Devices.Add(a.Device);
                }
                listofdevices.ItemsSource = Devices;

            };
            adapter.DeviceConnected += async (s, a) =>
            {
                Console.WriteLine("Connected to: " + a.Device.Name.ToString());
                currentStatus.Text = "Connected to: " + a.Device.Name.ToString();
                deviceIWant = a.Device;
                listofdevices.IsVisible = false;
                sendDataToBT.IsVisible = true;
            };
            adapter.DeviceConnectionLost += (s, a) =>
            {
                Console.WriteLine("Lost connection to: " + a.Device.Name.ToString());
                currentStatus.Text = "Disconnected from: " + a.Device.Name.ToString();
                listofdevices.IsVisible = true;
                sendDataToBT.IsVisible = false;
                Devices.Clear();
            };
            adapter.DeviceDisconnected += (s, a) =>
            {
                Console.WriteLine("Lost connection to: " + a.Device.Name.ToString());
                currentStatus.Text = "Disconnected from: " + a.Device.Name.ToString();
                listofdevices.IsVisible = true;
                sendDataToBT.IsVisible = false;
                Devices.Clear();
            };
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

            if (deviceIWant != null)
            {
                await adapter.DisconnectDeviceAsync(deviceIWant);
                await adapter.ConnectToDeviceAsync(selectedDevice);
            }
            else
            {
                await adapter.ConnectToDeviceAsync(selectedDevice);
            }
        }

        private async void sendDataToBT_Clicked(object sender, EventArgs e)
        {
            var servicetosend = await deviceIWant.GetServiceAsync(Guid.Parse("50dae772-d8aa-4378-9602-792b3e4c198d"));
            var characteristictosend = await servicetosend.GetCharacteristicAsync(Guid.Parse("50dae772-d8aa-4378-9602-792b3e4c198e"));
            var stringtoconvert = messageToSEND.Text;
            var bytestotransmit = Encoding.ASCII.GetBytes(stringtoconvert);
            await characteristictosend.WriteAsync(bytestotransmit);
            Console.WriteLine(bytestotransmit);
        }

        private void dcFromBT_Clicked(object sender, EventArgs e)
        {
            adapter.DisconnectDeviceAsync(deviceIWant);
        }
    }
}