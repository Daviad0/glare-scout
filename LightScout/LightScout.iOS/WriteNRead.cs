using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace LightScout.iOS
{
    public class WriteNRead : DataStore
    {
        public void SaveData(string filename, string data)
        {
            
        }
        public string LoadData(string filename)
        {
            return "Hello :)";
        }
    }
}