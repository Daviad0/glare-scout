using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace LightScout.CustomControllers
{
    public class DBAnimationTrigger : TriggerAction<Expander>
    {
        public AnimationAction Action { get; set; }
        public enum AnimationAction
        { Start, Stop }

        protected override async void Invoke(Expander sender)
        {
            if (sender != null)
            {
                if (Action == AnimationAction.Start)
                    await PerformAnimation(sender);
                else if (Action == AnimationAction.Stop)
                    CancelAnimation(sender);
            }
        }

        private async Task PerformAnimation(Expander myElement)
        {
            uint timeout = 100;

            myElement.TranslationY = 70;
            myElement.TranslateTo(0, 0, easing: Easing.CubicInOut);

            /*
            new Animation {
    { 0, 0.5, new Animation (v => myElement.Scale = v, 1, 2) },
    { 0, 1, new Animation (v => myElement.Rotation = v, 0, 360) },
    { 0.5, 1, new Animation (v => myElement.Scale = v, 2, 1) }
    }.Commit(myElement, "ChildAnimations", 16, 4000, null, null, () => true);
            */


        }

        private void CancelAnimation(Expander myElement)
        {
            ViewExtensions.CancelAnimations(myElement);
            //           myElement.AbortAnimation("ChildAnimations");

        }
    }
}
