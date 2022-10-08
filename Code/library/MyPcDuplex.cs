using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSimul.Component
{
    public class MyPcDuplex : MyPC, IPcDuplex
    {
        //MyPC:
        //
        //Colisionar cuando toca 
        //Leer cuando se escribe también
        //Ir construyendo el frame
        //Construir el nuevo recorder
        //El nuevo send
        //Setear la mac
        //El write

        //MyHub:
        //

        int timeReading = 0;

        protected LinkedList<bool> currentFrame = new LinkedList<bool>();

        public string Mac
        {
            get;
            set;
        }

        CableState lastResponse;

        CableState lastLecture = CableState.empty;

        IFrameRecorder frameRecorder;

        IErrorDetectionAlg errorDetectionAlg;

        protected virtual void MessageRecieved(string fromMac, string data, int time_ms)
        {

        }

        void Cut()
        {
            if (currentFrame.Count > 0)
            {
                bool[] data = currentFrame.ToArray();
                Frame frame;
                if (Frame.DisPackFrame(data, out frame) && (frame.HexaToMac == Mac || frame.HexaToMac == "ffff"))
                {
                    
                    if (errorDetectionAlg.DetectNoise(frame.BinData, frame.BinDataV) == null)
                    {
                        frameRecorder.Record(frame.HexaFromMac, frame.HexaData, clock - currentFrame.Count * signalTime, false);

                    }
                    else
                    {
                        frameRecorder.Record(frame.HexaFromMac, frame.HexaData, clock - currentFrame.Count * signalTime, true);

                        MessageRecieved(frame.HexaFromMac, frame.BinData, clock - currentFrame.Count * signalTime);
                    }
                }
                currentFrame = new LinkedList<bool>();
            }
        }

        protected override bool HaveCollision()
        {
            return lastResponse != Response[0];
        }

        public override bool Read()
        {

            if (PortState[0] != CableState.empty)
            {

                if (PortState[0] == lastLecture)
                {
                    timeReading++;
                }
                else
                {
                    timeReading = 1;
                }
                if (timeReading == signalTime)
                {
                    currentFrame.AddLast(PortState[0] == CableState.one);
                    timeReading = 0;
                }
                lastLecture = PortState[0];
            }
            else
            {
                Cut();
            }

            bool result = base.Read();
            lastResponse = Response[0];
            return result;
        }

        public override bool Write()
        {
            if (neighbours[0].Item1 != null)
            {
                neighbours[0].Item1.PortState[neighbours[0].Item2] = Response[0];
            }
            return false;
        }

        public MyPcDuplex(string name, int startTime, int signalTime, IRecorder recorder, IFrameRecorder frameRecorder, IErrorDetectionAlg errorDetectionAlg) : base(name, startTime, signalTime, recorder)
        {
            this.frameRecorder = frameRecorder;
            this.errorDetectionAlg = errorDetectionAlg;
        }
        /// <summary>
        /// Empaqueta el frame y lo envía usando la capa física
        /// </summary>
        /// <param name="toMac">La mac de destino en Hexadecimal</param>
        /// <param name="data">Los datos en Hexadecimal</param>
        public void SendFrame(string toMac, string data, DataConverterOption dataType = DataConverterOption.hexadecimal)
        {
            Frame frame = new Frame(Mac, toMac, data, errorDetectionAlg, dataType);
            bool[] pack;
            if (Frame.PackFrame(frame, out pack))
            {
                SetData(pack);
            }
        }
    }
}
