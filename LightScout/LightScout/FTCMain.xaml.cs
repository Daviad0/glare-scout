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
    public partial class FTCMain : ContentPage
    {
        private static bool[] ControlPanel = new bool[2];
        private static bool Balanced;
        public FTCMain()
        {
            InitializeComponent();
            ControlPanel[0] = false;
            ControlPanel[1] = false;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

        }
        

        private void progressRandom_Clicked(object sender, EventArgs e)
        {
            var progress = (float)new Random().Next(0, 100) / (float)100;
            progressTest.Progress = progress;
        }
    }
}