using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace LightScout
{
    public sealed class BLEConnection
    {
        private static readonly object l1 = new object();  
        public static BLEConnection instance = null;
        public static BLEConnection Instance
        {
            get
            {
                lock (l1)
                {
                    if (instance == null)
                    {
                        instance = new BLEConnection();
                    }
                    return instance;
                }
                
            }
        }

        public List<IDevice> detectedDevices = new List<IDevice>();
        public IDevice currentlyConnectedDevice = null;

        private BLEConnection()
        {
            ble.StateChanged += (sender, eArgs) =>
            {
                Console.WriteLine("The BLE state has been changed to " + eArgs.NewState.ToString());
            };
            adapter.DeviceDiscovered += (sender, eArgs) =>
            {
                if (DeviceDetected != null)
                {
                    DeviceDetected.Invoke(eArgs.Device, DateTime.Now);
                }
                
            };
            adapter.DeviceConnected += (sender, eArgs) =>
            {
                if (DeviceConnected != null)
                {
                    DeviceConnected.Invoke(eArgs.Device, true);
                }
                
                currentlyConnectedDevice = eArgs.Device;
            };
            adapter.DeviceDisconnected += (sender, eArgs) =>
            {
                if(DeviceDisconnected != null)
                {
                    DeviceDisconnected.Invoke(eArgs.Device);
                }
                
                currentlyConnectedDevice = null;
            };
        }
        public DateTime previousDetection = DateTime.Now;

        public int teamNumber = 862;
        public string deviceId = "1234567a";
        public string schemaId = "af7820be";
        
        public static IBluetoothLE ble = CrossBluetoothLE.Current;
        public static IAdapter adapter = CrossBluetoothLE.Current.Adapter;

        public delegate void DeviceDetectionHandler(object device, DateTime at);
        public event DeviceDetectionHandler DeviceDetected;

        public async void RedetectDevices()
        {
            
            adapter.ScanTimeout = 15000;
            await adapter.StartScanningForDevicesAsync();
            previousDetection = DateTime.Now;
        }
        public async void StopDetecting()
        {
            await adapter.StopScanningForDevicesAsync();
        }

        public delegate void DeviceConnectionHandler(object device, bool successful);
        public event DeviceConnectionHandler DeviceConnected;

        public delegate void DeviceDisconnectionHandler(object device);
        public event DeviceDisconnectionHandler DeviceDisconnected;

        public async void ConnectToUnknownDevice(IDevice device)
        {
            try
            {
                await adapter.ConnectToDeviceAsync(device, new Plugin.BLE.Abstractions.ConnectParameters(true, true));
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                DeviceConnected.Invoke(null, false);
            }
            
        }
        public async void ConnectToKnownDevice(string GUID)
        {
            try
            {
                await adapter.ConnectToKnownDeviceAsync(Guid.Parse(GUID), new Plugin.BLE.Abstractions.ConnectParameters(true, true));
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                DeviceConnected.Invoke(null, false);
            }
        } 

        public async void DisconnectDevice()
        {
            try
            {
                await adapter.DisconnectDeviceAsync(currentlyConnectedDevice);
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to disconnect from device!");
            }
        }
        public delegate void MessageSendingHandler(string message, int totalMessages, int currentMessage, bool finished);
        public event MessageSendingHandler MessageSent;

        public async void SubmitData(string serviceId, string characteristicId, string data, IDevice device)
        {
            
            
            var service = await device.GetServiceAsync(Guid.Parse(serviceId));
            var characteristic = await service.GetCharacteristicAsync(Guid.Parse(characteristicId));
            var communicationId = GenerateRandomHexString();
            var encodedMessage = Encoding.ASCII.GetBytes(data);
            var numMessages = (int)Math.Ceiling((float)encodedMessage.Length / (float)459);
            for(int i = 0; i < numMessages; i++)
            {
                var headerString = teamNumber.ToString("0000") + deviceId + schemaId + (i + 1 == numMessages ? "ee" : "aa") + (i + 1).ToString("0000") + communicationId;
                var finalByteArray = StringToByteArray(headerString).Concat(encodedMessage.Skip(i * 459).ToArray()).ToArray();
                await characteristic.WriteAsync(finalByteArray);
                MessageSent.Invoke(Encoding.ASCII.GetString(encodedMessage.Skip(i * 459).ToArray()), numMessages, i + 1, false);
            }
            MessageSent.Invoke(data, numMessages, numMessages, true);
            await adapter.DisconnectDeviceAsync(device);
        }

        public bool RemoveEventReferences()
        {
            if(DeviceDetected != null)
            {
                foreach (Delegate d in DeviceDetected.GetInvocationList())
                {
                    DeviceDetected -= (DeviceDetectionHandler)d;
                }
            }
            if (DeviceConnected != null)
            {
                foreach (Delegate d in DeviceConnected.GetInvocationList())
                {
                    DeviceConnected -= (DeviceConnectionHandler)d;
                }
            }
            if (DeviceDisconnected != null)
            {
                foreach (Delegate d in DeviceDisconnected.GetInvocationList())
                {
                    DeviceDisconnected -= (DeviceDisconnectionHandler)d;
                }
            }
            if (MessageSent != null)
            {
                foreach (Delegate d in MessageSent.GetInvocationList())
                {
                    MessageSent -= (MessageSendingHandler)d;
                }
            }
            return true;
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static string GenerateRandomHexString()
        {
            var characters = "0 1 2 3 4 5 6 7 8 9 a b c d e f";
            var randomGen = new Random();
            var finalString = "";
            for(var i = 0; i < 8; i++)
            {
                finalString = finalString + characters.Split(' ')[randomGen.Next(0, 16)];
            }
            return finalString;
        }
    }
}
