using Akavache;
using LightScout.Models;
using Newtonsoft.Json;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
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
        private static Mutex mutex = new Mutex();
        public static IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        public bool resultsubmitted = false;
        public bool datagotten = false;
        public EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs> PdeviceDC;
        public EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs> PdeviceC;
        public EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs> PdeviceD;
        public EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs> GdeviceDC;
        public EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs> GdeviceC;
        public EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs> GdeviceD;
        public IDevice connectedPeripheral;
        public async Task SubmitBluetooth(CancellationToken token)
        {
            PdeviceDC = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (!resultsubmitted)
                {
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", -1);
                    adapter.DeviceDiscovered -= PdeviceDC;
                }
            });
            PdeviceC = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (!resultsubmitted)
                {
                    connectedPeripheral = a.Device;
                    KnownDeviceSubmit(a.Device);
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", 2);
                    resultsubmitted = true;
                    adapter.DeviceDiscovered -= PdeviceC;
                }
            });
            PdeviceD = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (a.Device.Name == "LRSS" || a.Device.Id == Guid.Parse("16FD1A9B-F36F-7EAB-66B2-499BF4DBB0F2"))
                {
                    adapter.ConnectToDeviceAsync(a.Device);
                    adapter.DeviceDiscovered -= PdeviceD;
                }
            });
            foreach (var device in adapter.ConnectedDevices)
            {
                await adapter.DisconnectDeviceAsync(device);
            }
            adapter.DeviceDisconnected += PdeviceDC;
            adapter.DeviceConnected += PdeviceC;
            adapter.DeviceDiscovered += PdeviceD;
            resultsubmitted = false;
            MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", 1);
            try
            {
                adapter.ScanMode = ScanMode.LowPower;
                await adapter.StartScanningForDevicesAsync();
                //await adapter.ConnectToKnownDeviceAsync(Guid.Parse("16FD1A9B-F36F-7EAB-66B2-499BF4DBB0F2"), default, token);
                Device.StartTimer(TimeSpan.FromSeconds(0.1), () =>
                {
                    if (resultsubmitted)
                    {
                        return false;
                    }
                    else
                    {
                        try
                        {
                            token.ThrowIfCancellationRequested();

                        }
                        catch (Exception ex)
                        {
                            MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", -1);
                            resultsubmitted = true;
                        }
                        return true;
                    }
                });
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", -1);
                resultsubmitted = true;
            }


        }
        public async void KnownDeviceSubmit(IDevice deviceIWant)
        {
            var rawreturnvalue = DependencyService.Get<DataStore>().LoadData("JacksonEvent2020.txt");
            List<TeamMatch> beforeList = JsonConvert.DeserializeObject<List<TeamMatch>>(rawreturnvalue);
            DateTime lastSubmitted = (DateTime)Application.Current.Properties["TimeLastSubmitted"];
            List<TeamMatch> afterMatch = beforeList.Where(x => x.ClientLastSubmitted == null || x.ClientLastSubmitted > lastSubmitted).ToList();
            string returnvalue = JsonConvert.SerializeObject(afterMatch);
            var servicetosend = await deviceIWant.GetServiceAsync(Guid.Parse("6ad0f836b49011eab3de0242ac130000"));
            var uuid = new Guid();
            var tabletid = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).TabletIdentifier;
            if (tabletid == "R1")
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
            await deviceIWant.RequestMtuAsync(512);
            var characteristictosend = await servicetosend.GetCharacteristicAsync(uuid);
            characteristictosend.ValueUpdated += (s, a) =>
            {
                Console.WriteLine(a.Characteristic.Value);
            };
            try
            {
                var deviceid = new Random().Next(1000, 9999);
                await characteristictosend.StartUpdatesAsync();
                var stringtoconvert = "S:" + deviceid.ToString() + ":" + returnvalue;
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
                MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", 3);
                stringtoconvert = "B:" + deviceid.ToString() + ":" + Battery.ChargeLevel.ToString();
                bytestotransmit = Encoding.ASCII.GetBytes(stringtoconvert);
                await characteristictosend.WriteAsync(bytestotransmit);
                resultsubmitted = true;
                Application.Current.Properties["TimeLastSubmitted"] = DateTime.Now;
                Console.WriteLine(bytestotransmit);
                await adapter.DisconnectDeviceAsync(connectedPeripheral);
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", -1);
            }

        }
        public async Task GetDefaultData(CancellationToken token)
        {

            foreach (var device in adapter.ConnectedDevices)
            {
                await adapter.DisconnectDeviceAsync(device);
            }
            GdeviceDC = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (!datagotten)
                {
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", -1);
                    adapter.DeviceDisconnected -= GdeviceDC;
                }
            });
            GdeviceC = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (!datagotten)
                {
                    connectedPeripheral = a.Device;
                    KnownDeviceGet(a.Device);
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", 2);
                    datagotten = true;
                    adapter.DeviceConnected -= GdeviceC;
                }
            });
            GdeviceD = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (a.Device.Name == "LRSS" || a.Device.Id == Guid.Parse("16FD1A9B-F36F-7EAB-66B2-499BF4DBB0F2"))
                {
                    adapter.ConnectToDeviceAsync(a.Device);
                    adapter.DeviceDiscovered -= GdeviceD;
                }
            });
            adapter.DeviceDisconnected += GdeviceDC;
            adapter.DeviceConnected += GdeviceC;
            adapter.DeviceDiscovered += GdeviceD;
            datagotten = false;
            MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", 1);
            try
            {
                adapter.ScanMode = ScanMode.LowPower;
                adapter.StartScanningForDevicesAsync();
                Device.StartTimer(TimeSpan.FromSeconds(0.1), () =>
                {
                    if (datagotten)
                    {
                        return false;
                    }
                    else
                    {
                        try
                        {
                            token.ThrowIfCancellationRequested();

                        }
                        catch (Exception ex)
                        {
                            MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", -1);
                            resultsubmitted = true;
                        }
                        return true;
                    }
                });
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", -1);
                datagotten = true;
            }


        }
        public async void KnownDeviceGet(IDevice deviceIWant)
        {
            var returnvalue = DependencyService.Get<DataStore>().LoadData("JacksonEvent2020.txt");
            var servicetosend = await deviceIWant.GetServiceAsync(Guid.Parse("6ad0f836b49011eab3de0242ac130000"));
            if (servicetosend != null)
            {
                var uuid = new Guid();
                var tabletid = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).TabletIdentifier;
                if (tabletid == "R1")
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
                var messagesleft = 0;
                var fullmessage = "";
                bool iscompleted = false;
                await deviceIWant.RequestMtuAsync(512);
                var deviceid = new Random().Next(1000, 9999);
                characteristictosend.ValueUpdated += async (s, a) =>
                {
                    if (!iscompleted)
                    {

                        var convertedmessage = Encoding.ASCII.GetString(a.Characteristic.Value);
                        if (convertedmessage.Substring(0, 7).Contains(deviceid.ToString()))
                        {
                            if (messagesleft > 0)
                            {
                                messagesleft--;
                                fullmessage = fullmessage + convertedmessage.Substring(5);
                                if (messagesleft == 0)
                                {
                                    await adapter.DisconnectDeviceAsync(connectedPeripheral);
                                    iscompleted = true;
                                    DependencyService.Get<DataStore>().SaveDefaultData("JacksonEvent2020.txt", JsonConvert.DeserializeObject<List<TeamMatch>>(fullmessage));
                                    Device.StartTimer(TimeSpan.FromSeconds(3), () =>
                                    {
                                        MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", 3);
                                        return false;
                                    });


                                }
                            }
                            else
                            {
                                if (convertedmessage.StartsWith("MM:"))
                                {
                                    fullmessage = "";
                                    messagesleft = int.Parse(convertedmessage.Substring(8));
                                }
                                else
                                {
                                    List<TeamMatch> possibleListOfTeams = new List<TeamMatch>();
                                    bool validlist = true;
                                    try
                                    {
                                        possibleListOfTeams = JsonConvert.DeserializeObject<List<TeamMatch>>(convertedmessage.Substring(5));
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("List of teams was not valid!");
                                        validlist = false;
                                    }
                                    if (validlist)
                                    {
                                        MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", 3);
                                        await adapter.DisconnectDeviceAsync(connectedPeripheral);
                                        iscompleted = true;
                                        DependencyService.Get<DataStore>().SaveDefaultData("JacksonEvent2020.txt", possibleListOfTeams);
                                    }

                                }
                            }
                            Console.WriteLine(a.Characteristic.Value);
                        }


                    }

                };
                try
                {

                    await characteristictosend.StartUpdatesAsync();
                    var stringtoconvert = "RD:" + deviceid.ToString() + ":" + DateTime.Now.ToString();
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

                    stringtoconvert = "B:" + deviceid.ToString() + ":" + Battery.ChargeLevel.ToString();
                    bytestotransmit = Encoding.ASCII.GetBytes(stringtoconvert);
                    await characteristictosend.WriteAsync(bytestotransmit);
                    resultsubmitted = true;
                    Console.WriteLine(bytestotransmit);

                }
                catch (Exception ex)
                {
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", -1);
                }
            }
            else
            {
                MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", -1);
            }


        }
        public async Task SendTBAData(CancellationToken token)
        {

            foreach (var device in adapter.ConnectedDevices)
            {
                await adapter.DisconnectDeviceAsync(device);
            }
            GdeviceDC = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (!datagotten)
                {
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "tbasenddata", -1);
                    adapter.DeviceDisconnected -= GdeviceDC;
                }
            });
            GdeviceC = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (!datagotten)
                {
                    connectedPeripheral = a.Device;
                    KnownDeviceSendTBA(a.Device);
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "tbasenddata", 2);
                    datagotten = true;
                    adapter.DeviceConnected -= GdeviceC;
                }
            });
            GdeviceD = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (a.Device.Name == "LRSS" || a.Device.Id == Guid.Parse("16FD1A9B-F36F-7EAB-66B2-499BF4DBB0F2"))
                {
                    adapter.ConnectToDeviceAsync(a.Device);
                    adapter.DeviceDiscovered -= GdeviceD;
                }
            });
            adapter.DeviceDisconnected += GdeviceDC;
            adapter.DeviceConnected += GdeviceC;
            adapter.DeviceDiscovered += GdeviceD;
            datagotten = false;
            MessagingCenter.Send<SubmitVIABluetooth, int>(this, "tbasenddata", 1);
            try
            {
                adapter.ScanMode = ScanMode.LowPower;
                adapter.StartScanningForDevicesAsync();
                Device.StartTimer(TimeSpan.FromSeconds(0.1), () =>
                {
                    if (datagotten)
                    {
                        return false;
                    }
                    else
                    {
                        try
                        {
                            token.ThrowIfCancellationRequested();

                        }
                        catch (Exception ex)
                        {
                            MessagingCenter.Send<SubmitVIABluetooth, int>(this, "tbasenddata", -1);
                            resultsubmitted = true;
                        }
                        return true;
                    }
                });
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<SubmitVIABluetooth, int>(this, "tbasenddata", -1);
                datagotten = true;
            }


        }
        public async void KnownDeviceSendTBA(IDevice deviceIWant)
        {
            var configurationfile = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile());
            var urltoget = "https://www.thebluealliance.com/api/v3/event/" + configurationfile.CurrentEventCode + "/matches";
            try
            {
                string tbamatches = "";
                var request = (HttpWebRequest)WebRequest.Create(urltoget);
                request.Headers.Add("X-TBA-Auth-Key", "kzyt55ci5iHn3X1T8BgXYu2yMXmAjdxV5OCXHVA16CRfX8C0Z6tfrwU4BajyleY3");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream))
                {
                    tbamatches = reader.ReadToEnd();
                    Console.WriteLine(tbamatches);
                }
                tbamatches = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<TBA_Match>>(tbamatches));
                var servicetosend = await deviceIWant.GetServiceAsync(Guid.Parse("6ad0f836b49011eab3de0242ac130000"));
                if (servicetosend != null)
                {
                    var uuid = new Guid();
                    var tabletid = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).TabletIdentifier;
                    if (tabletid == "R1")
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
                    var messagesleft = 0;
                    var fullmessage = "";
                    bool iscompleted = false;
                    await deviceIWant.RequestMtuAsync(512);
                    var deviceid = new Random().Next(1000, 9999);
                    characteristictosend.ValueUpdated += async (s, a) =>
                    {
                        Console.WriteLine(a.Characteristic.Value);

                    };
                    try
                    {

                        await characteristictosend.StartUpdatesAsync();
                        var stringtoconvert = "FS:" + deviceid.ToString() + ":" + tbamatches;
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

                        resultsubmitted = true;
                        Console.WriteLine(bytestotransmit);

                    }
                    catch (Exception ex)
                    {
                        MessagingCenter.Send<SubmitVIABluetooth, int>(this, "tbasenddata", -1);
                    }
                }
                else
                {
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "tbasenddata", -1);
                }
            }
            catch(Exception ex)
            {
                MessagingCenter.Send<SubmitVIABluetooth, int>(this, "tbasenddata", -1);
            }
            


        }





        //NEW PROTOCOL BELOW




        public async Task ConnectToDevice(BLEMessageArguments args, CancellationToken token)
        {
            datagotten = false;
            foreach (var device in adapter.ConnectedDevices)
            {
                await adapter.DisconnectDeviceAsync(device);
            }
            GdeviceDC = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (!datagotten)
                {
                    adapter.DeviceDisconnected -= GdeviceDC;
                }
            });
            GdeviceC = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (!datagotten)
                {
                    MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = "Connected to " + a.Device.Name, status = BluetoothControllerDataStatus.Connected });
                    connectedPeripheral = a.Device;
                    SendToLightSwitch(a.Device,args);
                    datagotten = true;
                    adapter.DeviceConnected -= GdeviceC;
                }
            });
            GdeviceD = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (a.Device.Name == "LRSS" || a.Device.Id == Guid.Parse("16FD1A9B-F36F-7EAB-66B2-499BF4DBB0F2"))
                {
                    adapter.ConnectToDeviceAsync(a.Device);
                    adapter.DeviceDiscovered -= GdeviceD;
                }
            });
            adapter.DeviceDisconnected += GdeviceDC;
            adapter.DeviceConnected += GdeviceC;
            adapter.DeviceDiscovered += GdeviceD;
            try
            {
                adapter.ScanMode = ScanMode.LowPower;
                adapter.StartScanningForDevicesAsync();
                Device.StartTimer(TimeSpan.FromSeconds(0.1), () =>
                {
                    if (datagotten)
                    {
                        return false;
                    }
                    else
                    {
                        try
                        {
                            token.ThrowIfCancellationRequested();

                        }
                        catch (Exception ex)
                        {
                            MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = ex.ToString(), status = BluetoothControllerDataStatus.Abort });
                            resultsubmitted = true;
                        }
                        return true;
                    }
                });
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = ex.ToString(), status = BluetoothControllerDataStatus.Abort });
                datagotten = true;
            }
        }
        public async void SendToLightSwitch(IDevice selectedDevice, BLEMessageArguments args)
        {
            var servicetosend = await selectedDevice.GetServiceAsync(Guid.Parse("6ad0f836b49011eab3de0242ac130000"));
            var uuid = new Guid();
            var tabletid = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).TabletIdentifier;
            if (tabletid == "R1")
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
            await selectedDevice.RequestMtuAsync(512);
            selectedDevice.UpdateConnectionInterval(ConnectionInterval.High);
            var messagesleft = 0;
            var fullmessage = "";
            bool iscompleted = false;
            var deviceid = await BlobCache.UserAccount.GetObject<string>("UniqueID");
            var characteristictosend = await servicetosend.GetCharacteristicAsync(uuid);
            characteristictosend.ValueUpdated += async (s, a) =>
            {

                var convertedmessage = Encoding.ASCII.GetString(a.Characteristic.Value);
                if (convertedmessage.Substring(4,10).Contains(deviceid))
                {
                    var data = convertedmessage.Substring(16);
                    if (messagesleft > 0)
                    {
                        if (convertedmessage.StartsWith("F!"))
                        {
                            messagesleft--;
                            Console.WriteLine(messagesleft.ToString() + " left!");
                            fullmessage = fullmessage + data;
                            if (messagesleft == 0)
                            {
                                await adapter.DisconnectDeviceAsync(connectedPeripheral);
                                iscompleted = true;
                                //SEND BACK TO HANDLER DATA
                                MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = fullmessage, status = BluetoothControllerDataStatus.DataGet });


                            }
                        }
                        else
                        {
                            Console.WriteLine("Expected a following message, got " + convertedmessage.Substring(0, 2));
                            MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = "Unexpected handler!", status = BluetoothControllerDataStatus.Abort });
                        }
                        
                    }
                    else
                    {
                        
                        if (convertedmessage.StartsWith("L!"))
                        {
                            fullmessage = "";
                            messagesleft = int.Parse(data);
                        }
                        else if(convertedmessage.StartsWith("S!"))
                        {
                            //SEND BACK TO HANDLER DATA
                            MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = "Got a single message!", status = BluetoothControllerDataStatus.DataGet });
                            await adapter.DisconnectDeviceAsync(connectedPeripheral);

                        }
                        else
                        {
                            Console.WriteLine("Expected a leading or single message, got " + convertedmessage.Substring(0, 2));
                            MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = "Unexpected handler!", status = BluetoothControllerDataStatus.Abort });
                        }
                    }
                    Console.WriteLine(a.Characteristic.Value);
                }


                Console.WriteLine(a.Characteristic.Value);
            };
            try
            {
                
                await characteristictosend.StartUpdatesAsync();
                
                var bytestotransmit = Encoding.ASCII.GetBytes(args.messageData);
                if (bytestotransmit.Length > 460)
                {
                    
                    int numberofmessages = (int)Math.Ceiling((float)bytestotransmit.Length / (float)460);
                    var leadingmessage = "L!" + deviceid + ":" + args.messageType.ToString("00") + "@" + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00")+ "*1>>" + numberofmessages.ToString();
                    var followingheader = Encoding.ASCII.GetBytes("F!" + deviceid + ">>");
                    await characteristictosend.WriteAsync(Encoding.ASCII.GetBytes(leadingmessage));
                    for (int i = numberofmessages; i > 0; i--)
                    {
                        MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = (100-((float)i/(float)numberofmessages)).ToString(), status = BluetoothControllerDataStatus.DataSent });
                        var bytesarray = followingheader.Concat(bytestotransmit.Skip((numberofmessages - i) * 460).Take(460).ToArray()).ToArray();
                        Console.WriteLine(Encoding.ASCII.GetString(bytesarray));
                        await characteristictosend.WriteAsync(bytesarray);
                    }
                }
                else
                {
                    var singlemessage = "S!" + deviceid + ":" + args.messageType.ToString("00") + "@" + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + "*1>>" + args.messageData;
                    await characteristictosend.WriteAsync(Encoding.ASCII.GetBytes(singlemessage));
                }
                resultsubmitted = true;
                MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = "100", status = BluetoothControllerDataStatus.DataSent });
                Application.Current.Properties["TimeLastSubmitted"] = DateTime.Now;
                MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = "Sent " + bytestotransmit.Length.ToString() + " message bytes successfully!", status = BluetoothControllerDataStatus.DataSent });
                Console.WriteLine(bytestotransmit);
                if(args.expectation == ResponseExpectation.NoResponse)
                {
                    await adapter.DisconnectDeviceAsync(connectedPeripheral);
                }
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", new BluetoothControllerData() { timestamp = DateTime.Now, data = ex.ToString(), status = BluetoothControllerDataStatus.Abort });
            }
        }
        public enum ResponseExpectation
        {
            Expected,
            Optional,
            NoResponse
        }
        public class BLEMessageArguments
        {
            public int messageType { get; set; }
            public string messageData { get; set; }
            public ResponseExpectation expectation { get; set; }
        }
    }
    public class BluetoothControllerData
    {
        public BluetoothControllerDataStatus status { get; set; }
        public string data { get; set; }
        public DateTime timestamp { get; set; }
    }
    public enum BluetoothControllerDataStatus
    {
        Abort = 0,
        Initialize = 1,
        Connected = 2,
        DataSent = 3,
        DataGet = 4,
        Progress = 8
    }
}
