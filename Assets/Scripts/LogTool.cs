using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class LogTool
{ 
#region  lsmlog
    static StringBuilder sb = new();
    public static void Log(object message)
    {
 
        message = $"<color=#008080>{message}</color>";
        Debug.Log(message);
      
    }  

    public static void Log(object item1, object item2 )
    {
 
        Log((item1, item2));
      
    }

    public static void Log(object item1, object item2 , object item3)
    {
 
        Log((item1, item2, item3));
      
    }

    public static void Log<T>(T data ) where T:IEnumerable,ICollection
    {
 
        sb.Clear();
         
        foreach (var item in data)
        {
            if(sb.ToString() != string.Empty)sb.Append(",");
            sb.Append(item);
            
        }
 
        Log(sb);
 
      
    }
    public static void Log<T>(object message, T data ) where T:IEnumerable,ICollection
    {
 
        sb.Clear();
        sb.Append(message); 
        foreach (var item in data)
        {
            sb.Append(",");
            sb.Append(item);
            
        }
 
        Log(sb);
 
      
    }

    public static void LogFormat(string message, params object[] args)
    {
 
        message = args.Length > 0 ? string.Format(message, args) : message;
        Log(message);
      
    }

    // public static void LogObject(object obj)
    // {
 
    //     string content = obj.ToJsonString(Formatting.Indented);
    //     LogLongStr(string.Empty, content);
      
    // }

    // public static void LogObject(string message, object obj)
    // {
 
    //     string content = obj.ToJsonString(Formatting.Indented);
    //     LogLongStr(message, content);
        
      
    // }

    public static void LogLongStr(string message, string content)
    {
 
        int cut = 10000;
        int pieces = Mathf.CeilToInt(content.Length/(float)cut);
        if(pieces <= 1)
        {
            Log(message, content);
            return;
        }
        if(!string.IsNullOrEmpty(message)) Log(message, "#longstr:");
        for (int i = 0; i < pieces; i++)
        {
            int l = cut; if(i == pieces -1) l = content.Length%cut;
            string p = content.Substring(i*cut,  l );
            Log($"#pieces:{i+1}#", p);
        }
        
  
    }

#endregion

}
