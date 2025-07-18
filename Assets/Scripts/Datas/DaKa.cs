using System;
using System.Collections.Generic;
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

    // 未入职或已离职
    public bool noWork { get; set; } = false;
    
    public bool shiftDiff = false;// 排班异常(排了OFF却有打卡)
     
    public string OutPut()
    {
        if (noWork) return string.Empty;
        if (shiftDiff) return WorkShiftTime.ShiftDiffName;
        if (noDakaTime) return WorkShiftTime.OffShiftName;
        if (lateTime == 0 && overTime == 0 && deductTime == 0 && missing == 0) return WorkShiftTime.WorkNormalName; //正常上下班的就显示V
        var parts = new List<string>();

        parts.Add(lateTime != 0 ? $"[color=#008080]迟到 {lateTime}分钟[/color]" : string.Empty);
        parts.Add(overTime != 0 ? $"加班 {overTime}小时" : string.Empty);
        parts.Add(deductTime != 0 ? $"调休 {deductTime}小时" : string.Empty);
        parts.Add(missing != 0 ? $"缺卡 {missing}次" : string.Empty);
        return string.Join("\n", parts);
    }
    public string OutPutShort()
    {
        if (noWork) return string.Empty;
        if (shiftDiff) return $"[color=#FF00FF]{WorkShiftTime.ShiftDiffName}[/color]";
        if (noDakaTime) return WorkShiftTime.OffShiftName;
       
        var parts = new List<string>();
        
        if(lateTime != 0 )  parts.Add($"[color=#FF0000]迟 {lateTime}[/color]" );
        if(overTime != 0 )  parts.Add($"[color=#000000]加 {overTime}[/color]");
        if(deductTime != 0 )parts.Add($"[color=#0000FF]补 {deductTime}[/color]");
        if(missing != 0 )   parts.Add($"[color=#FFA500]缺 {missing}[/color]");
        if (parts.Count == 0) return $"[color=#00FF00]{WorkShiftTime.WorkNormalName}[/color]"; //正常上下班的就显示V
        return string.Join("\n", parts);
    }
}

/// <summary>
/// 班次时间
/// </summary>
public class WorkShiftTime
{
    public static readonly string OffShiftName = "OFF";
    public static readonly string WorkNormalName = "V";
    public static readonly string ShiftDiffName = "异常";
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


}


public class Daka
{
    public Daka(int h, int m, bool missing = false)
    {
        H = h;
        M = m;
        Missing = missing;
    }

    // 打卡时间
    public int H { get; set; } = 0; // 小时 (0-23)
    public int M { get; set; } = 0;// 分钟 (0-59) 

    public bool Missing = false; //缺卡

    // 验证时间是否合法
    public bool IsValid()
    {
        return H >= 0 && H < 24 && M >= 0 && M < 60;
    }

    public float ToFloat() => H + M / 60.0f;
}

