using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSimul.Component
{
    public interface IDatagramRecorder
    {
        void Record(string fromIp, string data, int time_ms, byte protocolo);
    }
}