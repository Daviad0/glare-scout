using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout.Models
{
    public class MultiControlRestriction
    {
        public Dictionary<string, int?> valuePairs = new Dictionary<string, int?>();
        public int? max;
        public int? min;
    }
}