public class DayDaka 
{
    public DayDaka(bool nosorce = false, bool noWork = false)
    {
        NoSorce = nosorce;
        NoWork = noWork;
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
    public bool NoWork = false;// 未入职或已离职

    //解析表格文本,获得一个打卡时间数据
    public static DayDaka Get(string str)
    {
        bool noWork = str == "--";
        var result1 = TimeParser.ParsePunchTime(str);
        if (result1.Success)
        {
            var dayDaka = new DayDaka(false, noWork);
            dayDaka.InitDakaTime(result1);    
            return dayDaka;
        }
    
        return new DayDaka(true);

    }

    //根据打卡机表格文本解析结果,初始化打开时间数据
    public void InitDakaTime((
        bool Success,
        bool MorningMissing,
        int MorningHour,
        int MorningMinute,
        bool EveningMissing,
        int EveningHour,
        int EveningMinute
    ) result1)
    {
            Mdaka = new(result1.MorningHour, result1.MorningMinute, result1.MorningMissing);
            Edaka = new(result1.EveningHour, result1.EveningMinute, result1.EveningMissing);

            missing = (result1.MorningMissing?1:0) + (result1.EveningMissing?1:0);

            //NeedDo:请假时间计算,可能需要区分各种假
            // if (str.Contains("请假"))
            // {

            // }
    }

    /// <summary>
    /// 根据打卡时间计算该排班考勤结果 
    /// </summary>
    /// <param name="dayDaka"></param>
    /// <returns></returns>
    public AttendInfo GenAttendInfo(WorkShiftTime workShiftTime)
    {
        AttendInfo attendInfo = new();

        //没就职的
        if (this.NoWork)
        {
            attendInfo.noWork = true;
            return attendInfo;
        }
        //没打卡时间的
        if (this.NoSorce)
        {
            attendInfo.noDakaTime = true;
            return attendInfo;
        }
        //排班异常的
        if (workShiftTime == null)
        {
            attendInfo.shiftDiff = true;
            return attendInfo;
        }
        var reqMdaka = workShiftTime.GetMdaka();
        var reqEdaka = workShiftTime.GetEdaka();

        var deductTime = this.deductTime;
        attendInfo.deductTime = deductTime;
        attendInfo.offTime = this.offTime;
        attendInfo.missing = this.missing;

        //计算加班和迟到,若早上缺卡则早上无需计算迟到和加班了,若晚上缺卡则无需计算早退和加班了;
        var dEH = this.Edaka.ToFloat() - reqEdaka.ToFloat() + (this.Edaka.H < TimeParser.OverDayHour ? 24 : 0);
        var dMH = this.Mdaka.ToFloat() - reqMdaka.ToFloat();

        if (!Edaka.Missing && dEH < 0) deductTime -= dEH.CustomCeil(); //若早退则用调休抵扣

        // 早上加班:要按班次整点(向上取整)减去上班打卡时间,然后向下取整
        var dMH_over = Mdaka.Missing ? 0 : ( reqMdaka.ToFloat().CustomCeil() - this.Mdaka.ToFloat()).CustomFloor();
        //晚上加班
        var dEH_over = Edaka.Missing ? 0 : dEH.CustomFloor();
        attendInfo.overTime = Math.Max(0, dEH_over) + Math.Max(0, dMH_over);

        //迟到分钟
        attendInfo.lateTime = Mdaka.Missing ? 0 : Math.Max(0, Mathf.RoundToInt(dMH * 60f));

        attendInfo.deductTime = deductTime;

        return attendInfo;
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
        if(NoWork) return "未入职或已离职";
        return $"早上 {Mdaka.H:D2}:{Mdaka.M:D2}, 晚上 {Edaka.H:D2}:{Edaka.M:D2}, 调休 {deductTime}小时, 缺卡 {missing}次";
    }

}

/// <summary>
/// 人员考勤表和排班表的数据源都用这个结构
/// </summary>
public class StuffMonthData
{
    public string Name { get; set; }
    public string 月统计 { get; set; }//月统计
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

    //写入每日考勤汇总
    public void SetDailyValue( int day, AttendInfo attendInfo)
    {
        string propertyName = $"D{day}";
        var property = GetType().GetProperty(propertyName);

        var value = attendInfo.OutPutShort();
        object convertedValue = Convert.ChangeType(value, property.PropertyType);
        property.SetValue(this, convertedValue);
    }  

    //写入月考勤汇总
    public void SetMonthValue(List<AttendInfo> attendInfos)
    {
        AttendInfo attendInfo_month = new();
        foreach (var info in attendInfos)
        {
            attendInfo_month.lateTime += info.lateTime;
            attendInfo_month.overTime += info.overTime;
            attendInfo_month.deductTime += info.deductTime;
            attendInfo_month.offTime += info.offTime;
            attendInfo_month.missing += info.missing;
        }
        月统计 = attendInfo_month.OutPutShort();
    }  
}
