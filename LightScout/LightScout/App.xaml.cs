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
            MainPage = new NavigationPage(new MainPage());
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
