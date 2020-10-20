using LightScout.Models;
using Newtonsoft.Json;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LightScout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    
    public partial class SetNewData : ContentPage
    {
        private TabletType currentlySelectedTabletType;
        public EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs> DeviceDiscovered;
        private Button lastTabletButton;
        private IDevice connectedDevice;
        private List<IDevice> foundDevices = new List<IDevice>();
        public static IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        public SetNewData()
        {
            InitializeComponent();
        }

        private async void FinishedTeamNumber(object sender, EventArgs e)
        {
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            setTeamNumberPanel.TranslateTo(setTeamNumberPanel.TranslationX - 600, setTeamNumberPanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(150);
            setScoutNamePanel.TranslationX = 600;
            setScoutNamePanel.TranslationY = 0;
            await Task.Delay(50);
            setScoutNamePanel.IsVisible = true;
            setScoutNamePanel.TranslateTo(setScoutNamePanel.TranslationX - 600, setScoutNamePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(290);
            setTeamNumberPanel.IsVisible = false;
            await Task.Delay(10);
            setTeamNumberPanel.TranslationX = 0;
            await useQRCode.TranslateTo(useQRCode.TranslationX, useQRCode.TranslationY - 10, 75, Easing.CubicOut);
            await useQRCode.TranslateTo(useQRCode.TranslationX, useQRCode.TranslationY + 110, 250, Easing.CubicOut);
            useQRCode.IsVisible = false;
            await Task.Delay(15);
            useQRCode.TranslationY = 0;
            
        }
        private async void FinishedScoutName(object sender, EventArgs e)
        {
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            setScoutNamePanel.TranslateTo(setScoutNamePanel.TranslationX - 600, setScoutNamePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(150);
            setTabletTypePanel.TranslationX = 600;
            setTabletTypePanel.TranslationY = 0;
            await Task.Delay(50);
            setTabletTypePanel.IsVisible = true;
            setTabletTypePanel.TranslateTo(setTabletTypePanel.TranslationX - 600, setTabletTypePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(290);
            setScoutNamePanel.IsVisible = false;
            await Task.Delay(10);
            setScoutNamePanel.TranslationX = 0;
        }
        private async void FinishedTabletType(object sender, EventArgs e)
        {
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            setTabletTypePanel.TranslateTo(setTabletTypePanel.TranslationX - 600, setTabletTypePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(150);
            setCodePanel.TranslationX = 600;
            setCodePanel.TranslationY = 0;
            await Task.Delay(50);
            setCodePanel.IsVisible = true;
            setCodePanel.TranslateTo(setCodePanel.TranslationX - 600, setCodePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(290);
            setTabletTypePanel.IsVisible = false;
            await Task.Delay(10);
            setTabletTypePanel.TranslationX = 0;
            
            

        }
        private async void FinishedScoutCode(object sender, EventArgs e)
        {
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            setCodePanel.TranslateTo(setCodePanel.TranslationX - 600, setCodePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(150);
            setSelectedMaster.TranslationX = 600;
            setSelectedMaster.TranslationY = 0;
            await Task.Delay(50);
            setSelectedMaster.IsVisible = true;
            setSelectedMaster.TranslateTo(setSelectedMaster.TranslationX - 600, setSelectedMaster.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(290);
            setCodePanel.IsVisible = false;
            await Task.Delay(10);
            setCodePanel.TranslationX = 0;



        }
        private async void RestartForm(object sender, EventArgs e)
        {
            if (!setTeamNumberPanel.IsVisible)
            {
                DependencyService.Get<IKeyboardHelper>().HideKeyboard();
                await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY - 10, 75, Easing.CubicOut);
                await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY + 110, 250, Easing.CubicOut);
                finishForm.IsVisible = false;
                await Task.Delay(15);
                finishForm.TranslationY = 0;
                resetForm.IsEnabled = false;
                setTabletTypePanel.TranslateTo(setTabletTypePanel.TranslationX, setTabletTypePanel.TranslationY + 600, 500, Easing.CubicInOut);
                setCodePanel.TranslateTo(setCodePanel.TranslationX, setCodePanel.TranslationY + 600, 500, Easing.CubicInOut);
                setScoutNamePanel.TranslateTo(setScoutNamePanel.TranslationX, setScoutNamePanel.TranslationY + 600, 500, Easing.CubicInOut);
                setSelectedMaster.TranslateTo(setSelectedMaster.TranslationX, setSelectedMaster.TranslationY + 600, 500, Easing.CubicInOut);
                await Task.Delay(200);
                setTeamNumberPanel.TranslationX = 600;
                setupScoutName.Text = "";
                setupTeamNumber.Text = "";
                setupCode.Text = "";
                foundDevices.Clear();
                await Task.Delay(50);
                setTeamNumberPanel.IsVisible = true;
                setTeamNumberPanel.TranslateTo(setTeamNumberPanel.TranslationX - 600, setTeamNumberPanel.TranslationY, 500, Easing.CubicInOut);
                await Task.Delay(240);
                setTabletTypePanel.IsVisible = false;
                setCodePanel.IsVisible = false;
                setScoutNamePanel.IsVisible = false;
                setSelectedMaster.IsVisible = false;
                useQRCode.TranslationY = 100;

                await Task.Delay(15);
                useQRCode.IsVisible = true;
                await useQRCode.TranslateTo(useQRCode.TranslationX, useQRCode.TranslationY + 10, 75, Easing.CubicOut);
                await useQRCode.TranslateTo(useQRCode.TranslationX, useQRCode.TranslationY - 110, 250, Easing.CubicOut);
                await Task.Delay(10);
                setTabletTypePanel.TranslationY = 0;
                setTabletTypePanel.TranslationX = 0;
                setCodePanel.TranslationY = 0;
                setCodePanel.TranslationX = 0;
                setSelectedMaster.TranslationY = 0;
                setSelectedMaster.TranslationX = 0;
                setScoutNamePanel.TranslationY = 0;
                setScoutNamePanel.TranslationX = 0;
                resetForm.IsEnabled = true;
            }
            else
            {
                setupScoutName.Text = "";
                setupTeamNumber.Text = "";
                setupCode.Text = "";
            }
            

        }
        public async void SelectTabletType(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            Button selectedButton = (Button)sender as Button;
            nextFromTabletId.IsEnabled = true;
            switch (selectedButton.ClassId)
            {
                case "Red1":
                    currentlySelectedTabletType = TabletType.Red1;
                    break;
                case "Red2":
                    currentlySelectedTabletType = TabletType.Red2;
                    break;
                case "Red3":
                    currentlySelectedTabletType = TabletType.Red3;
                    break;
                case "Blue1":
                    currentlySelectedTabletType = TabletType.Blue1;
                    break;
                case "Blue2":
                    currentlySelectedTabletType = TabletType.Blue2;
                    break;
                case "Blue3":
                    currentlySelectedTabletType = TabletType.Blue3;
                    break;
            }
            if (selectedButton.ClassId.StartsWith("B"))
            {
                selectedButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("#3475da");
                selectedButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
            }
            else
            {
                selectedButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("#e53434");
                selectedButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
            }
            if (lastTabletButton != null)
            {
                if (lastTabletButton.ClassId.StartsWith("B"))
                {
                    lastTabletButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Transparent");
                    lastTabletButton.TextColor = (Color)converter.ConvertFromInvariantString("#3475da");
                }
                else
                {
                    lastTabletButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Transparent");
                    lastTabletButton.TextColor = (Color)converter.ConvertFromInvariantString("#e53434");
                }
            }
            lastTabletButton = selectedButton;
            DeviceDiscovered = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if(a.Device.Name != null && a.Device.Name != "")
                {
                    foundDevices.Add(a.Device);
                }
                
            });
            
            adapter.DeviceDiscovered += DeviceDiscovered;
            await adapter.StartScanningForDevicesAsync();
            Task.Delay(3000);
            bluetoothLoading.IsVisible = false;
            bluetoothDevices.ItemsSource = foundDevices;
            adapter.DeviceDiscovered -= DeviceDiscovered;
            await adapter.StopScanningForDevicesAsync();
            
        }
        private async void SkipTabletConnection(object sender, EventArgs e)
        {
            finishForm.TranslationY = 100;

            await Task.Delay(15);
            finishForm.IsVisible = true;
            await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY + 10, 75, Easing.CubicOut);
            await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY - 110, 250, Easing.CubicOut);
            bluetoothDevices.IsEnabled = false;
            await resetForm.TranslateTo(resetForm.TranslationX, resetForm.TranslationY - 10, 75, Easing.CubicOut);
            await resetForm.TranslateTo(resetForm.TranslationX, resetForm.TranslationY + 110, 250, Easing.CubicOut);
            resetForm.IsVisible = false;
            await Task.Delay(15);
            resetForm.TranslationY = 0;
        }
        private async void SubmitNewData(object sender, EventArgs e)
        {
            await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY - 10, 75, Easing.CubicOut);
            await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY + 110, 250, Easing.CubicOut);
            finishForm.IsVisible = false;
            await Task.Delay(15);
            finishForm.TranslationY = 0;
            switch (currentlySelectedTabletType)
            {
                case TabletType.Red1:
                    DependencyService.Get<DataStore>().SaveConfigurationFile("tabletId", "R1");
                    break;
                case TabletType.Red2:
                    DependencyService.Get<DataStore>().SaveConfigurationFile("tabletId", "R2");
                    break;
                case TabletType.Red3:
                    DependencyService.Get<DataStore>().SaveConfigurationFile("tabletId", "R3");
                    break;
                case TabletType.Blue1:
                    DependencyService.Get<DataStore>().SaveConfigurationFile("tabletId", "B1");
                    break;
                case TabletType.Blue2:
                    DependencyService.Get<DataStore>().SaveConfigurationFile("tabletId", "B2");
                    break;
                case TabletType.Blue3:
                    DependencyService.Get<DataStore>().SaveConfigurationFile("tabletId", "B3");
                   
                    break;
            }
            DependencyService.Get<DataStore>().SaveConfigurationFile("ownerTeamChange", int.Parse(setupTeamNumber.Text));
            DependencyService.Get<DataStore>().SaveConfigurationFile("ownerScoutChange", setupScoutName.Text);
            DependencyService.Get<DataStore>().SaveConfigurationFile("scoutCode", int.Parse(setupCode.Text));
            if(connectedDevice != null)
            {
                DependencyService.Get<DataStore>().SaveConfigurationFile("selectedMaster", new DeviceInformation() { DeviceName = connectedDevice.Name, DeviceID = connectedDevice.Id.ToString() });
                try
                {
                    adapter.DisconnectDeviceAsync(connectedDevice);
                }
                catch (Exception ex)
                {

                }
            }
            
            

            var createNewMatches = await DisplayAlert("Default Matches", "Would you like to add sample matches to the software?", "Sure", "No");
            DependencyService.Get<DataStore>().SaveDummyData("JacksonEvent2020.txt");
            await Task.Delay(1000);
            Navigation.PushAsync(new MainPage());
            

        }
        public enum TabletType
        {
            Red1,
            Red2,
            Red3,
            Blue1,
            Blue2,
            Blue3
        }

        private async void bluetoothDevices_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            bool successful;
            try
            {
                await adapter.ConnectToDeviceAsync((IDevice)e.Item);
                successful = true;
            }
            catch(Exception ex)
            {
                successful = false;
            }
            if (successful)
            {
                finishForm.TranslationY = 100;
                connectedDevice = (IDevice)e.Item;
                await Task.Delay(15);
                finishForm.IsVisible = true;
                await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY + 10, 75, Easing.CubicOut);
                await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY - 110, 250, Easing.CubicOut);
                bluetoothDevices.FadeTo(.5);
                bluetoothDevices.IsEnabled = false;
                bluetoothSelected.IsVisible = true;
                skipTablet.IsEnabled = false;
                await resetForm.TranslateTo(resetForm.TranslationX, resetForm.TranslationY - 10, 75, Easing.CubicOut);
                await resetForm.TranslateTo(resetForm.TranslationX, resetForm.TranslationY + 110, 250, Easing.CubicOut);
                resetForm.IsVisible = false;
                await Task.Delay(15);
                resetForm.TranslationY = 0;
            }
            
        }

        private async void StartUpQRReader(object sender, EventArgs e)
        {
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            await useQRCode.TranslateTo(useQRCode.TranslationX, useQRCode.TranslationY - 10, 75, Easing.CubicOut);
            await useQRCode.TranslateTo(useQRCode.TranslationX, useQRCode.TranslationY + 110, 250, Easing.CubicOut);
            useQRCode.IsVisible = false;
            await Task.Delay(15);
            useQRCode.TranslationY = 0;
            BarcodeScanView.IsVisible = true;
            BarcodeScanView.IsScanning = true;
            setTeamNumberPanel.TranslateTo(setTeamNumberPanel.TranslationX, setTeamNumberPanel.TranslationY+1200, 500, Easing.CubicInOut);
            await Task.Delay(150);
            scanQRCodePanel.TranslationX = 0;
            scanQRCodePanel.TranslationY = 1200;
            await Task.Delay(50);
            scanQRCodePanel.IsVisible = true;
            scanQRCodePanel.TranslateTo(scanQRCodePanel.TranslationX, scanQRCodePanel.TranslationY-1200, 500, Easing.CubicInOut);
            await Task.Delay(290);
            setTeamNumberPanel.IsVisible = false;
            await Task.Delay(10);
            setTeamNumberPanel.TranslationX = 0;
            setTeamNumberPanel.TranslationY = 0;
            
        }
        private async void CancelQRReader(object sender, EventArgs e)
        {
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            scanQRCodePanel.TranslateTo(scanQRCodePanel.TranslationX, scanQRCodePanel.TranslationY + 1200, 500, Easing.CubicInOut);
            await Task.Delay(150);
            setTeamNumberPanel.TranslationX = 0;
            setTeamNumberPanel.TranslationY = 1200;
            await Task.Delay(50);
            setTeamNumberPanel.IsVisible = true;
            setTeamNumberPanel.TranslateTo(setTeamNumberPanel.TranslationX, setTeamNumberPanel.TranslationY - 1200, 500, Easing.CubicInOut);
            await Task.Delay(290);
            scanQRCodePanel.IsVisible = false;
            await Task.Delay(10);
            scanQRCodePanel.TranslationX = 0;
            scanQRCodePanel.TranslationY = 0;
            useQRCode.TranslationY = 100;
            await Task.Delay(15);
            
            useQRCode.IsVisible = true;
            await useQRCode.TranslateTo(useQRCode.TranslationX, useQRCode.TranslationY + 10, 75, Easing.CubicOut);
            await useQRCode.TranslateTo(useQRCode.TranslationX, useQRCode.TranslationY - 110, 250, Easing.CubicOut);
            await Task.Delay(150);
            BarcodeScanView.IsVisible = false;
            BarcodeScanView.IsScanning = false;
        }

        private async void BarcodeScanView_OnScanResult(ZXing.Result result)
        {
            
            var converter = new ColorTypeConverter();
            var rawresultstring = result.ToString();
            if (rawresultstring.StartsWith("LRSSQR"))
            {
                string[] differentValues = rawresultstring.Split('>');
                Device.BeginInvokeOnMainThread(async () =>
                {
                    setupScoutName.Text = differentValues[2];
                    checkScoutName.Text = differentValues[2];
                    setupTeamNumber.Text = differentValues[1];
                    checkTeamNumber.Text = differentValues[1];
                    setupCode.Text = differentValues[4];
                    checkScoutCode.Text = differentValues[4];
                    switch (differentValues[3])
                    {
                        case "R1":
                            currentlySelectedTabletType = TabletType.Red1;
                            checkTabletIdBox.BackgroundColor = (Color)converter.ConvertFromInvariantString("#e53434");
                            checkTabletIdLabel.Text = "Red 1";
                            break;
                        case "R2":
                            currentlySelectedTabletType = TabletType.Red2;
                            checkTabletIdBox.BackgroundColor = (Color)converter.ConvertFromInvariantString("#e53434");
                            checkTabletIdLabel.Text = "Red 2";
                            break;
                        case "R3":
                            currentlySelectedTabletType = TabletType.Red3;
                            checkTabletIdBox.BackgroundColor = (Color)converter.ConvertFromInvariantString("#e53434");
                            checkTabletIdLabel.Text = "Red 3";
                            break;
                        case "B1":
                            currentlySelectedTabletType = TabletType.Blue1;
                            checkTabletIdBox.BackgroundColor = (Color)converter.ConvertFromInvariantString("#3475da");
                            checkTabletIdLabel.Text = "Blue 1";
                            break;
                        case "B2":
                            currentlySelectedTabletType = TabletType.Blue2;
                            checkTabletIdBox.BackgroundColor = (Color)converter.ConvertFromInvariantString("#3475da");
                            checkTabletIdLabel.Text = "Blue 2";
                            break;
                        case "B3":
                            currentlySelectedTabletType = TabletType.Blue3;
                            checkTabletIdBox.BackgroundColor = (Color)converter.ConvertFromInvariantString("#3475da");
                            checkTabletIdLabel.Text = "Blue 3";
                            break;
                    }
                    scanQRCodePanel.TranslateTo(scanQRCodePanel.TranslationX - 600, scanQRCodePanel.TranslationY, 500, Easing.CubicInOut);
                    await Task.Delay(150);
                    checkScanValues.TranslationX = 600;
                    checkScanValues.TranslationY = 0;
                    await Task.Delay(50);
                    checkScanValues.IsVisible = true;
                    checkScanValues.TranslateTo(checkScanValues.TranslationX - 600, checkScanValues.TranslationY, 500, Easing.CubicInOut);
                    await Task.Delay(290);
                    scanQRCodePanel.IsVisible = false;
                    await Task.Delay(10);
                    scanQRCodePanel.TranslationX = 0;
                    await Task.Delay(140);
                    BarcodeScanView.IsVisible = false;
                    BarcodeScanView.IsScanning = false;

                });
            }
            
            
        }
        private void setupTeamNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(e.NewTextValue != null && e.NewTextValue != "")
            {
                nextFromTeamNumber.IsEnabled = true;
            }
        }

        private void setupScoutName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue != null && e.NewTextValue != "")
            {
                nextFromScoutName.IsEnabled = true;
            }
        }

        private void setupCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue != null && e.NewTextValue != "")
            {
                nextFromScouterCode.IsEnabled = true;
            }
        }
        private async void CorrectScanValues(object sender, EventArgs e)
        {
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            checkScanValues.TranslateTo(checkScanValues.TranslationX - 600, checkScanValues.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(150);
            setSelectedMaster.TranslationX = 600;
            setSelectedMaster.TranslationY = 0;
            await Task.Delay(50);
            setSelectedMaster.IsVisible = true;
            setSelectedMaster.TranslateTo(setSelectedMaster.TranslationX - 600, setSelectedMaster.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(290);
            checkScanValues.IsVisible = false;
            await Task.Delay(10);
            checkScanValues.TranslationX = 0;
            DeviceDiscovered = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                if (a.Device.Name != null && a.Device.Name != "")
                {
                    foundDevices.Add(a.Device);
                }
            });

            adapter.DeviceDiscovered += DeviceDiscovered;
            await adapter.StartScanningForDevicesAsync();
            Task.Delay(3000);
            bluetoothLoading.IsVisible = false;
            bluetoothDevices.ItemsSource = foundDevices;
            adapter.DeviceDiscovered -= DeviceDiscovered;
            await adapter.StopScanningForDevicesAsync();


        }
        private async void IncorrectScanValues(object sender, EventArgs e)
        {
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            checkScanValues.TranslateTo(checkScanValues.TranslationX + 600, checkScanValues.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(150);
            scanQRCodePanel.TranslationX = -600;
            scanQRCodePanel.TranslationY = 0;
            await Task.Delay(50);
            scanQRCodePanel.IsVisible = true;
            scanQRCodePanel.TranslateTo(scanQRCodePanel.TranslationX + 600, scanQRCodePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(290);
            checkScanValues.IsVisible = false;
            await Task.Delay(10);
            checkScanValues.TranslationX = 0;
            BarcodeScanView.IsVisible = true;
            BarcodeScanView.IsScanning = true;


        }
    }
}