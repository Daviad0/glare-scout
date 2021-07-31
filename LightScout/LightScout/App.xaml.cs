using LightScout.AppThemes;
using LightScout.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Reactive.Linq;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Linq;
using Akavache;

namespace LightScout
{
    public partial class App : Application
    {
        public App()
        {
            Akavache.Registrations.Start("LightScout");
            InitializeComponent();
            try
            {
                BlobCache.UserAccount.InsertObject("BluetoothMethod", new SubmitVIABluetooth());
            }
            catch(Exception e)
            {

            }
            try
            {
                BlobCache.UserAccount.InsertObject("TimeLastSubmitted", DateTime.Now);
            }
            catch (Exception e)
            {

            }
            try
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
                BlobCache.UserAccount.InsertObject("UniqueID", deviceOSString + "-" + finalString);
            }
            catch (Exception e)
            {

            }
            /*if (JsonConvert.DeserializeObject<LSConfiguration>(DependencyService.Get<DataStore>().LoadConfigFile()).TeamOfOwnership != 0)
            {
                //MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                //MainPage = new NavigationPage(new SetNewData());
            }*/
            //MainPage = new NavigationPage(new FTCMain());
            Application.Current.SavePropertiesAsync();
            MainPage = new NavigationPage(new MasterPage());
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            mergedDictionaries.Clear();
            mergedDictionaries.Add(new LightTheme());

            
        }

        protected async override void OnStart()
        {
            
            base.OnStart(); 
        }

        protected override void OnSleep()
        {
            BlobCache.Shutdown().Wait();
        }

        protected override void OnResume()
        {

        }
    }
}
