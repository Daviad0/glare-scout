using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LightScout
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private static bool[] ControlPanel = new bool[2];
        private static bool Balanced;
        public MainPage()
        {
            InitializeComponent();
            ControlPanel[0] = false;
            ControlPanel[1] = false;
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
    }
}
