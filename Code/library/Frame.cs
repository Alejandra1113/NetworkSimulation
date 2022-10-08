using NetSimul.Extensions;
namespace NetSimul.Component;

public class Frame
{
    private ushort fromMac;
    private string hexaFromMac;
    private ushort toMac;
    private string hexaToMac;
    private byte dataSize;
    private byte dataVSize;
    private string hexaData;
    private string binData;
    private string hexaDataV;
    private string binDataV;

    public Frame(string fromMac, string toMac, string data, IErrorDetectionAlg ErrorDetect, DataConverterOption option = DataConverterOption.hexadecimal)
    {
        ConvertData(data, out binData, out hexaData, option);
        binDataV = ErrorDetect.GetVerificationData(binData);
        hexaDataV = binDataV.ConvertToBase(2, 16);
        dataSize = (byte)binData.Length;
        dataVSize = (byte)binDataV.Length;
        this.hexaFromMac = (fromMac.IsHexNumber()) ? fromMac : throw new FormatException("la mac tiene que ser un hexadecimal");
        this.hexaToMac = (toMac.IsHexNumber()) ? toMac : throw new FormatException("la mac tiene que ser un hexadecimal");
        this.fromMac = ConvertStrToShort(fromMac);
        this.toMac = ConvertStrToShort(toMac);
    }

    public Frame(string fromMac, string toMac, string data, string dataV, DataConverterOption option = DataConverterOption.hexadecimal)
    {
        ConvertData(data, out binData, out hexaData, option);
        ConvertData(dataV, out binDataV, out hexaDataV, option);
        dataSize = (byte)binData.Length;
        dataVSize = (byte)binDataV.Length;
        this.hexaFromMac = (fromMac.IsHexNumber()) ? fromMac : throw new FormatException("la mac tiene que ser un hexadecimal");
        this.hexaToMac = (toMac.IsHexNumber()) ? toMac : throw new FormatException("la mac tiene que ser un hexadecimal");
        this.fromMac = ConvertStrToShort(fromMac);
        this.toMac = ConvertStrToShort(toMac);
    }

    public Frame(ushort fromMac, ushort toMac, string data, IErrorDetectionAlg ErrorDetect, DataConverterOption option = DataConverterOption.hexadecimal)
    {
        ConvertData(data, out binData, out hexaData, option);
        binDataV = ErrorDetect.GetVerificationData(binData);
        hexaDataV = binDataV.ConvertToBase(2, 16);
        dataSize = (byte)binData.Length;
        dataVSize = (byte)binDataV.Length;
        hexaFromMac = ConvertShortToStr(fromMac);
        hexaToMac = ConvertShortToStr(toMac);
        this.fromMac = fromMac;
        this.toMac = toMac;
    }

    public Frame(ushort fromMac, ushort toMac, string data, string dataV, DataConverterOption option = DataConverterOption.hexadecimal)
    {
        ConvertData(data, out binData, out hexaData, option);
        ConvertData(dataV, out binDataV, out hexaDataV, option);
        dataSize = (byte)binData.Length;
        dataVSize = (byte)binDataV.Length;
        hexaFromMac = ConvertShortToStr(fromMac);
        hexaToMac = ConvertShortToStr(toMac);
        this.fromMac = fromMac;
        this.toMac = toMac;
    }

    public ushort FromMac => fromMac;
    public string HexaFromMac => hexaFromMac;
    public ushort ToMac => toMac;
    public string HexaToMac => hexaToMac;
    public byte DataSize => dataSize;
    public byte DataVSize => dataVSize;
    public string HexaData => hexaData;
    public string HexaDataV => hexaDataV;
    public string BinData => binData;
    public string BinDataV => binDataV;

    private void ConvertData(string data, out string binData, out string hexaData, DataConverterOption option)
    {
        switch (option)
        {
            case DataConverterOption.hexadecimal:
                hexaData = data;
                binData = data.ConvertToBase(frombase:16, tobase:2, mullsize:4, size:(data.Length));
                return;
            case DataConverterOption.binario:
                binData = data;
                hexaData = data.ConvertToBase(frombase:2, tobase:16, mullsize:2, size:(data.Length / 8));
                return;
            default:
                throw new InvalidCastException();
        }
    }

    private ushort ConvertStrToShort(string number)
    {
        byte[] numberPart = number.Chunk(2).Select(x => Convert.ToByte(string.Concat(x), 16)).ToArray();
        return (ushort)((numberPart[0] << 8) + numberPart[1]);
    }

    private string ConvertShortToStr(ushort number)
    {
        return Convert.ToHexString(new byte[] { (byte)(number / 256), (byte)(number % 256) }).ToLower();
    }

    public static bool PackFrame(Frame frame, out List<bool>? pack)
    {
        bool[]? arraypack;
        bool result = PackFrame(frame, out arraypack);
        pack = (arraypack != null) ? arraypack.ToList() : null;
        return result;
    }

    public static bool PackFrame(Frame frame, out bool[]? pack)
    {
        pack = null;
        if (frame.dataSize != frame.binData.Length || frame.dataVSize != frame.binDataV.Length) return false;

        pack = new bool[48 + frame.dataSize + frame.dataVSize];
        byte[] bytepack = new byte[6];

        byte[] aux;
        int packIndex = -1;

        aux = BitConverter.GetBytes(frame.fromMac);
        bytepack[++packIndex] = aux[1];
        bytepack[++packIndex] = aux[0];
        aux = BitConverter.GetBytes(frame.toMac);
        bytepack[++packIndex] = aux[1];
        bytepack[++packIndex] = aux[0];
        bytepack[++packIndex] = frame.dataSize;
        bytepack[++packIndex] = frame.dataVSize;
        for (int i = 0; i < bytepack.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                pack[i * 8 + j] = ((bytepack[i] << j) & 128) == 128;
            }
        }
        Array.Copy(frame.binData.Select(x => x == '1').ToArray(), 0, pack, 48, frame.dataSize);
        Array.Copy(frame.binDataV.Select(x => x == '1').ToArray(), 0, pack, 48 + frame.dataSize, frame.dataVSize);

        return true;
    }

    public static bool DisPackFrame(List<bool> pack, out Frame? frame) => DisPackFrame(pack.ToArray(), out frame);

    public static bool DisPackFrame(bool[] pack, out Frame? frame)
    {
        frame = null;
        if (pack.Length < 48) return false;
        string data = "";
        string dataV = "";

        byte[] bytepack = new byte[6];
        for (int i = 0; i < bytepack.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                bytepack[i] = (byte)((bytepack[i] << 1) + (pack[i * 8 + j] ? 1 : 0));
            }
        }
        data = Select(x => x >= 48 && x < 48 + bytepack[4], string.Concat(pack.Select(x => x ? '1' : '0')));
        dataV = Select(x => x >= 48 + bytepack[4] && x < 48 + bytepack[4] + bytepack[5], string.Concat(pack.Select(x => x ? '1' : '0')));

        frame = new Frame((ushort)((bytepack[0] << 8) + bytepack[1]), (ushort)((bytepack[2] << 8) + bytepack[3]), data, dataV, DataConverterOption.binario);
        return 48 + bytepack[4] + bytepack[5] == pack.Length;
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