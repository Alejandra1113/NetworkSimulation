using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSimul.Component
{
    public interface IPcIP : IPcDuplex
    {
        (string,string) Ip
        {
            get; set;
        }

        void Ping(string ToIp);

        void SendDatagram(string ToIp, string data);
    }
}
