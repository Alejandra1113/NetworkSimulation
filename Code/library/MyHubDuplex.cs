using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSimul.Component
{
    public class MyHubDuplex : MyHub
    {
        bool[] dontTouchPleaseYameteKudasaiToniSan;
        bool sendCollisions = false;

        CableState[] OldResponse;
        public MyHubDuplex(string Name, int amountOfPorts, int startTime, IRecorder recorder) : base(Name, amountOfPorts, startTime, recorder)
        {
            dontTouchPleaseYameteKudasaiToniSan = new bool[amountOfPorts];
            OldResponse = new CableState[amountOfPorts];

        }

        public override void Touch(int port)
        {
            base.Touch(port);
            bool recieveCollision = false;
            for (int i = 0; i < Response.Length; i++)
            {
                recieveCollision |= Response[i] != OldResponse[i];
            }

            if (IsEnd && CanWrite)
            {
                int count = 0;
                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (PortState[i] != CableState.empty)
                    {
                        count++;
                    }
                    if (count >= 3 || recieveCollision)
                    {
                        for (int j = 0; j < PortList.Length; j++)
                        {
                            neighbours[j].Item1.Response[neighbours[j].Item2] = Response[j];

                            if (neighbours[j].Item1 is IHub && !dontTouchPleaseYameteKudasaiToniSan[j])
                            {
                                dontTouchPleaseYameteKudasaiToniSan[j] = true;
                                ((IHub)neighbours[j].Item1).Touch(neighbours[j].Item2);
                            }
                            else
                            {
                                dontTouchPleaseYameteKudasaiToniSan[j] = true;
                            }
                        }
                        break;
                    }
                }
            }
        }

        protected override void SetResponses()
        {
            CableState acum = CableState.empty;
            int count = 0;
            int index = -1;
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i].Item1 != null)
                {
                    acum = Xor(acum, PortState[i]);
                    if (PortState[i] != CableState.empty)
                    {
                        count++;
                        index = i;
                    }
                }
            }

            for (int i = 0; i < neighbours.Length; i++)
            {
                doneOut[i] = true;
                if (count == 1 && i == index)
                {
                    Response[i] = CableState.empty;
                    OldResponse[i] = CableState.empty;
                    continue;
                }
                Response[i] = Xor(acum, PortState[i]);
                OldResponse[i] = Xor(acum, PortState[i]);

            }
        }

        public override async void Reset()
        {
            base.Reset();
            sendCollisions = false;
            for (int i = 0; i < dontTouchPleaseYameteKudasaiToniSan.Length; i++)
            {
                dontTouchPleaseYameteKudasaiToniSan[i] = false;
            }
        }
    }
}
