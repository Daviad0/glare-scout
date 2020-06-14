using LightScout.Models;
using Newtonsoft.Json;
using Plugin.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LightScout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FRCMain : ContentPage
    {
        private static bool[] ControlPanel = new bool[2];
        private static bool Balanced;
        private int CurrentSubPage;
        private int DisabledSeconds;
        private bool CurrentlyDisabled;
        private bool InitLineAchieved;
        public FRCMain()
        {
            var converter = new ColorTypeConverter();
            InitializeComponent();
            ControlPanel[0] = false;
            ControlPanel[1] = false;
            BackgroundColor = (Color)converter.ConvertFromInvariantString("#009cd7");
            
        }
        protected override async void OnAppearing()
        {
            var converter = new ColorTypeConverter();
            /*var objectLoaded = new TeamMatch();
            try
            {
                var dataLoaded = DependencyService.Get<DataStore>().LoadData("frctest050220.txt");
                objectLoaded = JsonConvert.DeserializeObject<TeamMatch>(dataLoaded);
                ControlPanel[0] = objectLoaded.ControlPanelRotation;
                ControlPanel[1] = objectLoaded.ControlPanelPosition;
                Balanced = objectLoaded.ClimbBalance;
                scoutName.Text = objectLoaded.ScoutName;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }*/
            base.OnAppearing();
            
            await Task.Delay(TimeSpan.FromSeconds(1));
            await animatedelement.TranslateTo(TranslationX, TranslationY - 50, 1000, Easing.SinOut);
            HiddenLabel.FadeTo(1, 350);
            HiddenLabelName.FadeTo(1, 350);
            HiddenLabelDetails.FadeTo(1, 350);
            await Task.Delay(TimeSpan.FromSeconds(.5));
            HiddenButtonGo.FadeTo(1, 500);

        }
        private async void StartScouting(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.White");
            await overLay.FadeTo(0, 500, Easing.SinOut);
            overLay.IsVisible = false;
            TextFlipTimer();
        }
        private void CPChange(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (sender == cp_lv1)
            {
                ControlPanel[0] = !ControlPanel[0];
                if (ControlPanel[0])
                {
                    cp_lv1.Style = Resources["lightPrimarySelected"] as Style;
                }
                else
                {
                    cp_lv1.Style = Resources["lightPrimary"] as Style;
                }

            }
            if (sender == cp_lv2)
            {
                ControlPanel[1] = !ControlPanel[1];
                if (ControlPanel[1])
                {
                    cp_lv2.Style = Resources["lightPrimarySelected"] as Style;
                }
                else
                {
                    cp_lv2.Style = Resources["lightPrimary"] as Style;
                }

            }
        }
        private void BalancedChange(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (sender == balanced_opt1)
            {
                Balanced = true;
                balanced_opt1.Style = Resources["lightSecondarySelected"] as Style;
                balanced_opt2.Style = Resources["lightSecondary"] as Style;
            }
            else
            {
                Balanced = false;
                balanced_opt2.Style = Resources["lightSecondarySelected"] as Style;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
            }
        }
        private void SendTheData(object sender, EventArgs e)
        {
            var SaveMatch = new TeamMatch();
            SaveMatch.E_Balanced = Balanced;
            SaveMatch.T_ControlPanelRotation = ControlPanel[0];
            SaveMatch.T_ControlPanelPosition = ControlPanel[1];
            SaveMatch.ScoutName = scoutName.Text;
            SaveMatch.MatchNumber = 1;
            SaveMatch.NumCycles = 2;
            DependencyService.Get<DataStore>().SaveData("frctest051920.txt", SaveMatch);
        }
        private void LoadTheData(object sender, EventArgs e)
        {
            showData.Text = DependencyService.Get<DataStore>().LoadData("frctest051920.txt");
        }
        private void BackToMainMenu(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }

        private void EnableDisabledMenu(object sender, EventArgs e)
        {
            disabledMenu.IsVisible = true;
            CurrentlyDisabled = true;
            DisabledTimer();

        }
        private void DisableDisabledMenu(object sender, EventArgs e)
        {
            
            CurrentlyDisabled = false;
            disabledMenu.IsVisible = false;
            

        }
        private void ChangeInitLine(object sender, EventArgs e)
        {
            InitLineAchieved = !InitLineAchieved;
            if (InitLineAchieved)
            {
                initLineToggle.Style = Resources["lightPrimarySelected"] as Style;
                initLineToggle.Text = "Achieved";
            }
            else
            {
                initLineToggle.Style = Resources["lightPrimary"] as Style;
                initLineToggle.Text = "Not Achieved";
            }
        }
        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }


        private async void prevForm_Clicked(object sender, EventArgs e)
        {
            CurrentSubPage--;
            nextForm.IsEnabled = true;
            finishForm.IsVisible = false;
            if (CurrentSubPage == 0)
            {
                exitForm.IsVisible = true;
                prevForm.IsEnabled = false;
                autoForm.FadeTo(0, 250);
                autoForm.IsVisible = false;
                nameForm.IsVisible = true;
                nameForm.FadeTo(1,250);
            }else if (CurrentSubPage == 1)
            {
                teleopForm.FadeTo(0, 250);
                teleopForm.IsVisible = false;
                autoForm.IsVisible = true;
                autoForm.FadeTo(1, 250);
            }
            else if (CurrentSubPage == 2)
            {
                endgameForm.FadeTo(0, 250);
                endgameForm.IsVisible = false;
                teleopForm.IsVisible = true;
                teleopForm.FadeTo(1, 250);
            }
            else if (CurrentSubPage == 3)
            {
                confirmForm.FadeTo(0, 250);
                confirmForm.IsVisible = false;
                endgameForm.IsVisible = true;
                endgameForm.FadeTo(1, 250);
            }
        }

        private async void nextForm_Clicked(object sender, EventArgs e)
        {
            CurrentSubPage++;
            exitForm.IsVisible = false;
            prevForm.IsEnabled = true;
            if (CurrentSubPage == 1)
            {
                nameForm.FadeTo(0, 250);
                nameForm.IsVisible = false;
                autoForm.IsVisible = true;
                autoForm.FadeTo(1, 250);
                
            }else if(CurrentSubPage == 2)
            {
                autoForm.FadeTo(0, 250);
                autoForm.IsVisible = false;
                teleopForm.IsVisible = true;
                teleopForm.FadeTo(1, 250);
            }
            else if (CurrentSubPage == 3)
            {
                teleopForm.FadeTo(0, 250);
                teleopForm.IsVisible = false;
                endgameForm.IsVisible = true;
                endgameForm.FadeTo(1, 250);
            }
            else if (CurrentSubPage == 4)
            {
                nextForm.IsEnabled = false;
                finishForm.IsVisible = true;
                endgameForm.FadeTo(0, 250);
                endgameForm.IsVisible = false;
                confirmForm.IsVisible = true;
                confirmForm.FadeTo(1, 250);
            }
        }
        private void DisabledTimer()
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                
                if (CurrentlyDisabled)
                {
                    OnSLLight.IsVisible = !OnSLLight.IsVisible;
                    OffSLLight.IsVisible = !OffSLLight.IsVisible;
                    DisabledSeconds++;
                    disabledSeconds.Text = DisabledSeconds.ToString() + "s";
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }
        private void TextFlipTimer()
        {
            bool firstCycle = false;
            Device.StartTimer(TimeSpan.FromSeconds(6), () =>
            {
                Task.Run(async () =>
                {
                    if (!firstCycle)
                    {
                        matchNumber.TranslateTo(0, -20);
                        await teamName.TranslateTo(0, 0, 250, Easing.SinIn);
                        teamName.FadeTo(0, 250, Easing.SinOut);
                        matchNumber.FadeTo(1, 250, Easing.SinOut);
                        matchNumber.TranslateTo(0, 0, 250, Easing.SinIn);
                        await teamName.TranslateTo(0, -20);
                    }
                    else
                    {
                        teamName.TranslateTo(0, -20);
                        await matchNumber.TranslateTo(0, 0, 250, Easing.SinIn);
                        matchNumber.FadeTo(0, 250, Easing.SinOut);
                        teamName.FadeTo(1, 250, Easing.SinOut);
                        teamName.TranslateTo(0, 0, 250, Easing.SinIn);
                        await matchNumber.TranslateTo(0, -20);
                    }
                    firstCycle = !firstCycle;
                    
                });
                return true;
            });
        }
    }
}