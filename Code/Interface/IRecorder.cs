namespace NetSimul.Component;

public interface IRecorder
{
    string Path
    {
        get;
    }

    void Record(bool is_send, bool is_one, bool is_collision, string name, int time_ms);

    void Record(bool is_send, bool is_one, string name, int time_ms);
}