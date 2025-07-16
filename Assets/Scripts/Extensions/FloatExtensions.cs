using UnityEngine;


/// <summary>
///  Float
/// </summary>
public static partial class FloatExtensions
{
    
    public static int CustomCeil(this float value)
    {
        if (value >= 0)
            return Mathf.CeilToInt(value); // 正数向上取整
        else
            return Mathf.FloorToInt(value); // 负数向下取整
    }
    
    public static int CustomFloor(this float value)
    {
        if (value >= 0)
            return Mathf.FloorToInt(value); // 正数向下取整
        else
            return Mathf.CeilToInt(value); // 负数向上取整
    }
 
 }
 
 
