using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace NetSimul.Component;

public static class ARPMessage
{
    public static bool IsARPQ(string binaryData, string toMac)
    {
        if(binaryData.Length == 64 && toMac == "ffff")
        {
            string ARPQ = binaryData.Substring(0,32);
            return ARPQ == "01000001010100100101000001010001";
        }
        return false;
    }
    public static bool IsARPR(string binaryData)
    {
        if(binaryData.Length == 64)
        {
            string ARPR = binaryData.Substring(0,32);
            return ARPR == "01000001010100100101000001010010";
        }
        return false;
    }

    public static string DataARPQ(string toIP)
    {
        return "01000001010100100101000001010001" + toIP;
    }

    public static string DataARPR(string fromIP)
    {
       
        return "01000001010100100101000001010010" + fromIP;
    }

       public static List<bool> ARPQFrame(string fromMac, string toIp, IErrorDetectionAlg error)
        {
              Frame msg = new Frame(fromMac,"ffff",ARPMessage.DataARPQ(Route.DotToBin(toIp)), error, DataConverterOption.binario);
              List<bool> neww;
              Frame.PackFrame(msg,out neww);
              return neww;
        }
        public static List<bool> ARPRFrame(string fromMac, string toMac, string fromIP,IErrorDetectionAlg error)
        {
            Frame msg = new Frame(fromMac,toMac,ARPMessage.DataARPR(Route.DotToBin(fromIP)), error,DataConverterOption.binario);
            List<bool> neww;
            Frame.PackFrame(msg,out neww);
            return neww;
        }
}