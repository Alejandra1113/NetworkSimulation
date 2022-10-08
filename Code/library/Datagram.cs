using NetSimul.Extensions;
namespace NetSimul.Component;

public class Datagram
{
    private byte[] fromIp;
    private string decFromIp;
    private byte[] toIp;
    private string decToIp;
    private byte timeToLive;
    private byte protocol;
    private byte dataSize;
    private string hexaData;
    private string binData;

    public Datagram(string fromIp, string toIp, string data, byte timeToLive, byte protocol, DataConverterOption option = DataConverterOption.hexadecimal)
    {
        DataFilterer(data, option, out hexaData, out binData);
        dataSize = (byte)binData.Length;
        this.timeToLive = timeToLive;
        this.protocol = protocol;
        this.decFromIp = (fromIp.IsIpNumber()) ? fromIp : throw new FormatException("la mac tiene que ser un hexadecimal");
        this.decToIp = (toIp.IsIpNumber()) ? toIp : throw new FormatException("la mac tiene que ser un hexadecimal");
        this.fromIp = Route.StrToByte(fromIp);
        this.toIp = Route.StrToByte(toIp);
    }

    public Datagram(byte[] fromIp, byte[] toIp, string data, byte timeToLive, byte protocol, DataConverterOption option = DataConverterOption.hexadecimal)
    {
        DataFilterer(data, option, out hexaData, out binData);
        dataSize = (byte)binData.Length;
        decFromIp = Route.ByteToStr(fromIp);
        decToIp = Route.ByteToStr(toIp);
        this.timeToLive = timeToLive;
        this.protocol = protocol;
        this.fromIp = fromIp;
        this.toIp = toIp;
    }

    public byte[] FromIp => fromIp;
    public string DecFromIp => decFromIp;
    public byte[] ToIp => toIp;
    public string DecToIp => decToIp;
    public byte DataSize => dataSize;
    public string HexaData => hexaData;
    public string BinData => binData;
    public byte TimeToLive => timeToLive;
    public byte Protocol => protocol;

    private void DataFilterer(string data, DataConverterOption option, out string hexaData, out string binData)
    {
        switch (option)
        {
            case DataConverterOption.binario:
                binData = data;
                hexaData = data.ConvertToBase(frombase:2, tobase:16, mullsize:2, size:(data.Length / 8));
                break;
            case DataConverterOption.hexadecimal:
                hexaData = data;
                binData = data.ConvertToBase(frombase:16, tobase:2, mullsize:4, size:(data.Length));
                break;
            default:
                throw new InvalidCastException();
        }
    }

    public static bool PackDatagram(Datagram datagram, out string? pack)
    {
        pack = null;
        if (datagram.dataSize != datagram.binData.Length) return false;

        pack = "";
        byte[] bytepack = new byte[11];
        int packIndex = 0;

        Array.Copy(datagram.FromIp, 0, bytepack, packIndex, datagram.FromIp.Length);
        packIndex += datagram.FromIp.Length;
        Array.Copy(datagram.ToIp, 0, bytepack, packIndex, datagram.ToIp.Length);
        packIndex += datagram.ToIp.Length;
        bytepack[packIndex] = datagram.TimeToLive;
        bytepack[++packIndex] = datagram.Protocol;
        bytepack[++packIndex] = datagram.DataSize;
        
        for (int i = 0; i < bytepack.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                pack += (((bytepack[i] << j) & 128) == 128) ? '1' : '0';
            }
        }
        pack += datagram.binData;
        return true;
    }

    public static bool DisPackDatagram(string pack, out Datagram? datagram)
    {
        datagram = null;
        if (pack.Length < 88) return false;
        string data = "";

        byte[] bytepack = new byte[11];
        for (int i = 0; i < bytepack.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                bytepack[i] = (byte)((bytepack[i] << 1) + ((pack[i * 8 + j] == '1') ? 1 : 0));
            }
        }
        data = Select(x => x >= 88 && x < 88 + bytepack[10], pack);

        byte[] fromIp = new byte[4] { bytepack[0], bytepack[1], bytepack[2], bytepack[3] };
        byte[] toIp = new byte[4] { bytepack[4], bytepack[5], bytepack[6], bytepack[7] };
        datagram = new Datagram(fromIp, toIp, data, bytepack[8], bytepack[9], DataConverterOption.binario);
        return 88 + bytepack[10] == pack.Length;
    }

    private static string Select(Func<int, bool> func, string data)
    {
        string result = "";
        for (int i = 0; i < data.Length; i++)
        {
            if (func(i))
            {
                result += data[i];
            }
        }
        return result;
    }
}