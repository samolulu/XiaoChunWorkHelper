using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TimeParser
{
    public static readonly int OverDayHour = 6;
    private static readonly Regex ActualPunchPattern = new Regex(@"(?<!\()\b(\d{1,2}:\d{2})\b(?!\))");
    private static readonly Regex MissingPunchPattern = new Regex(@"缺卡\((\d{1,2}:\d{2})\)");
    private static readonly Regex NotScheduledPattern = new Regex(@"^正常\（未排班\）$");

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

        // 解析所有实际打卡时间
        var punchTimes = new List<TimeSpan>();
        foreach (Match match in actualMatches)
        {
            if (TryParseTime(match.Groups[1].Value, out TimeSpan time))
            {
                punchTimes.Add(ApplyOverDayAdjustment(time));
            }
        }

        // 解析缺卡标记
        bool hasMorningMissing = false;
        bool hasEveningMissing = false;

        foreach (Match match in missingMatches)
        {
            if (TryParseTime(match.Groups[1].Value, out TimeSpan missingTime))
            {
                if (missingTime.Hours < 15) //因为现在所有班次的要求上班时间都是小于15的,所以可以这样简单判断来推断出缺卡的是早上还是晚上
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

        if (punchTimes.Count >= 1)
        {
            // 按时间排序
            punchTimes.Sort();
            
            // 最早的打卡作为早上时间
            var morningTime = punchTimes[0];
            SetMorningTime(morningTime, ref morningHour, ref morningMinute);
            
            // 最晚的打卡作为晚上时间
            var eveningTime = punchTimes[punchTimes.Count - 1];
            SetEveningTime(eveningTime, ref eveningHour, ref eveningMinute);
            
            // 如果只有一次打卡，判断是早上还是晚上
            if (punchTimes.Count == 1)
            {
                if (hasEveningMissing)
                {
                    // 有晚上缺卡标记，说明这次打卡是早上
                    eveningHour = 0;
                    eveningMinute = 0;
                    hasEveningMissing = true;
                }
                else if (hasMorningMissing)
                {
                    // 有早上缺卡标记，说明这次打卡是晚上
                    morningHour = 0;
                    morningMinute = 0;
                    hasMorningMissing = true;
                }
                else
                {
                    // 没有缺卡标记，根据时间判断
                    if (morningTime.Hours < 12 || morningTime.Hours >= 24)
                    {
                        eveningHour = 0;
                        eveningMinute = 0;
                        hasEveningMissing = true;
                    }
                    else
                    {
                        morningHour = 0;
                        morningMinute = 0;
                        hasMorningMissing = true;
                    }
                }
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