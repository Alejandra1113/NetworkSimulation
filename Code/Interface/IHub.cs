namespace NetSimul.Component;

public interface IHub : INode
{
    IRecorder Recorder
    {
        set;
    }

    void Touch(int port);
    void Reset();
}