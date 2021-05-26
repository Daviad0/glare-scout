using LightScout.CustomControllers;
using LightScout.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LightScout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Database : ContentPage
    {
        private bool isOpen = false;
        private ObservableCollection<DatabaseItemView> databaseItemViews = new ObservableCollection<DatabaseItemView>();
        private ObservableCollection<DatabaseItemView> originalDatabaseItemViews = new ObservableCollection<DatabaseItemView>();
        private List<HideableListItem> itemReferences = new List<HideableListItem>();
        public Database()
        {
            
            InitializeComponent();
            
            originalDatabaseItemViews = new ObservableCollection<DatabaseItemView>() { new DatabaseItemView() { Visible = false, Id = "0", SetHeight = 80 }, new DatabaseItemView() { Visible = true, Id = "1", SetHeight = 80 }, new DatabaseItemView() { Visible = true, Id = "2", SetHeight = 80 } };
            databaseItemViews = new ObservableCollection<DatabaseItemView>(originalDatabaseItemViews);
            for(int item = 0; item < 3; item++)
            {
                itemReferences.Add(new HideableListItem(new HideableListItemInstance() { Id = item, Visible = true }));
                itemReferences.ToArray()[item].TriggerDeletion += async (id) =>
                {
                    var indexof = itemReferences.IndexOf(itemReferences.Where(x => x.currentInstance.Id == id).FirstOrDefault());
                    Console.WriteLine("Test Index: " + indexof.ToString());
                    try
                    {
                        for (int i = indexof; i < itemReferences.Count; i++)
                        {
                            itemReferences.ToArray()[i].AnimateRemoveItem();
                        }


                    }
                    catch (Exception ex)
                    {

                    }
                    await Task.Delay(150);
                    if(entireScroll.ContentSize.Height > 820)
                    {
                        if (entireScroll.ContentSize.Height - entireScroll.ScrollY < 810)
                        {
                            await entireScroll.ScrollToAsync(0, entireScroll.ScrollY - 115, true);
                        }
                        else
                        {
                            await Task.Delay(250);
                        }
                    }
                    else
                    {
                        await Task.Delay(250);
                    }

                    listOfItems.Children.Remove(itemReferences.Where(x => x.currentInstance.Id == id).FirstOrDefault());
                    itemReferences.Remove(itemReferences.Where(x => x.currentInstance.Id == id).FirstOrDefault());
                    try
                    {
                        for (int i = 0; i < itemReferences.Count; i++)
                        {
                            itemReferences.ToArray()[i].ResetTranslation();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    Console.WriteLine("Height: " + entireScroll.ScrollY.ToString() + " / " + entireScroll.ContentSize.Height.ToString());
                    
                    if (listOfItems.Children.Count <= 0)
                    {
                        everythingHiddenDialogue.IsVisible = true;
                        everythingHiddenDialogue.FadeTo(1, easing: Easing.CubicInOut);
                    }
                    else
                    {
                        everythingHiddenDialogue.IsVisible = false;

                    }
                };
                listOfItems.Children.Add(itemReferences.ToArray()[item]);
            }
            if(listOfItems.Children.Count <= 0)
            {
                noEntriesDialogue.IsVisible = true;
                noEntriesDialogue.Opacity = 1;
            }
            //listOfEntries.ItemsSource = databaseItemViews;
        }
        protected override async void OnAppearing()
        {
            //textBox.TranslateTo(-100, 0, easing: Easing.CubicInOut);
        }
        private async void RemoveItem(int id)
        {

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

            int indexId = databaseItemViews.IndexOf(databaseItemViews.Where(x => x.Id == expander.ClassId).FirstOrDefault());

            if(Device.RuntimePlatform == Device.iOS)
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
                            databaseItemViews.RemoveAt(indexId);
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
                            databaseItemViews.RemoveAt(indexId);
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
            }
            

            
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
            Frame frame = (Frame)sender as Frame;
            Expander expander = (Expander)frame.Parent.Parent;
            expander.ExpandAnimationEasing = Easing.CubicInOut;
            expander.CollapseAnimationEasing = Easing.CubicInOut;
            expander.CollapseAnimationLength = 500;
            expander.ExpandAnimationLength = 500;
            Label hidingIcon = (Label)((Grid)((Frame)((Grid)frame.Parent).Children.Where(x => x.ClassId.Contains("below")).FirstOrDefault()).Content).Children.Where(x => x.ClassId.Contains("label"));
            Label hidingTag = (Label)((Grid)((Frame)((Grid)frame.Parent).Children.Where(x => x.ClassId.Contains("below")).FirstOrDefault()).Content).Children.Where(x => x.ClassId.Contains("icon"));
            Frame testoption = (Frame)((Grid)frame.Parent).Children.Where(x => x.ClassId.Contains("below")).FirstOrDefault();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            itemReferences.Clear();
            listOfItems.Children.Clear();
            for (int item = 0; item < 20; item++)
            {
                itemReferences.Add(new HideableListItem(new HideableListItemInstance() { Id = item, Visible = true }));
                itemReferences.ToArray()[item].TriggerDeletion += async (id) =>
                {
                    var indexof = itemReferences.IndexOf(itemReferences.Where(x => x.currentInstance.Id == id).FirstOrDefault());
                    Console.WriteLine("Test Index: " + indexof.ToString());
                    try
                    {
                        for (int i = indexof; i < itemReferences.Count; i++)
                        {
                            itemReferences.ToArray()[i].AnimateRemoveItem();
                        }


                    }
                    catch (Exception ex)
                    {

                    }
                    await Task.Delay(150);
                    if (entireScroll.ContentSize.Height > 820)
                    {
                        if (entireScroll.ContentSize.Height - entireScroll.ScrollY < 810)
                        {
                            await entireScroll.ScrollToAsync(0, entireScroll.ScrollY - 115, true);
                        }
                        else
                        {
                            await Task.Delay(250);
                        }
                    }
                    else
                    {
                        await Task.Delay(250);
                    }

                    listOfItems.Children.Remove(itemReferences.Where(x => x.currentInstance.Id == id).FirstOrDefault());
                    itemReferences.Remove(itemReferences.Where(x => x.currentInstance.Id == id).FirstOrDefault());
                    try
                    {
                        for (int i = 0; i < itemReferences.Count; i++)
                        {
                            itemReferences.ToArray()[i].ResetTranslation();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    Console.WriteLine("Height: " + entireScroll.ScrollY.ToString() + " / " + entireScroll.ContentSize.Height.ToString());
                    if (listOfItems.Children.Count <= 0)
                    {
                        everythingHiddenDialogue.IsVisible = true;
                        everythingHiddenDialogue.FadeTo(1, easing: Easing.CubicInOut);
                    }
                    else
                    {
                        everythingHiddenDialogue.IsVisible = false;

                    }
                };
                listOfItems.Children.Add(itemReferences.ToArray()[item]);
            }
            if (listOfItems.Children.Count <= 0)
            {
                noEntriesDialogue.IsVisible = true;
                noEntriesDialogue.FadeTo(1,easing:Easing.CubicInOut);
            }
            else
            {
                
                await everythingHiddenDialogue.FadeTo(0, easing: Easing.CubicInOut);
                everythingHiddenDialogue.IsVisible = false;
            }
        }
        private async void NewQueryDrag(object sender, PanUpdatedEventArgs e)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                if (e.StatusType == GestureStatus.Canceled || e.StatusType == GestureStatus.Completed)
                {
                    if (queryInterface.TranslationY > 250)
                    {
                        queryInterface.TranslateTo(queryInterface.TranslationX, queryInterface.TranslationY + 1200, easing: Easing.SinIn);
                        await Task.Delay(350);
                        queryInterface.IsVisible = false;
                        queryInterface.TranslationY = 0;
                    }
                    else
                    {
                        queryInterface.TranslateTo(0, 0, easing: Easing.CubicInOut);
                        queryInterface.FadeTo(1, 250, easing: Easing.CubicInOut);
                    }
                }
                else
                {
                    if (e.TotalY < 0)
                    {
                        queryInterface.TranslateTo(0, 0, easing: Easing.CubicInOut);
                    }
                    else
                    {
                        queryInterface.TranslationY = e.TotalY;
                    }

                }
            }
            else
            {
                if (e.StatusType == GestureStatus.Canceled || e.StatusType == GestureStatus.Completed)
                {
                    if (e.TotalY + queryInterface.TranslationY > 250)
                    {
                        queryInterface.TranslateTo(queryInterface.TranslationX, queryInterface.TranslationY + 1200, easing: Easing.SinIn);
                        await Task.Delay(350);
                        queryInterface.IsVisible = false;
                        queryInterface.TranslationY = 0;
                    }
                    else
                    {
                        queryInterface.TranslateTo(0, 0, easing: Easing.CubicInOut);
                        queryInterface.FadeTo(1, 250, easing: Easing.CubicInOut);
                    }
                }
                else
                {
                    if (e.TotalY < 0)
                    {
                        queryInterface.TranslateTo(0, 0, easing: Easing.CubicInOut);
                    }
                    else
                    {
                        queryInterface.TranslationY += e.TotalY;
                    }

                }

            }
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            queryInterface.TranslationY = 1200;
            await Task.Delay(100);
            queryInterface.IsVisible = true;
            queryInterface.TranslateTo(queryInterface.TranslationX, queryInterface.TranslationY - 1200, 500, Easing.SinOut);
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }
    }
}