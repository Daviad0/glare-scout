using LightScout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace LightScout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FRCHome : TabbedPage
    {
        private static MatchViewModel matchViewModel = new MatchViewModel();
        public FRCHome()
        {
            matchViewModel.Update();
            BindingContext = matchViewModel;
            InitializeComponent();
        }
    }
}