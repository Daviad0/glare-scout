using LightScout.Models;
using Newtonsoft.Json;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        public bool resultsubmitted = false;
        public bool datagotten = false;
        public IDevice connectedPeripheral;
        public async Task SubmitBluetooth(CancellationToken token)
        {

            foreach (var device in adapter.ConnectedDevices)
            {
                await adapter.DisconnectDeviceAsync(device);
            }
            adapter.DeviceDisconnected += async (s, a) =>
            {
                if (!resultsubmitted)
                {
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", -1);
                }
            };
            adapter.DeviceConnected += async (s, a) =>
            {
                if (!resultsubmitted)
                {
                    connectedPeripheral = a.Device;
                    KnownDeviceSubmit(a.Device);
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", 2);
                    resultsubmitted = true;
                }

            };
            adapter.DeviceDiscovered += async (s, a) =>
            {
                if (a.Device.Name == "LRSS")
                {
                    adapter.ConnectToDeviceAsync(a.Device);
                }
            };
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
                MessagingCenter.Send<SubmitVIABluetooth, int>(this, "boom", 3);
                stringtoconvert = "B:" + Battery.ChargeLevel.ToString();
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
            adapter.DeviceDisconnected += async (s, a) =>
            {
                if (!datagotten)
                {
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", -1);
                }
            };
            adapter.DeviceConnected += async (s, a) =>
            {
                if (!datagotten)
                {
                    connectedPeripheral = a.Device;
                    KnownDeviceGet(a.Device);
                    MessagingCenter.Send<SubmitVIABluetooth, int>(this, "receivedata", 2);
                    datagotten = true;
                }

            };
            adapter.DeviceDiscovered += async (s, a) =>
            {
                if (a.Device.Name == "LRSS")
                {
                    adapter.ConnectToDeviceAsync(a.Device);
                }
            };
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
                characteristictosend.ValueUpdated += async (s, a) =>
                {
                    if (!iscompleted)
                    {
                        
                        var convertedmessage = Encoding.ASCII.GetString(a.Characteristic.Value);
                        if (messagesleft > 0)
                        {
                            messagesleft--;
                            fullmessage = fullmessage + convertedmessage;
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
                                messagesleft = int.Parse(convertedmessage.Substring(3));
                            }
                            else
                            {
                                List<TeamMatch> possibleListOfTeams = new List<TeamMatch>();
                                bool validlist = true;
                                try
                                {
                                    possibleListOfTeams = JsonConvert.DeserializeObject<List<TeamMatch>>(convertedmessage);
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

                };
                try
                {
                    await characteristictosend.StartUpdatesAsync();
                    var stringtoconvert = "RD:" + DateTime.Now.ToString();
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
    }
}
