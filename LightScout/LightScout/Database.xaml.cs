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
    public partial class Database : ContentPage
    {
        private bool isOpen = false;
        public Database()
        {
            
            InitializeComponent();
            expander1.ExpandAnimationEasing = Easing.CubicInOut;
            expander1.CollapseAnimationEasing = Easing.CubicInOut;
            expander1.CollapseAnimationLength = 500;
            expander1.ExpandAnimationLength = 500;
            expander2.ExpandAnimationEasing = Easing.CubicInOut;
            expander2.CollapseAnimationEasing = Easing.CubicInOut;
            expander2.CollapseAnimationLength = 500;
            expander2.ExpandAnimationLength = 500;
            underContainer.AnchorY = underContainer.AnchorY - 50;
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) => {
                if (!isOpen)
                {
                    underContainer.ScaleY = .1;
                    await Task.Delay(5);
                    underContainer.IsVisible = true;
                    underContainer.ScaleYTo(1,500, easing: Easing.CubicInOut);
                    isOpen = true;
                }
                else
                {
                    await underContainer.ScaleYTo(0.1,500, easing: Easing.CubicInOut);
                    underContainer.IsVisible = false;
                    isOpen = false;
                }
                
            };
            container.GestureRecognizers.Add(tapGestureRecognizer);
        }

        protected override async void OnAppearing()
        {
            //textBox.TranslateTo(-100, 0, easing: Easing.CubicInOut);
        }
        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            BoxView boxView = (BoxView)sender as BoxView;
            boxView.TranslationX = boxView.TranslationX + e.TotalX;
            if(e.StatusType == GestureStatus.Completed)
            {
                
                if(boxView.TranslationX < -250)
                {
                    boxView.TranslateTo(-400, 0, easing: Easing.CubicInOut);
                }
                else if (boxView.TranslationX < -25)
                {
                    boxView.TranslateTo(-100, 0, easing: Easing.CubicInOut);
                }
                else
                {
                    boxView.TranslateTo(50, 0, easing: Easing.CubicInOut);
                }
            }
        }

        private async void PanGestureRecognizer_PanUpdated_1(object sender, PanUpdatedEventArgs e)
        {
            Frame frame = (Frame)sender as Frame;
           
            


            if (e.StatusType == GestureStatus.Completed || e.StatusType == GestureStatus.Canceled)
            {
                if (frame.TranslationX + e.TotalX < -70)
                {
                    if (frame.TranslationX + e.TotalX < -150)
                    {
                        frame.TranslateTo(-500, 0, easing: Easing.CubicInOut);
                        testoption.TranslateTo(-500, 0, easing: Easing.CubicInOut);
                        //AUTORESET FOR DEBUG
                        await Task.Delay(2000);
                        frame.TranslateTo(0, 0, easing: Easing.CubicInOut);
                        testoption.TranslateTo(0, 0, easing: Easing.CubicInOut);
                    }
                    else
                    {
                        frame.TranslateTo(-70, 0, easing: Easing.CubicInOut);
                    }
                    
                }
                else
                {
                    frame.TranslateTo(0, 0, easing: Easing.CubicInOut);
                }
            }
            else
            {
                if (frame.TranslationX + e.TotalX < -70)
                {
                    if(frame.TranslationX + e.TotalX < -350)
                    {
                        frame.TranslateTo(-500, 0, easing: Easing.CubicInOut);
                        testoption.TranslateTo(-500, 0, easing: Easing.CubicInOut);
                        //AUTORESET FOR DEBUG
                        await Task.Delay(2000);
                        frame.TranslateTo(0, 0, easing: Easing.CubicInOut);
                        testoption.TranslateTo(0, 0, easing: Easing.CubicInOut);
                    }
                    else
                    {
                        frame.TranslationX += e.TotalX * 0.8;
                    }
                    
                }
                else if (frame.TranslationX + e.TotalX > 0)
                {
                    frame.TranslateTo(0, 0, easing: Easing.CubicInOut);

                }
                else
                {
                    frame.TranslationX += e.TotalX;
                }
            }

            
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Frame frame = (Frame)sender as Frame;
            Expander selected = (Expander)frame.Parent.Parent.Parent;
            selected.IsExpanded = !selected.IsExpanded;
        }

        private async void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            expander1.IsExpanded = false;
            testFrame1.TranslateTo(-500, 0, easing: Easing.CubicInOut);
            testoption.TranslateTo(-500, 0, easing: Easing.CubicInOut);
            //AUTORESET FOR DEBUG
            await Task.Delay(2000);
            testFrame1.TranslateTo(0, 0, easing: Easing.CubicInOut);
            testoption.TranslateTo(0, 0, easing: Easing.CubicInOut);
        }
    }
}