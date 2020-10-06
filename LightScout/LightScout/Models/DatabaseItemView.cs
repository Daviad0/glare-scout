using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout.Models
{
    public class DatabaseItemView
    {
        public string Id { get; set; }
        public string TeamNumber { get; set; }
        public bool Animated { get; set; }
        public bool Visible { get; set; }
        public double SetHeight { get; set; }
    }
}
