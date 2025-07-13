using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 员工表
/// </summary>
public class Stuff
{
    public string Name { get; set; }
    public string 报表名字 { get; set; }
    public string 店 { get; set; }
    public string 职位 { get; set; }
    public string 中文名 { get; set; }
    public string 英文名 { get; set; }
}

/// <summary>
/// 考勤汇总
/// </summary>
public class AttendInfo
{
    public string Name { get; set; }

    // 迟到分钟
    public int lateTime { get; set; }

    // 加班小时
    public int overTime { get; set; }

    // 调休小时
    public int deductTime { get; set; }

    // 请假小时
    public int offTime { get; set; }

    // 缺卡次数
    public int missing { get; set; }

    // 没有打卡时间
    public bool noDakaTime { get; set; } = false;

    public override string ToString()
    {
        if(noDakaTime) return $"OFF";
        return $"迟到 {lateTime}分钟, 加班 {overTime}小时, 调休 {deductTime}小时, 缺卡 {missing}次";
    }
    public string OutPut()
    {
        if(noDakaTime) return $"OFF";
        var parts = new List<string>();
        
        parts.Add(lateTime != 0 ? $"[color=#008080]迟到 {lateTime}分钟[/color]" : string.Empty);
        parts.Add(overTime != 0 ? $"加班 {overTime}小时" : string.Empty);
        parts.Add(deductTime != 0 ? $"调休 {deductTime}小时" : string.Empty);
        parts.Add(missing != 0 ? $"缺卡 {missing}次" : string.Empty);
 
        return string.Join("\n", parts);
    }
 
}

/// <summary>
/// 班次时间
/// </summary>
public class WorkShiftTime
{
    public string Name { get; set; }
    public string 上班时间 { get; set; }
    public string 下班时间 { get; set; }

    public Daka GetMdaka()
    {
        var Md = 上班时间.ToIntList(':');
        return new Daka(Md[0], Md[1]);
    }

    public Daka GetEdaka()
    {
        var Ed = 下班时间.ToIntList(':');
        return new Daka(Ed[0], Ed[1]);
    }

    /// <summary>
    /// 根据打卡时间计算该排班考勤结果 
    /// </summary>
    /// <param name="dayDaka"></param>
    /// <returns></returns>
    public AttendInfo GenAttendInfo(DayDaka dayDaka)
    {
        AttendInfo attendInfo = new();

        var reqMdaka = GetMdaka();
        var reqEdaka = GetEdaka();

        var deductTime = dayDaka.deductTime;
        attendInfo.deductTime = deductTime;
        attendInfo.offTime = dayDaka.offTime;
        attendInfo.missing = dayDaka.missing;

        if (dayDaka.NoSorce)
        {
            attendInfo.noDakaTime = true;
            return attendInfo;
        }
        var dEH = dayDaka.Edaka.H - reqEdaka.H;
        var dEM = dayDaka.Edaka.M - reqEdaka.M;
        var dMH = dayDaka.Mdaka.H - reqMdaka.H;
        var dMM = dayDaka.Mdaka.M - reqMdaka.M;

        if (dEH < 0 || dEH == 0 && dEM < 0) deductTime -= (dEH + (dEM < 0 ? 1 : 0)); //若早退则用调休抵扣
        var dMH_over = reqMdaka.H + (reqMdaka.M > 0 ? 1 : 0) - dayDaka.Mdaka.H; // 早晨加班要按班次整点算
        dMH_over += (dMM > 0) ?-1:0;
        var dEH_over = dEH + (dEM < 0 ?-1:0);
        attendInfo.overTime = Math.Max(0, dEH_over) + Math.Max(0, dMH_over);
        attendInfo.lateTime = Math.Max(0,dMH < 0 ? 0 : ((dMH - 0) * 60 + dMM)); //若还有调休可抵扣要从迟到中扣掉

        attendInfo.deductTime = deductTime;

        return attendInfo;
    }
}


public class Daka
{
    public Daka(int h, int m)
    {
        H = h;
        M = m;
    }

