using NetSimul.Component;

public class FrameRecorder : IFrameRecorder
{
    private string path;
    private bool firstTime;

    public FrameRecorder(string path)
    {
        this.path = path;
        firstTime = true;
    }

    public string Path => path;

    public void Record(string fromMac, string data, int time_ms, bool notError)
    {
        Send($"{time_ms} {fromMac} {data}{(notError? "" : " ERROR")}");
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

public class FrameRecorderDummy : IFrameRecorder
{
    public string Path => "";

    public void Record(string fromMac, string data, int time_ms, bool error) { }
}