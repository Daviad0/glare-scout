using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout.Models
{
    public class TeamMatchViewItem
    {
        public int MatchNumber { get; set; }
        public int TeamNumber { get; set; }
        public string TeamName { get; set; }
        public bool IsRed { get; set; }
        public bool IsBlue { get; set; }
        public bool IsUpNext { get; set; }
        public bool Completed { get; set; }
        public string TabletName { get; set; }
    }
}
