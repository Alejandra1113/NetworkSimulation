namespace NetSimul.Component;

public class CheckSum : IErrorDetectionAlg
{
    public string DetectNoise(string data, string verificationData)
    {    
        int verif = BinaryToDec(verificationData,0,verificationData.Length);
        string dataValue =OneComp(GetVerificationData(data)); 
        verif += BinaryToDec(dataValue,0,dataValue.Length);
        dataValue = DecToBinary(verif);
        if(dataValue.Contains('0'))
        {
            return null;
        }
        return data;
    }
    private string Increment(string data)
    {
        while (data.Length % 8 != 0)
        {
            data += "1";
        }
        return data;
    }
    public string GetVerificationData(string data)
    {
        int sum = 0;
        for (int i = 0; i < (int)data.Length/8; i++)
        {
            sum+= BinaryToDec(data,i*8,8);
        }
        return Increment(OneComp(DecToBinary(sum)));
    }

    private int BinaryToDec(string binary, int beg, int len)
    {
        int result = 0;
        for (int i = 0; i < len; i++)
        {
            result +=  (int)(int.Parse(binary[len +beg - 1 -i]+"") * Math.Pow(2,i));
        }
       return result;
    }

    private string DecToBinary(int dec)
    {
        string result = "";
        while(dec > 0)
        {
           result = $"{dec%2}" +result;
           dec= dec/2;
        }
        return result;
    }
    private string OneComp(string num)
    {
        string result = "";
        for (int i = 0; i < num.Length; i++)
        {
            result += num[i]=='0'?"1":"0";        
        }
        return result;
    }
}