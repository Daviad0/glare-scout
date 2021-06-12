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
        private bool menuExpanded = false;
        private string currentPage = "mainPage";
        public MasterPage()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            
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
    }
}