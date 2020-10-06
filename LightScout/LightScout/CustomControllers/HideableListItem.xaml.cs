using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LightScout.CustomControllers
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HideableListItem : ContentView
    {
        public HideableListItemInstance currentInstance { get; set; }
        public HideableListItem(HideableListItemInstance passedInstance)
        {
            InitializeComponent();
            currentInstance = passedInstance;
        }
        public void AnimateRemoveItem()
        {
            this.TranslationY = 70;
            this.TranslateTo(0, 0, easing: Easing.CubicInOut);
        }
        public void AnimateAddItem()
        {
            this.TranslationY = -70;
            this.TranslateTo(0, 0, easing: Easing.CubicInOut);
        }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Frame frame = (Frame)sender as Frame;
            Expander selected = (Expander)frame.Parent.Parent.Parent;
            selected.ExpandAnimationEasing = Easing.CubicInOut;
            selected.CollapseAnimationEasing = Easing.CubicInOut;
            selected.CollapseAnimationLength = 500;
            selected.ExpandAnimationLength = 500;
            selected.IsExpanded = !selected.IsExpanded;
        }

        private async void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            /*Frame frame = (Frame)sender as Frame;
            Expander expander = (Expander)frame.Parent.Parent;
            expander.ExpandAnimationEasing = Easing.CubicInOut;
            expander.CollapseAnimationEasing = Easing.CubicInOut;
            expander.CollapseAnimationLength = 500;
            expander.ExpandAnimationLength = 500;
            Label hidingIcon = (Label)((Grid)((Frame)((Grid)frame.Parent).Children.Where(x => x.ClassId.Contains("below")).FirstOrDefault()).Content).Children.Where(x => x.ClassId.Contains("label"));
            Label hidingTag = (Label)((Grid)((Frame)((Grid)frame.Parent).Children.Where(x => x.ClassId.Contains("below")).FirstOrDefault()).Content).Children.Where(x => x.ClassId.Contains("icon"));
            Frame testoption = (Frame)((Grid)frame.Parent).Children.Where(x => x.ClassId.Contains("below")).FirstOrDefault();*/
        }
        private async void PanGestureRecognizer_PanUpdated_1(object sender, PanUpdatedEventArgs e)
        {
            /*Frame frame = (Frame)sender as Frame;
            Expander expander = (Expander)frame.Parent.Parent.Parent;
            expander.AnchorY -= 30;
            expander.ExpandAnimationEasing = Easing.CubicInOut;
            expander.CollapseAnimationEasing = Easing.CubicInOut;
            expander.CollapseAnimationLength = 500;
            expander.ExpandAnimationLength = 500;
            Grid parent1 = (Grid)frame.Parent;
            Frame frame1 = (Frame)parent1.Children[0];
            Grid parent2 = (Grid)frame1.Content;
            Label hidingIcon = (Label)parent2.Children[0];
            Label hidingTag = (Label)parent2.Children[1];
            Frame testoption = (Frame)parent1.Children[0];

            //int indexId = databaseItemViews.IndexOf(databaseItemViews.Where(x => x.Id == expander.ClassId).FirstOrDefault());

            if (Device.RuntimePlatform == Device.iOS)
            {
                Console.WriteLine(e.TotalX);
                if (e.StatusType == GestureStatus.Completed || e.StatusType == GestureStatus.Canceled)
                {
                    if (frame.TranslationX + e.TotalX < -90)
                    {
                        if (frame.TranslationX + e.TotalX < -150)
                        {
                            frame.TranslateTo(-500, 0, 200, easing: Easing.CubicOut);
                            hidingIcon.TranslateTo(0, 0, easing: Easing.CubicInOut);
                            hidingTag.FadeTo(1, easing: Easing.CubicInOut);
                            if (((Expander)frame.Parent.Parent.Parent).IsExpanded)
                            {
                                ((Expander)frame.Parent.Parent.Parent).IsExpanded = false;
                                await Task.Delay(200);
                            }
                            await Task.Delay(100);
                            testoption.FadeTo(0, easing: Easing.CubicInOut);
                            //AUTORESET FOR DEBUG
                            await Task.Delay(500);
                            //databaseItemViews.RemoveAt(indexId);
                            //listOfEntries.ItemsSource = new ObservableCollection<DatabaseItemView>(databaseItemViews);
                            frame.TranslationX = 0;
                        }
                        else
                        {
                            hidingIcon.TranslateTo(0, 10, easing: Easing.CubicInOut);
                            frame.TranslateTo(0, 0, easing: Easing.CubicInOut);
                            hidingTag.FadeTo(0, easing: Easing.CubicInOut);
                        }

                    }
                    else
                    {
                        hidingIcon.TranslateTo(0, 10, easing: Easing.CubicInOut);
                        frame.TranslateTo(0, 0, easing: Easing.CubicInOut);
                        hidingTag.FadeTo(0, easing: Easing.CubicInOut);
                    }
                }
                else
                {
                    if (e.TotalX < -90)
                    {
                        var testdouble = ((Math.Abs(Math.Abs(e.TotalX))) - 90) * 0.05 + 0.4;
                        frame.TranslationX = e.TotalX * 0.8;
                        hidingTag.FadeTo(testdouble, easing: Easing.CubicInOut);
                        hidingIcon.TranslateTo(0, 0, easing: Easing.CubicInOut);

                    }
                    else if (e.TotalX > 0)
                    {
                        hidingIcon.TranslateTo(0, 10, easing: Easing.CubicInOut);
                        frame.TranslateTo(0, 0, easing: Easing.CubicInOut);

                    }
                    else
                    {
                        hidingIcon.TranslateTo(0, 10, easing: Easing.CubicInOut);
                        frame.TranslationX = e.TotalX;
                        hidingTag.FadeTo(0, easing: Easing.CubicInOut);
                    }
                }
            }
            else
            {
                Console.WriteLine(e.TotalX);
                if (e.StatusType == GestureStatus.Completed || e.StatusType == GestureStatus.Canceled)
                {
                    if (frame.TranslationX + e.TotalX < -90)
                    {
                        if (frame.TranslationX + e.TotalX < -150)
                        {
                            frame.TranslateTo(-500, 0, 200, easing: Easing.CubicOut);
                            hidingIcon.TranslateTo(0, 0, easing: Easing.CubicInOut);
                            hidingTag.FadeTo(1, easing: Easing.CubicInOut);
                            if (((Expander)frame.Parent.Parent.Parent).IsExpanded)
                            {
                                ((Expander)frame.Parent.Parent.Parent).IsExpanded = false;
                                await Task.Delay(200);
                            }
                            await Task.Delay(100);
                            testoption.FadeTo(0, easing: Easing.CubicInOut);
                            //AUTORESET FOR DEBUG
                            await Task.Delay(500);
                            //databaseItemViews.RemoveAt(indexId);
                            //listOfEntries.ItemsSource = new ObservableCollection<DatabaseItemView>(databaseItemViews);
                            frame.TranslationX = 0;
                        }
                        else
                        {
                            hidingIcon.TranslateTo(0, 10, easing: Easing.CubicInOut);
                            frame.TranslateTo(0, 0, easing: Easing.CubicInOut);
                            hidingTag.FadeTo(0, easing: Easing.CubicInOut);
                        }

                    }
                    else
                    {
                        hidingIcon.TranslateTo(0, 10, easing: Easing.CubicInOut);
                        frame.TranslateTo(0, 0, easing: Easing.CubicInOut);
                        hidingTag.FadeTo(0, easing: Easing.CubicInOut);
                    }
                }
                else
                {
                    if (frame.TranslationX + e.TotalX < -90)
                    {
                        hidingIcon.TranslateTo(0, 0, easing: Easing.CubicInOut);
                        var testdouble = ((Math.Abs(Math.Abs(e.TotalX) + Math.Abs(frame.TranslationX))) - 90) * 0.05 + 0.4;
                        frame.TranslationX += e.TotalX * 0.8;
                        hidingTag.FadeTo(testdouble, easing: Easing.CubicInOut);

                    }
                    else if (frame.TranslationX + e.TotalX > 0)
                    {
                        hidingIcon.TranslateTo(0, 10, easing: Easing.CubicInOut);
                        frame.TranslateTo(0, 0, easing: Easing.CubicInOut);
                        hidingTag.FadeTo(0, easing: Easing.CubicInOut);

                    }
                    else
                    {
                        hidingIcon.TranslateTo(0, 10, easing: Easing.CubicInOut);
                        frame.TranslationX += e.TotalX;
                        hidingTag.FadeTo(0, easing: Easing.CubicInOut);
                    }
                }
            }*/



        }
    }
    public class HideableListItemInstance
    {
        public int Id { get; set; }
        public bool Visible { get; set; }
    }
}