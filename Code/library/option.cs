namespace NetSimul;

public struct options
{
    private int signalTime;
    private string path;
    public const int numberOptions = 1;
    string alg;

    public options(StreamReader config)
    {
        signalTime = 10;
        string? script = config.ReadLine();
        string index = script.Substring(script.IndexOf('=') + 1);
        if (script == null || !int.TryParse(index, out signalTime))
        {
            throw new FormatException("no cargo bien el config.txt");
        }
        script = config.ReadLine();
        if (script == null)
        {
            throw new FormatException("no cargo bien el config.txt");
        }
        path = script.Substring(script.IndexOf('=') + 1);
        script = config.ReadLine();
        if (script == null)
        {
            throw new FormatException("no cargo bien el config.txt");
        }
        alg = script.Substring(script.IndexOf('=') + 1);
    }

    public options(string path, int signalTime = 10, string alg = "ParityChec")
    {
        this.path = path;
        this.signalTime = signalTime;
        this.alg = alg;
    }

    public int SignalTime => signalTime;

    public string Path => path;

    public string Alg => alg;
}