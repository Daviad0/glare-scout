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
        public Dictionary<string, MultiControlRestriction> multiRestrictionMapping = new Dictionary<string, MultiControlRestriction>();
        public DataEntry CurrentDataEntry;
        public bool loadingFromData = false;
        public int loadingFromIndex;
        public dynamic formObject;
        public Scouting(DataEntry toUse)
        {
            InitializeComponent();
            CurrentDataEntry = toUse;
            formObject = JObject.Parse(ApplicationDataHandler.Schemas.Single(e => e.Id == CurrentDataEntry.Schema).JSONData);
            if(CurrentDataEntry.Data != null)
            {
                loadingFromData = true;
                loadingFromIndex = 0;
            }
        }
        public enum RestrictionType
        {
            Max,
            Min,
            SecondsElapsed,
            GroupLock
        }
        public static object IsRestrictionValid(dynamic obj, RestrictionType type)
        {
            // there probably is a better way to check if a property exists in a dynamic
            dynamic toReturn = null;
            switch (type)
            {
                case RestrictionType.Max:
                    try
                    {
                        toReturn = int.Parse(obj.max.ToString());
                    }
                    catch(Exception e)
                    {
                        return null;
                    }
                    break;
                case RestrictionType.Min:
                    try
                    {
                        toReturn = int.Parse(obj.min.ToString());
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                    break;
                case RestrictionType.SecondsElapsed:
                    try
                    {
                        toReturn = int.Parse(obj.secondsElapsed.ToString());
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                    break;
                case RestrictionType.GroupLock:
                    try
                    {
                        toReturn = obj.groupLock.ToString();
                    }
                    catch (Exception e)
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
                        case "timer":
                            Button anotherButton = new Button() { Text = "Hasn't Happened", IsEnabled = true, CornerRadius = 8, BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Transparent"), BorderColor = (Color)converter.ConvertFromInvariantString("#4594f5"), BorderWidth = 4, TextColor = (Color)converter.ConvertFromInvariantString("#4594f5"), ClassId = uniqueId, Padding = new Thickness(6, 4) };
                            fields[uniqueId].controls.Add(anotherButton);
                            anotherButton.Clicked += (sender, args) =>
                            {
                                Button selectedButton = (Button)sender as Button;
                                var selectUID = selectedButton.ClassId;
                                fields[selectUID].value = Math.Floor((DateTime.Now - startForm).TotalSeconds);
                                selectedButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5");
                                selectedButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
                                selectedButton.Text = "Happened at " + (Math.Floor((DateTime.Now - startForm).TotalMinutes).ToString() + ":" + ((Math.Floor((DateTime.Now - startForm).TotalSeconds) - (Math.Floor((DateTime.Now - startForm).TotalMinutes) * 60)).ToString().Length == 1 ? "0" : "") + (Math.Floor((DateTime.Now - startForm).TotalSeconds) - (Math.Floor((DateTime.Now - startForm).TotalMinutes) * 60)).ToString());
                            };
                            inputContent.Children.Add(anotherButton);
                            fieldContent.Children.Add(inputContent, 1, 0);
                            if (loadingFromData)
                            {
                                try
                                {
                                    int seconds = int.Parse(CurrentDataEntry.Data.DataValues[loadingFromIndex].ToString());
                                    if (CurrentDataEntry.Data.DataValues[loadingFromIndex] != null && seconds > 0)
                                    {
                                        fields[uniqueId].value = CurrentDataEntry.Data.DataValues[loadingFromIndex];

                                        anotherButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5");
                                        anotherButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
                                        anotherButton.Text = "Previously Happened";
                                    }
                                    
                                    
                                }
                                catch(Exception c)
                                {

                                }
                                loadingFromIndex += 1;
                            }
                            break;
                        case "toggle":
                            Button anotherNewButton = new Button() { Text = content.conditions.options[0].ToString(), IsEnabled = true, CornerRadius = 8, BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Transparent"), BorderColor = (Color)converter.ConvertFromInvariantString("#4594f5"), BorderWidth = 4, TextColor = (Color)converter.ConvertFromInvariantString("#4594f5"), ClassId = uniqueId, Padding = new Thickness(6, 4) };
                            fields[uniqueId].value = content.conditions.options[0].ToString();
                            fields[uniqueId].controls.Add(anotherNewButton);
                            anotherNewButton.Clicked += (sender, args) =>
                            {
                                Button selectedButton = (Button)sender as Button;
                                var selectUID = selectedButton.ClassId;
                                if(selectedButton.Text == fields[selectUID].schemaObject.conditions.options[0].ToString())
                                {
                                    // this will mean it needs to be toggled on
                                    fields[selectUID].value = fields[selectUID].schemaObject.conditions.options[1].ToString();
                                    selectedButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5");
                                    selectedButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
                                    selectedButton.Text = fields[selectUID].value.ToString();
                                }
                                else
                                {
                                    // not lmao
                                    fields[selectUID].value = fields[selectUID].schemaObject.conditions.options[0].ToString();
                                    selectedButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Transparent");
                                    selectedButton.TextColor = (Color)converter.ConvertFromInvariantString("#4594f5");
                                    
                                    selectedButton.Text = fields[selectUID].value.ToString();
                                }
                            };
                            inputContent.Children.Add(anotherNewButton);
                            fieldContent.Children.Add(inputContent, 1, 0);
                            if (loadingFromData)
                            {
                                try
                                {
                                    fields[uniqueId].value = CurrentDataEntry.Data.DataValues[loadingFromIndex];
                                    if (fields[uniqueId].value == fields[uniqueId].schemaObject.conditions.options[1].ToString())
                                    {
                                        // this will mean it needs to be toggled on
                                        fields[uniqueId].value = fields[uniqueId].schemaObject.conditions.options[1].ToString();
                                        anotherNewButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5");
                                        anotherNewButton.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
                                        anotherNewButton.Text = fields[uniqueId].value.ToString();
                                    }
                                    else
                                    {
                                        // not lmao
                                        fields[uniqueId].value = fields[uniqueId].schemaObject.conditions.options[0].ToString();
                                        anotherNewButton.BackgroundColor = (Color)converter.ConvertFromInvariantString("Color.Transparent");
                                        anotherNewButton.TextColor = (Color)converter.ConvertFromInvariantString("#4594f5");

                                        anotherNewButton.Text = fields[uniqueId].value.ToString();
                                    }
                                    
                                }
                                catch (Exception c)
                                {

                                }
                                loadingFromIndex += 1;
                            }
                            break;
                        case "text":
                            fieldContent.Children.Clear();
                            fieldContent.ColumnDefinitions.Clear();
                            fieldContent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star});
                            Label newContent = new Label() { FontSize = 14, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Text = content.prettyName, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(15,0,15,5), TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C") };
                            fieldContent.Children.Add(newContent, 0, 0);
                            break;
                        case "choices":
                            int col = 0;
                            int row = 0;
                            // first check if they should be on separate rows or columns
                            bool inCols = true;
                            int numButtons = 0;
                            int numCharacters = 0;
                            
                            singleRestrictionMapping.Add(content.uniqueId.ToString(), new SingleControlRestriction());
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
                            fieldContent.Children.Add(inputContent, 1, 0);
                            if (loadingFromData)
                            {
                                try
                                {
                                    fields[uniqueId].value = CurrentDataEntry.Data.DataValues[loadingFromIndex];
                                    Button buttonHighlight = ((Button)fields[uniqueId].controls.Where(c => ((Button)c).Text == (string)fields[uniqueId].value).FirstOrDefault());
                                    buttonHighlight.BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5");
                                    buttonHighlight.TextColor = (Color)converter.ConvertFromInvariantString("Color.White");
                                }
                                catch(Exception c)
                                {

                                }
                                
                                loadingFromIndex += 1;
                            }
                            break;
                        case "stepper":
                            Button downButton = new Button() { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, TextColor = (Color)converter.ConvertFromInvariantString("Color.White"), Text = "-", BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5"), CornerRadius = 8, Padding = new Thickness(4), FontAttributes = FontAttributes.Bold, ClassId = uniqueId, FontSize = 18 };
                            Entry stepperValue = new Entry() { VerticalOptions = LayoutOptions.Center, Margin = new Thickness(5,0), Keyboard = Keyboard.Numeric, Text = "0", HorizontalTextAlignment = TextAlignment.Center, TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C"), ClassId = uniqueId};
                            Button upButton = new Button() { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, TextColor = (Color)converter.ConvertFromInvariantString("Color.White"), Text = "+", BackgroundColor = (Color)converter.ConvertFromInvariantString("#4594f5"), CornerRadius = 8, Padding = new Thickness(4), FontAttributes = FontAttributes.Bold, ClassId = uniqueId, FontSize = 18 };
                            if (IsRestrictionValid(content.conditions, RestrictionType.GroupLock) != null)
                            {
                                ((MultiControlRestriction)multiRestrictionMapping[content.conditions.groupLock.ToString()]).valuePairs.Add(content.uniqueId.ToString(), null);
                            }
                            singleRestrictionMapping.Add(content.uniqueId.ToString(), new SingleControlRestriction() { max = IsRestrictionValid(content.conditions, RestrictionType.Max) == null ? null : int.Parse(IsRestrictionValid(content.conditions, RestrictionType.Max).ToString()), min = IsRestrictionValid(content.conditions, RestrictionType.Min) == null ? null : int.Parse(IsRestrictionValid(content.conditions, RestrictionType.Min).ToString()), secondsElapsed = IsRestrictionValid(content.conditions, RestrictionType.SecondsElapsed) == null ? null : int.Parse(IsRestrictionValid(content.conditions, RestrictionType.SecondsElapsed).ToString()) });
                            stepperValue.TextChanged += async (sender, args) =>
                            {
                                
                                Entry selectedStepper = (Entry)sender as Entry;
                                if(selectedStepper.Text != "")
                                {
                                    bool shouldPush = true;
                                    string selectUID = selectedStepper.ClassId;


                                    if(fields[selectUID].value == null)
                                    {
                                        // maybe add a default value structure? This would be easy to code
                                        fields[selectUID].value = 0;
                                    }
                                    

                                    if(!int.TryParse(selectedStepper.Text, out int output))
                                    {
                                        shouldPush = false;
                                    }else if((((SingleControlRestriction)singleRestrictionMapping[selectUID]).max != null && ((SingleControlRestriction)singleRestrictionMapping[selectUID]).max < int.Parse(selectedStepper.Text)) || (((SingleControlRestriction)singleRestrictionMapping[selectUID]).min != null && ((SingleControlRestriction)singleRestrictionMapping[selectUID]).min > int.Parse(selectedStepper.Text)))
                                    {
                                        shouldPush = false;
                                    }
                                    else
                                    {
                                        
                                        var containingGroupLock = IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock) == null ? null : IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock).ToString();
                                        if(containingGroupLock != null)
                                        {
                                            int? min = ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).min;
                                            int? max = ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).max;
                                            int? ours = 0;
                                            foreach (var lockItem in ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).valuePairs)
                                            {
                                                if(lockItem.Value != null && lockItem.Key != selectUID)
                                                {
                                                    ours += lockItem.Value;
                                                }
                                                
                                            }
                                            // this is checking if this value is valid!
                                            ours += int.Parse(selectedStepper.Text);
                                            if (min != null && ours < min)
                                            {
                                                shouldPush = false;
                                            }
                                            else if(max != null && ours > max)
                                            {
                                                shouldPush = false;
                                            }

                                        }
                                    }

                                    if (shouldPush)
                                    {
                                        fields[selectUID].value = int.Parse(selectedStepper.Text);
                                        var containingGroupLock = IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock) == null ? null : IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock).ToString();
                                        if (containingGroupLock != null)
                                        {
                                            ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).valuePairs[selectUID] = int.Parse(selectedStepper.Text);
                                        }
                                    }
                                    else
                                    {
                                        selectedStepper.TextColor = (Color)converter.ConvertFromInvariantString("#EB264A");
                                        selectedStepper.Text = fields[selectUID].value.ToString();
                                    }
                                    
                                    await Task.Delay(3000);
                                    selectedStepper.TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C");



                                }
                                
                            };
                            

                            downButton.Clicked += async (sender, args) =>
                            {
                                Button selectedButton = (Button)sender as Button;
                                string selectUID = selectedButton.ClassId;
                                if (fields[selectUID].value == null)
                                {
                                    fields[selectUID].value = 0;
                                }
                                Entry selectedStepper = (Entry)(fields[selectUID].controls.Where(c => c.GetType().GetProperty("Text").GetValue(c, null).ToString() == fields[selectUID].value.ToString()).FirstOrDefault());
                                bool shouldPush = true;
                                int prevInt = int.Parse(fields[selectUID].value.ToString());


                                if (!int.TryParse(selectedStepper.Text, out int output))
                                {
                                    shouldPush = false;
                                }
                                else if ((((SingleControlRestriction)singleRestrictionMapping[selectUID]).max != null && ((SingleControlRestriction)singleRestrictionMapping[selectUID]).max < prevInt-1) || (((SingleControlRestriction)singleRestrictionMapping[selectUID]).min != null && ((SingleControlRestriction)singleRestrictionMapping[selectUID]).min > prevInt-1))
                                {
                                    shouldPush = false;
                                }
                                else
                                {
                                    var containingGroupLock = IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock) == null ? null : IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock).ToString();
                                    if (containingGroupLock != null)
                                    {
                                        int? min = ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).min;
                                        int? max = ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).max;
                                        int? ours = 0;
                                        foreach (var lockItem in ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).valuePairs)
                                        {
                                            if (lockItem.Value != null && lockItem.Key != selectUID)
                                            {
                                                ours += lockItem.Value;
                                            }

                                        }
                                        // this is checking if this value is valid!
                                        ours += prevInt-1;
                                        if (min != null && ours < min)
                                        {
                                            shouldPush = false;
                                        }
                                        else if (max != null && ours > max)
                                        {
                                            shouldPush = false;
                                        }

                                    }
                                }

                                if (shouldPush)
                                {
                                    fields[selectUID].value = (prevInt-1);
                                    var containingGroupLock = IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock) == null ? null : IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock).ToString();
                                    if (containingGroupLock != null)
                                    {
                                        ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).valuePairs[selectUID] = prevInt-1;
                                    }
                                }
                                else
                                {
                                    selectedStepper.TextColor = (Color)converter.ConvertFromInvariantString("#EB264A");
                                }
                                selectedStepper.Text = fields[selectUID].value.ToString();
                                await Task.Delay(3000);
                                selectedStepper.TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C");

                            };
                            upButton.Clicked += async (sender, args) =>
                            {
                                Button selectedButton = (Button)sender as Button;
                                string selectUID = selectedButton.ClassId;
                                if (fields[selectUID].value == null)
                                {
                                    fields[selectUID].value = 0;
                                }
                                Entry selectedStepper = (Entry)(fields[selectUID].controls.Where(c => c.GetType().GetProperty("Text").GetValue(c, null).ToString() == fields[selectUID].value.ToString()).FirstOrDefault());
                                bool shouldPush = true;
                                int prevInt = int.Parse(fields[selectUID].value.ToString());


                                if (!int.TryParse(selectedStepper.Text, out int output))
                                {
                                    shouldPush = false;
                                }
                                else if ((((SingleControlRestriction)singleRestrictionMapping[selectUID]).max != null && ((SingleControlRestriction)singleRestrictionMapping[selectUID]).max < prevInt + 1) || (((SingleControlRestriction)singleRestrictionMapping[selectUID]).min != null && ((SingleControlRestriction)singleRestrictionMapping[selectUID]).min > prevInt + 1))
                                {
                                    shouldPush = false;
                                }
                                else
                                {
                                    var containingGroupLock = IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock) == null ? null : IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock).ToString();
                                    if (containingGroupLock != null)
                                    {
                                        int? min = ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).min;
                                        int? max = ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).max;
                                        int? ours = 0;
                                        foreach (var lockItem in ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).valuePairs)
                                        {
                                            if (lockItem.Value != null && lockItem.Key != selectUID)
                                            {
                                                ours += lockItem.Value;
                                            }

                                        }
                                        // this is checking if this value is valid!
                                        ours += prevInt + 1;
                                        if (min != null && ours < min)
                                        {
                                            shouldPush = false;
                                        }
                                        else if (max != null && ours > max)
                                        {
                                            shouldPush = false;
                                        }

                                    }
                                }

                                if (shouldPush)
                                {
                                    fields[selectUID].value = (prevInt + 1);
                                    var containingGroupLock = IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock) == null ? null : IsRestrictionValid(fields[selectUID].schemaObject.conditions, RestrictionType.GroupLock).ToString();
                                    if (containingGroupLock != null)
                                    {
                                        ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).valuePairs[selectUID] = prevInt + 1;
                                    }
                                }
                                else
                                {
                                    selectedStepper.TextColor = (Color)converter.ConvertFromInvariantString("#EB264A");
                                }
                                selectedStepper.Text = fields[selectUID].value.ToString();
                                await Task.Delay(3000);
                                selectedStepper.TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C");
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
                            fieldContent.Children.Add(inputContent, 1, 0);
                            if (loadingFromData)
                            {
                                try
                                {
                                    fields[uniqueId].value = CurrentDataEntry.Data.DataValues[loadingFromIndex];
                                    var containingGroupLock = IsRestrictionValid(fields[uniqueId].schemaObject.conditions, RestrictionType.GroupLock) == null ? null : IsRestrictionValid(fields[uniqueId].schemaObject.conditions, RestrictionType.GroupLock).ToString();
                                    ((MultiControlRestriction)multiRestrictionMapping[containingGroupLock]).valuePairs[uniqueId] = (int)fields[uniqueId].value;
                                    stepperValue.Text = fields[uniqueId].value.ToString();
                                }
                                catch(Exception c)
                                {

                                }
                                
                                loadingFromIndex += 1;
                            }
                            break;
                        case "dropdown":
                            Picker newPicker = new Picker() { TextColor = (Color)converter.ConvertFromInvariantString("#4594f5"), FontAttributes = FontAttributes.Bold, ClassId = uniqueId, HorizontalTextAlignment=TextAlignment.Center };
                            newPicker.Title = "Select an Option";
                            foreach(var choice in content.conditions.options)
                            {
                                // first one is selected
                                newPicker.Items.Add(choice.ToString());
                            }
                            newPicker.SelectedIndexChanged += async (sender, args) =>
                            {
                                Picker selectedPicker = (Picker)sender as Picker;
                                string selectUID = selectedPicker.ClassId;
                                fields[uniqueId].value = selectedPicker.Items[selectedPicker.SelectedIndex];
                            };
                            inputContent.Children.Add(newPicker, 0, 0);
                            fields[uniqueId].controls.Add(newPicker);
                            fieldContent.Children.Add(inputContent, 1, 0);
                            if (loadingFromData)
                            {
                                try
                                {
                                    fields[uniqueId].value = CurrentDataEntry.Data.DataValues[loadingFromIndex];
                                    newPicker.SelectedIndex = newPicker.Items.IndexOf((string)CurrentDataEntry.Data.DataValues[loadingFromIndex]);
                                }
                                catch(Exception c)
                                {

                                }
                                
                                loadingFromIndex += 1;
                            }
                            break;
                        default:
                            singleRestrictionMapping.Add(content.uniqueId.ToString(), new SingleControlRestriction());
                            inputContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            inputContent.Children.Add(new Label() { Text = "Not Implemented", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }, 0, 0);
                            fieldContent.Children.Add(inputContent, 1, 0);
                            break;
                    }
                    
                    
                    ((StackLayout)dynamicLayouts[starter.uniqueId.ToString()]).Children.Add(fieldContent);
                    // remember submission order and add to specific dictionary based on the content needed
                    submitMethod.Add(content.uniqueId.ToString());
                    
                }
                else
                {
                    // add stack to parent, and then create another object for the next iteration
                    StackLayout newParent = new StackLayout() { ClassId = content.uniqueId };
                    if(IsRestrictionValid(content.conditions, RestrictionType.Max) != null || IsRestrictionValid(content.conditions, RestrictionType.Min) != null)
                    {
                        MultiControlRestriction newRestriction = new MultiControlRestriction();
                        newRestriction.max = IsRestrictionValid(content.conditions, RestrictionType.Max) == null ? null : int.Parse(IsRestrictionValid(content.conditions, RestrictionType.Max).ToString());
                        newRestriction.min = IsRestrictionValid(content.conditions, RestrictionType.Min) == null ? null : int.Parse(IsRestrictionValid(content.conditions, RestrictionType.Min).ToString());
                        multiRestrictionMapping.Add(content.uniqueId.ToString(), newRestriction);
                    }
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
                disabledMenu.TranslationY = disabledMenu.Height + 20;
            };

            
            foreach(var category in formObject.categories)
            {
                StackLayout newElement = new StackLayout() { ClassId = category.uniqueId };
                // create top grid or label depending on if it's needed due to "expectedTime" property
                if(category.expectedStart != null)
                {
                    Grid categoryHeader = new Grid() { ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Auto } }, Margin = new Thickness(0, 30, 0, 0), HorizontalOptions = LayoutOptions.Center };
                    Label categoryLabel = new Label() { Text = category.prettyName, FontSize = 28, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, TextColor = (Color)converter.ConvertFromInvariantString("#0F3F8C") };
                    /*Frame categoryStartFrame = new Frame() { CornerRadius = 8, Padding = new Thickness(8, 4), HorizontalOptions = LayoutOptions.Start, BackgroundColor = (Color)converter.ConvertFromInvariantString("#0F3F8C"), VerticalOptions = LayoutOptions.Center, Margin = new Thickness(3, 0), ClassId = category.uniqueId };
                    Label categoryStartLabel = new Label() { Text = "Start", TextColor = (Color)converter.ConvertFromInvariantString("Color.White"), FontSize = 18, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center, ClassId = category.uniqueId };
                    categoryStartFrame.Content = categoryStartLabel;
                    
                    
                    categoryHeader.Children.Add(categoryStartFrame, 1, 0);*/
                    categoryHeader.Children.Add(categoryLabel, 0, 0);
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
            bar_MatchNumber.Text = "Match " + CurrentDataEntry.Number.ToString();
            bar_TeamNumber.Text = CurrentDataEntry.TeamIdentifier.ToString();
            StartTimer();
            startForm = DateTime.Now;
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
        private bool firstAdjust = true;
        private async void expandOptions(object sender, EventArgs e)
        {
            if (firstAdjust)
            {
                disabledMenu.TranslationY = disabledMenu.Height + 20;
                disabledMenu.IsVisible = true;
                firstAdjust = false;
            }
                
            Action<double> heicallback = input => { options.Height = input; };
            Action<double> corcallback = input => { optionsBar.CornerRadius = (float)input; };
            //var startingHeight = optionsExpanded ? (checkWidth.Width == -1 ? 100 : checkWidth.Width)+ 60 : 0;
            
            // ensure that no overflow content is showed
            

            if (optionsExpanded)
            {
                
                optionsExpanded = false;
                var endingHeight = 0;
                //await optionsContent.FadeTo(1, 100);
                //optionsContent.Opacity = 0;
                //optionsContent.IsVisible = true;

                var heianim = new Animation(heicallback, 160, 0, Easing.CubicInOut);
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
                //lastExpandHeight = (checkWidth.Width == -1 ? 120 : checkWidth.Width) + 60;
                AbsoluteLayout.SetLayoutBounds(optionsBarParent, new Rectangle(0, 0, 1, .36));
                optionsExpanded = true;
                //var endingHeight = lastExpandHeight;
                var heianim = new Animation(heicallback, 0, 160, Easing.CubicInOut);
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
        private int[] ignoreControls = new int[] { -1, 2 };
        private async void saveMatch(object sender, EventArgs e)
        {
            bool ans = true;
            if((DateTime.Now - startForm).TotalSeconds < 150 || fields.Where(f => f.Value.value == null && (!ignoreControls.Contains((int)f.Value.schemaType))).ToArray().Length > 0)
            {
                ans = await DisplayAlert("Not Yet Finished", "This form hasn't been completely filled out OR you haven't been in this match for long enough yet. Do you wish to still submit?", "Submit", "Cancel");
            }
            
            if (ans)
            {
                var exportedData = new MatchData();
                exportedData.ControlContinuity = "";
                var dataValuesRaw = new List<object>();
                foreach (var control in fields)
                {
                    if (!ignoreControls.Contains((int)control.Value.schemaType))
                    {
                        // do not ignore a targeted control
                        exportedData.ControlContinuity += ((int)control.Value.schemaType).ToString();
                        dataValuesRaw.Add(control.Value.value);
                    }

                }
                exportedData.DataValues = dataValuesRaw.ToArray();
                Console.WriteLine(exportedData.DataValues.Length);
                ApplicationDataHandler.AvailableEntries.Single(f => f.Id == CurrentDataEntry.Id).LastEdited = DateTime.Now;
                ApplicationDataHandler.AvailableEntries.Single(f => f.Id == CurrentDataEntry.Id).Data = exportedData;
                ApplicationDataHandler.AvailableEntries.Single(f => f.Id == CurrentDataEntry.Id).Completed = true;
                await ApplicationDataHandler.Instance.SaveMatches();


                Navigation.PushAsync(new MasterPage());
            }
            else
            {
                // we know that options IS expanded, so we can contract it
                expandOptions(null, null);
            }
            

        }
    }
}