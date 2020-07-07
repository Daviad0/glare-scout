using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LightScout
{
    class SubmitVIABluetooth
    {
        public IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        public bool resultsubmitted = false;
        public async Task SubmitBluetooth()
        {

            foreach (var device in adapter.ConnectedDevices)
            {
                await adapter.DisconnectDeviceAsync(device);
            }
            adapter.DeviceConnected += async (s, a) =>
            {
                KnownDeviceSubmit(a.Device);
            };
            adapter.DeviceDiscovered += async (s, a) =>
            {
                adapter.ConnectToDeviceAsync(a.Device);
            };
            adapter.ScanTimeout = 10;
            await adapter.StartScanningForDevicesAsync(new Guid[] { Guid.Parse("6ad0f836b49011eab3de0242ac130000") });
        }
        public async void KnownDeviceSubmit(IDevice deviceIWant)
        {
            var returnvalue = DependencyService.Get<DataStore>().LoadData("JacksonEvent2020.txt");
            var servicetosend = await deviceIWant.GetServiceAsync(Guid.Parse("6ad0f836b49011eab3de0242ac130000"));
            var characteristictosend = await servicetosend.GetCharacteristicAsync(Guid.Parse("6ad0f836b49011eab3de0242ac130001"));
            characteristictosend.ValueUpdated += (s, a) =>
            {
                Console.WriteLine(a.Characteristic.Value);
            };
            await characteristictosend.StartUpdatesAsync();
            var stringtoconvert = "S:" + returnvalue;
            var bytestotransmit = Encoding.ASCII.GetBytes(stringtoconvert);
            if (bytestotransmit.Length > 480)
            {
                int numberofmessages = (bytestotransmit.Length / 480);
                var startidentifier = "MM:" + numberofmessages.ToString();
                var startbytesarray = Encoding.ASCII.GetBytes(startidentifier);
                await characteristictosend.WriteAsync(startbytesarray);
                for (int i = numberofmessages; i > 0; i--)
                {
                    var bytesarray = bytestotransmit.Skip((numberofmessages - i) * 480).Take(480).ToArray();
                    await characteristictosend.WriteAsync(bytesarray);
                }
            }
            else
            {
                await characteristictosend.WriteAsync(bytestotransmit);
            }

            stringtoconvert = "B:" + Battery.ChargeLevel.ToString();
            bytestotransmit = Encoding.ASCII.GetBytes(stringtoconvert);
            await characteristictosend.WriteAsync(bytestotransmit);
            Console.WriteLine(bytestotransmit);
        }
    }
}
