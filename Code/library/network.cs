namespace NetSimul.NetWork;
using System.Collections;
using System.Dynamic;
using NetSimul.Component;
using NetSimul.Intruction;

public class Network : ICollection<INode>, IReadOnlyCollection<INode>, IEnumerable<INode>, IEnumerable
{
    List<INode> network;
    int[] netcc;
    List<string> macList;
    List<(string, string)> ipList;
    List<ISwitch> switches;
    List<IRouter> routers;
    List<IHub> hubs;
    List<IPC> pcs;

    private int count;
    private bool isReadOnly;

    public Network(bool isReadOnly)
    {
        ipList = new List<(string, string)>();
        macList = new List<string>();
        network = new List<INode>();
        hubs = new List<IHub>();
        pcs = new List<IPC>();
        switches = new List<ISwitch>();
        routers = new List<IRouter>();
        netcc = new int[0];
        this.isReadOnly = isReadOnly;
        count = 0;
    }

    public Network()
    {
        ipList = new List<(string, string)>();
        macList = new List<string>();
        network = new List<INode>();
        hubs = new List<IHub>();
        pcs = new List<IPC>();
        switches = new List<ISwitch>();
        routers = new List<IRouter>();
        netcc = new int[0];
        isReadOnly = false;
        count = 0;
    }

    public int Count => count;

    public bool IsReadOnly => isReadOnly;


    public IEnumerator<INode> GetEnumerator() => network.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => network.GetEnumerator();

    public void Add(INode item)
    {
        if (network.Contains(item)) return;
        network.Add(item);
        switch (item)
        {
            case IPcIP:
                pcs.Add(item as IPcIP);
                break;
            case IHub:
                hubs.Add(item as IHub);
                break;
            case ISwitch:
                switches.Add(item as ISwitch);
                break;
            case IRouter:
                routers.Add(item as IRouter);
                break;
            default:
                break;
        }
        count++;
    }

    public void Clear()
    {
        network.Clear();
        pcs.Clear();
        hubs.Clear();
        switches.Clear();
        count = 0;
        netcc = new int[0];
    }

    public bool Contains(INode item) => network.Contains(item);

    public bool Connected(INode item) => (network.Contains(item)) ? item.Neighbours.Length != 0 : false;

    public void CopyTo(INode[] array, int arrayIndex) => Array.Copy(network.ToArray(), 0, array, arrayIndex, network.Count);

    public bool Remove(INode item)
    {
        bool result = network.Remove(item);
        if (result) count--;
        if (item is IPcIP) pcs.Remove(item as IPcIP);
        else if (item is IHub) hubs.Remove(item as IHub);
        else if (item is ISwitch) switches.Remove(item as ISwitch);
        else if (item is IRouter) routers.Remove(item as IRouter);
        return result;
    }

    public void CopyTo(Array array, int index) => Array.Copy(network.ToArray(), 0, array, index, network.Count);

