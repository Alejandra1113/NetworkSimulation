namespace NetSimul;

using System;
using System.IO;
using System.Collections.Generic;
using NetSimul.Intruction;
using NetSimul.NetWork;

public class Program
{

    public static void Main(string[] args)
    {
        StreamReader script;
        StreamReader config;
        Comand[] comands;
        options option;

        try
        {
            script = new StreamReader("./script.txt");
            config = new StreamReader("./config.txt");
            comands = ParserStream(script);
            option = new options(config);
        }
        catch (System.Exception)
        {
            throw new FileLoadException("no se pudo parsear la configuracion");
        }

        Network net = new Network();
        int index = 0;
        int time_ms = -1;
        do
        {
            time_ms++;
            UpdateInfo(comands, option, ref index, time_ms, net);
        } while (index < comands.Length | UpdateStats(net));
        UpdateStats(net);
        UpdateStats(net);
        UpdateStats(net);
        UpdateStats(net);
    }

    private static Comand[] ParserStream(StreamReader script)
    {
        string? line;
        Comand newcomand;
        int count = 0;
        List<Comand> comands = new List<Comand>();
        while ((line = script.ReadLine()) != null)
        {
            try
            {
                ParserLine(line, out newcomand);
                comands.Add(newcomand);
            }
            catch (Exception)
            {
                Console.WriteLine($"error de sintaxis en la linea {count} del script\n");
            }
            count++;
        }
        comands.Sort(new ComandComparer());
        return comands.ToArray();
    }

    private static void ParserLine(string line, out Comand newcomand)
    {
        string[] comand = line.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (comand.Length < 3) throw new FormatException("Una linea correcta tiene al menos la sintaxis <time> <comand> <optioncomand>\n");

        int time_ms;
        if (!int.TryParse(comand[0], out time_ms)) throw new FormatException("Por favor, ingrese primero el time\n");

        switch (comand[1])
        {
            case "create":
                newcomand = new CreateComand(comand, time_ms);
                break;
            case "connect":
                newcomand = new ConnectComand(comand, time_ms);
                break;
            case "mac":
                newcomand = new MacComand(comand, time_ms);
                break;
            case "ip":
                newcomand = new IPComand(comand, time_ms);
                break;
            case "send":
                newcomand = new SendComand(comand, time_ms);
                break;
            case "send_frame":
                newcomand = new Send_FrameComand(comand, time_ms);
                break;
            case "send_packet":
                newcomand = new Send_PacketComand(comand, time_ms);
                break;
            case "ping":
                newcomand = new PingComand(comand, time_ms);
                break;
            case "route_reset":
                newcomand = new Route_ResetComand(comand, time_ms);
                break;
            case "route_add":
                newcomand = new Route_AddComand(comand, time_ms);
                break;
            case "route_delete":
                newcomand = new Route_DeleteComand(comand, time_ms);
                break;
            case "disconnect":
                newcomand = new DisconnectComand(comand, time_ms);
                break;
            default:
                throw new NotSupportedException("No existe ese comando\n");
        }
    }

    private static void UpdateInfo(Comand[] comands, options options, ref int index, int time_ms, Network net)
    {
        for (; index < comands.Length && comands[index].Time_ms == time_ms; index++)
            net.Execute(comands[index], options);
    }

    private static bool UpdateStats(Network net) => net.Update();
}