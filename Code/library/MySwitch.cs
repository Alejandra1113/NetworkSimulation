using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace NetSimul.Component
{

    public class MySwitch : NodeBase, ISwitch
    {
        //Array que contiene para cada puerto una cola de mensajes que se deben de enviar
        private Queue<List<bool>>[] frameQueue;
        //Diccionario de direcciones Mac, tiene el puerto y el tiempo del ultimo mensaje recibido por el
        private Dictionary<int, (int, int)> macAddressDictionary = new Dictionary<int, (int, int)>();

        //Tiempo en que se creo el switch el cual se incrementa en cada clock del reloj
        private int clock;
        //Intervalo de tiempo en que se transmiten los bits
        private int signalTime;
        //por cada puerto conoce el indice del bit del mensaje que esta transmitiendo actualmente 
        private int[] currentIndexBit;

        private int[] timeRecieving;

        private int[] timeSending;

        private List<bool>[] currentRecievingFrame;



        public MySwitch(string name, int PortNum, int startTime, int signalTime)
        {
            Id = name;
            portList = new string[PortNum];
            PortState = new CableState[PortNum];
            Response = new CableState[PortNum];
            frameQueue = new Queue<List<bool>>[PortNum];
            timeRecieving = new int[PortNum];
            timeSending = new int[PortNum];
            currentIndexBit = new int[PortNum];
            currentRecievingFrame = new List<bool>[PortNum];

            for (int i = 0; i < portList.Length; i++)
            {
                portList[i] = $"{name}_{i + 1}";
                PortState[i] = CableState.empty;
                frameQueue[i] = new Queue<List<bool>>();
            }
            neighbours = new (INode, int)[PortNum];
            clock = startTime;
            this.signalTime = signalTime;

        }

        //incompleto
        public override bool Read()
        {
            Frame currentFrame;
            bool active = false;



            for (int i = 0; i < portList.Length; i++)
            {

                if (PortState[i] == CableState.empty)
                {
                    if (currentRecievingFrame[i] != null && currentRecievingFrame[i].Count > 0)
                    {
                        if (timeRecieving[i] <= -1 && Frame.DisPackFrame(currentRecievingFrame[i].ToArray(), out currentFrame))
                        {
                            RedirectFrame(i, currentFrame.ToMac);
                            IUDict(i, currentFrame.FromMac);
                            active = true;
                        }
                        currentRecievingFrame[i] = new List<bool>();
                    }

                    timeRecieving[i] = -1;
                }
                else
                {

                    active = true;
                    timeRecieving[i] = timeRecieving[i] <= -1 ? clock : timeRecieving[i];
                    if (clock - timeRecieving[i] + 1 >= signalTime)
                    {

                        if (currentRecievingFrame[i] == null)
                            currentRecievingFrame[i] = new List<bool>();
                        currentRecievingFrame[i].Add(PortState[i] == CableState.one);
                        timeRecieving[i] = -1;

                    }

                }
            }
            clock += 1;
            return active;
        }

        private bool WritePorts()
        {
            int portNumber;
            List<bool> x;
            bool active = false;

            for (int i = 0; i < portList.Length; i++)
            {
                if (frameQueue[i] != null && frameQueue[i].Count > 0)
                {
                    x = frameQueue[i].Peek();
                    active = true;
                    portNumber = neighbours[i].Item2;
                    if ((currentIndexBit[i] == 0 && timeSending[i] < -4) || (currentIndexBit[i] <= x.Count - 1 && currentIndexBit[i] > 0 && clock - timeSending[i] + 1 > signalTime))
                    {


                        if (neighbours[i].Item1 != null)
                        {

                            neighbours[i].Item1.PortState[portNumber] = frameQueue[i].Peek()[currentIndexBit[i]] ? CableState.one : CableState.zero;
                            Response[i] = frameQueue[i].Peek()[currentIndexBit[i]] ? CableState.one : CableState.zero;
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
                            frameQueue[i].Dequeue();
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

        void RedirectFrame(int i, int macId)
        {
            int recPort = Contain(macId).Item1;
            if (recPort != -1)
            {
                frameQueue[recPort].Enqueue(currentRecievingFrame[i]);

            }
            else
            {
                for (int j = 0; j < portList.Length; j++)
                {
                    if (j == i) continue;
                    frameQueue[j].Enqueue(currentRecievingFrame[i]);
                }

            }


        }
        private (int, int) Contain(int macId)
        {
            (int, int) x;
            return macAddressDictionary.TryGetValue(macId, out x) ? x : (-1, -1);
        }
        //Elimina de la tabla de macs las direcciones que lleven almacenadas m�s de 10000ms
        private void UpdateDict()
        {
            foreach (var key in macAddressDictionary.Keys)
            {
                if (macAddressDictionary[key].Item2 < clock - 10000)
                {
                    macAddressDictionary.Remove(key);
                }
            }
        }

        private void IUDict(int port, int macId)
        {
            if (Contain(macId).Item1 == port)
            {
                macAddressDictionary[macId] = (port, clock);
            }
            else
            {
                macAddressDictionary.Add(macId, (port, clock));
            }
        }
        public override bool Write()
        {

            UpdateDict();
            return WritePorts();
        }
    }
}

