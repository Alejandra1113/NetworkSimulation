namespace NetSimul.Component;

public interface IRouter : INode
{

    string[] macList { get; set; }

    (string, string)[] ipList { get; set; }

}