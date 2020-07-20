using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout
{
    public interface USBCommunication
    {
        void SendData(string data);
    }
}
