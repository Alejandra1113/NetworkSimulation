
using System;
using System.Collections.Generic;

namespace NetSimul.Component
{
    public class MyPC : NodeBase, IPC
    {
        protected int signalTime;
        protected int clock;

        public IRecorder Recorder { set => recorder = value; }

        protected LinkedList<bool[]> pendingData = new LinkedList<bool[]>();

        protected bool[] currentData;

        protected int index;

        protected int timer;

        protected bool wasEmpty;

        protected ProtoState state = ProtoState.idle;

        protected IRecorder recorder;
        protected IRecorder recorder2;

        protected CableState lastBitRecieved;

        protected int timeRecieving;

        public MyPC(string name, int startTime, int signalTime, IRecorder recorder)
        {
            this.signalTime = signalTime;
            this.clock = startTime;
            state = ProtoState.idle;
            PortState = new CableState[1];
            PortState[0] = CableState.empty;
            Id = name;
            this.recorder = recorder;
            portList = new string[1];
            portList[0] = name + "_1";
            Response = new CableState[1];
            neighbours = new (INode, int)[1];
            recorder2 = new Recorder($"..\\output{name}");
        }

        public void SetData(bool[] data)
        {
            pendingData.AddFirst(data);
        }

        protected virtual bool HaveCollision()
        {
            return PortState[0] != Response[0];
        }

        /// <summary>
        /// Responde empty y en la pr�xima iteraci�n comienza a contar los ms con el cable vac�o, pues se asume que se pas� a waiting pq alguien escribi�.
        /// </summary>
        void passToWaiting()
        {
            timer = 0;
            state = ProtoState.waiting;
            Response[0] = CableState.empty;
            lastBitRecieved = PortState[0];
            timeRecieving = 1;
        }
        /// <summary>
        /// Si no hay que escribir pasa a idle, si hay algo que escribir entonces extrae el �ltimo conjunto de bits de la lista de pendientes para comenzar a escribirlo.
        /// Responde el primer bit del conjunto extraido, prepara el timer para mantenerlo como respuesta durante signalTime ms
        /// </summary>
        void passToWriting()
        {
            if (pendingData.Count == 0)
            {
                passToIdle();
                return;
            }

            timer = 1;
            state = ProtoState.writing;
            currentData = pendingData.Last.Value;
            pendingData.RemoveLast();
            index = 0;
            if (currentData[0])
            {
                Response[0] = CableState.one;
            }
            else
            {
                Response[0] = CableState.zero;
            }
        }

        /// <summary>
        /// Pone el timer en 0 para, a partir del siguiente ms comenzar contar los 4 ms que deben transcurrir con el cable vac�o para asegurar que el 
        /// mensaje ha llegado correctamente, Responde empty.
        /// </summary>
        void passToChecking()
        {
            timer = 0;
            state = ProtoState.checking;
            Response[0] = CableState.empty;
        }
        /// <summary>
        /// Cambia el estado de la pc a Idle y responde empty
        /// </summary>
        void passToIdle()
        {
            state = ProtoState.idle;
            Response[0] = CableState.empty;
        }
        /// <summary>
        /// A partir de la siguiente iteraci�n se comienza a esperar de 1 a 3 ms con el cable en empty para volver a intentar enviar el conjunto de datos.
        /// </summary>
        void passToPending()
        {
            pendingData.AddLast(currentData);
            state = ProtoState.pending;
            Random gen = new Random();
            timer = gen.Next(1, 4);
            wasEmpty = false;
            Response[0] = CableState.empty;
        }

        /// <summary>
        /// Avisa a la pc que ocurri� una instancia de tiempo
        /// </summary>
        public override bool Read()
        {
            clock++;
            switch (state)
            {
                case ProtoState.idle:
                    Response[0] = CableState.empty;
                    //Si alguien escribi� espera
                    if (PortState[0] != CableState.empty)
                    {
                        passToWaiting();
                    }
                    else
                    {
                        //Si nadie ha escrito y tienes algo que escribir empieza a escribirlo
                        if (pendingData.Count > 0)
                        {
                            passToWriting();
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
                case ProtoState.waiting:
                    //reportando recibidos
                    if (lastBitRecieved != CableState.empty)
                    {
                        bool bitToRec = (lastBitRecieved == CableState.one) ? true : false;
                        timeRecieving++;
                        if (lastBitRecieved != PortState[0] || timeRecieving > signalTime)
                        {
                            lastBitRecieved = PortState[0];
                            recorder.Record(false, bitToRec, false, Id, clock - timeRecieving - 1);
                            timeRecieving = 1;
                        }
                    }
                    else
                    {
                        lastBitRecieved = PortState[0];
                        timeRecieving = 1;
                    }

                    Response[0] = CableState.empty;
                    if (PortState[0] == CableState.empty)
                    {
                        timer++;
                        if (timer > 4)
                        {
                            passToWriting();
                            return true;
                        }
                    }
                    else
                    {
                        timer = 0;
                    }
                    break;
                case ProtoState.writing:
                    timer++;
                    //comprobar colision
                    if (HaveCollision())
                    {
                        recorder.Record(true, currentData[index], true, Id, clock - timer);
                        passToPending();
                        return true;
                    }
                    //comprobar bit enviado correctamente
                    if (timer > signalTime)
                    {
                        recorder.Record(true, currentData[index], false, Id, clock - timer);
                        timer = 1;
                    }

                    //avanzar de bit
                    if (timer == 1)
                    {
                        if (index < currentData.Length - 1)
                        {
                            index++;
                        }
                        else
                        {
                            passToChecking();
                            return true;
                        }
                    }

                    if (currentData[index])
                    {
                        Response[0] = CableState.one;
                    }
                    else
                    {
                        Response[0] = CableState.zero;
                    }

                    break;
                case ProtoState.checking:
                    timer++;
                    if (PortState[0] != CableState.empty)
                    {
                        // pendingData.AddLast(currentData);// vuelve a poner en pendiente el conjunto de datos 
                        passToWaiting();
                        return true;
                    }
                    if (timer > 4)
                    {
                        passToWriting();
                    }
                    break;
                case ProtoState.pending:
                    Response[0] = CableState.empty;
                    if (wasEmpty)
                    {
                        if (PortState[0] != CableState.empty)
                        {
                            passToWaiting();
                        }
                        else
                        {
                            if (timer == 0)
                            {
                                passToWriting();
                            }
                            else
                            {
                                timer--;
                            }
                        }
                    }
                    else
                    {
                        if (PortState[0] == CableState.empty)
                        {
                            wasEmpty = true;
                            timer--;
                        }
                    }
                    break;

            }
            return true;
        }

        public override bool Write()
        {
            return false;
        }
    }
}

