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
    public partial class Scouting : ContentPage
    {
        public Scouting()
        {
            InitializeComponent();
        }

        private async void expandOptions(object sender, EventArgs e)
        {
            Frame theOptionsMenu = this.FindByName<Frame>("options");
            Action<double> callback = input => { theOptionsMenu.HeightRequest = input; };
            var startingHeight = theOptionsMenu.Height;
            // ensure that no overflow content is showed
            theOptionsMenu.HeightRequest = startingHeight;
            
            var endingHeight = startingHeight + 60;
            theOptionsMenu.Animate("expand", callback, startingHeight, endingHeight, easing: Easing.CubicInOut, length: 500);
            await Task.Delay(500);
            optionsContent.Opacity = 0;
            optionsContent.IsVisible = true;
            optionsContent.FadeTo(1, 100);


        }
    }
}