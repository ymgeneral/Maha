using PoplarCloud.EventsAndEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoplarCloud.Interfaces
{
    public interface IAnalysis
    {
        event EventHandler InvalidPacketReceived;
        event IDataPacketHanler DataPacketReceived;
        void Write(byte[] buffer);
    }
}
