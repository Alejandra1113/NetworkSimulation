using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using NetSimul.Extensions;
namespace NetSimul.Component;

public class Route
{

    public int[] destination = new int[4];
    public int[] mask =  new int[4];

    public string gateway;
    public int Interface; 

    public Route(string destination, string mask, string gateway, int Interface)
    {
        string[] stringDestination = destination.Split('.');
        string[] strMask = mask.Split('.');
        for (int i=0; i< 4;i++)
        {
            this.destination[i] = int.Parse(stringDestination[i]);
            this.mask[i] = int.Parse(strMask[i]);
        }
      
        this.gateway = gateway;
        this.Interface = Interface;
    }

    public static bool SelectRoute(Route route, string IP)
    {
       string[] stringIp = IP.Split('.'); 
       int match;
       for (int i = 0; i < 4; i++)
       {   
           match = route.mask[i]&int.Parse(stringIp[i]);
           if( match != route.destination[i])
           {
              return false;
           }
       }
       return true;

    } 
    
    public static int CompareDestinationRoutes(Route r1, Route r2)
    {
        
        for (int i = 0; i < 4; i++)
        {
            if(r1.destination[i] > r2.destination[i])
              return 1;
            if(r1.destination[i]< r2.destination[i])
              return -1;
        }
        return 0;

    }
    public static int CompareMaskRoutes(Route r1, Route r2)
    {
        
        for (int i = 0; i < 4; i++)
        {
            if(r1.mask[i] > r2.mask[i])
              return 1;
            if(r1.mask[i]< r2.mask[i])
              return -1;
        }
        return 0;

    }
    public static string DotToBin(string dot)
    {
         string[] stringDot = dot.Split('.'); 
         string bin = "";
         for (int i = 0; i < 4; i++)
         {
             bin += stringDot[i].ConvertToBase(10,2,8);
         }
         return bin;
    }

    public static string BinToDot(string bin)
    {
        string dot="";
        for (int i = 0; i < 3; i++)
        {
            dot += bin.Substring(i*8,8).ConvertToBase(2,10) + ".";
        }
        dot += bin.Substring(24,8).ConvertToBase(2,10);
        return dot;
    }

    public static string ByteToStr(byte[] byteIp)
    {
        if (byteIp.Length != 4) throw new InvalidCastException();
        string str = $"{byteIp[0]}";
        
        for (int i = 1; i < byteIp.Length; i++)
        {
            str += $".{byteIp[i]}";
        }
        return str;
    }

    public static byte[] StrToByte(string ip)
    {
        byte[] byteIp = new byte[4];
        string[] strIp = ip.Split('.');

        if (strIp.Length != 4) throw new InvalidCastException();
        for (int i = 0; i < byteIp.Length; i++)
        {
            int num = int.Parse(strIp[i]);
            if (num < 0 || num > 255) throw new InvalidCastException();
            byteIp[i] = (byte)num;
        }
        return byteIp;
    }
}