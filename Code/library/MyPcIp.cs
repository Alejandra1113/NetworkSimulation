using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSimul.Component
{
    public class MyPcIp : MyPcDuplex, IPcIP, IRouteable
    {
        LinkedList<Route> routeList = new LinkedList<Route>();

        IDatagramRecorder datagramRecorder;


        Dictionary<string, string> IpMacDict = new Dictionary<string, string>();
        /// <summary>
        /// Lista de tuplas que contienen el Datagram a enviar y la direccion Ip en notación 
        /// de punto del dispositivo al que quieren enviar el mensaje
        /// </summary>
        LinkedList<(Datagram, string)> dataWaitingForSendARPQ = new LinkedList<(Datagram, string)>();

        (Datagram, string) dataWaitingForARPR = (null, null);

        public (string, string) Ip
        {
            get;
            set;
        }

        public MyPcIp(string name, int startTime, int signalTime, IRecorder recorder, IFrameRecorder frameRecorder, IDatagramRecorder datagramRecorder, IErrorDetectionAlg errorDetectionAlg) : base(name, startTime, signalTime, recorder, frameRecorder, errorDetectionAlg)
        {
            this.datagramRecorder = datagramRecorder;
        }

        public void Ping(string ToIp)
        {
            SendDatagram(ToIp, "08", 0, 1);
            SendDatagram(ToIp, "08", 0, 1);
            SendDatagram(ToIp, "08", 0, 1);
            SendDatagram(ToIp, "08", 0, 1);
        }

        public void SendDatagram(string ToIp, string data)
        {
            SendDatagram(ToIp, data, 0, 0);
        }

        public string GetGateway(string ToIp)
        {
            var currentNode = routeList.First;
            while (currentNode != null)
            {
                if (Route.SelectRoute(currentNode.Value, ToIp))
                {
                    return currentNode.Value.gateway;
                }
                currentNode = currentNode.Next;
            }
            return "";
        }
        //El ip está mal
        public void SendARPQFor(string ToIp)
        {
            string arpqData = ARPMessage.DataARPQ(Route.DotToBin(ToIp));
            SendFrame("ffff", arpqData, DataConverterOption.binario);
        }

        public void SendARPRFor(string ToMac)
        {
            string data = ARPMessage.DataARPR(Route.DotToBin(Ip.Item1));
            SendFrame(ToMac, data, DataConverterOption.binario);
        }

        public void SendDatagram(Datagram data, string mid)
        {
            string toMac;
            if (IpMacDict.TryGetValue(mid, out toMac))
            {
                // Si la tengo enviar el mensaje
                string rawData;
                if (Datagram.PackDatagram(data, out rawData))
                {
                    SendFrame(toMac, rawData, DataConverterOption.binario);
                }
            }
            else
            {
                // Si no tengo que pedirla
                dataWaitingForSendARPQ.AddLast((data, mid));
                NextToARPQ();
            }
        }

        private void NextToARPQ()
        {
            if (dataWaitingForARPR.Item1 == null && dataWaitingForARPR.Item2 == null && dataWaitingForSendARPQ.Count != 0)
            {
                dataWaitingForARPR = dataWaitingForSendARPQ.First.Value;
                dataWaitingForSendARPQ.RemoveFirst();
                string toMac;
                if (IpMacDict.TryGetValue(dataWaitingForARPR.Item2, out toMac))
                {
                    // Si la tengo enviar el mensaje
                    string rawData;
                    if (Datagram.PackDatagram(dataWaitingForARPR.Item1, out rawData))
                    {
                        SendFrame(toMac, rawData, DataConverterOption.binario);
                        dataWaitingForARPR = (null, null);
                        NextToARPQ();
                    }
                }
                else
                {
                    SendARPQFor(dataWaitingForARPR.Item2);
                }
            }
        }

        public void SendDatagram(string ToIp, string data, byte ttl, byte protocol)
        {
            // Buscar la Ip de la que tengo que buscar la mac
            string mid = GetGateway(ToIp);
            if (mid == "")
            {
                throw new Exception(Id + "no tiene gateway para la Ip" + ToIp);
            }

            Datagram frameData = new Datagram(Ip.Item1, ToIp, data, ttl, protocol);

            if (mid == "0.0.0.0")
            {
                mid = ToIp;
            }

            SendDatagram(frameData, mid);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromMac"></param>
        /// <param name="data"> Datos en binario </param>
        /// <param name="time_ms"></param>
        protected override void MessageRecieved(string fromMac, string data, int time_ms)
        {
            base.MessageRecieved(fromMac, data, time_ms);
            if (ARPMessage.IsARPR(data))
            {
                string arprIp = Route.BinToDot(data.Substring(32));
                if (arprIp == dataWaitingForARPR.Item2)
                {
                    IpMacDict.Add(arprIp, fromMac);
                    string frameData;
                    if (Datagram.PackDatagram(dataWaitingForARPR.Item1, out frameData))
                    {
                        SendFrame(fromMac, frameData, DataConverterOption.binario);
                        dataWaitingForARPR = (null, null);
                        NextToARPQ();
                    }
                }

            }
            else
            {
                if (ARPMessage.IsARPQ(data, "ffff") && Route.BinToDot(data.Substring(32)) == Ip.Item1)
                {
                    SendARPRFor(fromMac);
                }
                else
                {
                    Datagram datagram;
                    if (Datagram.DisPackDatagram(data, out datagram))
                    {
                        if (datagram.DecToIp == Ip.Item1)
                        {
                            datagramRecorder.Record(datagram.DecFromIp, datagram.HexaData, time_ms, datagram.Protocol);
                            if (datagram.Protocol == 1 && datagram.HexaData == "08")
                            {
                                SendDatagram(datagram.DecFromIp, "00", 0, 1);
                            }
                        }
                    }
                }
            }

        }



        public void Reset()
        {
            routeList = new LinkedList<Route>();
        }

        public void Add(Route route)
        {
            var currentNode = routeList.First;
            int result = 1;
            while (currentNode != null && (result = Route.CompareMaskRoutes(route, currentNode.Value)) == 1)
            {
                currentNode = currentNode.Next;
            }

            if (currentNode == null)
            {
                routeList.AddLast(route);
                return;
            }

            if (result == 0 && Route.CompareDestinationRoutes(route, currentNode.Value) == 0 &&
            route.gateway == currentNode.Value.gateway && route.Interface == currentNode.Value.Interface)
            {
                return;
            }

            routeList.AddBefore(currentNode, route);
        }

        public void Delete(Route route)
        {
            var currentNode = routeList.First;
            while (currentNode != null)
            {
                if (Route.CompareDestinationRoutes(currentNode.Value, route) == 0 && Route.CompareMaskRoutes(currentNode.Value, route) == 0
                    && route.gateway == currentNode.Value.gateway && route.Interface == currentNode.Value.Interface)
                {
                    routeList.Remove(currentNode);
                    break;
                }
                currentNode = currentNode.Next;
            }
        }
    }
}
