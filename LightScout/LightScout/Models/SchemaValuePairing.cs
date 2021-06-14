using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout.Models
{
    public class SchemaValuePairing
    {
        public dynamic schemaObject;
        public enum SchemaType
        {
            parent = -1,
            choices = 0,
            stepper = 1,
            text = 2,
            integer = 3,
            dropdown = 4,
            timer = 5
        }
        public SchemaType schemaType;
        public object value;
        public List<object> controls = new List<object>();
    }
}
