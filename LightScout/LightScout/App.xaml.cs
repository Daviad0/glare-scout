using LightScout.AppThemes;
using LightScout.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Linq;

namespace LightScout
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Application.Current.Properties["BluetoothMethod"] = new SubmitVIABluetooth();
            if(!Application.Current.Properties.ContainsKey("TimeLastSubmitted"))
            {
                Application.Current.Properties["TimeLastSubmitted"] = DateTime.Now;
            }
            if (JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).TeamOfOwnership != 0)
            {
                MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                MainPage = new NavigationPage(new SetNewData());
            }
            if(!Application.Current.Properties.ContainsKey("UniqueID"))
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var stringChars = new char[8];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                var finalString = new String(stringChars);
                string deviceOSString = (DeviceInfo.Platform == DevicePlatform.iOS ? "i" : "a");
                Application.Current.Properties["UniqueID"] = deviceOSString + "-" + finalString;
                Console.WriteLine(Application.Current.Properties["UniqueID"]);
                
            }
            else {
                Console.WriteLine(Application.Current.Properties["UniqueID"]);
            }
            Application.Current.SavePropertiesAsync();
            //MainPage = new NavigationPage(new SetNewData());
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            mergedDictionaries.Clear();
            mergedDictionaries.Add(new LightTheme());


        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
