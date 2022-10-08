using NetSimul.Component;
using NetSimul.Extensions;
/* 
▪ 0 → echo reply (la respuesta a un Ping, tambien conocido como Pong)
▪ 3 → destination host unreachable
▪ 8 → echo request (lo que conocemos como Ping)
▪ 11 → time exceeded (tiene que ver con el campo TTL que todavía no estamos usando) 
*/

public class DatagramRecorder : IDatagramRecorder
{
    public readonly (int, string)[] ICMP = 
    { 
        (0, "echo reply"),
        (3, "destination host unreachable"),
        (8, "echo request"),
        (11, "time exceeded")
    };
    private string path;
    private bool firstTime;

    public DatagramRecorder(string path)
    {
        this.path = path;
        firstTime = true;
    }

    public string Path => path;

    public void Record(string fromIp, string data, int time_ms, byte protocolo)
    {
        string source = "";
        if (protocolo == 1)
        {
            int numpayload = int.Parse(data.ConvertToBase(frombase: 16, tobase: 10));
            source = ICMP.First(x => x.Item1.Equals(numpayload)).Item2;
        }
        Send($"{time_ms} {fromIp} {data}{((source == "") ? "" : $" {source}")}");
    }

    private void Send(string response)
    {
        StreamWriter stream;
        if (firstTime)
        {
            stream = File.CreateText(path);
            firstTime = false;
        }
        else
        {
            stream = File.AppendText(path);
        }
        stream.WriteLine(response);
        stream.Close();
    }
}

public class DatagramRecorderDummy : IDatagramRecorder
{
    public string Path => "";

    public void Record(string fromIp, string data, int time_ms, byte protocolo) { }
}