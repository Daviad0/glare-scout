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
        private int[] ShotCoordinates = { 0, 0 };
        private Button oldSelectedShotButton;
        private int[] LoadCoordinates = { 0, 0 };
        private Button oldSelectedLoadButton;
        private int NumCycles = 0;
        private int TotalCycleTime = 0;
        private DateTime lastRecordedCycle;
        private int CurrentCycle = 1;
        private int CurrentSubPage;
        private TeamMatch selectedMatch;
        private double DisabledSeconds;
        private int StackLightCounter;
        private bool DefenseFor;
        private bool DefenseAgainst;
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
            var listofscouts = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).ScouterNames.ToList();
            scouterPicker.ItemsSource = listofscouts;
            if (matchTemplate.ScoutName != null)
            {
                try
                {
                    scouterPicker.SelectedIndex = listofscouts.IndexOf(matchTemplate.ScoutName);
                }
                catch (Exception ex)
                {

                }

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
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock1.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
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
            if (scouterPicker.SelectedItem != null)
            {
                if (scouterPicker.SelectedItem.ToString() != null || scouterPicker.SelectedItem.ToString() != "")
                {
                    nextForm.Opacity = 0;
                    nextForm.IsVisible = true;
                    nextForm.FadeTo(1, 500, Easing.CubicIn);
                    enableDisabled.Opacity = 0;
                    enableDisabled.IsVisible = true;
                    enableDisabled.FadeTo(1, 500, Easing.CubicIn);
                }
            }


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
                    trackingLogs.Add(SecondsScouting.ToString() + ":3010");
                }
                else
                {
                    cp_lv1.Style = Resources["lightSecondary"] as Style;
                    trackingLogs.Add(SecondsScouting.ToString() + ":3011");
                }

            }
            if (sender == cp_lv2)
            {
                ControlPanel[1] = !ControlPanel[1];
                if (ControlPanel[1])
                {
                    cp_lv2.Style = Resources["lightSecondarySelected"] as Style;
                    trackingLogs.Add(SecondsScouting.ToString() + ":3020");
                }
                else
                {
                    cp_lv2.Style = Resources["lightSecondary"] as Style;
                    trackingLogs.Add(SecondsScouting.ToString() + ":3021");
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
                trackingLogs.Add(SecondsScouting.ToString() + ":4031");
            }
            else
            {
                Balanced = true;
                balanced_opt1.Style = Resources["lightSecondarySelected"] as Style;
                balanced_opt1.Text = "Robot(s) Balanced";
                trackingLogs.Add(SecondsScouting.ToString() + ":4030");
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
                trackingLogs.Add(SecondsScouting.ToString() + ":4021");
                trackingLogs.Add(SecondsScouting.ToString() + ":4031");
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
                trackingLogs.Add(SecondsScouting.ToString() + ":4020");
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
                trackingLogs.Add(SecondsScouting.ToString() + ":4001");
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
                trackingLogs.Add(SecondsScouting.ToString() + ":4000");
                trackingLogs.Add(SecondsScouting.ToString() + ":4021");
                trackingLogs.Add(SecondsScouting.ToString() + ":4031");
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
                trackingLogs.Add(SecondsScouting.ToString() + ":4011");
                trackingLogs.Add(SecondsScouting.ToString() + ":4021");
                trackingLogs.Add(SecondsScouting.ToString() + ":4031");
            }
            else
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":4010");
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

                if (CurrentCycle > 1)
                {
                    TotalCycleTime += (int)(DateTime.Now - lastRecordedCycle).TotalSeconds;
                }
                lastRecordedCycle = DateTime.Now;
                NumCycles = CurrentCycle;
                trackingLogs.Add(SecondsScouting.ToString() + ":3000");
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
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock1.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 1)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
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
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            }
            else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
            {
                pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                pcStock1.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
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
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
                {
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
                {
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
                {
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
                {
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock1.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
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
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
                {
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
                {
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
                {
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                }
                else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
                {
                    pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    pcStock1.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
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
        private async void ChangeDefenseFor(object sender, EventArgs e)
        {
            DefenseFor = !DefenseFor;
            if (DefenseFor)
            {
                deffor_opt1.Style = Resources["lightSecondarySelected"] as Style;
                deffor_opt1.Text = "Yes";
                //trackingLogs.Add(SecondsScouting.ToString() + ":2000");*
                trackingLogs.Add(SecondsScouting.ToString() + ":8010:");
            }
            else
            {
                deffor_opt1.Style = Resources["lightSecondary"] as Style;
                deffor_opt1.Text = "No";
                //trackingLogs.Add(SecondsScouting.ToString() + ":2001");
                trackingLogs.Add(SecondsScouting.ToString() + ":8011:");
            }
        }
        private async void ChangeDefenseAgainst(object sender, EventArgs e)
        {
            DefenseAgainst = !DefenseAgainst;
            if (DefenseAgainst)
            {
                defagn_opt1.Style = Resources["lightSecondarySelected"] as Style;
                defagn_opt1.Text = "Yes";
                //trackingLogs.Add(SecondsScouting.ToString() + ":2000");
                trackingLogs.Add(SecondsScouting.ToString() + ":8020:");
            }
            else
            {
                defagn_opt1.Style = Resources["lightSecondary"] as Style;
                defagn_opt1.Text = "No";
                //trackingLogs.Add(SecondsScouting.ToString() + ":2001");
                trackingLogs.Add(SecondsScouting.ToString() + ":8021:");
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
                if (continuetosubmission) trackingLogs.Add(SecondsScouting.ToString() + ":10");
            }
            if (continuetosubmission)
            {


                var thismatch = new TeamMatch();
                if (NumCycles > 0)
                {
                    thismatch.CycleTime = TotalCycleTime / NumCycles;
                }

                thismatch.AlliancePartners = selectedMatch.AlliancePartners;
                thismatch.A_InitiationLine = InitLineAchieved;
                thismatch.DisabledSeconds = (int)Math.Floor(DisabledSeconds);
                if (selectedMatch.EventCode != null && selectedMatch.EventCode != "")
                {
                    thismatch.EventCode = selectedMatch.EventCode;
                }
                else
                {
                    thismatch.EventCode = JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).CurrentEventCode;
                }

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
                thismatch.LoadCoordinates = LoadCoordinates;
                thismatch.ShotCoordinates = ShotCoordinates;
                thismatch.DefenseFor = DefenseFor;
                thismatch.DefenseAgainst = DefenseAgainst;
                thismatch.ScoutName = scouterPicker.SelectedItem.ToString();
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
                        var argumentsToUse = new SubmitVIABluetooth.BLEMessageArguments() { messageType = 10, messageData = DependencyService.Get<DataStore>().LoadData("JacksonEvent2020.txt"), expectation = SubmitVIABluetooth.ResponseExpectation.Optional };
                        var highestlevelresponse = 0;
                        MessagingCenter.Subscribe<SubmitVIABluetooth, BluetoothControllerData>(this, "bluetoothController", async (mssender, value) =>
                        {
                            switch (value.status)
                            {
                                case BluetoothControllerDataStatus.Initialize:
                                    if (highestlevelresponse < 1)
                                    {
                                        highestlevelresponse = 1;
                                    }
                                    break;
                                case BluetoothControllerDataStatus.Connected:
                                    if (highestlevelresponse < 2)
                                    {
                                        highestlevelresponse = 2;
                                    }
                                    break;
                                case BluetoothControllerDataStatus.DataSent:
                                    DependencyService.Get<DataStore>().SaveConfigurationFile("bluetoothStage", 0);
                                    if (highestlevelresponse < 3)
                                    {
                                        highestlevelresponse = 3;
                                    }
                                    if (argumentsToUse.expectation == SubmitVIABluetooth.ResponseExpectation.NoResponse)
                                    {
                                        MessagingCenter.Unsubscribe<SubmitVIABluetooth, int>(this, "bluetoothController");
                                    }
                                    break;
                                case BluetoothControllerDataStatus.DataGet:
                                    DependencyService.Get<DataStore>().SaveConfigurationFile("bluetoothStage", 0);
                                    if (highestlevelresponse < 3)
                                    {
                                        highestlevelresponse = 3;
                                    }
                                    var listofmatches = JsonConvert.DeserializeObject<List<TeamMatch>>(value.data);
                                    Console.WriteLine(listofmatches);
                                    MessagingCenter.Unsubscribe<SubmitVIABluetooth, int>(this, "bluetoothController");
                                    break;
                                case BluetoothControllerDataStatus.Abort:
                                    DependencyService.Get<DataStore>().SaveConfigurationFile("bluetoothStage", 1);
                                    if (JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).BluetoothFailureStage == 1)
                                    {
                                        DependencyService.Get<DataStore>().SaveConfigurationFile("bluetoothStage", 2);
                                        DisplayAlert("Something Went Wrong!", "We encountered an error trying to transmit your data to the host computer. We tried this twice and it failed both times. Please notify the scouter managing tablets soon!", "I'll Do That!");
                                    }
                                    MessagingCenter.Unsubscribe<SubmitVIABluetooth, int>(this, "bluetoothController");
                                    break;
                            }
                        });
                        savingMessage.Text = "Sending to Computer...";
                        submittingOverlayPanel.TranslationX = 600;
                        await Task.Delay(15);
                        submittingOverlay.IsVisible = true;
                        submittingOverlayPanel.TranslateTo(submittingOverlayPanel.TranslationX - 600, submittingOverlayPanel.TranslationY, 500, Easing.CubicInOut);
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
                                    waitingWithSubmit.IsVisible = false;
                                    doneWithSubmit.IsVisible = true;
                                    int j = 0;
                                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                                    {
                                        if (j < 3)
                                        {
                                            if (j == 2)
                                            {
                                                submittingOverlayPanel.TranslateTo(submittingOverlayPanel.TranslationX - 600, submittingOverlayPanel.TranslationY, 500, Easing.CubicInOut);
                                            }
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

                            }
                            else
                            {
                                int j = 0;
                                waitingWithSubmit.IsVisible = false;
                                doneWithSubmit.IsVisible = true;
                                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                                {
                                    if (j < 3)
                                    {
                                        if (j == 2)
                                        {
                                            submittingOverlayPanel.TranslateTo(submittingOverlayPanel.TranslationX - 600, submittingOverlayPanel.TranslationY, 500, Easing.CubicInOut);
                                        }
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
                            
                            await (Application.Current.Properties["BluetoothMethod"] as SubmitVIABluetooth).ConnectToDevice(argumentsToUse,token);
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
                        submittingOverlayPanel.TranslationX = 600;
                        await Task.Delay(15);
                        submittingOverlay.IsVisible = true;
                        submittingOverlayPanel.TranslateTo(submittingOverlayPanel.TranslationX - 600, submittingOverlayPanel.TranslationY, 500, Easing.CubicInOut);
                        await DisplayAlert("Uh Oh", "We are unable to connect to Bluetooth. Please notify the person in charge of tablet scouting!", "Ok!");
                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            if (!taskcompleted)
                            {
                                if (i < 5)
                                {
                                    if (i == 4)
                                    {
                                        submittingOverlayPanel.TranslateTo(submittingOverlayPanel.TranslationX - 600, submittingOverlayPanel.TranslationY, 500, Easing.CubicInOut);
                                    }
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
                                int j = 3;
                                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                                {
                                    if (j < 3)
                                    {
                                        if (j == 2)
                                        {
                                            submittingOverlayPanel.TranslateTo(submittingOverlayPanel.TranslationX - 600, submittingOverlayPanel.TranslationY, 500, Easing.CubicInOut);
                                        }
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
                    }


                }
                else
                {
                    savingProgress.ProgressTo(1, 500, Easing.CubicInOut);
                    savingMessage.Text = "Saving to Database...";
                    submittingOverlayPanel.TranslationX = 600;
                    await Task.Delay(15);
                    submittingOverlay.IsVisible = true;
                    submittingOverlayPanel.TranslateTo(submittingOverlayPanel.TranslationX - 600, submittingOverlayPanel.TranslationY, 500, Easing.CubicInOut);
                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                    {
                        if (!taskcompleted)
                        {
                            if (i < 5)
                            {
                                if (i == 2)
                                {
                                    waitingWithSubmit.IsVisible = false;
                                    doneWithSubmit.IsVisible = true;
                                }
                                i++;
                                if (i == 5)
                                {
                                    submittingOverlayPanel.TranslateTo(submittingOverlayPanel.TranslationX - 600, submittingOverlayPanel.TranslationY, 500, Easing.CubicInOut);
                                }
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
                            int j = 3;
                            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                            {
                                if (j < 3)
                                {
                                    if (j == 2)
                                    {
                                        submittingOverlayPanel.TranslateTo(submittingOverlayPanel.TranslationX - 600, submittingOverlayPanel.TranslationY, 500, Easing.CubicInOut);
                                    }
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
                }
            }
            else
            {
                finishForm.IsEnabled = true;
            }





        }

        private void EnableDisabledMenu(object sender, EventArgs e)
        {
            disabledMenuPanel.TranslationX = 600;
            disabledMenu.IsVisible = true;
            disabledMenuPanel.TranslateTo(disabledMenuPanel.TranslationX - 600, disabledMenuPanel.TranslationY, 500, Easing.CubicInOut);
            CurrentlyDisabled = true;
            DisabledTimer();
            trackingLogs.Add(SecondsScouting.ToString() + ":9000");

        }
        private async void DisableDisabledMenu(object sender, EventArgs e)
        {

            CurrentlyDisabled = false;

            await disabledMenuPanel.TranslateTo(disabledMenuPanel.TranslationX - 600, disabledMenuPanel.TranslationY, 500, Easing.CubicInOut);
            disabledMenu.IsVisible = false;
            disabledMenuPanel.TranslationX = 0;
            trackingLogs.Add(SecondsScouting.ToString() + ":9001");

        }
        private void ShowChangeShot(object sender, EventArgs e)
        {
            changeShotLocationPanel.TranslationX = 600;
            changeShotLocationOverlay.IsVisible = true;
            changeShotLocationPanel.TranslateTo(changeShotLocationPanel.TranslationX - 600, changeShotLocationPanel.TranslationY, 500, Easing.CubicInOut);
            CurrentlyDisabled = true;
            DisabledTimer();


        }
        private async void HideChangeShot(object sender, EventArgs e)
        {

            CurrentlyDisabled = false;

            await changeShotLocationPanel.TranslateTo(changeShotLocationPanel.TranslationX - 600, changeShotLocationPanel.TranslationY, 500, Easing.CubicInOut);
            changeShotLocationOverlay.IsVisible = false;
            changeShotLocationPanel.TranslationX = 0;


        }
        private void ShowChangeLoad(object sender, EventArgs e)
        {
            changeLoadLocationPanel.TranslationX = 600;
            changeLoadLocationOverlay.IsVisible = true;
            changeLoadLocationPanel.TranslateTo(changeLoadLocationPanel.TranslationX - 600, changeLoadLocationPanel.TranslationY, 500, Easing.CubicInOut);
            CurrentlyDisabled = true;
            DisabledTimer();


        }
        private async void HideChangeLoad(object sender, EventArgs e)
        {

            CurrentlyDisabled = false;

            await changeLoadLocationPanel.TranslateTo(changeLoadLocationPanel.TranslationX - 600, changeLoadLocationPanel.TranslationY, 500, Easing.CubicInOut);
            changeLoadLocationOverlay.IsVisible = false;
            changeLoadLocationPanel.TranslationX = 0;


        }
        private void ChangeInitLine(object sender, EventArgs e)
        {
            InitLineAchieved = !InitLineAchieved;
            if (InitLineAchieved)
            {
                initLineToggle.Style = Resources["lightSecondarySelected"] as Style;
                initLineToggle.Text = "Achieved";
                trackingLogs.Add(SecondsScouting.ToString() + ":2000");
            }
            else
            {
                initLineToggle.Style = Resources["lightSecondary"] as Style;
                initLineToggle.Text = "Not Achieved";
                trackingLogs.Add(SecondsScouting.ToString() + ":2001");
            }
        }
        private async void Button_Clicked(object sender, EventArgs e)
        {

            bool answer = await DisplayAlert("Hol' Up", "Are you sure that you want to cancel tracking this entry. Your progress will not be saved!", "Exit", "Cancel");
            if (answer)
            {
                Navigation.PushAsync(new MainPage());
            }

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
                afterMatchForm.FadeTo(0, 250);
                afterMatchForm.IsVisible = false;
                endgameForm.IsVisible = true;
                endgameForm.FadeTo(1, 250);
                toolTipLabel.Text = "Endgame";
            }
            else if (CurrentSubPage == 4)
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":1005");
                confirmForm.FadeTo(0, 250);
                confirmForm.IsVisible = false;
                afterMatchForm.IsVisible = true;
                afterMatchForm.FadeTo(1, 250);
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
                endgameForm.FadeTo(0, 250);
                endgameForm.IsVisible = false;
                afterMatchForm.IsVisible = true;
                afterMatchForm.FadeTo(1, 250);
                toolTipLabel.Text = "After Match";
            }
            else if (CurrentSubPage == 5)
            {
                trackingLogs.Add(SecondsScouting.ToString() + ":1006");
                nextForm.IsEnabled = false;
                finishForm.IsVisible = true;
                afterMatchForm.FadeTo(0, 250);
                afterMatchForm.IsVisible = false;
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

        private async void SwipeToTeleOpCard(bool Left)
        {
            var converter = new ColorTypeConverter();
            if (Left)
            {
                if (CurrentCycle < 20)
                {
                    CurrentCycle++;
                   
                    cardToFlip.TranslationX = 400;
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
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
                    {
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
                    {
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
                    {
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
                    {
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock1.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    }
                    await cardToFlip.TranslateTo(0, cardToFlip.TranslationY, 175, Easing.CubicInOut);
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
                    
                    cardToFlip.TranslationX = -400;
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
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 2)
                    {
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock3.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 3)
                    {
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock2.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 4)
                    {
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock1.BackgroundColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
                    }
                    else if ((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) == 5)
                    {
                        pcStock5.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock4.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock3.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock2.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                        pcStock1.BackgroundColor = (Color)App.Current.Resources["PCFillBack"];
                    }
                    await cardToFlip.TranslateTo(0, cardToFlip.TranslationY, 175, Easing.CubicInOut);
                    
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

        private void scoutName_Completed(object sender, EventArgs e)
        {
            if (!nextForm.IsVisible)
            {
                nextForm.Opacity = 0;
                nextForm.IsVisible = true;
                nextForm.FadeTo(1, 500, Easing.CubicIn);
                enableDisabled.Opacity = 0;
                enableDisabled.IsVisible = true;
                enableDisabled.FadeTo(1, 500, Easing.CubicIn);
            }

            trackingLogs.Add(SecondsScouting.ToString() + ":100:" + scouterPicker.SelectedItem.ToString());
        }
        private async void SelectShotFieldPosition(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            Button selectedButton = (Button)sender as Button;
            string rawclassid = selectedButton.ClassId;
            ShotCoordinates[0] = int.Parse(rawclassid.Split('-')[0]);
            ShotCoordinates[1] = int.Parse(rawclassid.Split('-')[1]);
            if (oldSelectedShotButton != null)
            {
                oldSelectedShotButton.FadeTo(0.1, easing: Easing.CubicInOut);
                
            }
            await selectedButton.FadeTo(0.75, easing: Easing.CubicInOut);
            if(oldSelectedShotButton != null)
            {
                oldSelectedShotButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Transparent");
                oldSelectedShotButton.TextColor = (Color)converter.ConvertFromInvariantString("Transparent");
                oldSelectedShotButton.BorderColor = (Color)converter.ConvertFromInvariantString("Transparent");
            }
            selectedButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightBlue");
            selectedButton.TextColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            selectedButton.BorderColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            oldSelectedShotButton = selectedButton;
            await currentShotLocation.FadeTo(0, 200, Easing.CubicInOut);
            switch (rawclassid)
            {
                case "1-1":
                    currentShotLocation.Text = "Power Port Close Right";
                    break;
                case "1-2":
                    currentShotLocation.Text = "Close Initiation Line";
                    break;
                case "1-3":
                    currentShotLocation.Text = "Far Initiation Line";
                    break;
                case "1-4":
                    currentShotLocation.Text = "Close End of Trench";
                    break;
                case "1-5":
                    currentShotLocation.Text = "Middle of Trench";
                    break;
                case "1-6":
                    currentShotLocation.Text = "Middle of Trench";
                    break;
                case "1-7":
                    currentShotLocation.Text = "Far Trench";
                    break;
                case "1-8":
                    currentShotLocation.Text = "Behind Trench";
                    break;
                case "2-1":
                    currentShotLocation.Text = "Power Port Close Center";
                    break;
                case "2-2":
                    currentShotLocation.Text = "Close Initiation Line";
                    break;
                case "2-3":
                    currentShotLocation.Text = "Far Initiation Line";
                    break;
                case "2-4":
                    currentShotLocation.Text = "Next to Rendezvoux";
                    break;
                case "2-5":
                    currentShotLocation.Text = "Next to Rendezvoux";
                    break;
                case "2-6":
                    currentShotLocation.Text = "Against Rendezvoix Bar";
                    break;
                case "2-7":
                    currentShotLocation.Text = "Behind Rendezvoix";
                    break;
                case "2-8":
                    currentShotLocation.Text = "Behind Rendezvoix";
                    break;
                case "3-1":
                    currentShotLocation.Text = "Power Port Close Left";
                    break;
                case "3-2":
                    currentShotLocation.Text = "Close Initiation Line";
                    break;
                case "3-3":
                    currentShotLocation.Text = "Far Initiation Line";
                    break;
                case "3-4":
                    currentShotLocation.Text = "Against Rendezvoix Bar";
                    break;
                case "3-5":
                    currentShotLocation.Text = "In Rendezvoix";
                    break;
                case "3-6":
                    currentShotLocation.Text = "In Rendezvoix";
                    break;
                case "3-7":
                    currentShotLocation.Text = "Behind Rendezvoix";
                    break;
                case "3-8":
                    currentShotLocation.Text = "Behind Rendezvoix";
                    break;
                case "4-1":
                    currentShotLocation.Text = "Enemy Loading Bay Close";
                    break;
                case "4-2":
                    currentShotLocation.Text = "Enemy Loading Bay Counter Defense";
                    break;
                case "4-3":
                    currentShotLocation.Text = "Enemy Loading Bay Counter Defense";
                    break;
                case "4-4":
                    currentShotLocation.Text = "Next to Rendezvoux";
                    break;
                case "4-5":
                    currentShotLocation.Text = "In Rendezvoix";
                    break;
                case "4-6":
                    currentShotLocation.Text = "In Rendezvoix";
                    break;
                case "4-7":
                    currentShotLocation.Text = "Behind Rendezvoix";
                    break;
                case "4-8":
                    currentShotLocation.Text = "Behind Rendezvoix";
                    break;
                case "5-1":
                    currentShotLocation.Text = "Enemy Loading Bay Close";
                    break;
                case "5-2":
                    currentShotLocation.Text = "Enemy Loading Bay";
                    break;
                case "5-3":
                    currentShotLocation.Text = "Enemy Loading Bay Counter Defense";
                    break;
                case "5-4":
                    currentShotLocation.Text = "Next to Rendezvoux";
                    break;
                case "5-5":
                    currentShotLocation.Text = "Against Rendezvoix Bar";
                    break;
                case "5-6":
                    currentShotLocation.Text = "In Rendezvoix";
                    break;
                case "5-7":
                    currentShotLocation.Text = "Behind Rendezvoix";
                    break;
                case "5-8":
                    currentShotLocation.Text = "Behind Rendezvoix";
                    break;
                case "6-1":
                    currentShotLocation.Text = "Enemy Loading Bay Corner";
                    break;
                case "6-2":
                    currentShotLocation.Text = "Enemy Loading Bay Edge";
                    break;
                case "6-3":
                    currentShotLocation.Text = "Enemy Loading Bay Edge";
                    break;
                case "6-4":
                    currentShotLocation.Text = "Enemy Close End of Trench";
                    break;
                case "6-5":
                    currentShotLocation.Text = "Enemy Mid of Trench";
                    break;
                case "6-6":
                    currentShotLocation.Text = "Enemy Mid of Trench";
                    break;
                case "6-7":
                    currentShotLocation.Text = "Enemy Far Trench";
                    break;
                case "6-8":
                    currentShotLocation.Text = "Behind Enemy Trench";
                    break;
            }
            await currentShotLocation.FadeTo(1, 200, Easing.CubicInOut);
            locsho_opt1.Style = Resources["lightSecondarySelected"] as Style;
            locsho_opt1.Text = currentShotLocation.Text;
            trackingLogs.Add(SecondsScouting.ToString() + ":8000:" + currentLoadLocation.Text);
        }
        private async void SelectLoadFieldPosition(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            Button selectedButton = (Button)sender as Button;
            string rawclassid = selectedButton.ClassId;
            LoadCoordinates[0] = int.Parse(rawclassid.Split('-')[0]);
            LoadCoordinates[1] = int.Parse(rawclassid.Split('-')[1]);
            if (oldSelectedLoadButton != null)
            {
                oldSelectedLoadButton.FadeTo(0.1, easing: Easing.CubicInOut);

            }
            selectedButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.LightBlue");
            selectedButton.TextColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            selectedButton.BorderColor = (Color)converter.ConvertFromInvariantString("#2a7afa");
            await selectedButton.FadeTo(0.75, easing: Easing.CubicInOut);
            if (oldSelectedLoadButton != null)
            {
                oldSelectedLoadButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Transparent");
                oldSelectedLoadButton.TextColor = (Color)converter.ConvertFromInvariantString("Transparent");
                oldSelectedLoadButton.BorderColor = (Color)converter.ConvertFromInvariantString("Transparent");
            }
            
            oldSelectedLoadButton = selectedButton;
            await currentLoadLocation.FadeTo(0, 200, Easing.CubicInOut);
            switch (rawclassid)
            {
                case "1-1":
                    currentLoadLocation.Text = "Clean Up";
                    break;
                case "1-2":
                    currentLoadLocation.Text = "Clean Up";
                    break;
                case "1-3":
                    currentLoadLocation.Text = "Clean Up";
                    break;
                case "1-4":
                    currentLoadLocation.Text = "Trench";
                    break;
                case "1-5":
                    currentLoadLocation.Text = "Trench";
                    break;
                case "1-6":
                    currentLoadLocation.Text = "Trench";
                    break;
                case "1-7":
                    currentLoadLocation.Text = "Trench";
                    break;
                case "1-8":
                    currentLoadLocation.Text = "Trench";
                    break;
                case "1-9":
                    currentLoadLocation.Text = "Loading Bay";
                    break;
                case "1-10":
                    currentLoadLocation.Text = "Loading Bay";
                    break;
                case "2-1":
                    currentLoadLocation.Text = "Clean Up";
                    break;
                case "2-2":
                    currentLoadLocation.Text = "Clean Up";
                    break;
                case "2-3":
                    currentLoadLocation.Text = "Clean Up";
                    break;
                case "2-4":
                    currentLoadLocation.Text = "Rendezvoux";
                    break;
                case "2-5":
                    currentLoadLocation.Text = "Rendezvoux";
                    break;
                case "2-6":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "2-7":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "2-8":
                    currentLoadLocation.Text = "Loading Bay";
                    break;
                case "2-9":
                    currentLoadLocation.Text = "Loading Bay";
                    break;
                case "2-10":
                    currentLoadLocation.Text = "Loading Bay";
                    break;
                case "3-1":
                    currentLoadLocation.Text = "Clean Up";
                    break;
                case "3-2":
                    currentLoadLocation.Text = "Clean Up";
                    break;
                case "3-3":
                    currentLoadLocation.Text = "Clean Up";
                    break;
                case "3-4":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "3-5":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "3-6":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "3-7":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "3-8":
                    currentLoadLocation.Text = "Loading Bay";
                    break;
                case "3-9":
                    currentLoadLocation.Text = "Loading Bay";
                    break;                      
                case "3-10":                    
                    currentLoadLocation.Text = "Loading Bay";
                    break;
                case "4-1":
                    currentLoadLocation.Text = "Enemy Loading Bay";
                    break;
                case "4-2":
                    currentLoadLocation.Text = "Enemy Loading Bay";
                    break;
                case "4-3":
                    currentLoadLocation.Text = "Enemy Loading Bay";
                    break;
                case "4-4":
                    currentLoadLocation.Text = "Rendezvoux";
                    break;
                case "4-5":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "4-6":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "4-7":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "4-8":
                    currentLoadLocation.Text = "Enemy Clean Up";
                    break;
                case "4-9":
                    currentLoadLocation.Text = "Enemy Clean Up";
                    break;
                case "4-10":
                    currentLoadLocation.Text = "Enemy Clean Up";
                    break;
                case "5-1":
                    currentLoadLocation.Text = "Enemy Loading Bay";
                    break;
                case "5-2":
                    currentLoadLocation.Text = "Enemy Loading Bay";
                    break;
                case "5-3":
                    currentLoadLocation.Text = "Enemy Loading Bay";
                    break;
                case "5-4":
                    currentLoadLocation.Text = "Rendezvoux";
                    break;
                case "5-5":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "5-6":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "5-7":
                    currentLoadLocation.Text = "Rendezvoix";
                    break;
                case "5-8":
                    currentLoadLocation.Text = "Enemy Clean Up";
                    break;
                case "5-9":
                    currentLoadLocation.Text = "Enemy Clean Up";
                    break;
                case "5-10":
                    currentLoadLocation.Text = "Enemy Clean Up";
                    break;
                case "6-1":
                    currentLoadLocation.Text = "Enemy Loading Bay";
                    break;
                case "6-2":
                    currentLoadLocation.Text = "Enemy Loading Bay";
                    break;
                case "6-3":
                    currentLoadLocation.Text = "Enemy Loading Bay";
                    break;
                case "6-4":
                    currentLoadLocation.Text = "Enemy Trench";
                    break;
                case "6-5":
                    currentLoadLocation.Text = "Enemy Trench";
                    break;
                case "6-6":
                    currentLoadLocation.Text = "Enemy Trench";
                    break;
                case "6-7":
                    currentLoadLocation.Text = "Enemy Trench";
                    break;
                case "6-8":
                    currentLoadLocation.Text = "Enemy Clean Up";
                    break;
                case "6-9":
                    currentLoadLocation.Text = "Enemy Clean Up";
                    break;
                case "6-10":
                    currentLoadLocation.Text = "Enemy Clean Up";
                    break;
            }
            await currentLoadLocation.FadeTo(1, 200, Easing.CubicInOut);
            locloa_opt1.Style = Resources["lightSecondarySelected"] as Style;
            locloa_opt1.Text = currentLoadLocation.Text;
            trackingLogs.Add(SecondsScouting.ToString() + ":8001:" + currentLoadLocation.Text);
        }
        private async void ResetShotLocation(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            LoadCoordinates[0] = 0;
            LoadCoordinates[1] = 0;
            if (oldSelectedShotButton != null)
            {
                await oldSelectedShotButton.FadeTo(0.1, easing: Easing.CubicInOut);
                oldSelectedShotButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Transparent");
                oldSelectedShotButton.TextColor = (Color)converter.ConvertFromInvariantString("Transparent");
                oldSelectedShotButton.BorderColor = (Color)converter.ConvertFromInvariantString("Transparent");
            }

            oldSelectedLoadButton = null;
        }
        private async void ResetLoadLocation(object sender, EventArgs e)
        {
            var converter = new ColorTypeConverter();
            LoadCoordinates[0] = 0;
            LoadCoordinates[1] = 0;
            if (oldSelectedLoadButton != null)
            {
                await oldSelectedLoadButton.FadeTo(0.1, easing: Easing.CubicInOut);
                oldSelectedLoadButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Transparent");
                oldSelectedLoadButton.TextColor = (Color)converter.ConvertFromInvariantString("Transparent");
                oldSelectedLoadButton.BorderColor = (Color)converter.ConvertFromInvariantString("Transparent");
            }

            oldSelectedLoadButton = null;
        }
        private async void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            Frame boxView = (Frame)sender as Frame;
            boxView.TranslationX = boxView.TranslationX + e.TotalX;
            
            if (e.StatusType == GestureStatus.Completed)
            {
                if (boxView.TranslationX < -130)
                {
                    boxView.IsEnabled = false;
                    
                    if(CurrentCycle < 20)
                    {
                        await boxView.TranslateTo(-400, 0,150 , easing: Easing.CubicInOut);
                        SwipeToTeleOpCard(true);
                    }
                    else
                    {
                        await boxView.TranslateTo(10, 0, easing: Easing.CubicInOut);
                        await boxView.TranslateTo(-10, 0, easing: Easing.CubicInOut);
                        await boxView.TranslateTo(0, 0, easing: Easing.CubicInOut);
                    }
                    boxView.IsEnabled = true;
                }
                else if (boxView.TranslationX > 130)
                {
                    boxView.IsEnabled = false;
                    
                    if (CurrentCycle > 1)
                    {
                        await boxView.TranslateTo(400, 0, 150, easing: Easing.CubicInOut);
                        SwipeToTeleOpCard(false);
                    }
                    else
                    {
                        await boxView.TranslateTo(-10, 0, easing: Easing.CubicInOut);
                        await boxView.TranslateTo(10, 0, easing: Easing.CubicInOut);
                        await boxView.TranslateTo(0, 0, easing: Easing.CubicInOut);
                    }
                    boxView.IsEnabled = true;

                }
                else
                {
                    await boxView.TranslateTo(0, 0, 150, easing: Easing.CubicInOut);
                }
            }
            
        }

    }
}