using Plugin.BLE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace LightScout.Models
{
    public class BluetoothDeviceViewModel
    {
        private static ObservableCollection<string> bluetoothDeviceNames = new ObservableCollection<string>();
        //private static HttpClient client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> BluetoothDeviceNames
        {
            get { return bluetoothDeviceNames; }
            set
            {

                bluetoothDeviceNames = value;
            }
        }

        public async Task Update()
        {
            BluetoothDeviceNames.Clear();
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            adapter.DeviceDiscovered += (s, a) =>
            {

                BluetoothDeviceNames.Add(a.Device.Name.ToString());

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
    }
}
