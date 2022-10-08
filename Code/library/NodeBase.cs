
using NetSimul.Component;

public abstract class NodeBase : INode
{
    public string Id
    {
        get;
        protected set;
    }

    protected string[] portList;
    public string[] PortList
    {
        get
        {
            var result = new string[portList.Length];
            for (int i = 0; i < portList.Length; i++)
            {
                result[i] = portList[i];
            }
            return result;
        }
    }

    public CableState[] PortState
    {
        get;
        protected set;
    }
    public CableState[] Response
    {
        get;
        protected set;
    }

    protected (INode, int)[] neighbours;

    public (INode, int)[] Neighbours
    {
        get
        {
            (INode, int)[] result = new (INode, int)[neighbours.Length];
            neighbours.CopyTo(result, 0);
            return result;
        }
    }

    /// <summary>
    /// a�ade al nodo a la lista de vecinos
    /// </summary>
    /// <param name="node">El nodo que se va a conectar</param>
    /// <param name="port">El puerto a donde se va a conectar</param>
    /// <returns>0 si el nodo fu� a�adido, 1 si ya era vecino</returns>
    public virtual int Connect(INode node, int portIn, int portOut)
    {

        if (neighbours[portIn].Item1 != null)
        {
            return 1;
        }
        else
        {
            neighbours[portIn].Item1 = node;
            neighbours[portIn].Item2 = portOut;
            return 0;
        }
    }
    /// <summary>
    /// remueve el nodo indicado de la lista de vecinos
    /// </summary>
    /// <param name="port"></param>
    /// <returns>0 si el nodo se encontraba en la lista de vecinos, 1 si no estaba</returns>
    public virtual int Disconect(int portIn)
    {

        if (neighbours[portIn].Item1 == null)
        {
            return 1;
        }
        else
        {
            neighbours[portIn].Item1 = null;
            neighbours[portIn].Item2 = -1;
            return 0;
        }
    }
    public abstract bool Read();
    public abstract bool Write();

}