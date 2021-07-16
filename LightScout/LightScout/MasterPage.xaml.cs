using Akavache;
using LightScout.Models;
using Newtonsoft.Json;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LightScout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterPage : ContentPage
    {
        private static BLEConnection bleManager = BLEConnection.Instance;
        private bool menuExpanded = false;
        private string currentPage = "mainPage";
        public MasterPage()
        {
            InitializeComponent();
            Console.WriteLine(bleManager.teamNumber);
        }
        protected override async void OnAppearing()
        {
            
            MessagingCenter.Subscribe<string, string>("MasterPage", "DataGot", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(() => {
                    DisplayAlert("BLEMessage", message, "Cancel");
                });
                

            });
            DependencyService.Get<BLEPeripheral>().StartAdvertising("a3db5ad7-ac7b-4a48-b4e0-13f7c087194d", "DT1");
        }

        private async void expandMenu_Clicked(object sender, EventArgs e)
        {
            Grid pageAlready = this.FindByName<Grid>(currentPage);
            menuExpanded = !menuExpanded;
            if (menuExpanded)
            {
                pageAlready.TranslateTo(pageAlready.X, pageAlready.Y - 180, 500, Easing.CubicInOut);
                this.FindByName<Label>(pageAlready.ClassId + "Nav1").RotateTo(180, 250, Easing.CubicInOut);
                
            }
            else
            {
                pageAlready.TranslateTo(pageAlready.X, pageAlready.Y, 500, Easing.CubicInOut);
                this.FindByName<Label>(pageAlready.ClassId + "Nav1").RotateTo(0, 250, Easing.CubicInOut);
                
            }
                
        }

        private async void changeMenuPage(object sender, EventArgs e)
        {
            Frame clickedObj = (Frame)sender as Frame;
            Grid pageAlready = this.FindByName<Grid>(currentPage);
            Grid newPage = this.FindByName<Grid>(clickedObj.ClassId);
            if (currentPage != clickedObj.ClassId)
            {
                newPage.TranslationY = pageAlready.TranslationY;
                this.FindByName<Label>(newPage.ClassId + "Nav1").Rotation = 180;
                newPage.IsVisible = true;
                this.FindByName<Label>(newPage.ClassId + "Nav1").RotateTo(0, 250, Easing.CubicInOut);
                pageAlready.IsVisible = false;
                pageAlready.TranslationY = 0;
                this.FindByName<Label>(pageAlready.ClassId + "Nav1").Rotation = 180;
                currentPage = clickedObj.ClassId;
            }
            else
            {
                this.FindByName<Label>(pageAlready.ClassId + "Nav1").RotateTo(0, 250, Easing.CubicInOut);
            }
            menuExpanded = false;

            newPage.TranslateTo(newPage.X, newPage.Y, 500, Easing.CubicInOut);
        }
        private async void changeMenuPage_image(object sender, EventArgs e)
        {
            Image clickedObj = (Image)sender as Image;
            Grid pageAlready = this.FindByName<Grid>(currentPage);
            Grid newPage = this.FindByName<Grid>(clickedObj.ClassId);
            if (currentPage != clickedObj.ClassId)
            {
                newPage.TranslationY = pageAlready.TranslationY;
                this.FindByName<Label>(newPage.ClassId + "Nav1").Rotation = 180;
                newPage.IsVisible = true;
                this.FindByName<Label>(newPage.ClassId + "Nav1").RotateTo(0, 250, Easing.CubicInOut);
                pageAlready.IsVisible = false;
                pageAlready.TranslationY = 0;
                this.FindByName<Label>(pageAlready.ClassId + "Nav1").Rotation = 180;
                currentPage = clickedObj.ClassId;
            }
            else
            {
                this.FindByName<Label>(pageAlready.ClassId + "Nav1").RotateTo(0, 250, Easing.CubicInOut);
            }
            menuExpanded = false;

            newPage.TranslateTo(newPage.X, newPage.Y, 500, Easing.CubicInOut);
        }

        private async void openEditPage(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            matchEdit.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2A7AFA");
            matchEditLabel.TextColor = (Color)converter.ConvertFromInvariantString("White");
            overlayEdit.TranslationY = 1200;
            overlayEdit.IsVisible = true;
            overlayEdit.TranslateTo(overlayEdit.X, overlayEdit.Y + 16, 750, Easing.CubicInOut);
            await Task.Delay(500);
            overlayEditArrow.RotateTo(0, 250, Easing.CubicInOut);
        }

        private async void closeEditPage(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            matchEdit.BackgroundColor = (Color)converter.ConvertFromInvariantString("White");
            matchEditLabel.TextColor = (Color)converter.ConvertFromInvariantString("#2A7AFA");
            await overlayEdit.TranslateTo(overlayEdit.X, overlayEdit.Y + 1200, 750, Easing.CubicInOut);
            overlayEditArrow.Rotation = 180;
            overlayEdit.IsVisible = false;
        }
        private async void openMatchPage(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            matchGo.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2A7AFA");
            matchGoLabel.TextColor = (Color)converter.ConvertFromInvariantString("White");
            overlayMatch.TranslationY = 1200;
            overlayMatch.IsVisible = true;
            overlayMatch.TranslateTo(overlayMatch.X, overlayMatch.Y + 16, 750, Easing.CubicInOut);
            await Task.Delay(500);
            overlayMatchArrow.RotateTo(0, 250, Easing.CubicInOut);
        }

        private async void closeMatchPage(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            matchGo.BackgroundColor = (Color)converter.ConvertFromInvariantString("White");
            matchGoLabel.TextColor = (Color)converter.ConvertFromInvariantString("#2A7AFA");
            await overlayMatch.TranslateTo(overlayMatch.X, overlayMatch.Y + 1200, 750, Easing.CubicInOut);
            overlayMatchArrow.Rotation = 180;
            overlayMatch.IsVisible = false;
        }
        private async void loadScouting(object sender, EventArgs e)
        {
            // try to get a dynamicly loading page
            buttonTest1.IsVisible = false;
            labelTest1.IsVisible = true;
            Navigation.PushAsync(new Scouting());
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            /*bleManager.RemoveEventReferences();
            bleManager.MessageSent += (message, totalMessages, currentMessage, finished) =>
            {
                Console.WriteLine("Message " + currentMessage.ToString() + " out of " + totalMessages.ToString() + " sent! (" + message + ")");
            };
            bleManager.DeviceConnected += (device, successful) =>
            {
                if (successful)
                {
                    Console.WriteLine("Connection Successful");
                    bleManager.SubmitData("00000862-0000-1000-8000-00805f9b34fb", "00000001-0000-1000-8000-00805f9b34fb", "ABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZABCDEFGHIKJKLMNOPQRSTUVWXYZ", (IDevice)device);
                }
                else
                {
                    Console.WriteLine("Connection Failure");
                }
            };
            bleManager.DeviceDetected += (device, at) =>
            {
                IDevice d = device as IDevice;
                Console.WriteLine("New device found at " + at.ToString() + " with name " + d.Name + " (" + d.Id + ")");
                if(d.Name == "Glare Bluetooth Service" || d.Id.ToString() == "00000000-0000-0000-0000-dca63262cab5")
                {
                    bleManager.StopDetecting();
                    // start connecting and sending data
                    bleManager.ConnectToUnknownDevice(d);
                }
            };
            bleManager.RedetectDevices();
            
            Console.WriteLine("Detecting Devices!");*/
            
        }
    }
}