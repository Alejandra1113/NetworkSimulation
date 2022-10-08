namespace NetSimul.Intruction;

using NetSimul.Extensions;

public enum TypeComponent
{
    Hub,
    Host,
    Switch,
    Router
}

public enum TypeComand
{
    CreateComand,
    MacComand,
    IPComand,
    Route_ResetComand,
    Route_AddComand,
    Route_DeleteComand,
    ConnectComand,
    SendComand,
    Send_FrameComand,
    PingComand,
    Send_PacketComand,
    DisconnectComand,
    nullComand
}

public class ComandComparer : IComparer<Comand>
{
    public int Compare(Comand? x, Comand? y)
    {
        int priorityX = (x != null) ? (int)x.Type : (int)TypeComand.nullComand;
        int priorityY = (y != null) ? (int)y.Type : (int)TypeComand.nullComand;
        int CompareToTime = (x != null && y != null) ? x.Time_ms.CompareTo(y.Time_ms) : (x == null && y != null) ? -1 : (x != null && y == null) ? 1 : 0;
        return (CompareToTime == 0) ? priorityX - priorityY : CompareToTime;
    }
}

public abstract class Comand
{
    protected int time_ms;
    protected TypeComand type;

    protected Comand(int time_ms)
    {
        this.time_ms = time_ms;
    }

    public int Time_ms => time_ms;
    public TypeComand Type => type;
}

public class CreateComand : Comand
{
    private TypeComponent component;
    private int countPort;
    private string name;

    public CreateComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.CreateComand;
        switch (comand[2])
        {
            case "host":
                if (comand.Length != 4) throw new FormatException($"el comando {comand[1]} {comand[2]} tiene 1 argumentos, y se pasaron {comand.Length - 3}\n");
                countPort = 1;
                component = TypeComponent.Host;
                name = comand[3];
                break;
            case "hub":
                if (comand.Length != 5) throw new FormatException($"el comando {comand[1]} {comand[2]} tiene 2 argumentos, y se pasaron {comand.Length - 3}\n");
                if (!int.TryParse(comand[4], out countPort)) throw new FormatException("Por favor, ingrese un numero de puertos\n");
                component = TypeComponent.Hub;
                name = comand[3];
                break;
            case "switch":
                if (comand.Length != 5) throw new FormatException($"el comando {comand[1]} {comand[2]} tiene 2 argumentos, y se pasaron {comand.Length - 3}\n");
                if (!int.TryParse(comand[4], out countPort)) throw new FormatException("Por favor, ingrese un numero de puertos\n");
                component = TypeComponent.Switch;
                name = comand[3];
                break;
            case "router":
                if (comand.Length != 5) throw new FormatException($"el comando {comand[1]} {comand[2]} tiene 2 argumentos, y se pasaron {comand.Length - 3}\n");
                if (!int.TryParse(comand[4], out countPort)) throw new FormatException("Por favor, ingrese un numero de puertos\n");
                component = TypeComponent.Router;
                name = comand[3];
                break;
            default:
                throw new NotSupportedException("No existe ese componente\n");
        }
    }

    public CreateComand(int time_ms, TypeComponent component, string name, int countPort = 1) : base(time_ms)
    {
        type = TypeComand.CreateComand;
        this.component = component;
        this.countPort = countPort;
        this.name = name;
    }

    public TypeComponent Component => component;
    public int CountPort => countPort;
    public string Name => name;
}

public class MacComand : Comand
{
    private string name;
    
    private string macId;
    private int portNumber;

    public MacComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.MacComand;
        if (comand.Length != 4) throw new FormatException($"el comando {comand[1]} tiene 2 argumentos, y se pasaron {comand.Length - 2}\n");
        macId = (comand[3].IsHexNumber()) ? comand[3] : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        string[] name_interface = comand[2].Split(":", StringSplitOptions.RemoveEmptyEntries);
        name = name_interface[0];
        portNumber = (name_interface.Length > 1) ? int.Parse(name_interface[1]) - 1 : 0;
    }

    public MacComand(int time_ms, string name, int portNumber, string macId) : base(time_ms)
    {
        type = TypeComand.MacComand;
        this.macId = (macId.IsHexNumber()) ? macId : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        this.name = name;
        this.portNumber = portNumber;
    }

    public string Name => name;
    public string MacId => macId;
    public int PortNumber => portNumber;
}

public class IPComand : Comand
{
    private string name;
    private (string, string) ip;
    private int portNumber;

