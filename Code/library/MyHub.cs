
namespace NetSimul.Component
{
    public class MyHub : NodeBase, IHub
    {
        protected int amountOfPorts;
        protected int clock;
        protected bool[] doneIn;
        protected bool[] doneOut;
        protected bool[] doneUpdated;
        protected int hubsPendingIn;
        protected int amountOfPendingOut;
        protected int amountOfHubs = 0;
        protected int amountOfConnectedDevice = 0;
        protected bool IsFirstTime = true;
        protected bool CanWrite = false;
        protected bool IsEnd = false;

        IRecorder recorder;

        public IRecorder Recorder { set => recorder = value; }

        public MyHub(string Name, int amountOfPorts, int startTime, IRecorder recorder)
        {
            this.amountOfPorts = amountOfPorts;
            clock = startTime;
            Id = Name;
            portList = new string[amountOfPorts];
            for (int i = 0; i < amountOfPorts; i++)
            {
                portList[i] = Id + "_" + (i + 1);
            }
            neighbours = new (INode, int)[amountOfPorts];
            PortState = new CableState[amountOfPorts];
            Response = new CableState[amountOfPorts];
            doneIn = new bool[amountOfPorts];
            doneOut = new bool[amountOfPorts];
            doneUpdated = new bool[amountOfPorts];
            this.recorder = recorder;
            IsFirstTime = true;
        }
        public override bool Read()
        {
            Touch(-1);
            return false;
        }

        public override bool Write()
        {
            CanWrite = true;
            clock++;
            Touch(-1);
            return false;
        }

        public static CableState Xor(CableState A, CableState B)
        {
            if (A == CableState.empty)
            {
                return B;
            }

            if (B == CableState.empty)
            {
                return A;
            }

            if (A == B)
            {
                return CableState.zero;
            }
            return CableState.one;
        }

        protected virtual void SetResponses()
        {
            CableState acum = CableState.empty;
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i].Item1 != null)
                {
                    acum = Xor(acum, PortState[i]);
                }
            }

            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i].Item1 is IPC)
                {
                    Response[i] = acum;
                }
                else
                {
                    Response[i] = Xor(acum, PortState[i]);
                    doneOut[i] = true;
                }
            }
        }

        public virtual void Touch(int port)
        {
            //leyendo entradas de la pc
            if (!IsEnd)
            {
                //Recibir las entradas de las pc solo la primera vez
                if (IsFirstTime)
                {
                    for (int i = 0; i < neighbours.Length; i++)
                    {
                        if (neighbours[i].Item1 != null && PortState[i] != CableState.empty && neighbours[i].Item1 is IPC && !doneIn[i])
                        {
                            doneIn[i] = true;
                            recorder.Record(false, PortState[i] == CableState.one, portList[i], clock);
                        }
                    }
                    IsFirstTime = false;
                }

                if (port != -1)
                {
                    if (neighbours[port].Item1 != null && PortState[port] != CableState.empty)
                    {
                        recorder.Record(false, PortState[port] == CableState.one, portList[port], clock);
                    }
                    hubsPendingIn--;
                    doneIn[port] = true;
                }

                int index = -1;
                CableState acum = CableState.empty;

                if (hubsPendingIn == 1)//Solo le puede responder al hub que falta por responder
                {
                    for (int i = 0; i < neighbours.Length; i++)
                    {
                        if (neighbours[i].Item1 != null && doneIn[i])
                        {
                            acum = Xor(acum, neighbours[i].Item1.Response[neighbours[i].Item2]);
                        }
                        else
                        {
                            if (neighbours[i].Item1 != null && !doneIn[i])
                            {
                                index = i;
                            }
                        }
                    }
                    Response[index] = acum;
                    doneOut[index] = true;
                }

                if (hubsPendingIn == 0)
                {
                    SetResponses();
                }

                if (CanWrite)
                {
                    for (int i = 0; i < neighbours.Length; i++)
                    {
                        if (doneOut[i] && !doneUpdated[i] && neighbours[i].Item1 != null)
                        {
                            if (Response[i] != CableState.empty)
                            {
                                recorder.Record(true, Response[i] == CableState.one, portList[i], clock);
                            }
                            neighbours[i].Item1.PortState[neighbours[i].Item2] = Response[i];
                            doneUpdated[i] = true;
                            amountOfPendingOut--;
                            if (neighbours[i].Item1 is IHub)
                            {
                                ((IHub)neighbours[i].Item1).Touch(neighbours[i].Item2);
                            }
                        }
                    }
                    if (amountOfPendingOut == 0)
                    {
                        IsEnd = true;
                    }

                }
            }

        }

        public override int Connect(INode node, int portIn, int PortOut)//QUITAR LA LISTA QUE ESTÁ DE MÁS
        {
            if (neighbours[portIn].Item1 == null && node is IHub)
            {
                amountOfHubs++;
                hubsPendingIn = amountOfHubs;
            }
            amountOfConnectedDevice++;
            amountOfPendingOut = amountOfConnectedDevice;
            return base.Connect(node, portIn, PortOut);
        }

        public override int Disconect(int port)
        {
            if (neighbours[port].Item1 is IHub)
            {
                amountOfHubs--;
            }
            amountOfConnectedDevice--;
            amountOfPendingOut = amountOfConnectedDevice;
            return base.Disconect(port);
        }

        public virtual void Reset()
        {
            hubsPendingIn = amountOfHubs;
            for (int i = 0; i < doneIn.Length; i++)
            {
                doneIn[i] = false;
                doneOut[i] = false;
                doneUpdated[i] = false;
                Response[i] = CableState.empty;
            }
            amountOfPendingOut = amountOfConnectedDevice;
            IsFirstTime = true;
            IsEnd = false;
            CanWrite = false;
        }
    }
}

