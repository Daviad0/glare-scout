using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LightScout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Scouting : ContentPage
    {
        private bool optionsExpanded = false;
        private bool disabled = false;
        private DateTime startDisabled = DateTime.Now;
        private TimeSpan totalDisabled = TimeSpan.FromSeconds(0);
        private DateTime startForm = DateTime.Now;
        public List<string> submitMethod = new List<string>();
        // could change dynamic to an object technically
        public Dictionary<string, object> fields = new Dictionary<string, object>();
        public Dictionary<string, StackLayout> dynamicLayouts = new Dictionary<string, StackLayout>();
        public dynamic formObject = JObject.Parse(@"{
  'id': '76628abc',
  'prettyName': 'Infinite Recharge',
  'categories': [
      {
        'prettyName' : 'Autonomous',
        'autoStart?' : true,
        'expectedStart' : null,
        'type': 'category',
        'uniqueId' : 'autonomous',
        'contents' : [
          {
            'type' : 'parent',
            'prettyName' : 'Power Cells',
            'uniqueId' : 'powerCellA_parent',
            'conditions' : [
              {
                'allType' : 'stepper',
                'max' : 15,
                'min' : 0
              }
            ],
            'contents' : [
              {
                'type' : 'stepper',
                'prettyName' : 'Power Cells Inner',
                'uniqueId' : 'powerCellA_inner'
              }
            ]
          }  
        ]
      },
      {
        'prettyName' : 'Tele-Op',
        'autoStart?' : false,
        'expectedStart' : 16,
        'type': 'category',
        'uniqueId' : 'teleop',
        'contents' : [
          {
            'type' : 'parent',
            'prettyName' : 'Power Cells',
            'uniqueId' : 'powerCellT_parent',
            'conditions' : [
              {
                'allType' : 'stepper',
                'max' : 5,
                'min' : 0,
                'cycles' : {
                  'max' : 20,
                  'min' : 5
                }
              }
            ],
            'contents' : [
              {
                'type' : 'stepper',
                'prettyName' : 'Power Cells Inner',
                'uniqueId' : 'powerCellT_inner'
              }
            ]
          }  
        ]
      }
    ]
}
");
        public Scouting()
        {
            InitializeComponent();
        }
        public async Task<bool> deeperSchemaLevel(dynamic starter)
        {
            //Console.WriteLine("A");
            foreach(var content in starter.contents)
            {
                //Console.WriteLine(content.uniqueId.ToString());
                if(content.type != "parent")
                {
                    Label addContent = new Label() { Text = content.prettyName.ToString(), HorizontalOptions = LayoutOptions.Center };
                    ((StackLayout)dynamicLayouts[starter.uniqueId.ToString()]).Children.Add(addContent);
                    // remember submission order and add to specific dictionary based on the content needed
                    submitMethod.Add(content.uniqueId.ToString());
                    fields.Add(content.uniqueId.ToString(), content);
                }
                else
                {
                    // add stack to parent, and then create another object for the next iteration
                    StackLayout newParent = new StackLayout() { ClassId = content.uniqueId };
                    Label parentLabel = new Label() { Text = content.prettyName, FontSize = 20, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
                    newParent.Children.Add(parentLabel);
                    ((StackLayout)dynamicLayouts[starter.uniqueId.ToString()]).Children.Add(newParent);
                    dynamicLayouts.Add(content.uniqueId.ToString(), newParent);
                    await deeperSchemaLevel(content);
                }
            }
            return true;
        }
        protected override async void OnAppearing()
        {
            StartTimer();
            foreach(var category in formObject.categories)
            {
                StackLayout newElement = new StackLayout() { ClassId = category.uniqueId };
                Label categoryLabel = new Label() { Text = category.prettyName, FontSize = 24, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
                dynamicLayouts.Add(category.uniqueId.ToString(), newElement);
                newElement.Children.Add(categoryLabel);
                mainParent.Children.Add(newElement);
                // recursion until all of the separate items are finally added to the Dictionary and list
                await deeperSchemaLevel(category);
                Console.WriteLine("Category " + category.uniqueId.ToString() + " finished!");
            }
            Console.WriteLine("Split down completed!");
        }
        private async void exitPhase(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MasterPage());
        }
        private async void toggleDisabled(object sender, EventArgs e)
        {
            if (disabled)
            {
                elapsedContainer.TranslateTo(0, 0, 500, Easing.CubicInOut);
                disabledMenu.TranslateTo(0, disabledMenu.Height+8, 500, Easing.CubicInOut);
                disabled = false;
                totalDisabled = DateTime.Now - startDisabled + totalDisabled;
            }
            else
            {
                elapsedContainer.TranslateTo(0, Math.Round((disabledMenu.Height * -1) + 8), 500, Easing.CubicInOut);
                disabledMenu.TranslateTo(0, 8, 500, Easing.CubicInOut);
                startDisabled = DateTime.Now;
                disabled = true;
            }
        }
        private async void expandOptions(object sender, EventArgs e)
        {

            Action<double> heicallback = input => { options.Height = input; };
            Action<double> corcallback = input => { optionsBar.CornerRadius = (float)input; };
            var startingHeight = optionsExpanded ? 150 : 0;
            // ensure that no overflow content is showed
            

            if (optionsExpanded)
            {
                optionsExpanded = false;
                var endingHeight = 0;
                //await optionsContent.FadeTo(1, 100);
                //optionsContent.Opacity = 0;
                //optionsContent.IsVisible = true;

                var heianim = new Animation(heicallback, startingHeight, endingHeight, Easing.CubicInOut);
                var coranim = new Animation(corcallback, 8, 0, Easing.CubicInOut);
                optionsContent.FadeTo(0, 300, Easing.CubicInOut);
                
                await Task.Delay(100);
                coranim.Commit(this, "CornerAnimation", length: 500);
                heianim.Commit(this, "HeightAnimation", length:500);
                //optionsLabel.TranslateTo(optionsLabel.X, optionsLabel.Y, 500, Easing.CubicInOut);
                await Task.Delay(200);
                optionsContent.IsVisible = false;
                await optionsLabel.RotateTo(0, 250, Easing.CubicInOut);
                
            }
            else
            {
                optionsExpanded = true;
                var endingHeight = 150;
                var heianim = new Animation(heicallback, startingHeight, endingHeight, Easing.CubicInOut);
                var coranim = new Animation(corcallback, 0, 8, Easing.CubicInOut);

                coranim.Commit(this, "CornerAnimation", length: 500);
                heianim.Commit(this, "HeightAnimation", length: 500);
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
            Device.StartTimer(TimeSpan.FromMilliseconds(20), () =>
            {
                elapsed.Text = Math.Floor((DateTime.Now - startForm).TotalMinutes).ToString() + ":" + ((Math.Floor((DateTime.Now - startForm).TotalSeconds) - (Math.Floor((DateTime.Now - startForm).TotalMinutes) * 60)).ToString().Length == 1 ? "0" : "") + (Math.Floor((DateTime.Now - startForm).TotalSeconds)- (Math.Floor((DateTime.Now - startForm).TotalMinutes)*60)).ToString();
                if (disabled)
                {
                    var totalMinutes = Math.Floor((totalDisabled + (DateTime.Now - startDisabled)).TotalMinutes);
                    var totalSeconds = Math.Floor((totalDisabled + (DateTime.Now - startDisabled)).TotalSeconds - (totalMinutes*60));
                    disabledElapsed.Text = "Disabled for " + totalMinutes.ToString() + ":" + (totalSeconds.ToString().Length == 1 ? "0" : "") + totalSeconds.ToString();
                }
                return true;
            });
        }
    }
}