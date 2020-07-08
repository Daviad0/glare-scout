using LightScout.Models;
using Newtonsoft.Json;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
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
        private static ObservableCollection<IDevice> Devices = new ObservableCollection<IDevice>();
        private static bool Balanced;
        private List<TeamMatch> listofmatches = new List<TeamMatch>();
        private List<string> MatchNames = new List<string>();
        private static int BluetoothDevices = 0;
        private static bool TimerAlreadyCreated = false;
        private static int timesalive = 0;
        private List<string> tabletlist = new List<string>();
        private static SubmitVIABluetooth bluetoothHandler = new SubmitVIABluetooth();
        
        public MainPage()
        {
            InitializeComponent();
            ControlPanel[0] = false;
            ControlPanel[1] = false;
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
                //status.Text = "Connected to: " + a.Device.Name.ToString();
                deviceIWant = a.Device;
                listofdevices.IsVisible = false;
            };
            adapter.DeviceConnectionLost += (s, a) =>
            {
                Console.WriteLine("Lost connection to: " + a.Device.Name.ToString());
                //status.Text = "Disconnected from: " + a.Device.Name.ToString();
                listofdevices.IsVisible = true;
                Devices.Clear();
            };
            adapter.DeviceDisconnected += (s, a) =>
            {
                Console.WriteLine("Lost connection to: " + a.Device.Name.ToString());
                //status.Text = "Disconnected from: " + a.Device.Name.ToString();
                listofdevices.IsVisible = true;
                Devices.Clear();
            };

            if (!TimerAlreadyCreated)
            {
                Console.WriteLine("Test started :)");
                Device.StartTimer(TimeSpan.FromMinutes(1), () =>
                {
                    timesalive++;
                    Console.WriteLine("This message has appeared " + timesalive.ToString() + " times. Last ping at " + DateTime.Now.ToShortTimeString());
                    TimerAlreadyCreated = true;
                    return true;
                });
            }

            /*Device.StartTimer(TimeSpan.FromMinutes(1), () =>
            {
                if(deviceIWant != null)
                {
                    bluetoothHandler.SubmitBluetooth(adapter, deviceIWant);
                }
                
                return true;
            });*/
            var allmatchesraw = DependencyService.Get<DataStore>().LoadData("JacksonEvent2020.txt");
            listofmatches = JsonConvert.DeserializeObject<List<TeamMatch>>(allmatchesraw);
            listOfMatches.ItemsSource = listofmatches;

            tabletlist = new string[6] { "R1", "R2", "R3", "B1", "B2", "B3" }.ToList();
            tabletPicker.ItemsSource = tabletlist;
        }

        private void FTCShow_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FTCMain());
        }

        private void FRCShow_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FRCMain(new TeamMatch() { PowerCellInner = new int[21], PowerCellOuter = new int[21], PowerCellLower = new int[21], PowerCellMissed = new int[21], MatchNumber = 1, TeamNumber = 862 }));
            
            
        }

        private void commscheck_Clicked(object sender, EventArgs e)
        {
            
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
                deviceIWant = selectedDevice;
            }
            else
            {
                await adapter.ConnectToDeviceAsync(selectedDevice);
                deviceIWant = selectedDevice;
            }
        }

        private async void sendDataToBT_Clicked(object sender, EventArgs e)
        {
            /*var servicetosend = await deviceIWant.GetServiceAsync(Guid.Parse("50dae772-d8aa-4378-9602-792b3e4c198d"));
            var characteristictosend = await servicetosend.GetCharacteristicAsync(Guid.Parse("50dae772-d8aa-4378-9602-792b3e4c198e"));
            var stringtoconvert = "Test!";
            var bytestotransmit = Encoding.ASCII.GetBytes(stringtoconvert);
            await characteristictosend.WriteAsync(bytestotransmit);
            Console.WriteLine(bytestotransmit);*/
        }

        private void dcFromBT_Clicked(object sender, EventArgs e)
        {
            adapter.DisconnectDeviceAsync(deviceIWant);
        }

        private async void listOfMatches_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Console.WriteLine(listofmatches[e.ItemIndex].TeamNumber.ToString() + "'s match at match #" + listofmatches[e.ItemIndex].MatchNumber.ToString());
            bool answer = true;
            if (listofmatches[e.ItemIndex].ClientSubmitted)
            {
                answer = await DisplayAlert("Match Completed", "This match has already been completed by someone using this tablet, would you still like to continue?", "Continue", "Cancel");
            }
            if (answer)
            {
                Navigation.PushAsync(new FRCMain(listofmatches[e.ItemIndex]));
            }
            else
            {
                var listobject = sender as ListView;
                listobject.SelectedItem = null;
            }
            
        }

        private void MenuItem_Clicked(object sender, EventArgs e)
        {
            moreinfoMenu.IsVisible = true;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            moreinfoMenu.IsVisible = false;
        }

        private void tabletPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var list = sender as Picker;
            DependencyService.Get<DataStore>().SaveConfigurationFile("tabletId", tabletlist[list.SelectedIndex]);
        }
    }
}
