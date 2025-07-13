using System;
using System.Text.RegularExpressions;

public class TimeParser
{
    //小于该时间的打卡被认为是晚上跨天加班打卡
    public static readonly int OverDayHour = 6;

    // 正则表达式模式：匹配 HH:MM 格式的时间
    private static readonly Regex TimePattern = new Regex(@"(\d{1,2}:\d{2})");

    public static (bool Success, int MorningHour, int MorningMinute, int EveningHour, int EveningMinute) 
        ParsePunchTime(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (false, 0, 0, 0, 0);

        // 查找所有匹配的时间
        var matches = TimePattern.Matches(text);
        
        if (matches.Count < 2)
            return (false, 0, 0, 0, 0); // 至少需要两个时间点

        // 提取并解析时间
        if (TryParseTime(matches[0].Value, out TimeSpan time1) &&
            TryParseTime(matches[1].Value, out TimeSpan time2))
        {
            // 直接比较时间大小，较小的为早上
            bool overToNextDay = time1.Hours <= OverDayHour || time2.Hours <= OverDayHour;

            TimeSpan morningTime;
            TimeSpan eveningTime;
            if (overToNextDay)
            {
                eveningTime = time1.Hours <= 6 ? time1 : time2;
                morningTime = time1.Hours <= 6 ? time2 : time1;
            }
            else
            {
                morningTime = time1 < time2 ? time1 : time2;
                eveningTime = time1 < time2 ? time2 : time1;
            }


            return (true, 
                morningTime.Hours, morningTime.Minutes, 
                eveningTime.Hours, eveningTime.Minutes);
        }

        return (false, 0, 0, 0, 0);
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