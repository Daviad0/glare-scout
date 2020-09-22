using LightScout.Models;
using Newtonsoft.Json;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            //MainPage = new NavigationPage(new SetNewData());

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
