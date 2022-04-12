using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout
{
    public interface USBCommunication
    {
        void StartChecking();

        bool CheckForConnection();

       
        void SendData(string data);
    }
}
