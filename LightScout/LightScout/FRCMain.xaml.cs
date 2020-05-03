using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public FRCMain()
        {
            InitializeComponent();
            ControlPanel[0] = false;
            ControlPanel[1] = false;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(TimeSpan.FromSeconds(1));
            await animatedelement.TranslateTo(TranslationX, TranslationY - 50, 1000, Easing.SinOut);
            HiddenLabel.FadeTo(1, 350);
            HiddenLabelName.FadeTo(1, 350);
            await Task.Delay(TimeSpan.FromSeconds(2.5));
            await overLay.FadeTo(0, 800);
            overLay.IsVisible = false;

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
            DependencyService.Get<DataStore>().SaveData("frctest050220.txt", "Hello Android World! :)");
        }
        private void LoadTheData(object sender, EventArgs e)
        {
            showData.Text = DependencyService.Get<DataStore>().LoadData("frctest050220.txt");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }
    }
}