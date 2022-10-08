using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using NetSimul.Extensions;

namespace NetSimul.Component
{
    public class MyRouter : NodeBase, IRouteable, IRouter
    {

        private Dictionary<string, (string, int)> macAddressDictionary = new Dictionary<string, (string, int)>();

        private List<Route> routeTable = new List<Route>();

        public string[] macList { get; set; }

        public (string, string)[] ipList { get; set; }

        private Queue<List<bool>>[] sendingQueue;

        private Queue<(List<bool>, string)>[] waitingQueue;

        private int clock;

        //Intervalo de tiempo en que se transmiten los bits
        private int signalTime;

        //por cada puerto conoce el indice del bit del mensaje que esta transmitiendo actualmente 
        private int[] currentIndexBit;

        private int[] timeRecieving;

        private int[] timeSending;

        private List<bool>[] currentRecieving;

        private IErrorDetectionAlg error;

        public MyRouter(string name, int PortNum, int startTime, int signalTime, IErrorDetectionAlg error)
        {
            Id = name;
            macList = new string[PortNum];
            ipList = new (string, string)[PortNum];
            portList = new string[PortNum];
            PortState = new CableState[PortNum];
            Response = new CableState[PortNum];
            sendingQueue = new Queue<List<bool>>[PortNum];
            waitingQueue = new Queue<(List<bool>, string)>[PortNum];
            timeRecieving = new int[PortNum];
            this.error = error;
            timeSending = new int[PortNum];
            currentIndexBit = new int[PortNum];
            currentRecieving = new List<bool>[PortNum];

            for (int i = 0; i < portList.Length; i++)
            {
                waitingQueue[i] = new Queue<(List<bool>, string)>();
                sendingQueue[i] = new Queue<List<bool>>();
                portList[i] = $"{name}_{i + 1}";
                PortState[i] = CableState.empty;
            }
            neighbours = new (INode, int)[PortNum];
            clock = startTime;
            this.signalTime = signalTime;

        }
        public void Add(Route route)
        {
            int comp;
            for (int i = 0; i < routeTable.Count; i++)
            {
                comp = Route.CompareMaskRoutes(route, routeTable[i]);
                if (comp == 1)
                {
                    routeTable.Insert(i, route);
                    return;
                }
                if (comp == 0 && Route.CompareDestinationRoutes(route, routeTable[i]) == 0
                && route.gateway == routeTable[i].gateway &&
                 route.Interface == routeTable[i].Interface)
                {
                    return;
                }
            }
            routeTable.Add(route);
        }

        public void Delete(Route route)
        {
            for (int i = 0; i < routeTable.Count; i++)
            {
                if (Route.CompareMaskRoutes(route, routeTable[i]) == 0 && Route.CompareDestinationRoutes(route, routeTable[i]) == 0
                && route.gateway == routeTable[i].gateway &&
                 route.Interface == routeTable[i].Interface)
                {
                    routeTable.RemoveAt(i);
                    return;
                }
            }
        }
        private (string, int) Contain(string ip)
        {
            (string, int) x;
            return macAddressDictionary.TryGetValue(ip, out x) ? x : ("", -1);
        }

        private void IUDict(string mac, string ip)
        {
            if (Contain(ip).Item1 == mac)
            {
                macAddressDictionary[ip] = (mac, clock);
            }
            else
            {
                macAddressDictionary.Add(ip, (mac, clock));
            }
        }
        protected virtual void RedirectDatagram(int port, Frame current)
        {
            bool match;
            if (ARPMessage.IsARPR(current.BinData) && current.HexaToMac == macList[port])
            {
                sendingQueue[port].Enqueue(NewMessage(waitingQueue[port].Dequeue().Item1, current.HexaFromMac));

                IUDict(current.HexaFromMac, Route.BinToDot(current.BinData.Substring(32)));

                while (waitingQueue[port].Count > 0)
                {
                    string IP = waitingQueue[port].First().Item2 == "0.0.0.0" ? GetToIp(waitingQueue[port].First().Item1) : waitingQueue[port].First().Item2;
                    (string, int) x;

                    if (macAddressDictionary.TryGetValue(IP, out x))
                    {

                        sendingQueue[port].Enqueue(NewMessage(waitingQueue[port].Dequeue().Item1, x.Item1.ToString()));

                    }
                    else
                    {
                        sendingQueue[port].Enqueue(ARPMessage.ARPQFrame(macList[port], IP, error));
                        break;
                    }

                }
                return;

            }
            if (ARPMessage.IsARPQ(current.BinData, current.HexaToMac) && Route.BinToDot(current.BinData.Substring(32)) == ipList[port].Item1)
            {
                sendingQueue[port].Enqueue(ARPMessage.ARPRFrame(macList[port], current.HexaFromMac, ipList[port].Item1, error));
                return;
            }

            if (current.HexaToMac == macList[port])
            {
                Datagram datagram;
                Datagram.DisPackDatagram(current.BinData, out datagram);


                for (int i = 0; i < routeTable.Count; i++)
                {
                    match = Route.SelectRoute(routeTable[i], datagram.DecToIp);
                    if (match && routeTable[i].gateway != "0.0.0.0")
                    {
                        DoWhateverYouWant(current, routeTable[i].gateway, routeTable[i].Interface, port);
                        return;
                    }
                    else if (match && routeTable[i].gateway == "0.0.0.0")
                    {
                        DoWhateverYouWant(current, datagram.DecToIp, routeTable[i].Interface, port);
                        return;
                    }
                }

                datagram = new Datagram(ipList[port].Item1, datagram.DecFromIp, "03", 0, 1);
                string rawData;
                Datagram.PackDatagram(datagram, out rawData);
                Frame f = new Frame(macList[port], macList[port], rawData, error, DataConverterOption.binario);
                RedirectDatagram(port, f);

            }
        }

