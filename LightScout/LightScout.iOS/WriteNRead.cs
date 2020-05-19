using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using LightScout.Models;
using UIKit;

namespace LightScout.iOS
{
    public class WriteNRead : DataStore
    {
        public void SaveData(string filename, TeamMatch data)
        {
            
        }
        public string LoadData(string filename)
        {
            return "Hello :)";
        }
    }
}