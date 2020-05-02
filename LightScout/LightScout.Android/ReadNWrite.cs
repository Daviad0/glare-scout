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

[assembly: Xamarin.Forms.Dependency(typeof(LightScout.Droid.ReadNWrite))]
namespace LightScout.Droid
{
    public class ReadNWrite : DataStore
    {
        public void SaveData(string filename, string data)
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
            var finalPath = Path.Combine(docpath, filename);
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