    public IPComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.IPComand;
        if (comand.Length != 5) throw new FormatException($"el comando {comand[1]} tiene 2 argumentos, y se pasaron {comand.Length - 2}\n");
        ip.Item1 = (comand[3].IsIpNumber()) ? comand[3] : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        ip.Item2 = (comand[4].IsIpNumber()) ? comand[4] : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        string[] name_interface = comand[2].Split(":", StringSplitOptions.RemoveEmptyEntries);
        name = name_interface[0];
        portNumber = (name_interface.Length > 1) ? int.Parse(name_interface[1]) - 1 : 0;
    }

    public IPComand(int time_ms, string name, int portNumber, string ipaddress, string mask) : base(time_ms)
    {
        type = TypeComand.IPComand;
        this.ip.Item1 = (ipaddress.IsIpNumber()) ? ipaddress : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        this.ip.Item2 = (mask.IsIpNumber()) ? mask : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        this.name = name;
        this.portNumber = portNumber;
    }

    public string Name => name;

    public (string, string) Ip => ip;

    public int PortNumber => portNumber;
}

public class ConnectComand : Comand
{
    private string name1;
    private string name2;
    private int port1;
    private int port2;

    public ConnectComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.ConnectComand;
        if (comand.Length != 4) throw new FormatException($"el comando {comand[1]} tiene 2 argumentos, y se pasaron {comand.Length - 2}\n");
        string[] split1 = comand[2].Split('_');
        string[] split2 = comand[3].Split('_');
        name1 = string.Concat(split1[..(split1.Length - 1)]);
        name2 = string.Concat(split2[..(split2.Length - 1)]);
        port1 = int.Parse(split1[split1.Length - 1]) - 1;
        port2 = int.Parse(split2[split2.Length - 1]) - 1;
    }

    public ConnectComand(int time_ms, string name1, string name2, int port1, int port2) : base(time_ms)
    {
        type = TypeComand.ConnectComand;
        this.name1 = name1;
        this.name2 = name2;
        this.port1 = port1;
        this.port2 = port2;
    }

    public string Name1 => name1;
    public string Name2 => name2;
    public int Port1 => port1;
    public int Port2 => port2;
}

public class SendComand : Comand
{
    private string name;
    private bool[]? data;

    public SendComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.SendComand;
        if (comand.Length != 4) throw new FormatException($"el comando {comand[1]} tiene 2 argumentos, y se pasaron {comand.Length - 2}\n");
        if (!comand[3].IsBinNumber(out data)) throw new FormatException($"el campo <data> tiene que ser un numero binario");
        name = comand[2];
    }

    public SendComand(int time_ms, string name, bool[] data) : base(time_ms)
    {
        type = TypeComand.SendComand;
        this.name = name;
        this.data = data;
    }

    public string Name => name;
    public bool[]? Data => data;
}

public class Send_FrameComand : Comand
{
    private string name;
    private string macId;
    private string data;

    public Send_FrameComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.Send_FrameComand;
        if (comand.Length != 5) throw new FormatException($"el comando {comand[1]} tiene 3 argumentos, y se pasaron {comand.Length - 2}\n");
        macId = (comand[3].IsHexNumber()) ? comand[3] : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        data = (comand[4].IsHexNumber()) ? comand[4] : throw new FormatException($"el campo <data> tiene que ser un numero hexadecimal");
        name = comand[2];
    }

    public Send_FrameComand(int time_ms, string name, string macId, string data) : base(time_ms)
    {
        type = TypeComand.Send_FrameComand;
        this.macId = (macId.IsHexNumber()) ? macId : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        this.data = (data.IsHexNumber()) ? data : throw new FormatException($"el campo <data> tiene que ser un numero hexadecimal");
        this.name = name;
    }

    public string Name => name;
    public string Data => data;
    public string MacId => macId;
}

public class PingComand : Comand
{
    private string name;
    private string ipaddress;

    public PingComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.PingComand;
        if (comand.Length != 4) throw new FormatException($"el comando {comand[1]} tiene 3 argumentos, y se pasaron {comand.Length - 2}\n");
        ipaddress = (comand[3].IsIpNumber()) ? comand[3] : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        name = comand[2];
    }

    public PingComand(int time_ms, string name, string ipaddress, string data) : base(time_ms)
    {
        type = TypeComand.PingComand;
        this.ipaddress = (ipaddress.IsIpNumber()) ? ipaddress : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        this.name = name;
    }

    public string Name => name;
    public string Ipaddress => ipaddress;
}

