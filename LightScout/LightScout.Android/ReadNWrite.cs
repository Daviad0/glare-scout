using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LightScout.Models;
using Newtonsoft.Json;

[assembly: Xamarin.Forms.Dependency(typeof(LightScout.Droid.ReadNWrite))]
namespace LightScout.Droid
{
    public class ReadNWrite : DataStore
    {
        public void SaveData(string filename, TeamMatch modeldata)
        {
            
            //ADD FILE PARSING HERE
            var docpath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            try
            {
                System.IO.Directory.CreateDirectory(Path.Combine(docpath, "FRCLightScout"));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            docpath = Path.Combine(docpath, "FRCLightScout");
            var data = "";
            var finalPath = Path.Combine(docpath, filename);

            try
            {
                var beforedata = File.ReadAllText(finalPath);
                var modelstochange = JsonConvert.DeserializeObject<List<TeamMatch>>(beforedata);
                var specificmodeltochange = modelstochange.Where(x => x.TeamNumber == modeldata.TeamNumber && x.MatchNumber == modeldata.MatchNumber).FirstOrDefault();

                specificmodeltochange.MatchNumber = modeldata.MatchNumber;
                specificmodeltochange.TeamNumber = modeldata.TeamNumber;

                modelstochange.Remove(modelstochange.Where(x => x.TeamNumber == modeldata.TeamNumber && x.MatchNumber == modeldata.MatchNumber).FirstOrDefault());
                modelstochange.Add(specificmodeltochange);
                modelstochange = modelstochange.OrderBy(x => x.MatchNumber).ToList();

                data = JsonConvert.SerializeObject(modelstochange);
            }
            catch(Exception ex)
            {
                var createanewlist = new List<TeamMatch>();
                createanewlist.Add(modeldata);
                
                data = JsonConvert.SerializeObject(createanewlist);
            }
            
            try
            {
                System.IO.File.WriteAllText(finalPath, data);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public string LoadData(string filename)
        {
            var docpath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "FRCLightScout");
            var finalPath = Path.Combine(docpath, filename);
            var result = File.ReadAllText(finalPath);
            return result;
        }
    }
}