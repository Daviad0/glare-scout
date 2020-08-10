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
    public partial class SetNewData : ContentPage
    {
        public SetNewData()
        {
            InitializeComponent();
        }

        private async void FinishedTeamNumber(object sender, EventArgs e)
        {
            setTeamNumberPanel.TranslateTo(setTeamNumberPanel.TranslationX - 600, setTeamNumberPanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(150);
            setScoutNamePanel.TranslationX = 600;
            setScoutNamePanel.TranslationY = 0;
            await Task.Delay(50);
            setScoutNamePanel.IsVisible = true;
            setScoutNamePanel.TranslateTo(setScoutNamePanel.TranslationX - 600, setScoutNamePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(290);
            setTeamNumberPanel.IsVisible = false;
            await Task.Delay(10);
            setTeamNumberPanel.TranslationX = 0;
        }
        private async void FinishedScoutName(object sender, EventArgs e)
        {
            setScoutNamePanel.TranslateTo(setScoutNamePanel.TranslationX - 600, setScoutNamePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(150);
            setTabletTypePanel.TranslationX = 600;
            setTabletTypePanel.TranslationY = 0;
            await Task.Delay(50);
            setTabletTypePanel.IsVisible = true;
            setTabletTypePanel.TranslateTo(setTabletTypePanel.TranslationX - 600, setTabletTypePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(290);
            setScoutNamePanel.IsVisible = false;
            await Task.Delay(10);
            setScoutNamePanel.TranslationX = 0;
        }
        private async void FinishedTabletType(object sender, EventArgs e)
        {
            setTabletTypePanel.TranslateTo(setTabletTypePanel.TranslationX - 600, setTabletTypePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(150);
            setCodePanel.TranslationX = 600;
            setCodePanel.TranslationY = 0;
            await Task.Delay(50);
            setCodePanel.IsVisible = true;
            setCodePanel.TranslateTo(setCodePanel.TranslationX - 600, setCodePanel.TranslationY, 500, Easing.CubicInOut);
            await Task.Delay(290);
            setTabletTypePanel.IsVisible = false;
            await Task.Delay(10);
            setTabletTypePanel.TranslationX = 0;
            
        }
        private async void RestartForm(object sender, EventArgs e)
        {
            if (!setTeamNumberPanel.IsVisible)
            {
                resetForm.IsEnabled = false;
                setTabletTypePanel.TranslateTo(setTabletTypePanel.TranslationX, setTabletTypePanel.TranslationY + 600, 500, Easing.CubicInOut);
                setCodePanel.TranslateTo(setCodePanel.TranslationX, setCodePanel.TranslationY + 600, 500, Easing.CubicInOut);
                setScoutNamePanel.TranslateTo(setScoutNamePanel.TranslationX, setScoutNamePanel.TranslationY + 600, 500, Easing.CubicInOut);
                await Task.Delay(200);
                setTeamNumberPanel.TranslationX = 600;
                setupScoutName.Text = "";
                setupTeamNumber.Text = "";
                setupCode.Text = "";
                await Task.Delay(50);
                setTeamNumberPanel.IsVisible = true;
                setTeamNumberPanel.TranslateTo(setTeamNumberPanel.TranslationX - 600, setTeamNumberPanel.TranslationY, 500, Easing.CubicInOut);
                await Task.Delay(240);
                setTabletTypePanel.IsVisible = false;
                setCodePanel.IsVisible = false;
                setScoutNamePanel.IsVisible = false;
                await Task.Delay(10);
                setTabletTypePanel.TranslationY = 0;
                setTabletTypePanel.TranslationX = 0;
                setCodePanel.TranslationY = 0;
                setCodePanel.TranslationX = 0;
                setScoutNamePanel.TranslationY = 0;
                setScoutNamePanel.TranslationX = 0;
                resetForm.IsEnabled = true;
            }
            else
            {
                setupScoutName.Text = "";
                setupTeamNumber.Text = "";
                setupCode.Text = "";
            }
            

        }
    }
}