    public bool Execute(Comand comand, options options)
    {
        TypeComand type = (comand != null) ? comand.Type : TypeComand.nullComand;
        INode? node1;
        INode? node2;
        int index1;
        int index2;
        int port1;
        int port2;

        IErrorDetectionAlg alg;
        switch (options.Alg)
        {
            case "CheckSum":
                alg = new CheckSum();
                break;
            case "ParityCheck":
                alg = new ParityCheck();
                break;
            default:
                throw new Exception();
        }

        switch (type)
        {
            case TypeComand.CreateComand:
                if ((comand as CreateComand).Component == TypeComponent.Host)
                    Add(new MyPcIp((comand as CreateComand).Name, (comand as CreateComand).Time_ms, options.SignalTime, new Recorder($"{options.Path}/{((comand as CreateComand).Name + ".txt")}"), new FrameRecorder($"{options.Path}/{((comand as CreateComand).Name + "_data.txt")}"), new DatagramRecorder($"{options.Path}/{((comand as CreateComand).Name + "_payload.txt")}"), alg));
                if ((comand as CreateComand).Component == TypeComponent.Hub)
                    Add(new MyHubDuplex((comand as CreateComand).Name, (comand as CreateComand).CountPort, (comand as CreateComand).Time_ms, new Recorder($"{options.Path}/{((comand as CreateComand).Name + ".txt")}")));
                if ((comand as CreateComand).Component == TypeComponent.Switch)
                    Add(new MySwitch((comand as CreateComand).Name, (comand as CreateComand).CountPort, (comand as CreateComand).Time_ms, options.SignalTime));
                if ((comand as CreateComand).Component == TypeComponent.Router)
                    Add(new MyRouter((comand as CreateComand).Name, (comand as CreateComand).CountPort, (comand as CreateComand).Time_ms, options.SignalTime, alg));
                break;
            case TypeComand.ConnectComand:
                Find((comand as ConnectComand).Name1, (comand as ConnectComand).Name2, out node1, out node2, out index1, out index2);
                port1 = (comand as ConnectComand).Port1;
                port2 = (comand as ConnectComand).Port2;

                if (node1 == null || node2 == null) return false;
                if (netcc.Length == 0 || (netcc[index1] != netcc[index2]) || netcc[index1] == 0)
                {
                    node1.Connect(node2, port1, port2);
                    node2.Connect(node1, port2, port1);
                    DFS_CC();
                }
                else return false;
                break;
            case TypeComand.MacComand:
                node1 = network.Find(x => x.Id == (comand as MacComand).Name);

                if (node1 == null) return false;
                if (macList.Contains((comand as MacComand).MacId)) return false;

                if (node1 is IPcIP) (node1 as IPcIP).Mac = (comand as MacComand).MacId;
                else if (node1 is IRouter)
                {
                    if ((comand as MacComand).PortNumber >= (node1 as IRouter).macList.Length) throw new IndexOutOfRangeException();
                    (node1 as IRouter).macList[(comand as MacComand).PortNumber] = (comand as MacComand).MacId;
                }
                else return false;

                macList.Add((comand as MacComand).MacId);
                break;
            case TypeComand.IPComand:
                node1 = network.Find(x => x.Id == (comand as IPComand).Name);

                if (node1 == null) return false;
                if (node1 is IPcIP) (node1 as IPcIP).Ip = (comand as IPComand).Ip;
                else if (node1 is IRouter)
                {
                    if ((comand as IPComand).PortNumber >= (node1 as IRouter).macList.Length) throw new IndexOutOfRangeException();
                    (node1 as IRouter).ipList[(comand as IPComand).PortNumber] = (comand as IPComand).Ip;
                }
                else return false;

                ipList.Add((comand as IPComand).Ip);
                break;
            case TypeComand.SendComand:
                node1 = network.Find(x => x.Id == (comand as SendComand).Name);

                if (node1 == null || !(node1 is IPcIP)) return false;
                (node1 as IPC).SetData((comand as SendComand).Data);
                break;
            case TypeComand.Send_FrameComand:
                node1 = network.Find(x => x.Id == (comand as Send_FrameComand).Name);

                if (node1 == null || !(node1 is IPcIP)) return false;
                (node1 as IPcDuplex).SendFrame((comand as Send_FrameComand).MacId, (comand as Send_FrameComand).Data);
                break;
            case TypeComand.Send_PacketComand:
                node1 = network.Find(x => x.Id == (comand as Send_PacketComand).Name);

                if (node1 == null || !(node1 is IPcIP)) return false;
                (node1 as IPcIP).SendDatagram((comand as Send_PacketComand).Ipaddress, (comand as Send_PacketComand).Data);
                break;
            case TypeComand.PingComand:
                node1 = network.Find(x => x.Id == (comand as PingComand).Name);

                if (node1 == null || !(node1 is IPcIP)) return false;
                (node1 as IPcIP).Ping((comand as PingComand).Ipaddress);
                break;
            case TypeComand.Route_ResetComand:
                node1 = network.Find(x => x.Id == (comand as Route_ResetComand).Name);

                if (node1 == null || !(node1 is IRouteable)) return false;
                (node1 as IRouteable).Reset();
                break;
            case TypeComand.Route_AddComand:
                node1 = network.Find(x => x.Id == (comand as Route_AddComand).Name);

                if (node1 == null || !(node1 is IRouteable)) return false;
                (node1 as IRouteable).Add(new Route((comand as Route_AddComand).Ip_des, (comand as Route_AddComand).Mask, (comand as Route_AddComand).Gateway, (comand as Route_AddComand).Interfaz));
                break;
            case TypeComand.Route_DeleteComand:
                node1 = network.Find(x => x.Id == (comand as Route_DeleteComand).Name);

                if (node1 == null || !(node1 is IRouteable)) return false;
                (node1 as IRouteable).Delete(new Route((comand as Route_DeleteComand).Ip_des, (comand as Route_DeleteComand).Mask, (comand as Route_DeleteComand).Gateway, (comand as Route_DeleteComand).Interfaz));
                break;
            case TypeComand.DisconnectComand:
                port1 = (comand as DisconnectComand).Port;
                Find((comand as DisconnectComand).Name, port1, out node1, out node2, out port2);

                if (node1 == null || node2 == null) return false;
                node1.Disconect(port1);
                node2.Disconect(port2);
                DFS_CC();
                break;
            default:
                throw new FormatException("este comando no existe");
        }
        return true;
    }

