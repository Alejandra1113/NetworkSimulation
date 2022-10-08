namespace NetSimul.Component;

public class ParityCheck : IErrorDetectionAlg
{
    public string DetectNoise(string data, string verificationData)
    {
        if (GetVerificationData(data) == verificationData)
            return data;
        return null;

    }

    public string GetVerificationData(string data)
    {
        int p = data.Length / 8 == 1 ? 4 : data.Length / 8;
        int q = p * 8 == data.Length ? 8 : 2;
        string verificationData = GetParityBit(p, q, data, false);
        verificationData = verificationData + GetParityBit(q, p, data, true);
        return Increment(verificationData);
    }
    private string Increment(string data)
    {
        while (data.Length % 8 != 0)
        {
            data += "0";
        }
        return data;
    }
    private string GetParityBit(int x, int y, string data, bool t)
    {
        string verificationData = "";
        int dir;
        int currentCount = 0;
        for (int i = 0; i < x; i++)
        {
            currentCount = 0;
            for (int j = 0; j < y; j++)
            {
                dir = t ? (x * j) + i : (y * i) + j;
                if (data[dir] == '1')
                    currentCount++;
            }

            verificationData = $"{verificationData}{(y % 2 == 0 ? (currentCount % 2 == 0 ? "0" : "1") : (currentCount % 2 == 0 ? "1" : "0"))}";
        }
        return verificationData;
    }
}
