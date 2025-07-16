using System;
using System.Text.RegularExpressions;

public class TimeParser
{
    public static readonly int OverDayHour = 6;
    private static readonly Regex ActualPunchPattern = new Regex(@"(?<!\()\b(\d{1,2}:\d{2})\b(?!\))");
    private static readonly Regex MissingPunchPattern = new Regex(@"缺卡\((\d{1,2}:\d{2})\)");
    private static readonly Regex NotScheduledPattern = new Regex(@"正常\（未排班\）");

    // 规则 1：处理了跨天加班时间
    // 规则 2：通过时间大小判断早晚打卡
    // 规则 3：记录了早晚缺卡状态
    // 规则 4：不依赖 "正常" 文本判断
    // 规则 5：返回值结构符合要求
    public static (
        bool Success,
        bool MorningMissing,
        int MorningHour,
        int MorningMinute,
        bool EveningMissing,
        int EveningHour,
        int EveningMinute
    ) ParsePunchTime(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (false, true, 0, 0, true, 0, 0);

        // 检查是否为"正常（未排班）"
        if (NotScheduledPattern.IsMatch(text))
            return (false, true, 0, 0, true, 0, 0);

        // 提取实际打卡时间和缺卡标记
        var actualMatches = ActualPunchPattern.Matches(text);
        var missingMatches = MissingPunchPattern.Matches(text);

        // 解析实际打卡时间
        TimeSpan? punch1 = null;
        TimeSpan? punch2 = null;

        if (actualMatches.Count >= 1 && TryParseTime(actualMatches[0].Groups[1].Value, out TimeSpan time1))
            punch1 = ApplyOverDayAdjustment(time1);

        if (actualMatches.Count >= 2 && TryParseTime(actualMatches[1].Groups[1].Value, out TimeSpan time2))
            punch2 = ApplyOverDayAdjustment(time2);

        // 解析缺卡标记
        bool hasMorningMissing = false;
        bool hasEveningMissing = false;

        foreach (Match match in missingMatches)
        {
            if (TryParseTime(match.Groups[1].Value, out TimeSpan missingTime))
            {
                if (missingTime.Hours < 12)
                    hasMorningMissing = true;
                else
                    hasEveningMissing = true;
            }
        }

        // 检查是否存在矛盾（同时标记早晚缺卡）
        if (hasMorningMissing && hasEveningMissing)
            return (false, true, 0, 0, true, 0, 0);

        // 确定早晚打卡时间
        int morningHour = 0;
        int morningMinute = 0;
        int eveningHour = 0;
        int eveningMinute = 0;

        if (punch1.HasValue && punch2.HasValue)
        {
            // 两个打卡时间，按大小排序
            if (punch1.Value < punch2.Value)
            {
                SetMorningTime(punch1.Value, ref morningHour, ref morningMinute);
                SetEveningTime(punch2.Value, ref eveningHour, ref eveningMinute);
            }
            else
            {
                SetMorningTime(punch2.Value, ref morningHour, ref morningMinute);
                SetEveningTime(punch1.Value, ref eveningHour, ref eveningMinute);
            }

        }
        else if (punch1.HasValue)// 只有一个打卡时间
        {
            //只打了早上
            if (hasEveningMissing)
            {
                SetMorningTime(punch1.Value, ref morningHour, ref morningMinute);
            }
            else//打了晚上
            {
                SetEveningTime(punch1.Value, ref eveningHour, ref eveningMinute);
            }
        }
        else
        {
            // 没有打卡记录，且不是未排班，属于解析失败
            return (false, true, 0, 0, true, 0, 0);
        }

        return (true, hasMorningMissing, morningHour, morningMinute, hasEveningMissing, eveningHour, eveningMinute);
    }

    private static TimeSpan ApplyOverDayAdjustment(TimeSpan time)
    {
        return time.Hours < OverDayHour ? time.Add(TimeSpan.FromHours(24)) : time;
    }

    private static void SetMorningTime(TimeSpan time, ref int hour, ref int minute)
    {
        hour = time.Hours >= 24 ? time.Hours - 24 : time.Hours;
        minute = time.Minutes;
    }

    private static void SetEveningTime(TimeSpan time, ref int hour, ref int minute)
    {
        hour = time.Hours >= 24 ? time.Hours - 24 : time.Hours;
        minute = time.Minutes;
    }

    private static bool TryParseTime(string timeStr, out TimeSpan time)
    {
        if (TimeSpan.TryParseExact(timeStr, "h\\:mm", null, out time))
            return true;
            
        if (TimeSpan.TryParseExact(timeStr, "hh\\:mm", null, out time))
            return true;
            
        return false;
    }
}