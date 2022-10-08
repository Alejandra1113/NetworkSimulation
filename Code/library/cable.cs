using NetSimul.Component;

public class cable : ICable
{
    private IPC pc1;
    private IPC pc2;

    public cable(IPC pc1, IPC pc2)
    {
        this.pc1 = pc1;
        this.pc2 = pc2;
    }

    public IPC Pc1 => pc1;
    public IPC Pc2 => pc2;

    public void Update()
    {
        if (pc1.Response[0] == pc2.Response[0] && pc1.Response[0] == CableState.empty)
        {
            pc1.PortState[0] = pc2.PortState[0] = CableState.empty;
        }
        else if (pc1.Response[0] == CableState.empty)
        {
            pc1.PortState[0] = pc2.PortState[0] = pc2.Response[0];
        }
        else if (pc2.Response[0] == CableState.empty)
        {
            pc1.PortState[0] = pc2.PortState[0] = pc1.Response[0];
        }
        else
        {
            pc1.PortState[0] = pc2.PortState[0] = ((pc1.Response[0] == pc2.Response[0]) ? CableState.zero : CableState.one);
        }
    }
}