    // 打卡时间
    public int H { get; set; } = 0; // 小时 (0-23)
    public int M { get; set; } = 0;// 分钟 (0-59) 


    // 验证时间是否合法
    public bool IsValid()
    {
        return H >= 0 && H < 24 && M >= 0 && M < 60;
    }

}

public class DayDaka 
{
    public DayDaka(bool nosorce = false)
    {
        NoSorce = nosorce;
    }

    // 早上打卡时间
    public Daka Mdaka { get; set; }
 
    // 晚上打卡时间
    public Daka Edaka { get; set; }

    // 请假小时
    public int offTime { get; set; }

    // 调休小时
    public int deductTime { get; set; }  

    // 缺卡次数
    public int missing { get; set; }

    public bool NoSorce = false;// 获取不到时间

    //解析表格文本,获得一个打卡时间数据
    public static DayDaka Get(string str)
    {
        var result1 = TimeParser.ParsePunchTime(str);
        if (result1.Success)
        {
            var dayDaka = new DayDaka(false);
            dayDaka.Mdaka = new(result1.MorningHour, result1.MorningMinute);
            dayDaka.Edaka = new(result1.EveningHour, result1.EveningMinute);

            if (str.Contains("缺卡")) dayDaka.missing = 1;
            //NeedDo:请假时间计算,可能需要区分各种假
            // if (str.Contains("请假"))
            // {

            // }
            return dayDaka;
        }
    
        return new DayDaka(true);

    }

    // 验证时间是否合法
    public bool IsValid()
    {
        return Mdaka.IsValid() && Edaka.IsValid();
    }

    // 重写ToString方便打印
    public override string ToString()
    {
        if(NoSorce) return "没有打卡记录";
        return $"早上 {Mdaka.H:D2}:{Mdaka.M:D2}, 晚上 {Edaka.H:D2}:{Edaka.M:D2}, 调休 {deductTime}小时, 缺卡 {missing}次";
    }

}

/// <summary>
/// 人员考勤表和排班表的数据源都用这个结构
/// </summary>
public class StuffMonthData
{
    public string Name { get; set; }
    public string D1 { get; set; }
    public string D2 { get; set; }
    public string D3 { get; set; }
    public string D4 { get; set; }
    public string D5 { get; set; }
    public string D6 { get; set; }
    public string D7 { get; set; }
    public string D8 { get; set; }
    public string D9 { get; set; }
    public string D10 { get; set; }
    public string D11 { get; set; }
    public string D12 { get; set; }
    public string D13 { get; set; }
    public string D14 { get; set; }
    public string D15 { get; set; }
    public string D16 { get; set; }
    public string D17 { get; set; }
    public string D18 { get; set; }
    public string D19 { get; set; }
    public string D20 { get; set; }
    public string D21 { get; set; }
    public string D22 { get; set; }
    public string D23 { get; set; }
    public string D24 { get; set; }
    public string D25 { get; set; }
    public string D26 { get; set; }
    public string D27 { get; set; }
    public string D28 { get; set; }
    public string D29 { get; set; }
    public string D30 { get; set; }
    public string D31 { get; set; }


    // 遍历所有D1~D31属性并返回值数组
    public string[] GetDailyValues()
    {
        var values = new string[31];
        var type = GetType();

        for (int i = 1; i <= 31; i++)
        {
            string propertyName = $"D{i}";
            var property = type.GetProperty(propertyName);

            if (property != null && property.CanRead)
            {
                values[i - 1] = (string)property.GetValue(this);
            }
            else
            {
                throw new InvalidOperationException($"属性 {propertyName} 不存在或不可读");
            }
        }

        return values;
    }

    //写入考勤汇总
    public void SetDailyValue(StuffMonthData obj, int day, AttendInfo attendInfo)
    {
        string propertyName = $"D{day}";
        var property = GetType().GetProperty(propertyName);

        var value = attendInfo.OutPut();
        object convertedValue = Convert.ChangeType(value, property.PropertyType);
        property.SetValue(obj, convertedValue);
    }  
}
