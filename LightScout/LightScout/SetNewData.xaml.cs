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
                selectedButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Blue");
                selectedButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
            }
            else
            {
                selectedButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Red");
                selectedButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
            }
            if (lastTabletButton != null)
            {
                if (lastTabletButton.ClassId.StartsWith("B"))
                {
                    lastTabletButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.White");
                    lastTabletButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.Blue");
                }
                else
                {
                    lastTabletButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.White");
                    lastTabletButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.Red");
                }
            }
            lastTabletButton = selectedButton;
            DeviceDiscovered = new EventHandler<Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs>(async (s, a) =>
            {
                foundDevices.Add(a.Device);
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
            DependencyService.Get<DataStore>().SaveConfigurationFile("ownerTeamChange", setupTeamNumber.Text);
            DependencyService.Get<DataStore>().SaveConfigurationFile("ownerScoutChange", setupScoutName.Text);
            if(connectedDevice != null)
            {
                DependencyService.Get<DataStore>().SaveConfigurationFile("selectedMaster", new DeviceInformation() { DeviceName = connectedDevice.Name, DeviceID = connectedDevice.Id.ToString() });
            }
            
            try
            {
                adapter.DisconnectDeviceAsync(connectedDevice);
            }
            catch(Exception ex)
            {

            }
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

                await Task.Delay(15);
                finishForm.IsVisible = true;
                await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY + 10, 75, Easing.CubicOut);
                await finishForm.TranslateTo(finishForm.TranslationX, finishForm.TranslationY - 110, 250, Easing.CubicOut);
                bluetoothDevices.IsEnabled = false;
                bluetoothSelected.IsVisible = true;
            }
            
        }

        private void StartUpQRReader(object sender, EventArgs e)
        {

        }
    }
}