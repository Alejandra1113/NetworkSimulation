namespace NetSimul.Extensions;

public static class ExtensionClass
{

    public static bool IsIpNumber(this string str)
    {
        str += '.';
        int length = 0;
        string strnum = "";
        int number;
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '.')
            {
                number = 0;
                if (!int.TryParse(strnum, out number) || number > 255 || number < 0) return false;
                length++;
                strnum = (++i < str.Length) ? str[i].ToString() : "";
                continue;
            }
            if (length > 4) return false;
            strnum += str[i];
        }

        if (length < 4) return false;
        return true;
    }

    public static bool IsBinNumber(this string str, out bool[]? data)
    {
        bool[] resultData = new bool[str.Length];
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] != '0' && str[i] != '1')
            {
                data = null;
                return false;
            }
            resultData[i] = str[i] == '1';
        }
        data = resultData;
        return true;
    }

    public static bool IsHexNumber(this string str)
    {
        bool[] resultData = new bool[str.Length];
        for (int i = 0; i < str.Length; i++)
        {
            if ((str[i] < '0' || str[i] > '9') && (str[i] < 'a' || str[i] > 'f'))
            {
                return false;
            }
            resultData[i] = str[i] == '1';
        }
        return true;
    }

    public static string ConvertToBase(this string strnum, int frombase = 2, int tobase = 16, int mullsize = 1, int size = 0)
    {
        if (tobase < 2 || tobase > 16) throw new InvalidCastException();
        LinkedList<int> number;
        frombase = GetStrBase(strnum, out number, frombase);
        string strlowernum = "";

        while(number.Count != 0)
        {
            LinkedList<int> nexnumber = new LinkedList<int>();    
            int piv = 0;
            bool first = true;
            foreach (int act in number)
            {
                piv += act;

                if (piv >= tobase)
                {
                    nexnumber.AddLast(piv / tobase);
                    piv -= (piv / tobase) * tobase;
                    first = false;
                }
                else if (!first)
                {
                    nexnumber.AddLast(0);
                }

                piv *= frombase;
            }

            strlowernum = GetCharBase(piv / frombase) + strlowernum;
            number = nexnumber;
        }

        return FillConvertion(strlowernum, mullsize, size);
    }

    private static string FillConvertion(string strlowernum, int mullsize, int size)
    {
        string result = strlowernum;
        while (result.Length < size * mullsize || result.Length % mullsize != 0)
        {
            result = '0' + result;
        }
        return result;
    }

    private static int GetStrBase(string strnum, out LinkedList<int> number, int frombase = 0)
    {
        number = new LinkedList<int>();
        int max = 0;

        for (int i = 0; i < strnum.Length; i++)
        {
            number.AddLast(GetCharBase(strnum[i]));
            if (number.Last.Value == -1) throw new InvalidCastException();
            max = (max > number.Last.Value) ? max : number.Last.Value;
        }

        return (max + 1 > frombase) ? max + 1 : frombase;
    }

    private static int GetCharBase(char v)
    {
        if (v > 47 && v < 58) return v - 48;
        if (v > 64 && v - 64 < 17) return v - 65 + 10;
        return (v - 96 < 17) ? v - 97 + 10 : -1;
    }

    private static char GetCharBase(int v)
    {
        if (v < 10) return (char)(v + 48);
        return (v > 9 && v < 17) ? (char)(v + 97 - 10) : '!';
    }
}