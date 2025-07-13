using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using MiniExcelLibs.Attributes;


/// <summary>
///  Object
/// </summary>
public static partial class ObjectExtensions
{
    /// <summary>
    /// 对象所有属性均是初始值
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsDefault(this object obj, bool justExcelProperty = false)
    {
        foreach(PropertyInfo pi in obj.GetType().GetProperties())
        {
            if(justExcelProperty)
            {
                foreach (var item in pi.GetCustomAttributes())
                {
                    if(item.GetType()  == typeof(ExcelIgnoreAttribute))
                    {
                        continue;
                    } 
                }
            }

            var value =  pi.GetValue(obj);
            var type = pi.PropertyType;
            //var _default = type.IsValueType ? Activator.CreateInstance(type) : null;
            if(type.IsValueType ){
                if(!value.Equals(Activator.CreateInstance(type)))  return false;
            } else{
                if(null != value && !value.Equals(default)) return false;
            }
 
        }
        return true;
    }
 
    /// <summary>
    /// 反射获取属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static object GetPropertyValue<T>(this T model, string propertyName) where T : new()
    {
        Type type = model.GetType(); //这里不能用typeof(T) 因为外部不一定会把泛型准确传入，如果传入的是object,自然会找不到要找的属性

        PropertyInfo property = type.GetProperty(propertyName);

        if (property == null) 
        {
            UnityEngine.Debug.LogError($"反射获取属性失败，type:{type.Name},field:{propertyName}");
            return null;
        }

        object value = property.GetValue(model, null);

        return value;
    }

    /// <summary>
    /// 反射 设置属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public static void SetPropertyValue<T>(this T model, string propertyName, object value) where T : new()
    {
        if (model == null) return;
        // model.GetType().GetProperty(propertyName).SetValue(model, value);
        Type type = model.GetType();

        PropertyInfo property = type.GetProperty(propertyName);

        if (property == null) 
        {
           // UnityEngine.Debug.LogError($"反射设置属性失败，type:{type.Name},field:{propertyName}");
            return;
        }

 
        //支持int属性的赋值范围约束
        if(property.PropertyType == typeof(int))
        {
            if(value is long) //需处理long2int
            {
               value = int.Parse(value.ToString());
            }

        }

 
        property.SetValue(model, value);

    }
}
 
 
