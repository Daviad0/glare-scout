using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout.Models
{
    public class TeamMatch
    {
        public bool ControlPanelRotation { get; set; }
        public bool ControlPanelPosition { get; set; }
        public bool ClimbBalance { get; set; }
        public string ScoutName { get; set; }
    }
}
