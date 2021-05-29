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
    public partial class LazyViewTest : ContentView
    {
        public LazyViewTest()
        {
            InitializeComponent();
            Randomness.Text = new Random().Next(1, 1000).ToString();
        }
    }
}