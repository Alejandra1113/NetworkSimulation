namespace NetSimul.Component;

public interface IPC : INode
{

    IRecorder Recorder
    {
        set;
    }

    void SetData(bool[] data);
}
