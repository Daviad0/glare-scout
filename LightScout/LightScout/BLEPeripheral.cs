using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout
{
    public interface BLEPeripheral
    {
        void StartAdvertising(string serviceUUID, string serviceName);
    }
}
