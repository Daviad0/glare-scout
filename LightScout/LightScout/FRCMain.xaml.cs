using LightScout.Models;
using Newtonsoft.Json;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public partial class FRCMain : ContentPage
    {
        private bool[] ControlPanel = new bool[2];
        private bool Balanced;
        private bool Climb;
        private bool Parked;
        private int SecondsScouting;
        private bool Attempted;
        private int CurrentMatchNum = 1;
        private List<string> trackingLogs = new List<string>();
        private int[] PowerCellInner = new int[21];
        private int[] PowerCellOuter = new int[21];
        private int[] PowerCellLower = new int[21];
        private int[] PowerCellMissed = new int[21];
        private int NumCycles = 0;
        private int CurrentCycle = 1;
        private int CurrentSubPage;
        private TeamMatch selectedMatch;
        private double DisabledSeconds;
        private int StackLightCounter;
        private bool CurrentlyDisabled;
        private bool InitLineAchieved;
        private DateTime startScoutingTime;
        private DateTime endScoutingTime;
        private double[] OriginalCardCoords = { 0, 0 };
        private static IBluetoothLE ble = CrossBluetoothLE.Current;
        private static IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        private static IDevice deviceIWant;
        private static ObservableCollection<IDevice> Devices = new ObservableCollection<IDevice>();
        private static bool canTransmitData = true;
        public FRCMain(TeamMatch matchTemplate)
        {
            adapter.DeviceConnected += async (s, a) =>
            {
                Console.WriteLine("Connected to: " + a.Device.Name.ToString());
                //status.Text = "Connected to: " + a.Device.Name.ToString();
                canTransmitData = true;
            };
            var converter = new ColorTypeConverter();
            InitializeComponent();
            selectedMatch = matchTemplate;
            ControlPanel[0] = matchTemplate.T_ControlPanelRotation;
            ControlPanel[1] = matchTemplate.T_ControlPanelPosition;
            Balanced = matchTemplate.E_Balanced;
            Climb = matchTemplate.E_ClimbSuccess;
            Parked = matchTemplate.E_Park;
            Attempted = matchTemplate.E_ClimbAttempt;
            CurrentMatchNum = matchTemplate.MatchNumber;
            PowerCellInner = matchTemplate.PowerCellInner;
            PowerCellLower = matchTemplate.PowerCellLower;
            PowerCellMissed = matchTemplate.PowerCellMissed;
            PowerCellOuter = matchTemplate.PowerCellOuter;
            NumCycles = matchTemplate.NumCycles;
            DisabledSeconds = matchTemplate.DisabledSeconds;
            InitLineAchieved = matchTemplate.A_InitiationLine;
            HiddenLabel.Text = "Team " + matchTemplate.TeamNumber.ToString();
            if (matchTemplate.ScoutName != null)
            {
                scoutName.Text = matchTemplate.ScoutName;
            }

            teamName.Text = "Team " + matchTemplate.TeamNumber.ToString();
            if (matchTemplate.TabletId != null)
            {
                if (matchTemplate.TabletId.StartsWith("R"))
                {
                    HiddenLabelDetails.Text = "Match " + matchTemplate.MatchNumber + " - Red";
                    matchNumber.Text = "Match " + matchTemplate.MatchNumber + " - Red";
                }
                else if (matchTemplate.TabletId.StartsWith("B"))
                {
                    HiddenLabelDetails.Text = "Match " + matchTemplate.MatchNumber + " - Blue";
                    matchNumber.Text = "Match " + matchTemplate.MatchNumber + " - Blue";
                }
                else
                {
                    HiddenLabelDetails.Text = "Match " + matchTemplate.MatchNumber + " - ???";
                    matchNumber.Text = "Match " + matchTemplate.MatchNumber + " - ???";
                }
            }
            else
            {
                HiddenLabelDetails.Text = "Match " + matchTemplate.MatchNumber + " - ???";
                matchNumber.Text = "Match " + matchTemplate.MatchNumber + " - ???";
            }


            if (matchTemplate.TeamName == null)
            {
                HiddenLabelName.Text = "FRC Team " + matchTemplate.TeamNumber.ToString();
            }
            else
            {
                HiddenLabelName.Text = matchTemplate.TeamName;
            }
            SetCurrentVisualValues();
            BackgroundColor = (Color)converter.ConvertFromInvariantString("#009cd7");
            //adapter.ConnectToDeviceAsync(deviceIWant);
        }
        private void SetCurrentVisualValues()
        {
            var converter = new ColorTypeConverter();
            if (InitLineAchieved)
            {
                initLineToggle.Style = Resources["lightSecondarySelected"] as Style;
                initLineToggle.Text = "Achieved";
            }
            else
            {
                initLineToggle.Style = Resources["lightSecondary"] as Style;
                initLineToggle.Text = "Not Achieved";
            }
            if (ControlPanel[0])
            {
                cp_lv1.Style = Resources["lightSecondarySelected"] as Style;
            }
            else
            {
                cp_lv1.Style = Resources["lightSecondary"] as Style;
            }
            if (ControlPanel[1])
            {
                cp_lv2.Style = Resources["lightSecondarySelected"] as Style;
            }
            else
            {
                cp_lv2.Style = Resources["lightSecondary"] as Style;
            }
            if (!Balanced)
            {
                Balanced = false;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
                balanced_opt1.Text = "Not Balanced";
            }
            else
            {
                Balanced = true;
                balanced_opt1.Style = Resources["lightSecondarySelected"] as Style;
                balanced_opt1.Text = "Robot(s) Balanced";
            }
            if (!Climb)
            {
                Climb = false;
                climb_opt1.Style = Resources["lightSecondary"] as Style;
                climb_opt1.Text = "Not Successful";
                balancedMenu.IsEnabled = false;
                balancedMenu.Opacity = 0.35;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
                balanced_opt1.Text = "Did Not Contribute";
            }
            else
            {
                Climb = true;
                climb_opt1.Style = Resources["lightSecondarySelected"] as Style;
                climb_opt1.Text = "Climb Successful";
            }
            if (!Attempted)
            {
                Attempted = false;
                attempted_opt1.Style = Resources["lightSecondary"] as Style;
                attempted_opt1.Text = "Not Attemped";
                climb_opt1.Style = Resources["lightSecondary"] as Style;
                climb_opt1.Text = "Not Attempted";
                climbMenu.IsEnabled = false;
                climbMenu.Opacity = 0.35;
                balancedMenu.IsEnabled = false;
                balancedMenu.Opacity = 0.35;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
                balanced_opt1.Text = "Did Not Contribute";
            }
            else
            {
                if (Parked)
                {
                    climb_opt1.Style = Resources["lightSecondary"] as Style;
                    climb_opt1.Text = "Not Successful";
                    Attempted = true;
                    attempted_opt1.Style = Resources["lightSecondarySelected"] as Style;
                    attempted_opt1.Text = "Climb Attempted";
                }


            }
            if (!Parked)
            {
                Parked = false;
                park_opt1.Style = Resources["lightSecondary"] as Style;
                park_opt1.Text = "Did Not Park";
            }
            else
            {
                Parked = true;
                park_opt1.Style = Resources["lightSecondarySelected"] as Style;
                park_opt1.Text = "Successfully Parked";
                climbMenu.IsEnabled = false;
                climbMenu.Opacity = 0.35;
                climb_opt1.Style = Resources["lightSecondary"] as Style;
                climb_opt1.Text = "Not Attempted";
                Climb = false;
                Balanced = false;
                balancedMenu.IsEnabled = false;
                balancedMenu.Opacity = 0.35;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
                balanced_opt1.Text = "Did Not Contribute";
                if (Attempted)
                {
                    climb_opt1.Style = Resources["lightSecondary"] as Style;
                    climb_opt1.Text = "Not Successful";
                }
            }

            if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 0)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 1)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
            }

            disabledSeconds.Text = ((int)Math.Floor(DisabledSeconds)).ToString() + "s";

            AinnerAmount.Text = PowerCellInner[0].ToString() + " PC";
            if (PowerCellInner[0] <= 0)
            {
                AinnerDown.IsEnabled = false;
            }
            else
            {
                AinnerDown.IsEnabled = true;
            }
            AouterAmount.Text = PowerCellOuter[0].ToString() + " PC";
            if (PowerCellOuter[0] <= 0)
            {
                AouterDown.IsEnabled = false;
            }
            else
            {
                AouterDown.IsEnabled = true;
            }
            AlowerAmount.Text = PowerCellLower[0].ToString() + " PC";
            if (PowerCellLower[0] <= 0)
            {
                AlowerDown.IsEnabled = false;
            }
            else
            {
                AlowerDown.IsEnabled = true;
            }
            AmissedAmount.Text = PowerCellMissed[0].ToString() + " PC";
            if (PowerCellMissed[0] <= 0)
            {
                AmissedDown.IsEnabled = false;
            }
            else
            {
                AmissedDown.IsEnabled = true;
            }
            if ((PowerCellInner[0] + PowerCellLower[0] + PowerCellMissed[0] + PowerCellOuter[0]) >= 15)
            {
                AinnerUp.IsEnabled = false;
                AouterUp.IsEnabled = false;
                AlowerUp.IsEnabled = false;
                AmissedUp.IsEnabled = false;
            }

            innerAmount.Text = PowerCellInner[1].ToString() + " PC";
            if (PowerCellInner[1] <= 0)
            {
                innerDown.IsEnabled = false;
            }
            else
            {
                innerDown.IsEnabled = true;
            }
            outerAmount.Text = PowerCellOuter[1].ToString() + " PC";
            if (PowerCellOuter[1] <= 0)
            {
                outerDown.IsEnabled = false;
            }
            else
            {
                outerDown.IsEnabled = true;
            }
            lowerAmount.Text = PowerCellLower[1].ToString() + " PC";
            if (PowerCellLower[1] <= 0)
            {
                lowerDown.IsEnabled = false;
            }
            else
            {
                lowerDown.IsEnabled = true;
            }
            missedAmount.Text = PowerCellMissed[1].ToString() + " PC";
            if (PowerCellMissed[1] <= 0)
            {
                missedDown.IsEnabled = false;
            }
            else
            {
                missedDown.IsEnabled = true;
            }
            if ((PowerCellInner[1] + PowerCellLower[1] + PowerCellMissed[1] + PowerCellOuter[1]) >= 5)
            {
                innerUp.IsEnabled = false;
                outerUp.IsEnabled = false;
                lowerUp.IsEnabled = false;
                missedUp.IsEnabled = false;
            }
            int totalTInnerPCCount = 0;
            int totalTOuterPCCount = 0;
            int totalTLowerPCCount = 0;
            int totalTMissedPCCount = 0;
            foreach (var pccount in PowerCellInner.ToList().Skip(1))
            {
                totalTInnerPCCount += pccount;
            }
            foreach (var pccount in PowerCellOuter.ToList().Skip(1))
            {
                totalTOuterPCCount += pccount;
            }
            foreach (var pccount in PowerCellLower.ToList().Skip(1))
            {
                totalTLowerPCCount += pccount;
            }
            foreach (var pccount in PowerCellMissed.ToList().Skip(1))
            {
                totalTMissedPCCount += pccount;
            }
            totalTInnerPC.Text = totalTInnerPCCount.ToString();
            totalTOuterPC.Text = totalTOuterPCCount.ToString();
            totalTLowerPC.Text = totalTLowerPCCount.ToString();
            totalTMissedPC.Text = totalTMissedPCCount.ToString();
            totalAInnerPC.Text = PowerCellInner[0].ToString();
            totalAOuterPC.Text = PowerCellOuter[0].ToString();
            totalALowerPC.Text = PowerCellLower[0].ToString();
            totalAMissedPC.Text = PowerCellMissed[0].ToString();
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
            OriginalCardCoords[0] = cardToFlip.X;
            OriginalCardCoords[1] = cardToFlip.Y;
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
            startScoutingTime = DateTime.Now;
            trackingLogs.Add("0:1");
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!submittingFormToBluetooth.IsVisible)
                {
                    SecondsScouting++;
                    return true;
                }
                else
                {
                    return false;
                }
                
            });
            ShowToolTip();
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
                    cp_lv1.Style = Resources["lightSecondarySelected"] as Style;
                }
                else
                {
                    cp_lv1.Style = Resources["lightSecondary"] as Style;
                }

            }
            if (sender == cp_lv2)
            {
                ControlPanel[1] = !ControlPanel[1];
                if (ControlPanel[1])
                {
                    cp_lv2.Style = Resources["lightSecondarySelected"] as Style;
                }
                else
                {
                    cp_lv2.Style = Resources["lightSecondary"] as Style;
                }

            }
        }
        private async void ShowToolTip()
        {
            await toolTip.FadeTo(1, 800, Easing.SinOut);
            await toolTip.FadeTo(1, 2500, Easing.SinOut);
            await toolTip.FadeTo(0, 800, Easing.SinOut);
        }
        private void BalancedChange(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (Balanced)
            {
                Balanced = false;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
                balanced_opt1.Text = "Not Balanced";
            }
            else
            {
                Balanced = true;
                balanced_opt1.Style = Resources["lightSecondarySelected"] as Style;
                balanced_opt1.Text = "Robot(s) Balanced";
            }

        }
        private void ClimbChange(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (Climb)
            {
                Climb = false;
                climb_opt1.Style = Resources["lightSecondary"] as Style;
                climb_opt1.Text = "Not Successful";
                balancedMenu.IsEnabled = false;
                balancedMenu.Opacity = 0.35;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
                balanced_opt1.Text = "Did Not Contribute";
            }
            else
            {
                Climb = true;
                climb_opt1.Style = Resources["lightSecondarySelected"] as Style;
                climb_opt1.Text = "Climb Successful";
                balancedMenu.IsEnabled = true;
                balancedMenu.Opacity = 1;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
                balanced_opt1.Text = "Not Balanced";
            }

        }
        private void ParkChange(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (Parked)
            {
                Parked = false;
                park_opt1.Style = Resources["lightSecondary"] as Style;
                park_opt1.Text = "Did Not Park";
                balancedMenu.IsEnabled = false;
                balancedMenu.Opacity = 0.35;
                attemptedMenu.IsEnabled = true;
                attemptedMenu.Opacity = 1;
                climbMenu.IsEnabled = false;
                climbMenu.Opacity = 0.35;
                if (Attempted)
                {
                    climbMenu.IsEnabled = true;
                    climbMenu.Opacity = 1;
                }
            }
            else
            {
                Parked = true;
                park_opt1.Style = Resources["lightSecondarySelected"] as Style;
                park_opt1.Text = "Successfully Parked";
                climbMenu.IsEnabled = false;
                climbMenu.Opacity = 0.35;
                climb_opt1.Style = Resources["lightSecondary"] as Style;
                climb_opt1.Text = "Not Attempted";
                Climb = false;
                Balanced = false;
                balancedMenu.IsEnabled = false;
                balancedMenu.Opacity = 0.35;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
                balanced_opt1.Text = "Did Not Contribute";
                if (Attempted)
                {
                    climb_opt1.Style = Resources["lightSecondary"] as Style;
                    climb_opt1.Text = "Not Successful";
                }
            }

        }
        private void AttempedChange(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (Attempted)
            {
                Attempted = false;
                attempted_opt1.Style = Resources["lightSecondary"] as Style;
                attempted_opt1.Text = "Not Attemped";
                climb_opt1.Style = Resources["lightSecondary"] as Style;
                climb_opt1.Text = "Not Attempted";
                climbMenu.IsEnabled = false;
                climbMenu.Opacity = 0.35;
                balancedMenu.IsEnabled = false;
                balancedMenu.Opacity = 0.35;
                balanced_opt1.Style = Resources["lightSecondary"] as Style;
                balanced_opt1.Text = "Did Not Contribute";
            }
            else
            {
                if (!Parked)
                {
                    climbMenu.IsEnabled = true;
                    climbMenu.Opacity = 1;
                    balanced_opt1.Style = Resources["lightSecondary"] as Style;
                    balanced_opt1.Text = "Did Not Contribute";
                }
                climb_opt1.Style = Resources["lightSecondary"] as Style;
                climb_opt1.Text = "Not Successful";
                Attempted = true;
                attempted_opt1.Style = Resources["lightSecondarySelected"] as Style;
                attempted_opt1.Text = "Climb Attempted";

            }

        }
        private void SendTheData(object sender, EventArgs e)
        {

            DependencyService.Get<DataStore>().SaveDummyData("JacksonEvent2020.txt");
            /*var match1 = new TeamMatch();
            match1.MatchNumber = 1;
            match1.TeamNumber = 862;
            match1.PowerCellInner = new int[21];
            match1.PowerCellOuter = new int[21];
            match1.PowerCellLower = new int[21];
            match1.PowerCellMissed = new int[21];
            var match2 = new TeamMatch();
            match2.MatchNumber = 2;
            match2.TeamNumber = 862;
            match2.PowerCellInner = new int[21];
            match2.PowerCellOuter = new int[21];
            match2.PowerCellLower = new int[21];
            match2.PowerCellMissed = new int[21];
            var match3 = new TeamMatch();
            match3.MatchNumber = 3;
            match3.TeamNumber = 862;
            match3.PowerCellInner = new int[21];
            match3.PowerCellOuter = new int[21];
            match3.PowerCellLower = new int[21];
            match3.PowerCellMissed = new int[21];
            var match4 = new TeamMatch();
            match4.MatchNumber = 4;
            match4.TeamNumber = 862;
            match4.PowerCellInner = new int[21];
            match4.PowerCellOuter = new int[21];
            match4.PowerCellLower = new int[21];
            match4.PowerCellMissed = new int[21];
            var match5 = new TeamMatch();
            match5.MatchNumber = 5;
            match5.TeamNumber = 862;
            match5.PowerCellInner = new int[21];
            match5.PowerCellOuter = new int[21];
            match5.PowerCellLower = new int[21];
            match5.PowerCellMissed = new int[21];
            var match6 = new TeamMatch();
            match6.MatchNumber = 6;
            match6.TeamNumber = 862;
            match6.PowerCellInner = new int[21];
            match6.PowerCellOuter = new int[21];
            match6.PowerCellLower = new int[21];
            match6.PowerCellMissed = new int[21];
            var SaveMatches = new List<TeamMatch>();
            SaveMatches.Add(match1);
            SaveMatches.Add(match2);
            SaveMatches.Add(match3);
            SaveMatches.Add(match4);
            SaveMatches.Add(match5);
            SaveMatches.Add(match6);*/
        }
        private void LoadTheData(object sender, EventArgs e)
        {
            Console.WriteLine(DependencyService.Get<DataStore>().LoadData("JacksonEvent2020.txt"));
        }
        private void BackToMainMenu(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }
        private async void PowerCellsUp(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (CurrentCycle > NumCycles)
            {
                NumCycles = CurrentCycle;
            }
            var buttonClicked = sender as Button;
            if (buttonClicked == innerUp)
            {
                PowerCellInner[CurrentCycle]++;
                innerDown.IsEnabled = true;
                innerAmount.Text = PowerCellInner[CurrentCycle].ToString() + " PC";
            }
            else if (buttonClicked == outerUp)
            {
                PowerCellOuter[CurrentCycle]++;
                outerDown.IsEnabled = true;
                outerAmount.Text = PowerCellOuter[CurrentCycle].ToString() + " PC";
            }
            else if (buttonClicked == lowerUp)
            {
                PowerCellLower[CurrentCycle]++;
                lowerDown.IsEnabled = true;
                lowerAmount.Text = PowerCellLower[CurrentCycle].ToString() + " PC";
            }
            else if (buttonClicked == missedUp)
            {
                PowerCellMissed[CurrentCycle]++;
                missedDown.IsEnabled = true;
                missedAmount.Text = PowerCellMissed[CurrentCycle].ToString() + " PC";
            }
            if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
            {
                innerUp.IsEnabled = false;
                outerUp.IsEnabled = false;
                lowerUp.IsEnabled = false;
                missedUp.IsEnabled = false;
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 1)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 0)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            int totalTInnerPCCount = 0;
            int totalTOuterPCCount = 0;
            int totalTLowerPCCount = 0;
            int totalTMissedPCCount = 0;
            foreach (var pccount in PowerCellInner.ToList().Skip(1))
            {
                totalTInnerPCCount += pccount;
            }
            foreach (var pccount in PowerCellOuter.ToList().Skip(1))
            {
                totalTOuterPCCount += pccount;
            }
            foreach (var pccount in PowerCellLower.ToList().Skip(1))
            {
                totalTLowerPCCount += pccount;
            }
            foreach (var pccount in PowerCellMissed.ToList().Skip(1))
            {
                totalTMissedPCCount += pccount;
            }
            totalTInnerPC.Text = totalTInnerPCCount.ToString();
            totalTOuterPC.Text = totalTOuterPCCount.ToString();
            totalTLowerPC.Text = totalTLowerPCCount.ToString();
            totalTMissedPC.Text = totalTMissedPCCount.ToString();
        }
        private async void PowerCellsDown(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            var buttonClicked = sender as Button;
            if (buttonClicked == innerDown)
            {
                PowerCellInner[CurrentCycle]--;
                if (PowerCellInner[CurrentCycle] <= 0)
                {
                    innerDown.IsEnabled = false;
                }
                innerAmount.Text = PowerCellInner[CurrentCycle].ToString() + " PC";
            }
            else if (buttonClicked == outerDown)
            {
                PowerCellOuter[CurrentCycle]--;
                if (PowerCellOuter[CurrentCycle] <= 0)
                {
                    outerDown.IsEnabled = false;
                }
                outerAmount.Text = PowerCellOuter[CurrentCycle].ToString() + " PC";
            }
            else if (buttonClicked == lowerDown)
            {
                PowerCellLower[CurrentCycle]--;
                if (PowerCellLower[CurrentCycle] <= 0)
                {
                    lowerDown.IsEnabled = false;
                }
                lowerAmount.Text = PowerCellLower[CurrentCycle].ToString() + " PC";
            }
            else if (buttonClicked == missedDown)
            {
                PowerCellMissed[CurrentCycle]--;
                if (PowerCellMissed[CurrentCycle] <= 0)
                {
                    missedDown.IsEnabled = false;
                }
                missedAmount.Text = PowerCellMissed[CurrentCycle].ToString() + " PC";
            }
            innerUp.IsEnabled = true;
            outerUp.IsEnabled = true;
            lowerUp.IsEnabled = true;
            missedUp.IsEnabled = true;
            if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 0)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 1)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
            {
                pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
            }
            int totalTInnerPCCount = 0;
            int totalTOuterPCCount = 0;
            int totalTLowerPCCount = 0;
            int totalTMissedPCCount = 0;
            foreach (var pccount in PowerCellInner.ToList().Skip(1))
            {
                totalTInnerPCCount += pccount;
            }
            foreach (var pccount in PowerCellOuter.ToList().Skip(1))
            {
                totalTOuterPCCount += pccount;
            }
            foreach (var pccount in PowerCellLower.ToList().Skip(1))
            {
                totalTLowerPCCount += pccount;
            }
            foreach (var pccount in PowerCellMissed.ToList().Skip(1))
            {
                totalTMissedPCCount += pccount;
            }
            totalTInnerPC.Text = totalTInnerPCCount.ToString();
            totalTOuterPC.Text = totalTOuterPCCount.ToString();
            totalTLowerPC.Text = totalTLowerPCCount.ToString();
            totalTMissedPC.Text = totalTMissedPCCount.ToString();
        }
        private async void APowerCellsUp(object sender, EventArgs e)
        {

            var buttonClicked = sender as Button;
            if (buttonClicked == AinnerUp)
            {
                PowerCellInner[0]++;
                AinnerDown.IsEnabled = true;
                AinnerAmount.Text = PowerCellInner[0].ToString() + " PC";
            }
            else if (buttonClicked == AouterUp)
            {
                PowerCellOuter[0]++;
                AouterDown.IsEnabled = true;
                AouterAmount.Text = PowerCellOuter[0].ToString() + " PC";
            }
            else if (buttonClicked == AlowerUp)
            {
                PowerCellLower[0]++;
                AlowerDown.IsEnabled = true;
                AlowerAmount.Text = PowerCellLower[0].ToString() + " PC";
            }
            else if (buttonClicked == AmissedUp)
            {
                PowerCellMissed[0]++;
                AmissedDown.IsEnabled = true;
                AmissedAmount.Text = PowerCellMissed[0].ToString() + " PC";
            }
            if ((PowerCellInner[0] + PowerCellLower[0] + PowerCellMissed[0] + PowerCellOuter[0]) >= 15)
            {
                AinnerUp.IsEnabled = false;
                AouterUp.IsEnabled = false;
                AlowerUp.IsEnabled = false;
                AmissedUp.IsEnabled = false;
            }
            totalAInnerPC.Text = PowerCellInner[0].ToString();
            totalAOuterPC.Text = PowerCellOuter[0].ToString();
            totalALowerPC.Text = PowerCellLower[0].ToString();
            totalAMissedPC.Text = PowerCellMissed[0].ToString();
        }
        private async void APowerCellsDown(object sender, EventArgs e)
        {
            var buttonClicked = sender as Button;
            if (buttonClicked == AinnerDown)
            {
                PowerCellInner[0]--;
                if (PowerCellInner[0] <= 0)
                {
                    AinnerDown.IsEnabled = false;
                }
                AinnerAmount.Text = PowerCellInner[0].ToString() + " PC";
            }
            else if (buttonClicked == AouterDown)
            {
                PowerCellOuter[0]--;
                if (PowerCellOuter[0] <= 0)
                {
                    AouterDown.IsEnabled = false;
                }
                AouterAmount.Text = PowerCellOuter[0].ToString() + " PC";
            }
            else if (buttonClicked == AlowerDown)
            {
                PowerCellLower[0]--;
                if (PowerCellLower[0] <= 0)
                {
                    AlowerDown.IsEnabled = false;
                }
                AlowerAmount.Text = PowerCellLower[0].ToString() + " PC";
            }
            else if (buttonClicked == AmissedDown)
            {
                PowerCellMissed[0]--;
                if (PowerCellMissed[0] <= 0)
                {
                    AmissedDown.IsEnabled = false;
                }
                AmissedAmount.Text = PowerCellMissed[0].ToString() + " PC";
            }
            AinnerUp.IsEnabled = true;
            AouterUp.IsEnabled = true;
            AlowerUp.IsEnabled = true;
            AmissedUp.IsEnabled = true;
            totalAInnerPC.Text = PowerCellInner[0].ToString();
            totalAOuterPC.Text = PowerCellOuter[0].ToString();
            totalALowerPC.Text = PowerCellLower[0].ToString();
            totalAMissedPC.Text = PowerCellMissed[0].ToString();
        }
        private async void NextTeleOpCard(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (CurrentCycle < 20)
            {
                //trackingLogs.Add(SecondsScouting.ToString() + ":T:PC:" + CurrentCycle.ToString() + "," + PowerCellInner[CurrentCycle].ToString() + "," + PowerCellOuter[CurrentCycle].ToString() + "," + PowerCellLower[CurrentCycle].ToString() + "," + PowerCellMissed[CurrentCycle].ToString());
                CurrentCycle++;
                await cardToFlip.TranslateTo(cardToFlip.TranslationX + 10, cardToFlip.TranslationY, 175, Easing.CubicIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX - 510, cardToFlip.TranslationY, 175, Easing.CubicIn);
                cardToFlip.TranslationX = cardToFlip.TranslationX + 1000;
                CurrentCycleNumLabel.Text = "Tele-Op Cycle #" + CurrentCycle.ToString();
                innerAmount.Text = PowerCellInner[CurrentCycle].ToString() + " PC";
                outerAmount.Text = PowerCellOuter[CurrentCycle].ToString() + " PC";
                lowerAmount.Text = PowerCellLower[CurrentCycle].ToString() + " PC";
                missedAmount.Text = PowerCellMissed[CurrentCycle].ToString() + " PC";
                if (PowerCellMissed[CurrentCycle] <= 0)
                {
                    missedDown.IsEnabled = false;
                }
                else
                {
                    missedDown.IsEnabled = true;
                }
                if (PowerCellLower[CurrentCycle] <= 0)
                {
                    lowerDown.IsEnabled = false;
                }
                else
                {
                    lowerDown.IsEnabled = true;
                }
                if (PowerCellOuter[CurrentCycle] <= 0)
                {
                    outerDown.IsEnabled = false;
                }
                else
                {
                    outerDown.IsEnabled = true;
                }
                if (PowerCellInner[CurrentCycle] <= 0)
                {
                    innerDown.IsEnabled = false;
                }
                else
                {
                    innerDown.IsEnabled = true;
                }
                if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) >= 5)
                {
                    innerUp.IsEnabled = false;
                    outerUp.IsEnabled = false;
                    lowerUp.IsEnabled = false;
                    missedUp.IsEnabled = false;
                }
                else
                {
                    innerUp.IsEnabled = true;
                    outerUp.IsEnabled = true;
                    lowerUp.IsEnabled = true;
                    missedUp.IsEnabled = true;
                }
                if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 0)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 1)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                }
                await cardToFlip.TranslateTo(cardToFlip.TranslationX - 510, cardToFlip.TranslationY, 175, Easing.CubicOut);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX + 10, cardToFlip.TranslationY, 175, Easing.CubicOut);
                cardToFlip.TranslationX = 0;
                cardToFlip.TranslationY = 0;
            }
            else
            {
                await cardToFlip.TranslateTo(cardToFlip.TranslationX + 10, cardToFlip.TranslationY, 75, Easing.SinIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX - 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX + 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX - 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX + 10, cardToFlip.TranslationY, 75, Easing.SinIn);
                cardToFlip.TranslationX = 0;
                cardToFlip.TranslationY = 0;
            }

        }
        private async void PrevTeleOpCard(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (CurrentCycle > 1)
            {
                //trackingLogs.Add(SecondsScouting.ToString() + ":T:PC:" + CurrentCycle.ToString() + "," + PowerCellInner[CurrentCycle].ToString() + "," + PowerCellOuter[CurrentCycle].ToString() + "," + PowerCellLower[CurrentCycle].ToString() + "," + PowerCellMissed[CurrentCycle].ToString());
                CurrentCycle--;
                await cardToFlip.TranslateTo(cardToFlip.TranslationX - 10, cardToFlip.TranslationY, 175, Easing.CubicIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX + 510, cardToFlip.TranslationY, 175, Easing.CubicIn);
                cardToFlip.TranslationX = cardToFlip.TranslationX - 1000;
                CurrentCycleNumLabel.Text = "Tele-Op Cycle #" + CurrentCycle.ToString();
                innerAmount.Text = PowerCellInner[CurrentCycle].ToString() + " PC";
                outerAmount.Text = PowerCellOuter[CurrentCycle].ToString() + " PC";
                lowerAmount.Text = PowerCellLower[CurrentCycle].ToString() + " PC";
                missedAmount.Text = PowerCellMissed[CurrentCycle].ToString() + " PC";
                if (PowerCellMissed[CurrentCycle] <= 0)
                {
                    missedDown.IsEnabled = false;
                }
                else
                {
                    missedDown.IsEnabled = true;
                }
                if (PowerCellLower[CurrentCycle] <= 0)
                {
                    lowerDown.IsEnabled = false;
                }
                else
                {
                    lowerDown.IsEnabled = true;
                }
                if (PowerCellOuter[CurrentCycle] <= 0)
                {
                    outerDown.IsEnabled = false;
                }
                else
                {
                    outerDown.IsEnabled = true;
                }
                if (PowerCellInner[CurrentCycle] <= 0)
                {
                    innerDown.IsEnabled = false;
                }
                else
                {
                    innerDown.IsEnabled = true;
                }
                if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) >= 5)
                {
                    innerUp.IsEnabled = false;
                    outerUp.IsEnabled = false;
                    lowerUp.IsEnabled = false;
                    missedUp.IsEnabled = false;
                }
                else
                {
                    innerUp.IsEnabled = true;
                    outerUp.IsEnabled = true;
                    lowerUp.IsEnabled = true;
                    missedUp.IsEnabled = true;
                }
                if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 0)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 1)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
                {
                    pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                }
                await cardToFlip.TranslateTo(cardToFlip.TranslationX + 510, cardToFlip.TranslationY, 175, Easing.CubicOut);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX - 10, cardToFlip.TranslationY, 175, Easing.CubicOut);
                cardToFlip.TranslationX = 0;
                cardToFlip.TranslationY = 0;

            }
            else
            {
                await cardToFlip.TranslateTo(cardToFlip.TranslationX - 10, cardToFlip.TranslationY, 75, Easing.SinIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX + 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX - 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX + 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                await cardToFlip.TranslateTo(cardToFlip.TranslationX - 10, cardToFlip.TranslationY, 75, Easing.SinIn);
                cardToFlip.TranslationX = 0;
                cardToFlip.TranslationY = 0;
            }

        }
        private async void ConfirmForm(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            finishForm.IsEnabled = false;
            var continuetosubmission = true;
            endScoutingTime = DateTime.Now;
            //trackingLogs.Add(SecondsScouting.ToString() + ":T:PC:" + CurrentCycle.ToString() + "," + PowerCellInner[CurrentCycle].ToString() + "," + PowerCellOuter[CurrentCycle].ToString() + "," + PowerCellLower[CurrentCycle].ToString() + "," + PowerCellMissed[CurrentCycle].ToString());
            trackingLogs.Add(SecondsScouting.ToString() + ":2");
            TimeSpan amountOfTimeScouting = endScoutingTime - startScoutingTime;
            if (amountOfTimeScouting < TimeSpan.FromSeconds(150))
            {
                continuetosubmission = await DisplayAlert("You're too fast!", "A match is 150 seconds, and you have scouted this match for " + Math.Floor(amountOfTimeScouting.TotalSeconds).ToString() + " seconds! Are you sure you want to submit this match?", "Yes", "I'll Keep Scouting");
            }
            if (continuetosubmission)
            {


                var thismatch = new TeamMatch();
                thismatch.A_InitiationLine = InitLineAchieved;
                thismatch.DisabledSeconds = (int)Math.Floor(DisabledSeconds);
                thismatch.EventCode = "test_env";
                thismatch.E_Balanced = Balanced;
                thismatch.E_ClimbAttempt = Attempted;
                thismatch.E_ClimbSuccess = Climb;
                thismatch.E_Park = Parked;
                thismatch.MatchNumber = CurrentMatchNum;
                thismatch.NumCycles = NumCycles;
                thismatch.TapLogs = trackingLogs.ToArray();
                thismatch.PowerCellInner = PowerCellInner;
                thismatch.PowerCellLower = PowerCellLower;
                thismatch.PowerCellMissed = PowerCellMissed;
                thismatch.PowerCellOuter = PowerCellOuter;
                thismatch.ScoutName = scoutName.Text;
                thismatch.ClientLastSubmitted = DateTime.Now;
                thismatch.TabletId = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).TabletIdentifier;
                thismatch.TeamNumber = selectedMatch.TeamNumber;
                if (selectedMatch.TeamName != null)
                {
                    thismatch.TeamName = selectedMatch.TeamName;
                }
                else
                {
                    thismatch.TeamName = "FRC Team " + selectedMatch.TeamNumber.ToString();
                }
                thismatch.T_ControlPanelRotation = ControlPanel[0];
                thismatch.T_ControlPanelPosition = ControlPanel[1];
                thismatch.ClientSubmitted = true;
                DependencyService.Get<DataStore>().SaveData("JacksonEvent2020.txt", thismatch);
                if (Application.Current.Properties.ContainsKey("MatchesSubmitted"))
                {
                    Application.Current.Properties["MatchesSubmitted"] = ((int)Application.Current.Properties["MatchesSubmitted"]) + 1;
                    DependencyService.Get<DataStore>().SaveConfigurationFile("numMatches", Application.Current.Properties["MatchesSubmitted"]);
                }
                else
                {
                    var configurationfile = DependencyService.Get<DataStore>().LoadConfigFile();
                    try
                    {
                        LSConfiguration configFile = JsonConvert.DeserializeObject<LSConfiguration>(configurationfile);
                        Application.Current.Properties["MatchesSubmitted"] = configFile.NumberOfMatches + 1;
                        DependencyService.Get<DataStore>().SaveConfigurationFile("numMatches", Application.Current.Properties["MatchesSubmitted"]);
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Properties["MatchesSubmitted"] = 1;
                        DependencyService.Get<DataStore>().SaveConfigurationFile("numMatches", Application.Current.Properties["MatchesSubmitted"]);
                    }

                }
                int i = 0;
                bool taskcompleted = false;
                if (((int)Application.Current.Properties["MatchesSubmitted"] % 3) == 0 || JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).BluetoothFailureStage == 1)
                {
                    if (JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).BluetoothFailureStage < 2)
                    {
                        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                        CancellationToken token = cancellationTokenSource.Token;

                        var highestlevelresponse = 0;
                        MessagingCenter.Subscribe<SubmitVIABluetooth, int>(this, "boom", (messagesender, value) => {
                            switch (value)
                            {
                                case 1:
                                    savingProgress.ProgressTo(0.20, 1000, Easing.CubicInOut);
                                    if (highestlevelresponse < 1)
                                    {
                                        highestlevelresponse = 1;
                                    }
                                    break;
                                case 2:
                                    savingProgress.ProgressTo(0.60, 1000, Easing.CubicInOut);
                                    if (highestlevelresponse < 2)
                                    {
                                        highestlevelresponse = 2;
                                    }
                                    break;
                                case 3:
                                    savingProgress.ProgressTo(1, 1000, Easing.CubicInOut);
                                    DependencyService.Get<DataStore>().SaveConfigurationFile("bluetoothStage", 0);
                                    if (highestlevelresponse < 3)
                                    {
                                        highestlevelresponse = 3;
                                    }
                                    MessagingCenter.Unsubscribe<SubmitVIABluetooth, int>(this, "boom");
                                    break;
                                case -1:
                                    savingProgress.ProgressTo(0, 1000, Easing.CubicInOut);
                                    DependencyService.Get<DataStore>().SaveConfigurationFile("bluetoothStage", 1);
                                    if (JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).BluetoothFailureStage == 1)
                                    {
                                        DependencyService.Get<DataStore>().SaveConfigurationFile("bluetoothStage", 2);
                                        DisplayAlert("Something Went Wrong!", "We encountered an error trying to transmit your data to the host computer. We tried this twice and it failed both times. Please notify the scouter managing tablets soon!", "I'll Do That!");
                                    }
                                    MessagingCenter.Unsubscribe<SubmitVIABluetooth, int>(this, "boom");
                                    taskcompleted = true;
                                    break;
                            }
                        });
                        savingMessage.Text = "Sending to Computer...";
                        submittingFormToBluetooth.IsVisible = true;
                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            if (!taskcompleted)
                            {
                                if (i < 15)
                                {
                                    i++;
                                    if (i == 8)
                                    {
                                        cancellationTokenSource.Cancel();
                                    }
                                    return true;

                                }
                                else
                                {
                                    if (highestlevelresponse < 3)
                                    {
                                        
                                        DependencyService.Get<DataStore>().SaveConfigurationFile("bluetoothStage", 1);
                                    }
                                    Navigation.PushAsync(new MainPage());
                                    return false;
                                }

                            }
                            else
                            {
                                int j = 0;

                                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                                {
                                    if (j < 3)
                                    {
                                        j++;
                                        return true;
                                    }
                                    else
                                    {
                                        Navigation.PushAsync(new MainPage());
                                        return false;
                                    }


                                });
                                return false;
                            }

                        });
                        try
                        {
                            //var bluetoothclass = new SubmitVIABluetooth();
                            //await bluetoothclass.SubmitBluetooth(token);
                            await (Application.Current.Properties["BluetoothMethod"] as SubmitVIABluetooth).SubmitBluetooth(token);
                            adapter = CrossBluetoothLE.Current.Adapter;
                            if (adapter.ConnectedDevices.Count > 0)
                            {
                                taskcompleted = true;
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    else
                    {
                        savingProgress.ProgressTo(1, 1000, Easing.CubicInOut);
                        savingMessage.Text = "Saving to Database...";
                        submittingFormToBluetooth.IsVisible = true;
                        await DisplayAlert("Uh Oh", "It didn't work the first time, it probably won't work now. Please get the person in charge of the tablets to unlock the Bluetooth feature once they transfer the data VIA USB. We are saving the data to the tablet for now.", "Ok!");
                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            if (!taskcompleted)
                            {
                                if (i < 5)
                                {
                                    i++;
                                    return true;
                                }
                                else
                                {
                                    Navigation.PushAsync(new MainPage());
                                    return false;
                                }

                            }
                            else
                            {
                                Navigation.PushAsync(new MainPage());
                                return false;
                            }

                        });
                    }


                }
                else
                {
                    savingProgress.ProgressTo(1, 500, Easing.CubicInOut);
                    savingMessage.Text = "Saving to Database...";
                    submittingFormToBluetooth.IsVisible = true;
                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                    {
                        if (!taskcompleted)
                        {
                            if (i < 5)
                            {
                                i++;
                                return true;
                            }
                            else
                            {
                                Navigation.PushAsync(new MainPage());
                                return false;
                            }

                        }
                        else
                        {
                            Navigation.PushAsync(new MainPage());
                            return false;
                        }

                    });
                }
            }
            else
            {
                finishForm.IsEnabled = true;
            }





        }

        private void EnableDisabledMenu(object sender, EventArgs e)
        {
            disabledMenu.IsVisible = true;
            CurrentlyDisabled = true;
            DisabledTimer();
            trackingLogs.Add(SecondsScouting.ToString() + ":9000");

        }
        private void DisableDisabledMenu(object sender, EventArgs e)
        {

            CurrentlyDisabled = false;
            disabledMenu.IsVisible = false;
            trackingLogs.Add(SecondsScouting.ToString() + ":9001");

        }
        private void ChangeInitLine(object sender, EventArgs e)
        {
            InitLineAchieved = !InitLineAchieved;
            if (InitLineAchieved)
            {
                initLineToggle.Style = Resources["lightSecondarySelected"] as Style;
                initLineToggle.Text = "Achieved";
            }
            else
            {
                initLineToggle.Style = Resources["lightSecondary"] as Style;
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
                trackingLogs.Add(SecondsScouting.ToString() + ":1001");
                exitForm.IsVisible = true;
                prevForm.IsEnabled = false;
                autoForm.FadeTo(0, 250);
                autoForm.IsVisible = false;
                nameForm.IsVisible = true;
                nameForm.FadeTo(1, 250);
                toolTipLabel.Text = "Ready For Match";
            }
            else if (CurrentSubPage == 1)
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":1002");
                teleopForm.FadeTo(0, 250);
                teleopForm.IsVisible = false;
                autoForm.IsVisible = true;
                autoForm.FadeTo(1, 250);
                toolTipLabel.Text = "Autonomous";
            }
            else if (CurrentSubPage == 2)
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":1003");
                endgameForm.FadeTo(0, 250);
                endgameForm.IsVisible = false;
                teleopForm.IsVisible = true;
                teleopForm.FadeTo(1, 250);
                toolTipLabel.Text = "Tele-Op";
            }
            else if (CurrentSubPage == 3)
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":1004");
                confirmForm.FadeTo(0, 250);
                confirmForm.IsVisible = false;
                endgameForm.IsVisible = true;
                endgameForm.FadeTo(1, 250);
                toolTipLabel.Text = "Endgame";
            }
            ShowToolTip();
        }

        private async void nextForm_Clicked(object sender, EventArgs e)
        {
            CurrentSubPage++;
            exitForm.IsVisible = false;
            prevForm.IsEnabled = true;
            if (CurrentSubPage == 1)
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":1002");
                nameForm.FadeTo(0, 250);
                nameForm.IsVisible = false;
                autoForm.IsVisible = true;
                autoForm.FadeTo(1, 250);
                toolTipLabel.Text = "Autonomous";

            }
            else if (CurrentSubPage == 2)
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":1003");
                autoForm.FadeTo(0, 250);
                autoForm.IsVisible = false;
                teleopForm.IsVisible = true;
                teleopForm.FadeTo(1, 250);
                toolTipLabel.Text = "Tele-Op";
            }
            else if (CurrentSubPage == 3)
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":1004");
                teleopForm.FadeTo(0, 250);
                teleopForm.IsVisible = false;
                endgameForm.IsVisible = true;
                endgameForm.FadeTo(1, 250);
                toolTipLabel.Text = "Endgame";
            }
            else if (CurrentSubPage == 4)
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":1005");
                nextForm.IsEnabled = false;
                finishForm.IsVisible = true;
                endgameForm.FadeTo(0, 250);
                endgameForm.IsVisible = false;
                confirmForm.IsVisible = true;
                confirmForm.FadeTo(1, 250);
                toolTipLabel.Text = "Confirm Entry";
            }
            ShowToolTip();
        }
        private void DisabledTimer()
        {
            Device.StartTimer(TimeSpan.FromSeconds(.1), () =>
            {

                if (CurrentlyDisabled)
                {
                    StackLightCounter++;
                    DisabledSeconds += 0.1;
                    disabledSeconds.Text = ((int)Math.Floor(DisabledSeconds)).ToString() + "s";
                    if (StackLightCounter % 10 == 0)
                    {
                        OnSLLight.IsVisible = !OnSLLight.IsVisible;
                        OffSLLight.IsVisible = !OffSLLight.IsVisible;
                    }

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

        private async void SwipeToTeleOpCard(object sender, SwipedEventArgs e)
        {
            var converter = new ColorTypeConverter();
            if (e.Direction == SwipeDirection.Left)
            {
                if (CurrentCycle < 20)
                {
                    CurrentCycle++;
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX + 10, cardToFlip.TranslationY, 175, Easing.CubicOut);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX - 510, cardToFlip.TranslationY, 175, Easing.CubicOut);
                    cardToFlip.TranslationX = cardToFlip.TranslationX + 1000;
                    CurrentCycleNumLabel.Text = "Tele-Op Cycle #" + CurrentCycle.ToString();
                    innerAmount.Text = PowerCellInner[CurrentCycle].ToString() + " PC";
                    outerAmount.Text = PowerCellOuter[CurrentCycle].ToString() + " PC";
                    lowerAmount.Text = PowerCellLower[CurrentCycle].ToString() + " PC";
                    missedAmount.Text = PowerCellMissed[CurrentCycle].ToString() + " PC";
                    if (PowerCellMissed[CurrentCycle] <= 0)
                    {
                        missedDown.IsEnabled = false;
                    }
                    else
                    {
                        missedDown.IsEnabled = true;
                    }
                    if (PowerCellLower[CurrentCycle] <= 0)
                    {
                        lowerDown.IsEnabled = false;
                    }
                    else
                    {
                        lowerDown.IsEnabled = true;
                    }
                    if (PowerCellOuter[CurrentCycle] <= 0)
                    {
                        outerDown.IsEnabled = false;
                    }
                    else
                    {
                        outerDown.IsEnabled = true;
                    }
                    if (PowerCellInner[CurrentCycle] <= 0)
                    {
                        innerDown.IsEnabled = false;
                    }
                    else
                    {
                        innerDown.IsEnabled = true;
                    }
                    if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) >= 5)
                    {
                        innerUp.IsEnabled = false;
                        outerUp.IsEnabled = false;
                        lowerUp.IsEnabled = false;
                        missedUp.IsEnabled = false;
                    }
                    else
                    {
                        innerUp.IsEnabled = true;
                        outerUp.IsEnabled = true;
                        lowerUp.IsEnabled = true;
                        missedUp.IsEnabled = true;
                    }
                    if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 0)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 1)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    }
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX - 510, cardToFlip.TranslationY, 175, Easing.CubicOut);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX + 10, cardToFlip.TranslationY, 175, Easing.CubicOut);
                    cardToFlip.TranslationX = 0;
                    cardToFlip.TranslationY = 0;
                }
                else
                {
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX + 10, cardToFlip.TranslationY, 75, Easing.SinIn);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX - 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX + 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX - 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX + 10, cardToFlip.TranslationY, 75, Easing.SinIn);
                    cardToFlip.TranslationX = 0;
                    cardToFlip.TranslationY = 0;
                }
            }
            else
            {
                if (CurrentCycle > 1)
                {
                    CurrentCycle--;
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX - 10, cardToFlip.TranslationY, 175, Easing.CubicIn);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX + 510, cardToFlip.TranslationY, 175, Easing.CubicIn);
                    cardToFlip.TranslationX = cardToFlip.TranslationX - 1000;
                    CurrentCycleNumLabel.Text = "Tele-Op Cycle #" + CurrentCycle.ToString();
                    innerAmount.Text = PowerCellInner[CurrentCycle].ToString() + " PC";
                    outerAmount.Text = PowerCellOuter[CurrentCycle].ToString() + " PC";
                    lowerAmount.Text = PowerCellLower[CurrentCycle].ToString() + " PC";
                    missedAmount.Text = PowerCellMissed[CurrentCycle].ToString() + " PC";
                    if (PowerCellMissed[CurrentCycle] <= 0)
                    {
                        missedDown.IsEnabled = false;
                    }
                    else
                    {
                        missedDown.IsEnabled = true;
                    }
                    if (PowerCellLower[CurrentCycle] <= 0)
                    {
                        lowerDown.IsEnabled = false;
                    }
                    else
                    {
                        lowerDown.IsEnabled = true;
                    }
                    if (PowerCellOuter[CurrentCycle] <= 0)
                    {
                        outerDown.IsEnabled = false;
                    }
                    else
                    {
                        outerDown.IsEnabled = true;
                    }
                    if (PowerCellInner[CurrentCycle] <= 0)
                    {
                        innerDown.IsEnabled = false;
                    }
                    else
                    {
                        innerDown.IsEnabled = true;
                    }
                    if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) >= 5)
                    {
                        innerUp.IsEnabled = false;
                        outerUp.IsEnabled = false;
                        lowerUp.IsEnabled = false;
                        missedUp.IsEnabled = false;
                    }
                    else
                    {
                        innerUp.IsEnabled = true;
                        outerUp.IsEnabled = true;
                        lowerUp.IsEnabled = true;
                        missedUp.IsEnabled = true;
                    }
                    if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 0)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 1)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
                    {
                        pcStock5.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightGray");
                    }
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX + 510, cardToFlip.TranslationY, 175, Easing.CubicOut);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX - 10, cardToFlip.TranslationY, 175, Easing.CubicOut);
                    cardToFlip.TranslationX = 0;
                    cardToFlip.TranslationY = 0;

                }
                else
                {
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX - 10, cardToFlip.TranslationY, 75, Easing.SinIn);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX + 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX - 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX + 20, cardToFlip.TranslationY, 75, Easing.SinIn);
                    await cardToFlip.TranslateTo(cardToFlip.TranslationX - 10, cardToFlip.TranslationY, 75, Easing.SinIn);
                    cardToFlip.TranslationX = 0;
                    cardToFlip.TranslationY = 0;
                }
            }

        }


        private void resetDisabled_Clicked(object sender, EventArgs e)
        {
            DisabledSeconds = 0;
            disabledSeconds.Text = "0s";
        }
    }
}