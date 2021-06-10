using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace LightScout.CustomControllers
{
    class NoMarginButton : Xamarin.Forms.Button
    {
        public NoMarginButton() : base()
        {
            this.On<Android>().SetUseDefaultPadding(false);
            this.On<Android>().SetUseDefaultShadow(false);
        }
    }
}
