using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightScout.CustomControllers;
using LightScout.Models;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static LightScout.Models.SchemaValuePairing;

namespace LightScout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Scouting : ContentPage
    {
        private bool optionsExpanded = false;
        private bool disabled = false;
        public ColorTypeConverter converter = new ColorTypeConverter();
        private DateTime startDisabled = DateTime.Now;
        private TimeSpan totalDisabled = TimeSpan.FromSeconds(0);
        private DateTime startForm = DateTime.Now;
        public List<string> submitMethod = new List<string>();
        // could change dynamic to an object technically
        public Dictionary<string, object> categoryGrids = new Dictionary<string, object>();
        public Dictionary<string, SchemaValuePairing> fields = new Dictionary<string, SchemaValuePairing>();
        public Dictionary<string, StackLayout> dynamicLayouts = new Dictionary<string, StackLayout>();
        public Dictionary<string, SingleControlRestriction> singleRestrictionMapping = new Dictionary<string, SingleControlRestriction>();
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
            'conditions' : 
              {
                'allType' : 'stepper',
                'max' : 15,
                'min' : 0
              },
            
            'contents' : [
              {
                'type' : 'stepper',
                'prettyName' : 'Power Cells Inner',
                'uniqueId' : 'powerCellA_inner',
                'conditions':{
                    'max':5,
                    'min':0
}
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Outer',
                'uniqueId' : 'powerCellA_outer'
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Lower',
                'uniqueId' : 'powerCellA_lower'
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Missed',
                'uniqueId' : 'powerCellA_missed'
              }
            ]
          },
            {
            'type' : 'parent',
            'prettyName' : 'Robot Tasks',
            'uniqueId' : 'initLine_parent',
            'contents' : [
              {
                'type' : 'choices',
                'prettyName' : 'Initiation Line?',
                'uniqueId' : 'initLine',
                'conditions' : 
                    {
                        'options' : [
                            'Yes', 'No'
                        ]
                    }
            
              },
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
            'conditions' : 
              {
                'allType' : 'stepper',
                'max' : 5,
                'min' : 0,
                'cycles' : {
                  'max' : 20,
                  'min' : 5
                }
              },
            
            'contents' : [
              {
                'type' : 'stepper',
                'prettyName' : 'Power Cells Inner',
                'uniqueId' : 'powerCellT_inner'
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Outer',
                'uniqueId' : 'powerCellT_outer'
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Lower',
                'uniqueId' : 'powerCellT_lower'
              },
{
                'type' : 'stepper',
                'prettyName' : 'Power Cells Missed',
                'uniqueId' : 'powerCellT_missed'
              }
            ]
          },
{
            'type' : 'parent',
            'prettyName' : 'Control Panel',
            'uniqueId' : 'controlPanel_parent',
            'contents' : [
              {
                'type' : 'choices',
                'prettyName' : 'Rotation?',
                'uniqueId' : 'rotation',
                'conditions' : 
                    {
                        'options' : [
                            'Yes', 'No'
                        ]
                    }
            
              },{
                'type' : 'choices',
                'prettyName' : 'Position?',
                'uniqueId' : 'position',
                'conditions' : 
                    {
                        'options' : [
                            'Yes', 'No'
                        ]
                    }
            
              }
            ]
          }
        ]
      },
{
        'prettyName' : 'Endgame',
        'autoStart?' : false,
        'expectedStart' : 135,
        'type': 'category',
        'uniqueId' : 'endgame',
        'contents' : [
          {
            'type' : 'choices',
            'prettyName' : 'Parked?',
            'uniqueId' : 'parked',
            'conditions' : 
                {
                    'options' : [
                        'Yes', 'No'
                    ]
                }
            
            },
{
            'type' : 'choices',
            'prettyName' : 'Climbed?',
            'uniqueId' : 'climbed',
            'conditions' : 
                {
                    'options' : [
                        'No','Attempted', 'Succeeded'
                    ]
                }
            
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
        public enum RestrictionType
        {
            Max,
            Min,
            SecondsElapsed
        }
        public static object IsRestrictionValid(dynamic obj, RestrictionType type)
        {
            // there probably is a better way to check if a property exists in a dynamic
            var toReturn = 0;
            switch (name)
            {
                case RestrictionType.Max:
                    try
                    {
                        toReturn = obj.max;
                    }
                    catch
                    {
                        return null;
                    }
                    break;
                case RestrictionType.Min:
                    try
                    {
                        toReturn = obj.min;
                    }
                    catch
                    {
                        return null;
                    }
                    break;
                case RestrictionType.SecondsElapsed:
                    try
                    {
                        toReturn = obj.secondsElapsed;
                    }
                    catch
                    {
                        return null;
                    }
                    break;
            }
            return toReturn;
        }
        public async Task<bool> deeperSchemaLevel(dynamic starter)
        {
            
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            int maxButtonHandle = (int)Math.Floor((mainDisplayInfo.Width / mainDisplayInfo.Density) / (float)150);
            int maxNumCharacters = (int)Math.Floor((mainDisplayInfo.Width / mainDisplayInfo.Density) / (float)15);
            //Console.WriteLine("A");
            foreach (var content in starter.contents)
            {
                //Console.WriteLine(content.uniqueId.ToString());
                if(content.type.ToString() != "parent")
                {
                    string uniqueId = content.uniqueId.ToString();
                    Grid fieldContent = new Grid() { ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) }, new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) } }, Margin = new Thickness(25,5,25,0) };
                    fields.Add(content.uniqueId.ToString(), new SchemaValuePairing() { schemaObject = content, schemaType = (SchemaType)Enum.Parse(typeof(SchemaType), content.type.ToString()), value = null });
                    Label addContent = new Label() { Text = content.prettyName.ToString(), FontSize=14, HorizontalOptions = LayoutOptions.Center, TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C"), VerticalOptions = LayoutOptions.Center };
                    fieldContent.Children.Add(addContent, 0, 0);
                    Grid inputContent = new Grid();
                    switch (content.type.ToString())
                    {
                        case "choices":
                            int col = 0;
                            int row = 0;
                            // first check if they should be on separate rows or columns
                            bool inCols = true;
                            int numButtons = 0;
                            int numCharacters = 0;
                            singleRestrictionMapping.Add(content.uniqueId, content.conditions);
                            foreach (var choice in content.conditions.options)
                            {
                                numButtons += 1;
                                numCharacters += choice.ToString().Length;
                            }
                            

                            if (numButtons > maxButtonHandle || numCharacters > maxNumCharacters)
                                inCols = false;

                            // hardcode it to index 0 to find options, may change
                            foreach (var choice in content.conditions.options)
                            {
                                if (inCols)
                                {
                                    inputContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                                }
                                else
                                {
                                    inputContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                }
                               
                                Button newButton = new Button() { Text = choice.ToString(), IsEnabled = true, CornerRadius = 8, BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Transparent"), BorderColor = (Color)converter.ConvertFromInvariantString("#4594f5"), BorderWidth = 4, TextColor = (Color)converter.ConvertFromInvariantString("#4594f5"), ClassId = uniqueId, Padding = new Thickness(6,4) };
                                if(!inCols && row == 0)
                                {
                                    newButton.Margin = new Thickness(0, 10, 0, 0);
                                }
                                fields[uniqueId].controls.Add(newButton);
                                newButton.Clicked += async (sender, args) => {
                                    Button clicked = (Button)sender as Button;
                                    string selectUID = clicked.ClassId;
                                    if (fields[selectUID].value != null && fields[selectUID].value != clicked.Text.ToString())
                                    {

                                        Button buttonNoHighlight = ((Button)fields[selectUID].controls.Where(c => ((Button)c).Text == (string)fields[selectUID].value).FirstOrDefault());
                                        Button buttonHighlight = ((Button)fields[selectUID].controls.Where(c => ((Button)c).Text == (string)clicked.Text).FirstOrDefault());
                                        buttonNoHighlight.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Transparent");
                                        buttonNoHighlight.TextColor = (Color)converter.ConvertFromInvariantString("#4594f5");
                                        buttonHighlight.BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5");
                                        buttonHighlight.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
                                        fields[selectUID].value = clicked.Text;

                                    }
                                    else if (fields[selectUID].value == null)
                                    {
                                        Button buttonHighlight = ((Button)fields[selectUID].controls.Where(c => ((Button)c).Text == (string)clicked.Text).FirstOrDefault());
                                        buttonHighlight.BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5");
                                        buttonHighlight.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
                                        fields[selectUID].value = clicked.Text;
                                    }


                                };
                                inputContent.Children.Add(newButton, col, row);
                                if (inCols)
                                {
                                    col++;
                                }
                                else
                                {
                                    row++;
                                }
                                

                            }
                            break;
                        case "stepper":
                            Button downButton = new Button() { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, TextColor = (Color)converter.ConvertFromInvariantString("Color.White"), Text = "-", BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5"), CornerRadius = 8, Padding = new Thickness(4), FontAttributes = FontAttributes.Bold, ClassId = uniqueId, FontSize = 18 };
                            Entry stepperValue = new Entry() { VerticalOptions = LayoutOptions.Center, Margin = new Thickness(5,0), Keyboard = Keyboard.Numeric, Text = "0", HorizontalTextAlignment = TextAlignment.Center, TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C"), ClassId = uniqueId};
                            Button upButton = new Button() { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, TextColor = (Color)converter.ConvertFromInvariantString("Color.White"), Text = "+", BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5"), CornerRadius = 8, Padding = new Thickness(4), FontAttributes = FontAttributes.Bold, ClassId = uniqueId, FontSize = 18 };
                            singleRestrictionMapping.Add(content.uniqueId, new SingleControlRestriction() { max = IsRestrictionValid(content.conditions, RestrictionType.Max), min = IsRestrictionValid(content.conditions, RestrictionType.Min), secondsElapsed = IsRestrictionValid(content.conditions, RestrictionType.SecondsElapsed) });
                            stepperValue.TextChanged += (sender, args) =>
                            {
                                Entry selectedStepper = (Entry)sender as Entry;
                                string selectUID = selectedStepper.ClassId;
                                if(fields[selectUID].value != null)
                                {
                                    if(int.TryParse(selectedStepper.Text, out int output))
                                    {
                                        // it is an int, so we can set the value
                                        // first check if its going up or down so we can easily check for restrictions.
                                        if(int.Parse(selectedStepper.Text) > int.Parse(fields[selectUID].value.ToString()))
                                        {
                                            if(((SingleControlRestriction)singleRestrictionMapping[selectUID]).max != null && ((SingleControlRestriction)singleRestrictionMapping[selectUID]).max < int.Parse(selectedStepper.Text))
                                            {
                                                // this has to be reset then
                                                selectedStepper.Text = fields[selectUID].value.ToString();
                                                
                                            }
                                            else
                                            {
                                                fields[selectUID].value = int.Parse(selectedStepper.Text);
                                            }
                                        }
                                        else
                                        {
                                            if (((SingleControlRestriction)singleRestrictionMapping[selectUID]).min != null && ((SingleControlRestriction)singleRestrictionMapping[selectUID]).min > int.Parse(selectedStepper.Text))
                                            {
                                                // this has to be reset then
                                                selectedStepper.Text = fields[selectUID].value.ToString();

                                            }
                                            else
                                            {
                                                fields[selectUID].value = int.Parse(selectedStepper.Text);
                                            }
                                        }
                                        fields[selectUID].value = int.Parse(selectedStepper.Text);
                                    }
                                    else
                                    {
                                        // reset input changes
                                        selectedStepper.Text = fields[selectUID].value.ToString();
                                    }
                                }
                                else
                                {
                                    if (int.TryParse(selectedStepper.Text, out int output))
                                    {
                                        // it is an int, so we can set the value
                                        // TODO: SET MIN AND MAX RESTRICTIONS HERE
                                        fields[selectUID].value = int.Parse(selectedStepper.Text);
                                    }
                                    else
                                    {
                                        // reset input changes
                                        selectedStepper.Text = "0";
                                    }
                                }
                            };
                            

                            downButton.Clicked += (sender, args) =>
                            {
                                Button selectedButton = (Button)sender as Button;
                                string selectUID = selectedButton.ClassId;
                                try
                                {
                                    var prevInt = int.Parse(fields[selectUID].value.ToString());
                                    if (prevInt > 0)
                                    {

                                        Entry selectedStepper = (Entry)(fields[selectUID].controls.Where(c => c.GetType().GetProperty("Text").GetValue(c, null).ToString() == fields[selectUID].value.ToString()).FirstOrDefault());
                                        fields[selectUID].value = (prevInt - 1).ToString();
                                        selectedStepper.Text = fields[selectUID].value.ToString();
                                    }
                                }
                                catch(Exception ex)
                                {

                                }
                                
                            };
                            upButton.Clicked += (sender, args) =>
                            {
                                Button selectedButton = (Button)sender as Button;
                                string selectUID = selectedButton.ClassId;
                                try
                                {
                                    var prevInt = int.Parse(fields[selectUID].value.ToString());
                                    Entry selectedStepper = (Entry)(fields[selectUID].controls.Where(c => c.GetType().GetProperty("Text").GetValue(c, null).ToString() == fields[selectUID].value.ToString()).FirstOrDefault());
                                    fields[selectUID].value = (prevInt + 1).ToString();
                                    selectedStepper.Text = fields[selectUID].value.ToString();
                                }
                                catch(Exception ex)
                                {
                                    //must be a null value, set to 1
                                    Entry selectedStepper = (Entry)(fields[selectUID].controls.Where(c => c.GetType().GetProperty("Text").GetValue(c, null).ToString() == "0").FirstOrDefault());
                                    fields[selectUID].value = "1";
                                    selectedStepper.Text = fields[selectUID].value.ToString();
                                }
                                
                            };
                            fields[uniqueId].controls.Add(upButton);
                            fields[uniqueId].controls.Add(downButton);
                            fields[uniqueId].controls.Add(stepperValue);
                            inputContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            inputContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            inputContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            fields[uniqueId].value = "0";
                            inputContent.Children.Add(downButton, 0, 0);
                            inputContent.Children.Add(stepperValue, 1, 0);
                            inputContent.Children.Add(upButton, 2, 0);
                            break;
                        default:
                            singleRestrictionMapping.Add(content.uniqueId, content.conditions);
                            inputContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            inputContent.Children.Add(new Label() { Text = "Not Implemented", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }, 0, 0);
                            break;
                    }
                    
                    fieldContent.Children.Add(inputContent, 1, 0);
                    ((StackLayout)dynamicLayouts[starter.uniqueId.ToString()]).Children.Add(fieldContent);
                    // remember submission order and add to specific dictionary based on the content needed
                    submitMethod.Add(content.uniqueId.ToString());
                    
                }
                else
                {
                    // add stack to parent, and then create another object for the next iteration
                    StackLayout newParent = new StackLayout() { ClassId = content.uniqueId };
                    Label parentLabel = new Label() { Text = content.prettyName, Margin = new Thickness(0, 5, 0, 0), FontSize = 20, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, TextColor = (Color)converter.ConvertFromInvariantString("#4594f5") };
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

            // init height items
            disabledMenu.SizeChanged += (sender, args) =>
            {
                disabledMenu.TranslationY = disabledMenu.Height + 10;
            };
            checkWidth.SizeChanged += (sender, args) =>
            {
                menuItem1.HeightRequest = checkWidth.Width - 20;
                menuItem1.WidthRequest = checkWidth.Width - 20;
                menuItem2.HeightRequest = checkWidth.Width - 20;
                menuItem2.WidthRequest = checkWidth.Width - 20;
                menuItem3.HeightRequest = checkWidth.Width - 20;
                menuItem3.WidthRequest = checkWidth.Width - 20;
            };

            StartTimer();
            foreach(var category in formObject.categories)
            {
                StackLayout newElement = new StackLayout() { ClassId = category.uniqueId };
                // create top grid or label depending on if it's needed due to "expectedTime" property
                if(category.expectedStart != null)
                {
                    Grid categoryHeader = new Grid() { ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Auto } }, Margin = new Thickness(0, 30, 0, 0), HorizontalOptions = LayoutOptions.Center };
                    Label categoryLabel = new Label() { Text = category.prettyName, FontSize = 28, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C") };
                    Frame categoryStartFrame = new Frame() { CornerRadius = 8, Padding = new Thickness(8, 4), HorizontalOptions = LayoutOptions.Start, BackgroundColor = (Color)converter.ConvertFromInvariantString("#0F3F8C"), VerticalOptions = LayoutOptions.Center, Margin = new Thickness(3, 0), ClassId = category.uniqueId };
                    Label categoryStartLabel = new Label() { Text = "Start", TextColor = (Color)converter.ConvertFromInvariantString("Color.White"), FontSize = 18, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center, ClassId = category.uniqueId };
                    categoryStartFrame.Content = categoryStartLabel;
                    
                    categoryHeader.Children.Add(categoryLabel, 0, 0);
                    categoryHeader.Children.Add(categoryStartFrame, 1, 0);
                    mainParent.Children.Add(categoryHeader);
                }
                else
                {
                    Label categoryLabel = new Label() { Text = category.prettyName, FontSize = 28, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C"), Margin = new Thickness(0, 30, 0, 0) };
                    mainParent.Children.Add(categoryLabel);
                }
                
                dynamicLayouts.Add(category.uniqueId.ToString(), newElement);
                
                mainParent.Children.Add(newElement);
                // recursion until all of the separate items are finally added to the Dictionary and list
                await deeperSchemaLevel(category);
                Console.WriteLine("Category " + category.uniqueId.ToString() + " finished!");
            }
            Console.WriteLine("Split down completed!");
        }
        private async void exitPhase(object sender, EventArgs e)
        {
            bool ans = await DisplayAlert("Are you sure?", "Do you want to leave this form? Your changes will not be saved!", "Exit", "Cancel");
            if (ans)
            {
                Navigation.PushAsync(new MasterPage());
            }
            else
            {
                // we know that options IS expanded, so we can contract it
                expandOptions(null, null);
            }
            
        }
        private async void toggleDisabled(object sender, EventArgs e)
        {
            if (disabled)
            {
                elapsedContainer.TranslateTo(0, 0, 500, Easing.CubicInOut);
                disabledMenu.TranslateTo(0, disabledMenu.Height+10, 500, Easing.CubicInOut);
                disabled = false;
                totalDisabled = DateTime.Now - startDisabled + totalDisabled;
                //elapsedFrame2.FadeTo(0, 250, Easing.CubicInOut);
                // we know that options IS expanded, so we can contract it
                if (optionsExpanded)
                {
                    expandOptions(null, null);
                }   
            }
            else
            {
                elapsedContainer.TranslateTo(0, Math.Round((disabledMenu.Height * -1) + 10), 500, Easing.CubicInOut);
                disabledMenu.TranslateTo(0, 8, 500, Easing.CubicInOut);
                startDisabled = DateTime.Now;
                disabled = true;
                //elapsedFrame2.FadeTo(1, 250, Easing.CubicInOut);
                expandOptions(null, null);
            }
        }
        private async void resetDisabled(object sender, EventArgs e)
        {
            startDisabled = DateTime.Now;
            totalDisabled = TimeSpan.FromSeconds(0);
        }
        private async void expandOptions(object sender, EventArgs e)
        {

            Action<double> heicallback = input => { options.Height = input; };
            Action<double> corcallback = input => { optionsBar.CornerRadius = (float)input; };
            var startingHeight = optionsExpanded ? 160 : 0;
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
                AbsoluteLayout.SetLayoutBounds(optionsBarParent, new Rectangle(0, 0, 1, .15));

            }
            else
            {
                AbsoluteLayout.SetLayoutBounds(optionsBarParent, new Rectangle(0, 0, 1, .35));
                optionsExpanded = true;
                var endingHeight = 160;
                var heianim = new Animation(heicallback, startingHeight, endingHeight, Easing.CubicInOut);
                var coranim = new Animation(corcallback, 0, 8, Easing.CubicInOut);

                coranim.Commit(this, "CornerAnimation", length: 500);
                heianim.Commit(this, "HeightAnimation", length: 500);
                //optionsLabel.TranslateTo(optionsLabel.X, optionsLabel.Y + 60, 500, Easing.CubicInOut);
                await Task.Delay(100);
                optionsContent.Opacity = 0;
                await Task.Delay(50);
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
                elapsed1.Text = Math.Floor((DateTime.Now - startForm).TotalMinutes).ToString() + ":" + ((Math.Floor((DateTime.Now - startForm).TotalSeconds) - (Math.Floor((DateTime.Now - startForm).TotalMinutes) * 60)).ToString().Length == 1 ? "0" : "") + (Math.Floor((DateTime.Now - startForm).TotalSeconds)- (Math.Floor((DateTime.Now - startForm).TotalMinutes)*60)).ToString();
                //elapsed2.Text = Math.Floor((DateTime.Now - startForm).TotalMinutes).ToString() + ":" + ((Math.Floor((DateTime.Now - startForm).TotalSeconds) - (Math.Floor((DateTime.Now - startForm).TotalMinutes) * 60)).ToString().Length == 1 ? "0" : "") + (Math.Floor((DateTime.Now - startForm).TotalSeconds)- (Math.Floor((DateTime.Now - startForm).TotalMinutes)*60)).ToString();
                if (disabled)
                {
                    var totalMinutes = Math.Floor((totalDisabled + (DateTime.Now - startDisabled)).TotalMinutes);
                    var totalSeconds = Math.Floor((totalDisabled + (DateTime.Now - startDisabled)).TotalSeconds - (totalMinutes*60));
                    disabledElapsed.Text = totalMinutes.ToString() + ":" + (totalSeconds.ToString().Length == 1 ? "0" : "") + totalSeconds.ToString();
                }
                return true;
            });
        }
    }
}