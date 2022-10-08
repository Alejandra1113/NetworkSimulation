
namespace NetSimul.Component
{
    public interface IPcDuplex : IPC
    {
        string Mac { get; set; }

        void SendFrame(string toMac, string data, DataConverterOption dataType = DataConverterOption.hexadecimal);
    }
}