public class Send_PacketComand : Comand
{
    private string name;
    private string ipaddress;
    private string data;

    public Send_PacketComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.Send_PacketComand;
        if (comand.Length != 5) throw new FormatException($"el comando {comand[1]} tiene 3 argumentos, y se pasaron {comand.Length - 2}\n");
        ipaddress = (comand[3].IsIpNumber()) ? comand[3] : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        data = (comand[4].IsHexNumber()) ? comand[4] : throw new FormatException($"el campo <data> tiene que ser un numero hexadecimal");
        name = comand[2];
    }

    public Send_PacketComand(int time_ms, string name, string ipaddress, string data) : base(time_ms)
    {
        type = TypeComand.Send_PacketComand;
        this.ipaddress = (ipaddress.IsIpNumber()) ? ipaddress : throw new FormatException($"el campo <mac destino> tiene que ser un numero hexadecimal");
        this.data = (data.IsHexNumber()) ? data : throw new FormatException($"el campo <data> tiene que ser un numero hexadecimal");
        this.name = name;
    }

    public string Name => name;
    public string Data => data;
    public string Ipaddress => ipaddress;
}

public class Route_ResetComand : Comand
{
    private string name;

    public Route_ResetComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.Route_ResetComand;
        if (comand.Length != 3) throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        name = comand[2];
    }

    public Route_ResetComand(int time_ms, string name) : base(time_ms)
    {
        type = TypeComand.Route_ResetComand;
        this.name = name;
    }

    public string Name => name;
}

public class Route_AddComand : Comand
{
    private string name;
    private string ipAddress;
    private string mask;
    private string gateway;
    private int interfaz;

    public Route_AddComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.Route_AddComand;
        if (comand.Length != 7) throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        name = comand[2];
        ipAddress = (comand[3].IsIpNumber()) ? comand[3] : throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        mask = (comand[4].IsIpNumber()) ? comand[4] : throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        gateway = (comand[5].IsIpNumber()) ? comand[5] : throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        int numb;
        if (!int.TryParse(comand[6], out numb)) throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        interfaz = numb - 1;
    }

    public Route_AddComand(int time_ms, string name, string ipAddress, string mask, string gateway, int interfaz) : base(time_ms)
    {
        type = TypeComand.Route_AddComand;
        this.name = name;
        this.ipAddress = ipAddress;
        this.mask = mask;
        this.gateway = gateway;
        this.interfaz = interfaz;
    }

    public string Name => name;
    public string Ip_des => ipAddress;
    public string Mask => mask;
    public string Gateway => gateway;
    public int Interfaz => interfaz;
}

public class Route_DeleteComand : Comand
{
    private string name;
    private string ipAddress;
    private string mask;
    private string gateway;
    private int interfaz;

    public Route_DeleteComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.Route_DeleteComand;
        if (comand.Length != 7) throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        name = comand[2];
        ipAddress = (comand[3].IsIpNumber()) ? comand[3] : throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        mask = (comand[4].IsIpNumber()) ? comand[4] : throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        gateway = (comand[5].IsIpNumber()) ? comand[5] : throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        int numb;
        if (!int.TryParse(comand[6], out numb)) throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        interfaz = numb - 1;
    }

    public Route_DeleteComand(int time_ms, string name, string ipAddress, string mask, string gateway, int interfaz) : base(time_ms)
    {
        type = TypeComand.Route_DeleteComand;
        this.name = name;
        this.ipAddress = ipAddress;
        this.mask = mask;
        this.gateway = gateway;
        this.interfaz = interfaz;
    }

    public string Name => name;
    public string Ip_des => ipAddress;
    public string Mask => mask;
    public string Gateway => gateway;
    public int Interfaz => interfaz;
}

public class DisconnectComand : Comand
{
    private string name;
    private int port;

    public DisconnectComand(string[] comand, int time_ms) : base(time_ms)
    {
        type = TypeComand.DisconnectComand;
        if (comand.Length != 3) throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
        string[] name_port = comand[2].Split('_', StringSplitOptions.RemoveEmptyEntries);
        name = comand[2];
        if (!int.TryParse(name_port[1], out port)) throw new FormatException($"el comando {comand[1]} tiene 1 argumentos, y se pasaron {comand.Length - 2}\n");
    }

    public DisconnectComand(int time_ms, string name, int port) : base(time_ms)
    {
        type = TypeComand.DisconnectComand;
        this.name = name;
        this.port = port;
    }

    public string Name => name;
    public int Port => port;
}