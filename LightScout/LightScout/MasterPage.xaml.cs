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
    public class UniqueStartUp
    {
        private static readonly object l1 = new object();
        public static UniqueStartUp instance = null;
        public static UniqueStartUp Instance
        {
            get
            {
                lock (l1)
                {
                    if (instance == null)
                    {
                        instance = new UniqueStartUp();
                    }
                    return instance;
                }

            }
        }
        public static bool Initialized;
        public async Task StartTasks()
        {

            // application data protocol
            await ApplicationDataHandler.Instance.InitializeData();

            // bluetooth protocol
            Task.Run(() =>
            {
                DependencyService.Get<BLEPeripheral>().StartAdvertising("a3db5ad7-ac7b-4a48-b4e0-13f7c087194d", "DT1");
            });
            
            
            await ApplicationDataHandler.Instance.SaveUsers();
            Initialized = true;
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterPage : ContentPage
    {
        private static BLEConnection bleManager = BLEConnection.Instance;
        private bool menuExpanded = false;
        private string currentPage = "mainPage";
        private int CurrentMatchIndex = 0;
        private DataEntry CurrentMatchSelected;
        public ColorTypeConverter converter = new ColorTypeConverter();
        public MasterPage()
        {
            InitializeComponent();
            Console.WriteLine(bleManager.teamNumber);
        }
        protected override void OnAppearing()
        {

            MessagingCenter.Subscribe<string, string>("MasterPage", "DataGot", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("BLEMessage", message, "Cancel");
                });


            });
            MessagingCenter.Subscribe<string, string>("MasterPage", "DialogBox", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Just for you...", message, "Alright!");
                });


            });
            MessagingCenter.Subscribe<string, string>("MasterPage", "MatchesChanged", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await ApplicationDataHandler.Instance.GetAvailableMatches();
                    CurrentMatchSelected = ApplicationDataHandler.AvailableEntries.First();
                    UpdateMatchContainer();

                });


            });
            MessagingCenter.Subscribe<string, string>("MasterPage", "AnnouncementChanged", (sender, message) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {

                    UpdateAnnouncementContainer();

                });


            });
            MessagingCenter.Subscribe<string, string>("MasterPage", "UsersChanged", async (sender, message) =>
            {
                await Task.Delay(2000);
                Device.BeginInvokeOnMainThread(() =>
                {

                    UpdateUsers();

                });


            });
            Task.Run(async () =>
            {
                if (UniqueStartUp.Initialized != true)
                {
                    await UniqueStartUp.Instance.StartTasks();
                }
                CurrentMatchSelected = ApplicationDataHandler.AvailableEntries.First();
                UpdateMatchContainer();
                UpdateAnnouncementContainer();
                UpdateProgressContainer();
                UpdateUsers();
                tabletIdentifier.Text = ApplicationDataHandler.CurrentApplicationData.DeviceId;
                if (ApplicationDataHandler.CurrentApplicationData.Locked)
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        var result = await DisplayPromptAsync("Device Locked", "Only an administrator may access this device with the given code. This was the reason provided: " + ApplicationDataHandler.CurrentApplicationData.LockedMessage, "Submit", "Exit", "####", 4, Keyboard.Numeric);
                        if (result == null || result == "")
                        {
                            await ApplicationDataHandler.Instance.AddLog(new Log()
                            {
                                occured = DateTime.Now,
                                critical = false,
                                eventType = "Lock Override Failed"
                            });
                            Application.Current.Quit();
                        }
                        await ApplicationDataHandler.Instance.AddLog(new Log()
                        {
                            occured = DateTime.Now,
                            critical = false,
                            eventType = "Lock Override Successful"
                        });
                    });
                }
            });


            //MessagingCenter.Send("MasterPage", "DialogBox", "AAAAA");
        }
        private async void UpdateUsers()
        {
            start_ScoutPicker.ItemsSource = ApplicationDataHandler.Users;
        }
        private async void UpdateAnnouncementContainer()
        {
            ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement = new Announcement();
            ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.Title = "Hey Strategy!";
            ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.Data = "Welcome to Glare, the scouting app for anything that you want! Currently, this app is experimental and some things haven't been patched up completely yet. Just bear with it for a little bit as the final testing is being done! To actually scout, you must have the Glare Desktop Client to load the matches and schemas onto the tablet.";
            ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.GotAt = DateTime.Now;
            ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.ActiveUntil = DateTime.MaxValue;
            if (ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement == null)
            {
                announcement_Containment.IsVisible = false;
            }
            else
            {
                if (ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.ActiveUntil > DateTime.Now)
                {
                    announcement_Containment.IsVisible = true;
                    announcement_Title.Text = ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.Title;
                    announcement_Content.Text = ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.Data;
                    announcement_Time.Text = ApplicationDataHandler.CurrentApplicationData.CurrentAnnouncement.GotAt.ToShortTimeString();
                }
                else
                {
                    announcement_Containment.IsVisible = false;
                }

            }
        }
        private async void UpdateProgressContainer()
        {
            try
            {
                ApplicationDataHandler.CurrentApplicationData.CurrentCompetition = ApplicationDataHandler.Competitions.First().Id;
                ApplicationDataHandler.Instance.SaveAppData();
                if (ApplicationDataHandler.AvailableEntries.Count(e => e.Competition == ApplicationDataHandler.CurrentApplicationData.CurrentCompetition) == 0)
                {
                    completion_Container.IsVisible = false;
                }
                else
                {
                    completion_Container.IsVisible = true;
                    completion_Name.Text = ApplicationDataHandler.Competitions.Single(e => e.Id == ApplicationDataHandler.CurrentApplicationData.CurrentCompetition).Name;
                    completion_Progress.Text = ApplicationDataHandler.AvailableEntries.Where(e => e.Competition == ApplicationDataHandler.CurrentApplicationData.CurrentCompetition).Count(f => f.Completed).ToString() + " / " + ApplicationDataHandler.AvailableEntries.Count(e => e.Competition == ApplicationDataHandler.CurrentApplicationData.CurrentCompetition);
                    // there are no comps in the device


                }
            }
            catch (Exception e)
            {
                completion_Container.IsVisible = false;
            }

        }
        private async void UpdateMatchContainer()
        {
            CurrentMatch.Text = "Match " + CurrentMatchSelected.Number.ToString();
            Left.IsEnabled = CurrentMatchIndex == 0 ? false : true;
            Left.Opacity = CurrentMatchIndex == 0 ? .1 : 1;
            Right.IsEnabled = CurrentMatchIndex == ApplicationDataHandler.AvailableEntries.Count - 1 ? false : true;
            Right.Opacity = CurrentMatchIndex == ApplicationDataHandler.AvailableEntries.Count - 1 ? .1 : 1;

            match_TeamIdentifier.Text = "Team " + CurrentMatchSelected.TeamIdentifier;
            match_TeamName.Text = CurrentMatchSelected.TeamName;
            match_Position.Text = CurrentMatchSelected.Position;
            // check which color it should be
            if (CurrentMatchSelected.Position.ToLower().Contains("red"))
            {
                match_Position.TextColor = (Color)converter.ConvertFromInvariantString("Color.Red");
            }
            else if (CurrentMatchSelected.Position.ToLower().Contains("blue"))
            {
                match_Position.TextColor = (Color)converter.ConvertFromInvariantString("Color.Blue");
            }
            else
            {
                match_Position.TextColor = (Color)converter.ConvertFromInvariantString("Color.Green");
            }

            var assistedByString = "";
            foreach (string s in CurrentMatchSelected.AssistedBy)
            {
                assistedByString += s + " ";
            }
            match_AssistedBy.Text = assistedByString == "" ? "" : "Assisted by " + assistedByString;

            try
            {
                match_SchemaName.Text = ApplicationDataHandler.Schemas.Single(c => c.Id == CurrentMatchSelected.Schema).Name;
            }
            catch (Exception e)
            {
                match_SchemaName.Text = CurrentMatchSelected.Schema;
            }


            if (!(CurrentMatchSelected.Completed || CurrentMatchSelected.Audited))
                match_StatusContainer.IsVisible = false;
            else
            {
                match_StatusContainer.IsVisible = true;
                match_StatusContainer.BackgroundColor = CurrentMatchSelected.Audited ? (Color)converter.ConvertFromInvariantString("#2e85b8") : (Color)converter.ConvertFromInvariantString("#00D974");
                match_StatusLabel.Text = CurrentMatchSelected.Audited ? "This Match was Audited" : "This Match was Completed";
            }
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
        private Dictionary<string, string[]> menuImagePairs = new Dictionary<string, string[]>()
        {
            { "settingsPage", new string[]{ "LSS.png", "LSSS.png" } },
            { "additionalPage", new string[]{ "LSC.png", "LSCS.png" } },
            { "mainPage", new string[]{ "LSH.png", "LSHS.png" } }
        };
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



            this.FindByName<Image>(pageAlready.ClassId + "_image").Source = menuImagePairs[pageAlready.ClassId][0];
            this.FindByName<Image>(clickedObj.ClassId + "_image").Source = menuImagePairs[clickedObj.ClassId][1];

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


            this.FindByName<Image>(pageAlready.ClassId + "_image").Source = menuImagePairs[pageAlready.ClassId][0];
            this.FindByName<Image>(clickedObj.ClassId + "_image").Source = menuImagePairs[clickedObj.ClassId][1];

            newPage.TranslateTo(newPage.X, newPage.Y, 500, Easing.CubicInOut);
        }

        private async void openEditPage(object sender, EventArgs e)
        {
            if (await TryAdminLogin())
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
            if (ApplicationDataHandler.Schemas.Find(el => el.Id == CurrentMatchSelected.Schema) != null)
            {
                start_ScoutConfirm.IsEnabled = false;
                start_ScoutPicker.SelectedIndex = -1;
                start_MatchNumber.Text = "Match " + CurrentMatchSelected.Number.ToString();
                start_TeamNumber.Text = "Team " + CurrentMatchSelected.TeamIdentifier.ToString();
                start_TeamName.Text = CurrentMatchSelected.TeamName.ToString();
                start_Position.Text = CurrentMatchSelected.Position.ToString();
                start_ScoutContainer.IsVisible = true;
                start_ScoutContainer.Opacity = 1;
                start_StartContainer.IsVisible = false;

                var converter = new ColorTypeConverter();
                matchGo.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2A7AFA");
                matchGoLabel.TextColor = (Color)converter.ConvertFromInvariantString("White");
                overlayMatch.TranslationY = 1200;
                overlayMatch.IsVisible = true;
                overlayMatch.TranslateTo(overlayMatch.X, overlayMatch.Y + 16, 750, Easing.CubicInOut);
                await Task.Delay(500);
                overlayMatchArrow.RotateTo(0, 250, Easing.CubicInOut);
            }
            else
            {
                await DisplayAlert("Not Found", "This device doesn't have the corresponding schema to actually generate the match form!", "OK");
            }

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
        public List<string> loadingGags = new List<string>()
        {
            "Now loading...",
            "Loading awesomeness...",
            "Loading...",
            "Awaiting your chariot...",
            "Generation pending...",
            "Get ready..."
        };
        private async void loadScouting(object sender, EventArgs e)
        {

            Scouter selectedUser = ApplicationDataHandler.Users[start_ScoutPicker.SelectedIndex];
            await ApplicationDataHandler.Instance.AddLog(new Log()
            {
                occured = DateTime.Now,
                critical = false,
                eventType = "Starting Scouting with Match " + CurrentMatchSelected.Number.ToString() + " with Scouter " + selectedUser.Name
            });
            // try to get a dynamicly loading page
            start_StartContainer.FadeTo(0, 250, Easing.CubicInOut);
            start_Loading.Text = loadingGags[new Random().Next(0, 6)];
            start_Loading.Opacity = 0;
            start_Loading.IsVisible = true;
            await start_Loading.FadeTo(1, 250, Easing.CubicInOut);
            start_StartContainer.IsVisible = false;
            CurrentMatchSelected.ScoutedBy = selectedUser.Name;
            //ma.IsVisible = true;
            Navigation.PushAsync(new Scouting(CurrentMatchSelected));
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

        private void NextMatch(object sender, EventArgs e)
        {
            if (CurrentMatchIndex < ApplicationDataHandler.AvailableEntries.Count - 1)
            {
                CurrentMatchIndex += 1;
                CurrentMatchSelected = ApplicationDataHandler.AvailableEntries.ToArray()[CurrentMatchIndex];
                UpdateMatchContainer();
            }
        }
        private void PrevMatch(object sender, EventArgs e)
        {
            if (CurrentMatchIndex > 0)
            {
                CurrentMatchIndex -= 1;
                CurrentMatchSelected = ApplicationDataHandler.AvailableEntries.ToArray()[CurrentMatchIndex];
                UpdateMatchContainer();
            }
        }

        private async void start_ScoutConfirm_Clicked(object sender, EventArgs e)
        {
            start_ScoutContainer.FadeTo(0, 250, Easing.CubicInOut);
            start_StartContainer.Opacity = 0;
            start_StartContainer.IsVisible = true;
            await start_StartContainer.FadeTo(1, 250, Easing.CubicInOut);
            start_ScoutContainer.IsVisible = false;

        }

        private async void start_StartConfirm_Clicked(object sender, EventArgs e)
        {
            start_StartContainer.FadeTo(0, 250, Easing.CubicInOut);
            start_ScoutContainer.Opacity = 0;
            start_ScoutContainer.IsVisible = true;
            await start_ScoutContainer.FadeTo(1, 250, Easing.CubicInOut);
            start_StartContainer.IsVisible = false;
        }

        private void start_ScoutPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (start_ScoutPicker.SelectedIndex != -1)
            {
                start_ScoutConfirm.IsEnabled = true;
            }

        }

        private async Task<bool> TryAdminLogin()
        {
            if (ApplicationDataHandler.CurrentApplicationData.AdminCode != null && ApplicationDataHandler.CurrentApplicationData.AdminCode != "")
            {
                var result = await DisplayPromptAsync("Authorization Needed", "An administrator passcode is needed to perform this action. Please enter it in below!", "Submit", "Cancel", "####", 4, Keyboard.Numeric);
                if (result == null || result == "")
                {
                    // cancelled request
                    return false;

                }
                if (result == ApplicationDataHandler.CurrentApplicationData.AdminCode)
                    return true;
                return false;
            }
            return true;
        }


        private async void debug_cleardata_Clicked(object sender, EventArgs e)
        {
            var choice = await DisplayAlert("Are you sure?", "This is an irreversible action that will bring the app to its original state!", "Yes!", "Nah...");
            if (choice)
            {
                if (await TryAdminLogin())
                {
                    await ApplicationDataHandler.Instance.AddLog(new Log()
                    {
                        occured = DateTime.Now,
                        critical = false,
                        eventType = "Clearing all Data"
                    });
                    debug_cleardata.IsEnabled = false;
                    bool wasSuccessful = await ApplicationDataHandler.Instance.ClearAllData();
                    if (wasSuccessful)
                    {
                        await ApplicationDataHandler.Instance.InitializeData();
                        Navigation.PushAsync(new MasterPage());
                    }
                    else
                    {
                        await DisplayAlert("Not Available", "This tablet has not been set to debug mode by the server, and cannot perform this action!", "OK");

                    }
                }


            }

        }

        private async void create_backup_Clicked(object sender, EventArgs e)
        {
            await ApplicationDataHandler.Instance.SetBackup();
            await ApplicationDataHandler.Instance.AddLog(new Log()
            {
                occured = DateTime.Now,
                critical = false,
                eventType = "Setting Backup"
            });
            await DisplayAlert("Backup Set", "You have successfully set the backup!", "OK");
        }

        private async void load_backup_Clicked(object sender, EventArgs e)
        {
            var choice = await DisplayAlert("Are you sure?", "This is an irreversible action that will bring the app to a previous state!", "Yes!", "Nah...");
            if (choice)
            {
                if (await TryAdminLogin())
                {
                    await ApplicationDataHandler.Instance.AddLog(new Log()
                    {
                        occured = DateTime.Now,
                        critical = false,
                        eventType = "Getting Previous Backup"
                    });
                    await ApplicationDataHandler.Instance.GetBackup();
                    Navigation.PushAsync(new MasterPage());
                }


            }
        }

        private ObservableCollection<AbridgedDataEntry> itemsToShow;

        private async void ShowAllMatches(object sender, EventArgs e)
        {
            itemsToShow = new ObservableCollection<AbridgedDataEntry>();
            int i = 0;
            foreach(var rawMatch in ApplicationDataHandler.AvailableEntries)
            {
                itemsToShow.Add(new AbridgedDataEntry() { Completed = rawMatch.Completed, Index = i.ToString(), Number = "Match " + rawMatch.Number.ToString(), Position = rawMatch.Position });
                i++;
            }
            allMatches.ItemsSource = itemsToShow;
            insideMatchOverlay.Opacity = 0;
            overlaySelectAMatch.IsVisible = true;
            await insideMatchOverlay.FadeTo(1, 250, Easing.CubicInOut);
            
        }

        private async void SelectMatchFromAll(object sender, ItemTappedEventArgs e)
        {
            CurrentMatchIndex = e.ItemIndex;
            CurrentMatchSelected = ApplicationDataHandler.AvailableEntries.ToArray()[CurrentMatchIndex];
            UpdateMatchContainer();
            await insideMatchOverlay.FadeTo(0, 250, Easing.CubicInOut);
            overlaySelectAMatch.IsVisible = false;
        }

        private async void ExitAllMatches(object sender, EventArgs e)
        {
            await insideMatchOverlay.FadeTo(0, 250, Easing.CubicInOut);
            overlaySelectAMatch.IsVisible = false;
        }
    }

    public class AbridgedDataEntry
    {
        public string Index { get; set; }
        public string Number { get; set; }
        public bool Completed { get; set; }
        public string Position { get; set; }
    }
}