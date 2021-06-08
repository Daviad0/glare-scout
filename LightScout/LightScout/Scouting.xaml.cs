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
        private bool optionsExpanded = false;
        private DateTime startForm = DateTime.Now;
        public Scouting()
        {
            InitializeComponent();
        }

        private async void expandOptions(object sender, EventArgs e)
        {
            
            Action<double> callback = input => { options.Height = input; };
            var startingHeight = optionsExpanded ? 120 : 0;
            // ensure that no overflow content is showed
            

            if (optionsExpanded)
            {
                optionsExpanded = false;
                var endingHeight = 0;
                //await optionsContent.FadeTo(1, 100);
                //optionsContent.Opacity = 0;
                //optionsContent.IsVisible = true;

                var anim = new Animation(callback, startingHeight, endingHeight, Easing.CubicInOut);
                optionsContent.FadeTo(0, 300, Easing.CubicInOut);
                
                await Task.Delay(100);
                anim.Commit(this, "HeightAnimation", length:500);
                //optionsLabel.TranslateTo(optionsLabel.X, optionsLabel.Y, 500, Easing.CubicInOut);
                await Task.Delay(200);
                optionsContent.IsVisible = false;
                await optionsLabel.RotateTo(0, 250, Easing.CubicInOut);
                
            }
            else
            {
                optionsExpanded = true;
                var endingHeight = 120;
                var anim = new Animation(callback, startingHeight, endingHeight, Easing.CubicInOut);

                
                
                anim.Commit(this, "HeightAnimation", length: 500);
                //optionsLabel.TranslateTo(optionsLabel.X, optionsLabel.Y + 60, 500, Easing.CubicInOut);
                await Task.Delay(100);
                optionsContent.Opacity = 0;
                optionsContent.IsVisible = true;
                optionsContent.FadeTo(1, 300, Easing.CubicInOut);
                await optionsLabel.RotateTo(180, 250, Easing.CubicInOut);
              
                //optionsContent.Opacity = 0;
                //optionsContent.IsVisible = true;
                //optionsContent.FadeTo(1, 100);

            }

            


        }

        private void StartTimer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                Console.WriteLine(DateTime.Now - startForm);
                return true;
            });
        }
    }
}