using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout.Models
{
    public class LSConfiguration
    {
        public int NumberOfMatches { get; set; }
        public string TabletIdentifier { get; set; }
        public int MaxMatches { get; set; }
        public int SubmitOffset { get; set; }
        public int TeamOfOwnership { get; set; }
        public string ScouterOfOwnership { get; set; }
        public int BluetoothFailureStage { get; set; }
        public string[] ScouterNames { get; set; }
        public string CurrentEventCode { get; set; }
        public DeviceInformation SelectedDeviceInformation { get; set; }
    }
}
