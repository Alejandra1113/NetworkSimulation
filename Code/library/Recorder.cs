using NetSimul.Component;

public class Recorder : IRecorder
{
    private string path;
    private bool firstTime;

    public Recorder(string path)
    {
        this.path = path;
        firstTime = true;
    }

    public string Path => path;

    public void Record(bool is_send, bool is_one, string port, int time_ms)
    {
        Send($"{time_ms} {port} {(is_send ? "Send" : "Receive")} {(is_one ? 1 : 0)}");
    }

    public void Record(bool is_send, bool is_one, bool is_collision, string port, int time_ms)
    {
        if (!is_send)
        {
            Record(is_send, is_one, port, time_ms);
        }
        else
        {
            Send($"{time_ms} {port} {(is_send ? "Send" : "Receive")} {(is_one ? 1 : 0)} {(is_collision ? "Collision" : "Ok")}");
        }
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

public class RecorderDummy : IRecorder
{
    public string Path => "";

    public void Record(bool is_send, bool is_one, string port, int time_ms) { }
    public void Record(bool is_send, bool is_one, bool is_collision, string port, int time_ms) { }
}