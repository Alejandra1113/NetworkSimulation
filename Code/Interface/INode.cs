namespace NetSimul.Component
{
    public interface INode
    {
        string Id
        {
            get;
        }
        /// <summary>
        /// Nombres de los puertos del nodo
        /// </summary>
        string[] PortList
        {
            get;
        }
        /// <summary>
        /// Espacio en memoria donde se guarda lo que recibe el nodo
        /// </summary>
        CableState[] PortState
        {
            get;
        }
        /// <summary>
        /// Espacio en memoria donde se guarda lo que responde el nodo
        /// </summary>
        CableState[] Response
        {
            get;
        }
        (INode, int)[] Neighbours
        {
            get;
        }

        bool Read();

        bool Write();

        /// <summary>
        /// deconecta un nodo con Id igual al recibido en el par�metro
        /// </summary>
        /// <param name="Id"> nodo a desconectar</param>
        /// <returns>0 si el nodo estaba en la lista de vecinos y fu� eliminado y 0 si no estaba</returns>
        int Disconect(int port);
        /// <summary>
        /// A�ade al nodo en la lista de vecinos
        /// </summary>
        /// <param name="Id">Nodo a conectar</param>
        /// <returns>0 si el nodo fu� conectado correctamente, 1 si el nodo ya alcanz� su m�xima cantidad de vecinos</returns>
        int Connect(INode Id, int portIn, int portOut);
    }
}