    private void Find(string port1, string port2, out INode node1, out INode node2, out int index1, out int index2)
    {
        node1 = null;
        node2 = null;
        index1 = -1;
        index2 = -1;
        string name1 = port1.Split('_', StringSplitOptions.RemoveEmptyEntries)[0];
        string name2 = port2.Split('_', StringSplitOptions.RemoveEmptyEntries)[0];
        for (int i = 0; i < network.Count; i++)
        {
            if (network[i].Id == name1)
            {
                node1 = network[i];
                index1 = i;
            }
            else if (network[i].Id == name2)
            {
                node2 = network[i];
                index2 = i;
            }
            if (node1 != null && node2 != null)
            {
                return;
            }
        }
    }

    private void Find(string name, int port1, out INode node1, out INode node2, out int port2)
    {
        node1 = null;
        node2 = null;
        port2 = 0;
        for (int i = 0; i < network.Count; i++)
        {
            if (network[i].Id == name)
            {
                node1 = network[i];
                node2 = node1.Neighbours[port1].Item1;
                port2 = node1.Neighbours[port1].Item2;
                return;
            }
        }
    }

    public void DFS_CC()
    {
        bool[] visit = new bool[network.Count];
        netcc = new int[network.Count];
        for (int i = 0, cc = 1; i < network.Count; i++, cc++)
        {
            if (!visit[i] && (network[i] is IHub || network[i] is IPC || network[i] is ISwitch))
            {
                DFS_CC(visit, cc, i, network[i]);
            }
        }
    }

    public void DFS_CC(bool[] visit, int cc, int index, INode node)
    {
        visit[index] = true;
        netcc[index] = cc;

        foreach (var item in node.Neighbours)
        {
            int i = network.IndexOf(item.Item1);
            if (item.Item1 != null && !visit[i] && (item.Item1 is IHub || item.Item1 is IPC))
            {
                DFS_CC(visit, cc, i, network[i]);
            }
        }
    }

    public bool Update()
    {
        bool result = false;
        foreach (var item in pcs)
        {
            if (item.Read()) result = true;
        }
        foreach (var item in pcs)
        {
            if (item.Write()) result = true;
        }
        foreach (var item in switches)
        {
            if (item.Write()) result = true;
        }
        foreach (var item in routers)
        {
            if (item.Write()) result = true;
        }
        foreach (var item in hubs)
        {
            item.Read();
        }
        foreach (var item in hubs)
        {
            item.Write();
        }
        foreach (var item in switches)
        {
            if (item.Read()) result = true;
        }
        foreach (var item in routers)
        {
            if (item.Read()) result = true;
        }
        foreach (var item in hubs)
        {
            item.Reset();
        }
        return result;
    }
}