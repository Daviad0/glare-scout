using LightScout.Models;
using Newtonsoft.Json;
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
    class SubmitVIABluetooth : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
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
                if (!resultsubmitted)
                {
                    KnownDeviceSubmit(a.Device);
                    resultsubmitted = true;
                }
                
            };
            adapter.DeviceDiscovered += async (s, a) =>
            {
                adapter.ConnectToDeviceAsync(a.Device);
            };
            await adapter.ConnectToKnownDeviceAsync(Guid.Parse("16FD1A9B-F36F-7EAB-66B2-499BF4DBB0F2"));
        }
        public async void KnownDeviceSubmit(IDevice deviceIWant)
        {
            var returnvalue = DependencyService.Get<DataStore>().LoadData("JacksonEvent2020.txt");
            var servicetosend = await deviceIWant.GetServiceAsync(Guid.Parse("6ad0f836b49011eab3de0242ac130000"));
            var uuid = new Guid();
            var tabletid = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadData("LSConfiguration.txt")).TabletIdentifier;
            if(tabletid == "R1")
            {
                uuid = Guid.Parse("6ad0f836b49011eab3de0242ac130001");
            }
            else if (tabletid == "R2")
            {
                uuid = Guid.Parse("6ad0f836b49011eab3de0242ac130002");
            }
            else if (tabletid == "R3")
            {
                uuid = Guid.Parse("6ad0f836b49011eab3de0242ac130003");
            }
            else if (tabletid == "B1")
            {
                uuid = Guid.Parse("6ad0f836b49011eab3de0242ac130004");
            }
            else if (tabletid == "B2")
            {
                uuid = Guid.Parse("6ad0f836b49011eab3de0242ac130005");
            }
            else if (tabletid == "B3")
            {
                uuid = Guid.Parse("6ad0f836b49011eab3de0242ac130006");
            }
            else
            {
                uuid = Guid.Parse("6ad0f836b49011eab3de0242ac130001");
            }
            var characteristictosend = await servicetosend.GetCharacteristicAsync(uuid);
            characteristictosend.ValueUpdated += (s, a) =>
            {
                Console.WriteLine(a.Characteristic.Value);
            };
            await characteristictosend.StartUpdatesAsync();
            var stringtoconvert = "S:" + returnvalue;
            var bytestotransmit = Encoding.ASCII.GetBytes(stringtoconvert);
            if (bytestotransmit.Length > 480)
            {
                int numberofmessages = (int)Math.Ceiling((float)bytestotransmit.Length / (float)480);
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
