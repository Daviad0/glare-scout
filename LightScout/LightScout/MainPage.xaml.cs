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
using System.IO;
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
        private List<TeamMatchViewItem> listofviewmatches = new List<TeamMatchViewItem>();
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
            var upnext = false;
            var upnextselected = false;
            foreach(var match in listofmatches)
            {
                upnext = false;
                if (!match.ClientSubmitted)
                {
                    if (!upnextselected)
                    {
                        upnext = true;
                        upnextselected = true;
                    }
                }
                var newmatchviewitem = new TeamMatchViewItem();
                newmatchviewitem.Completed = match.ClientSubmitted;
                if(match.TabletId != null)
                {
                    newmatchviewitem.IsRed = match.TabletId.StartsWith("R");
                    newmatchviewitem.IsBlue = match.TabletId.StartsWith("B");
                }
                
                newmatchviewitem.IsUpNext = upnext;
                newmatchviewitem.TeamName = match.TeamName;
                if(match.TeamName == null)
                {
                    newmatchviewitem.TeamName = "FRC Team " + match.TeamNumber.ToString();
                }
                newmatchviewitem.TeamNumber = match.TeamNumber;
                newmatchviewitem.MatchNumber = match.MatchNumber;
                newmatchviewitem.TabletName = match.TabletId;
                newmatchviewitem.teamIcon = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACgAAAAoCAYAAACM/rhtAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAALiIAAC4iAari3ZIAAAAHdElNRQfkARkSCRSFytq7AAAAGXRFWHRDb21tZW50AENyZWF0ZWQgd2l0aCBHSU1QV4EOFwAADqxJREFUWEedmPd3lFd+xmdGo65RRwIrGDDL0kwxzYALtmlmvXgd3NZgNll7G2BjMF299zbSaHp9p49mRqOGGiAwmGLH+OTk95yTk5zknD3JOrt/QJ4895VGHoFgT/LDwx295d7P+233e1EoTlRAcbISis/roDhd/5iyzjRAyVHWGeqLeqSfa0DWhUYk8e903k/hqOb9PP5O4++0sw0o5P38y03QXGxC7vlGZJc2Q1PahIIrTcii8i42yr/VnEvN+8mU8kwdlJ9VQ3mSTKfJU2/CLGAVFJ/VLgipmh3VXFxMlvRlA3K5eA4XTOeYJq6frUcKlfFlIzIEVFkT0i/zmYoWqhmZFU3IrmxC2pVG5PK97EtNKL7QhJQL/Hjx0ZxDXu9M4wzDqeqZv+vMBBTWO0F9VvMjYHz8gkB8SUAUXW5DKr86jV+dUtGGLC6cUd6MJVXtyCptQUZlO3Jq26CpakZuXRsKW9pl5Te3oaCxTb6XXsaxvp3ALUi50go1P1B1rgmKs5SA+7JZXnMOtsZAwFO0nrDgSVInQs4+pL7SjuSyVqRWdUFT04HMug5kNPQgq0ELTXMv8rUGjlrkdhtQ0GtEkcGA4j49FvVy1OlQ2MVnujl29yK7pYfP9iCvvQfZjd3Iqu1Gcmk71JfbobzYCsW5FkIKEVSMZT1xQMIJswo3z1pOPKS+1I70yg5k1XOyRk7cqUNORx/yOgjTbUJxpxU/tdjwU5sTzzlcWO6S8KzLhRLJiSUOJ0pcDpTYbPgbkxVL+ywo6TVjcY8Ji7oMyO/SoYDzaTh3WnkbIVuhPNcsS4Y7R+ALHQJwFu7zR+Do0pRyWqy2C3ltPbSIjtYxYZfZjgNHJew96sb+YxIOUgco8Xv/xxL2fezG3kQdc2OfLN7jOHNdwiKdsHAfcmnVrBoagm5XEU51nmDCgsKasgWFWxMtJ9xKuHTGVnZDF/I7CEdXFVh1eMZhluHigP9f7TnlQn4fw0FvpDd6kdNED1XT1RcFIK14VsQjAWtFDIqY+1zAzcSc6nyLHHcZNd10aS8KGVf5en6tZEKJzzEP8C81S/9PigNu6JCwxOxAXp8ZOZ16GkGHTMZ3KpNILQC/mAVsEFksW05IZBLNfLENmfV0a7sO+b0mFOopixVFbtscnNA3F363IMST9C/lh+YANX4/0ow+5JtdyGQsa1r7mHQ9SKvolF2sYrlSfElXN1oIKGhFHTpL8gv8gtJOaBqZeQzk3B4LFlkcKHR4kOf1zrPen6rXLwjyJMXhhFaGYsj2BqAxe5CudSCzXUDqkFrdjSQaSCXi71wbAa0ElOFoPV5MJlw6XZvZJKxnpmvtyLN5oHH0z7OeWCRx8T8n/F7o738t3z8Ht+vqGNYMxZAbDEFj9yNVJyGba2W16pFcJazYAdUFWu98OxRNwoJnCEfrCdeqyzqRyfpWwBpWaCSgzYVcewCprtg8wH8uOywvfGhlEidQyIrDvLNa/di1ONxbzkHsv34Vm4ZHkTcURrYvgAyTB8kddmg6jayvOjJ0QXWJ1rvIEtMsLDib0qnlnciRiyhTn7WqyGFBntWNPJ9fnny+9Z6VF46DJMIkXhOW/PeKPXOAH96cxsHbw3hxZAorR8LIDQeRaQ0gU+dBttaC9EZasVIL1eUOKC91EtDOeWb9nUzyvDaWE61Rjr0lkg2FLh8yPP3zAO+c+2xBmKddiwM+qpxICGkuPzQGDzK6rEhvNiK9vod50AXl5e44oKjY3B/r6FqmfIHWhAK9DcVOCYVuL3IDIXmyuPX+s2rz3ML/VTkzPkn/XT0zJkIlKtUfQ3qAo9EDdRuTpc1EQB2UV5jNpQRsEYAiGC8yOWq18haW28XMNTmxWHKh2Oedm0wASqfq5xb/Y8WMtcTvP1XNB4snSaIVhf6tYt/cfJsjI0gNRpAhBZBmk5CpdSKr3YyUuj66uRuqMi0UrQKQ1lNc6uJFbuatrOzdViaIQ7ZgceBHwEcVX1RARI7mzQMRfz8K90P1mrl33wiPYX1sCOmhCLKdQeS5XUjvcSC7k4D1eqTV9EBVzm2u1cF5RLbQ3ynVfchkDGg67SwvrH12D4qDnnlQiUpcXMA8qsT7QtHTFXPv7p4axWqWmhxaME8KosDhRq7JjpxuGzKaDYTrhaqiFwq6XaEQ2XJFC2V5H2+a5IcKjC4UCcCAB4uifqwY6seO2MTcAvsfAfxrenjxE/kd8e7Bm6PYNT6IZ4ciyA9EGOcB5Ft9KDSxpPVYkdpgRGqNjoA6AjrjgD1IqtIho4V7o5bZy4eL+FUiBosiPiwdDGHr0MgcYN1veh6D+IEJ8WgsCiWWmYW00uZFvsVDr7GB0NmQWm+gNwlYKQCFBS93QVHK2lNJ3zeZoaEFFxGwkDWw2OdGUdiHvEhk3qSPQjxNie89qjWmAAqcPnZKrBgGid6jBeuNSCKcsqJv1oKi3pQyIHlBVWtGNgGLzMxiBzsOPyHDHhSEZmphXH+s2rEgzNMUL1NCr5/24DlXEPnM4AI7DWCSGFZOaDpmXJxMFysr44CMP0UZA7JCj1R2D3l08SKRxXYJiz1uLAnNJEp8AeuJpgUBnqZEuL0XvVjtYfz5wnjW78USpxeLzfQU9/3cdhsbFW55tWy/avuQ1knApe29WKXXYxW75ecMFqyy2bGefd/mmIRNQz9OPG+RBP1QvXpBqLgS39tX78G20QDWDQWxcciLHWPM6HE3tg26sC3qxJaQFRtcZjzvNFFGLLcQcINdj60BI2XG9n4rdgzY8OKwg5nmwq6JhaEStRBUXIlwhzoCeGnSj21Tbmwf82L3pBuvTUuyXqf2TDrw6oRV1o6ImbLgeR8Bi9i9LG7RYUmbAevsFmzpt2FTkBaMuLCuX8IaxmARu44V4RBeGQ3j6MT4XwVMBNt/3PM/fycN4KO7A9g7GcYGgj0TDWA53fuc14P1AQlbIk7sHLbLFlxttjB5DHiW/egiHQFzqruwxtiHDTTrFr8Vz7NzXut2Yhl3khLJI9ep5aEwXr4axS+vzZSaRICnwX1QFkbNd1+h9OEETn43gHfvhrBn2oNVA0GU9HtR7GGdtbDD5ja3WGfHZp8V2+jFdVYjlvEYm91FwK0+A7bRxS94zFhttWC5nUdGjx8FbCg3sEC/MRbG2zf68d7dAH77IDa3uNCfa1Y8Ee7t0140f/c1Sr+fwOV/HMX5h1dx6sEwPrwXwttfe7F/Mord9Mj6wSBWh3xY66G3HFZsZAzuiglZsCnAvXiLR49NknEmOD0ObIi5sXMigEPXw/jg2hB+zf7tJCf+/T8M4NNvZuqhfLTk+CS4I5cC6Pynr3Dl+3Fc+nYcn98fx4lvh/D7eyP49EEEv74fxa/uDOK9m1G8dSOMXWNBbIh6scHL9V0WbCSPMNqWADvqkpZeLGb8PaO1osTgwlKmfRG/aH0shINTYRy7G8YnXw/hXU4mFl/IvfPgagM49fU4fnt/EKcfEur2GI7wQ98aH8KbN0I4civMeGQs343g59ci2HaV8R0KYjlL2jILD/8GK5ax5VvW1YcVPNwrFGX8p4rVu96KJDaIyXon0sJOnh2CODzdT0v2Y/tgBJne8BzEIR7C/6PylcfgPtAyDKauY994BIcmY/j7O7TY7VHsG4lhRaQfxQQpYbJtHw3hrWtRzs/Y5txpTJhUbg6p7XY2LDYeO7hh1OmRXS0O7rwg910dLqhtbHm494ove//mAA6OD2ClPwqVI4wUx0zjGtdfapbNgzvMpvMP90dxbPQa9kRG8frAII5MDeF3tyZwZHKYFSEiN7/5vhCy2aWvjNJDEwP4cOoq3hm+Ck3Yj2QHk4JnZkU7xxZyNfFMss47iK3RAbwxHsKb1+jSW4M4cXscv7o+jp+PDeK1kSgzeP5ePHC6fB7c3zrD+M39EXzxzRj+MH0NH41N4pdXJ3D8+hhO3JnEibujeH+aB6+pfrxO677CGH+NyfcmDXH0zgA+eRDFx9PDOHDTz7roxg4eqDbGgvhJNAjFXi7+4Z0wjj/ox8++cWDPbQd2X/Ni+4QXm2e1c8I3DzAR7uCgFz+77cN733rwyfdBfHSPYfGVHwenmWjU4Tt+fPTQjWPfe3DonoRXb0rYPsUdhfOK+Ns5HmBxDmIv4d+dGMWnX43iOFuyg+ye1vsHoXiZVfzALT82TdP/ERNSrHqo9WYk9VmhNtigNtuQzO0vETCuNIcXaontUljCC9ckvHPXj52TXqSE3EgJuJDsdSNJ8mMr27X3b0bw0lgIKS4PVD1eKLo8dKeb7nRByWOnSmtHkp27DA/1vxgbwatDQ0hy04Iv+IehsviR1GWa+d8k0TyI7oYNrKLSAGWdcUE4TScnZ7wotRLU3DMzQrQYs7TYG4Gix0cA3u9kPAn1cZuLDOGNqSA/iIBsChTsnBTVXJNNipyobPkUopNuo3EcPmweZJMcjUGh1PIL6viA6AtF8xpvv8p5rYKADabH4DZqaYFWBnIb3+1kcrFdT/b7cWDaj1RPGIpuYRmC8b6ilRK/LYzxsQGsGw7weQHIGkcDyIYQ3RSbZnntK91Q1vTRg/Rovx8KtZEuNJiRbKI4pkvsaLntpdisSPMaoIn1zINLNoeg0AdmZKCMdAOvrYuE8YtbLElBxg1hlOYAlMbZ54Qs3JWiIzhyI4aC/iCS2KQqtHQzLa3s5od2uehmB8Ww0lsYXlaGAz9uzYQLQqumnFgzyQaB2njVh0380m1jfuyb9uHobVb+qRjLDzM6Nii7a2dkGC9Hh/HS0AD2MLYE3PFv+/E+C/Gr1308GPmwi93Li8xYEZe7Z3enj+9xh7oXxD4mxfaRENsu7iKM0bXsnNZOOrH2hhVrbrBhYDe1jpuFQmnyQsX2Xs0OWu2SkOymlagktuIZbCwPT8RwlGXjeUKn8JSX5PZD5Qz9KJ7KlK4wXooN4Oj1EbwyPAC1j7Hm8zFJKL6Tyq4ohQf0JCmEF/rZdNwYxDtMhHTXgGx9hSnINSUmCePTxPg1UHq6V+/D/wLOVm+uUx7EAgAAAABJRU5ErkJggg==")));
                listofviewmatches.Add(newmatchviewitem);
            }
            listOfMatches.ItemsSource = listofviewmatches;
            carouseluwu.ItemsSource = listofviewmatches;

            tabletlist = new string[6] { "R1", "R2", "R3", "B1", "B2", "B3" }.ToList();
            tabletPicker.ItemsSource = tabletlist;
            MatchProgressList.Progress = (float)((float)1 / (float)listofviewmatches.Count);
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
        private async void GoToFRCPage(object sender, EventArgs e)
        {
            var currentindex = listofviewmatches.FindIndex(a => a == carouseluwu.CurrentItem);
            Console.WriteLine(listofmatches[currentindex].TeamNumber.ToString() + "'s match at match #" + listofmatches[currentindex].MatchNumber.ToString());
            bool answer = true;
            if (listofmatches[currentindex].ClientSubmitted)
            {
                answer = await DisplayAlert("Match Completed", "This match has already been completed by someone using this tablet, would you still like to continue?", "Continue", "Cancel");
            }
            if (answer)
            {
                Navigation.PushAsync(new FRCMain(listofmatches[currentindex]));
            }
            else
            {
                //var listobject = sender as ListView;
                //listobject.SelectedItem = null;
            }
        }

        private void carouseluwu_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            var currentindex = listofviewmatches.FindIndex(a => a == carouseluwu.CurrentItem);
            MatchProgressList.ProgressTo((double)((float)(currentindex+1) / (float)listofviewmatches.Count),250, Easing.CubicInOut);
        }
    }
}
