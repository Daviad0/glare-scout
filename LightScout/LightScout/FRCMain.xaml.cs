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
        private bool Attempted;
        private int CurrentMatchNum = 1;
        private int[] PowerCellInner = new int[21];
        private int[] PowerCellOuter = new int[21];
        private int[] PowerCellLower = new int[21];
        private int[] PowerCellMissed = new int[21];
        private int NumCycles = 0;
        private int CurrentCycle = 1;
        private int CurrentSubPage;
        private int DisabledSeconds;
        private bool CurrentlyDisabled;
        private bool InitLineAchieved;
        private double[] OriginalCardCoords = { 0, 0 };
        private static IBluetoothLE ble = CrossBluetoothLE.Current;
        private static IAdapter adapter = CrossBluetoothLE.Current.Adapter;
        private static IDevice deviceIWant;
        private static ObservableCollection<IDevice> Devices = new ObservableCollection<IDevice>();
        private static bool canTransmitData = true;
        public FRCMain(IDevice passedDevice)
        {
            adapter.DeviceConnected += async (s, a) =>
            {
                Console.WriteLine("Connected to: " + a.Device.Name.ToString());
                //status.Text = "Connected to: " + a.Device.Name.ToString();
                canTransmitData = true;
            };
            deviceIWant = passedDevice;
            var converter = new ColorTypeConverter();
            InitializeComponent();
            ControlPanel[0] = false;
            ControlPanel[1] = false;
            BackgroundColor = (Color)converter.ConvertFromInvariantString("#009cd7");
            adapter.ConnectToDeviceAsync(deviceIWant);
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
            //showData.Text = DependencyService.Get<DataStore>().LoadData("frctest051920.txt");
        }
        private void BackToMainMenu(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }
        private async void PowerCellsUp(object sender, EventArgs e)
        {
            var buttonClicked = sender as Button;
            if(buttonClicked == innerUp)
            {
                PowerCellInner[CurrentCycle]++;
                innerDown.IsEnabled = true;
                innerAmount.Text = PowerCellInner[CurrentCycle].ToString() + " PC";
            }
            else if(buttonClicked == outerUp)
            {
                PowerCellOuter[CurrentCycle]++;
                outerDown.IsEnabled = true;
                outerAmount.Text = PowerCellOuter[CurrentCycle].ToString() + " PC";
            }
            else if(buttonClicked == lowerUp)
            {
                PowerCellLower[CurrentCycle]++;
                lowerDown.IsEnabled = true;
                lowerAmount.Text = PowerCellLower[CurrentCycle].ToString() + " PC";
            }
            else if(buttonClicked == missedUp)
            {
                PowerCellMissed[CurrentCycle]++;
                missedDown.IsEnabled = true;
                missedAmount.Text = PowerCellMissed[CurrentCycle].ToString() + " PC";
            }
            if((PowerCellInner[CurrentCycle] + PowerCellLower[CurrentCycle] + PowerCellMissed[CurrentCycle] + PowerCellOuter[CurrentCycle]) >= 5)
            {
                innerUp.IsEnabled = false;
                outerUp.IsEnabled = false;
                lowerUp.IsEnabled = false;
                missedUp.IsEnabled = false;
            }
        }
        private async void PowerCellsDown(object sender, EventArgs e)
        {
            var buttonClicked = sender as Button;
            if (buttonClicked == innerDown)
            {
                PowerCellInner[CurrentCycle]--;
                if(PowerCellInner[CurrentCycle] <= 0)
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
        }
        private async void NextTeleOpCard(object sender, EventArgs e)
        {
            if(CurrentCycle < 20)
            {
                if(CurrentCycle > NumCycles)
                {
                    NumCycles = CurrentCycle;
                }
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
            if(CurrentCycle > 1)
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
            if (canTransmitData)
            {
                string[] uuidrandomfun = { "6ad0f836b49011eab3de0242ac130001", "6ad0f836b49011eab3de0242ac130002", "6ad0f836b49011eab3de0242ac130003", "6ad0f836b49011eab3de0242ac130004", "6ad0f836b49011eab3de0242ac130005", "6ad0f836b49011eab3de0242ac130006" };
                string[] tabletidentifiers = { "R1", "R2", "R3", "B1", "B2", "B3" };
                var servicetosend = await deviceIWant.GetServiceAsync(Guid.Parse("6ad0f836b49011eab3de0242ac130000"));
                var randomnumgenerator = new Random();
                var randomnumber = randomnumgenerator.Next(0, 5);
                string randomuuid = uuidrandomfun[randomnumber];
                var matchtotransmit = new TeamMatch();
                matchtotransmit.TabletId = tabletidentifiers[randomnumber];
                matchtotransmit.EventCode = "test_env";
                matchtotransmit.A_InitiationLine = InitLineAchieved;
                matchtotransmit.E_Balanced = Balanced;
                matchtotransmit.T_ControlPanelPosition = ControlPanel[1];
                matchtotransmit.T_ControlPanelPosition = ControlPanel[0];
                matchtotransmit.ScoutName = scoutName.Text;
                matchtotransmit.DisabledSeconds = DisabledSeconds;
                matchtotransmit.E_ClimbAttempt = Attempted;
                matchtotransmit.E_ClimbSuccess = Climb;
                matchtotransmit.E_Park = Parked;
                matchtotransmit.TeamNumber = 862;
                matchtotransmit.PowerCellInner = PowerCellInner;
                matchtotransmit.PowerCellOuter = PowerCellOuter;
                matchtotransmit.PowerCellLower = PowerCellLower;
                matchtotransmit.PowerCellMissed = PowerCellMissed;
                matchtotransmit.NumCycles = NumCycles;
                matchtotransmit.MatchNumber = CurrentMatchNum;
                var stringtosend = JsonConvert.SerializeObject(matchtotransmit);
                Console.WriteLine(stringtosend);
                
                var characteristictosend = await servicetosend.GetCharacteristicAsync(Guid.Parse(randomuuid));
                characteristictosend.ValueUpdated += async (s, a) =>
                {
                    Console.WriteLine(a.Characteristic.Value);
                };
                await characteristictosend.StartUpdatesAsync();
                var stringtoconvert = "S:"+stringtosend;
                var bytestotransmit = Encoding.ASCII.GetBytes(stringtoconvert);
                if(bytestotransmit.Length > 480)
                {
                    
                    if(bytestotransmit.Length > 960)
                    {
                        var startidentifier = "MM:3";
                        var startbytesarray = Encoding.ASCII.GetBytes(startidentifier);
                        await characteristictosend.WriteAsync(startbytesarray);
                        var bytesarray1 = bytestotransmit.Take(480).ToArray();
                        var bytesarray2 = bytestotransmit.Skip(480).Take(480).ToArray();
                        var bytesarray3 = bytestotransmit.Skip(960).ToArray();
                        await characteristictosend.WriteAsync(bytesarray1);
                        await characteristictosend.WriteAsync(bytesarray2);
                        await characteristictosend.WriteAsync(bytesarray3);
                    }
                    else
                    {
                        var startidentifier = "MM:2";
                        var startbytesarray = Encoding.ASCII.GetBytes(startidentifier);
                        await characteristictosend.WriteAsync(startbytesarray);
                        var bytesarray1 = bytestotransmit.Take(480).ToArray();
                        var bytesarray2 = bytestotransmit.Skip(480).ToArray();
                        await characteristictosend.WriteAsync(bytesarray1);
                        await characteristictosend.WriteAsync(bytesarray2);
                    }
                }
                else
                {
                    await characteristictosend.WriteAsync(bytestotransmit);
                }
                
                stringtoconvert = "B:"+Battery.ChargeLevel.ToString();
                bytestotransmit = Encoding.ASCII.GetBytes(stringtoconvert);
                await characteristictosend.WriteAsync(bytestotransmit);
                Console.WriteLine(bytestotransmit);
                

            }
            else
            {
                Console.WriteLine("Can't connect :(");
            }
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
                toolTipLabel.Text = "Ready For Match";
            }else if (CurrentSubPage == 1)
            {
                teleopForm.FadeTo(0, 250);
                teleopForm.IsVisible = false;
                autoForm.IsVisible = true;
                autoForm.FadeTo(1, 250);
                toolTipLabel.Text = "Autonomous";
            }
            else if (CurrentSubPage == 2)
            {
                endgameForm.FadeTo(0, 250);
                endgameForm.IsVisible = false;
                teleopForm.IsVisible = true;
                teleopForm.FadeTo(1, 250);
                toolTipLabel.Text = "Tele-Op";
            }
            else if (CurrentSubPage == 3)
            {
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
                nameForm.FadeTo(0, 250);
                nameForm.IsVisible = false;
                autoForm.IsVisible = true;
                autoForm.FadeTo(1, 250);
                toolTipLabel.Text = "Autonomous";

            }
            else if(CurrentSubPage == 2)
            {
                autoForm.FadeTo(0, 250);
                autoForm.IsVisible = false;
                teleopForm.IsVisible = true;
                teleopForm.FadeTo(1, 250);
                toolTipLabel.Text = "Tele-Op";
            }
            else if (CurrentSubPage == 3)
            {
                teleopForm.FadeTo(0, 250);
                teleopForm.IsVisible = false;
                endgameForm.IsVisible = true;
                endgameForm.FadeTo(1, 250);
                toolTipLabel.Text = "Endgame";
            }
            else if (CurrentSubPage == 4)
            {
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

        private async void SwipeToTeleOpCard(object sender, SwipedEventArgs e)
        {
            if(e.Direction == SwipeDirection.Left)
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

        private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            
            CurrentMatchNum = (int)e.NewValue;
            matchNumberStepperLabel.Text = "For testing, match number (" + CurrentMatchNum.ToString() + "):";
        }
    }
}