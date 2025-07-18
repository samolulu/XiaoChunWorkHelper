using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
 


/// <summary>
/// 字符串拓展
/// </summary>
public static class StringExtensions
{ 

    /// <summary>
    /// 是否空或者“0”
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool IsNullOrZero(this string text)
    {
        if(text == null || text=="0" || text == "")
        {
            return true;
        }
        return false;
    }

  
    /// <summary>
    /// string 转 int   如果失败就返回0
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int ToInt(this string str)
    {
        //int value = int.MinValue;
        int value = 0;
        int.TryParse(str, out value);
        return value;
    }


    /// <summary>
    /// string 转 float   如果失败就返回0f
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static float ToFloat(this string str)
    {
        //int value = int.MinValue;
        float value = 0f;
        float.TryParse(str, out value);
        return value;
    }  
    
    /// <summary>
    /// 去掉换行
    /// </summary>
    /// <returns></returns>
    public static string ReplaceLineBreaks(this string str)
    {
        var newStr = str.Replace("\n", "", StringComparison.Ordinal);
        newStr = newStr.Replace("\\n", "", StringComparison.Ordinal);
        return newStr;
    }

    /// <summary>
    /// string 转 long  如果失败就返回0
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static long ToLong(this string str)
    {
        long value = 0;
        long.TryParse(str, out value);
        return value;
    }
    public static ulong ToUlong(this string str)
    {
        ulong value = 0;
        ulong.TryParse(str, out value);
        return value;
    }
 
    /// <summary>
    /// 得到int元组 2位
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splite"></param>
    /// <returns></returns>
    public static (int, int) ToIntValueTuple(this string str, char splite = ',')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new(0, 0);
        }
        string[] strs = str.ToStringArray(splite);
        if (strs != null)
        {
            return new(strs.Length > 0 ?strs[0].ToInt():0, strs.Length > 1 ?strs[1].ToInt():0);
        }

        return new(0, 0);
    }


 

    /// <summary>
    /// 得到int元组 3位
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splite"></param>
    /// <returns></returns>
    public static (int, int, int) ToIntValueTuple3(this string str, char splite = ',')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new(-1, -1, -1);
        }
        string[] strs = str.ToStringArray(splite);
        if (strs != null && strs.Length > 2)
        {
            return new(strs[0].ToInt(), strs[1].ToInt(), strs[2].ToInt());
        }

        return new(-1, -1, -1);
    }
 

    /// <summary>
    /// 得到int元组 4位
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splite"></param>
    /// <returns></returns>
    public static (int, int, int, int) ToIntValueTuple4(this string str, char splite = ',')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new(-1, -1, -1, -1);
        }
        string[] strs = str.ToStringArray(splite);
        if (strs != null && strs.Length > 1)
        {
            return new(strs[0].ToInt(), strs[1].ToInt(), strs[2].ToInt(), strs[3].ToInt());
        }

        return new(-1, -1, -1, -1);
    }
    public static string[] ToStringArray(this string str, char splite = ',')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new string[] { };
        }

        return str.Split(splite, StringSplitOptions.RemoveEmptyEntries);
    }


    /// <summary>
    /// 转字符数组
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splite">分割字符串 如:"||"</param>
    /// <returns></returns>
    public static string[] ToStringArray(this string str, string splite)//= ","
    {
        if (string.IsNullOrEmpty(str))
        {
            return new string[] { };
        }

        return str.Split(splite, StringSplitOptions.RemoveEmptyEntries);
    }
 
    public static List<string> ToStringList(this string str, char splite = ':', StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<string>();
        }

        return new List<string>(str.Split(splite, options));
    }
    
    public static List<string> ToStringList(this string str, string splite, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<string>();
        }

        return new List<string>(str.Split(splite, options));
    }

    public static List<int> ToIntList(this string str, char splite = ':')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<int>();
        }
        List<int> listInt = str.Trim().Split(splite, StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(s => s.ToInt());
        return listInt;
    }
 

    public static List<long> ToLongList(this string str, char splite = ':')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<long>();
        }

        List<long> listInt = str.Trim().Split(new char[] { splite }).ToList().ConvertAll(s => s.ToLong());
        return listInt;
    }
 

    public static string ConvertToString(this List<int> intList, char splite = ',')
    {
        return string.Join(splite, intList);
    }

    /// <summary>
    ///     Returns an enumerable collection of the specified type containing the substrings in this instance that are
    ///     delimited by elements of a specified Char array
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="separator">
    ///     An array of Unicode characters that delimit the substrings in this instance, an empty array containing no
    ///     delimiters, or null.
    /// </param>
    /// <typeparam name="T">
    ///     The type of the element to return in the collection, this type must implement IConvertible.
    /// </typeparam>
    /// <returns>
    ///     An enumerable collection whose elements contain the substrings in this instance that are delimited by one or more
    ///     characters in separator.
    /// </returns>
    public static IEnumerable<T> SplitTo<T>(this string str, params char[] separator) where T : IConvertible
    {
        return str.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(s => (T)Convert.ChangeType(s, typeof(T)));
    }


    public static List<List<int>> ToIntList2(this string str, char splite1 = '_', char splite2 = '|')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new();
        }
        List<List<int>> list  =  new();
        List<string> list1 =  str.ToStringList(splite2);
        for (int i = 0; i < list1.Count; i++)
        {
            list.Add( list1[i].ToIntList(splite1) );
        }
        return list;
    }

    public static List<List<List<int>>> ToIntList3(this string str, char splite1 = '_', char splite2 = '|', char splite3 = '#')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new();
        }
        List<List<List<int>>> list  =  new();
        List<string> list1 =  str.ToStringList(splite3);
        for (int i = 0; i < list1.Count; i++)
        {
            list.Add( list1[i].ToIntList2(splite1, splite2) );
        }
        return list;
    }


    /*
    public static List<string> ToList(this string[] strArray)
    {
        return new List<string>(strArray);
    }
    */

    public static void PrintCharArray(this string str)
    {
        if (str == null) return;
        char[] charLen = str.ToCharArray();
        Debug.LogError(string.Format("str Length {0}", charLen.Length));
        for (int i = 0; i < charLen.Length; i++)
        {
            Debug.LogError(string.Format("char {0}", charLen[i]));
        }
    }

    /// <summary>
    /// 计算utf8字符 占用字节数
    /// </summary>
    /// <param name="_char"></param>
    /// <returns></returns>
    public static int GetCharLength(this char _char)
    {
        if (_char > 240)
        {
            return 4;
        }
        else if (_char > 225)
        {
            return 3;
        }
        else if (_char > 192)
        {
            return 2;
        }

        return 1;
    }
}