        private List<bool> NewMessage(List<bool> oldMessage, string newToMac)
        {
            Frame oldFrame;
            Frame.DisPackFrame(oldMessage, out oldFrame);
            Frame newFrame = new Frame(oldFrame.HexaFromMac, newToMac, oldFrame.BinData, error, DataConverterOption.binario);
            List<bool> newMessage;
            Frame.PackFrame(newFrame, out newMessage);
            return newMessage;
        }

        private string GetToIp(List<bool> oldMessage)
        {
            Frame oldFrame;
            Frame.DisPackFrame(oldMessage, out oldFrame);
            Datagram datagram;
            Datagram.DisPackDatagram(oldFrame.BinData, out datagram);
            return datagram.DecToIp;
        }

        private void DoWhateverYouWant(List<bool> current, string IP, int Interface, int port)
        {
            Frame oldFrame;
            Frame.DisPackFrame(current.ToArray(), out oldFrame);
            DoWhateverYouWant(oldFrame, IP, Interface, port);
        }
        private void DoWhateverYouWant(Frame current, string IP, int Interface, int port)
        {
            (string, int) x;

            if (macAddressDictionary.TryGetValue(IP, out x))
            {
                sendingQueue[Interface].Enqueue(NewMessage(current, x.Item1.ToString()));
                return;
            }
            else if (waitingQueue[Interface].Count == 0)
            {
                sendingQueue[Interface].Enqueue(ARPMessage.ARPQFrame(macList[Interface], IP, error));
            }
            waitingQueue[Interface].Enqueue((currentRecieving[port], IP));

        }
        private List<bool> NewMessage(Frame oldFrame, string newToMac)
        {
            Frame newFrame = new Frame(oldFrame.HexaFromMac, newToMac, oldFrame.BinData, error, DataConverterOption.binario);
            bool[] newMessage;
            Frame.PackFrame(newFrame, out newMessage);
            return newMessage.ToList();
        }
        public override bool Read()
        {
            Frame currentFrame;
            bool active = false;
            bool waiting = false;
            for (int i = 0; i < portList.Length; i++)
            {

                if (PortState[i] == CableState.empty)
                {

                    if (timeRecieving[i] < -4 && currentRecieving[i] != null && currentRecieving[i].Count > 0)
                    {

                        if (Frame.DisPackFrame(currentRecieving[i].ToArray(), out currentFrame) && error.DetectNoise(currentFrame.BinData, currentFrame.BinDataV) != null)
                        {
                            RedirectDatagram(i, currentFrame);
                            currentRecieving[i] = new List<bool>();
                            active = true;
                        }

                    }
                    timeRecieving[i]--;
                    active = active || timeRecieving[i] >= -4;
                }
                else
                {
                    active = true;
                    timeRecieving[i] = timeRecieving[i] <= -1 ? clock : timeRecieving[i];
                    if (clock - timeRecieving[i] + 1 >= signalTime)
                    {
                        if (currentRecieving[i] == null)
                            currentRecieving[i] = new List<bool>();
                        currentRecieving[i].Add(PortState[i] == CableState.one);
                        timeRecieving[i] = -1;

                    }
                }
                waiting |= waitingQueue[i].Count > 0;
            }
            clock += 1;
            return active || waiting;
        }

        public void Reset()
        {
            routeTable = new List<Route>();
        }

        public override bool Write()
        {
            int portNumber;
            List<bool> x;
            bool active = false;

            for (int i = 0; i < portList.Length; i++)
            {
                if (sendingQueue[i] != null && sendingQueue[i].Count > 0)
                {
                    x = sendingQueue[i].Peek();
                    active = true;
                    portNumber = neighbours[i].Item2;
                    if ((currentIndexBit[i] == 0 && timeSending[i] < -4) || (currentIndexBit[i] <= x.Count - 1 && currentIndexBit[i] > 0 && clock - timeSending[i] + 1 > signalTime))
                    {


                        if (neighbours[i].Item1 != null)
                        {

                            neighbours[i].Item1.PortState[portNumber] = sendingQueue[i].Peek()[currentIndexBit[i]] ? CableState.one : CableState.zero;
                            Response[i] = sendingQueue[i].Peek()[currentIndexBit[i]] ? CableState.one : CableState.zero;
                        }
                        currentIndexBit[i]++;
                        timeSending[i] = clock;

                    }
                    else
                    {
                        if (currentIndexBit[i] >= x.Count && clock - timeSending[i] >= signalTime)
                        {
                            currentIndexBit[i] = 0;
                            timeSending[i] = 0;
                            sendingQueue[i].Dequeue();
                            neighbours[i].Item1.PortState[portNumber] = CableState.empty;

                        }
                        else if (currentIndexBit[i] == 0)
                        {
                            timeSending[i]--;
                        }
                    }
                }
            }
            return active;
        }
    }


}