using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSimul.Component
{
    public interface IFrameRecorder
    {
        void Record(string fromMac, string data, int time_ms, bool error);
    }
}
