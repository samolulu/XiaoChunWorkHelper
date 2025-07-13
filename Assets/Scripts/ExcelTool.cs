using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ExcelTool
{
    static string extension = ".xlsx";
    static string path_root = $"{Application.dataPath}/Excel";

    static string name_attendance = "考勤";
    static string name_attendResult = "汇总";
    static string name_dakaSheet = "打卡表";
    static string name_stuffSheet = "人员表";
    static string name_shiftTimeSheet = "班次表";
    static string name_stuffShiftSheet = "排班表";
    static string name_attendResultSheet = "汇总表";


    [MenuItem("Tool/汇总考勤表")]
    private static void DoAttendanceExcel()
    {
        //打卡记录表
        Dictionary<string, StuffMonthData> dic_daka = ExcelUtil.GetDictionary<string, StuffMonthData>(path_root, name_attendance, name_dakaSheet, keyName: "Name");
        //员工表
        Dictionary<string, Stuff> dic_stuffs = ExcelUtil.GetDictionary<string, Stuff>(path_root, name_attendance, name_stuffSheet, keyName: "Name");
        //班次表
        Dictionary<string, WorkShiftTime> dic_shiftTime = ExcelUtil.GetDictionary<string, WorkShiftTime>(path_root, name_attendance, name_shiftTimeSheet, keyName: "Name");
        //排班表
        Dictionary<string, StuffMonthData> dic_stuffShift = ExcelUtil.GetDictionary<string, StuffMonthData>(path_root, name_attendance, name_stuffShiftSheet, keyName: "Name");

        LogTool.Log("所有名字", dic_daka.Keys);
        //LogTool.Log("所有人的所有打卡记录", dic_daka.Values);
        Dictionary<string, StuffMonthData> dic_result = new();

        foreach (var item in dic_daka.Values)
        {
            var dailyDaka = item.GetDailyValues();
            //LogTool.Log($"{item.Name}的所有打卡记录", datas);
            if (!dic_stuffs.TryGetValue(item.Name, out Stuff stuff))
            {
                LogTool.Log($"!!人员表中没有登记:{item.Name}");
                break;
            }
            var name_stuff = stuff.报表名字;

            //LogTool.Log($"{item.Name}的所有打卡记录", datas);
            if (!dic_stuffShift.TryGetValue(name_stuff, out StuffMonthData shift))
            {
                LogTool.Log($"!!没有{name_stuff}本月的排班数据");
                break;
            }
            //此人每次排班
            var dailyShift = shift.GetDailyValues();

            StuffMonthData stuffMonthResult = new();
            stuffMonthResult.Name = name_stuff;

            for (int i = 0; i < dailyDaka.Length; i++)
            {
                var dayDakaStr = dailyDaka[i];
                if (dayDakaStr == null) continue;
                var dayDaka = DayDaka.Get(dayDakaStr);
                LogTool.Log($"{name_stuff}的{i + 1}号打卡记录:", dayDaka);
                //LogTool.Log($"{name_stuff}的{i + 1}号打卡记录", datas[i]);

                if (dailyShift.Length < i + 1)
                {
                    LogTool.Log($"!!{name_stuff}本月的排班数据不全");
                    break;
                }

                //今天排班
                var todayShift = dailyShift[i];
                if (todayShift == null) continue;
                LogTool.Log($"{name_stuff}的{i + 1}号排班:", todayShift);
                if (!dic_shiftTime.TryGetValue(todayShift, out WorkShiftTime workShiftTime))
                {
                    LogTool.Log($"!!班次时间没有定义:{todayShift}班,请检查{name_stuff}的{i + 1}号排班");
                    break;
                }

                //根据打卡时间和排班得出考勤结果的
                AttendInfo attendInfo = workShiftTime.GenAttendInfo(dayDaka);
                LogTool.Log($"!!{name_stuff}的{i + 1}号考勤汇总:", attendInfo);
                stuffMonthResult.SetDailyValue(stuffMonthResult, i + 1, attendInfo);
            }

            dic_result.Add(name_stuff, stuffMonthResult);
        }

        //保存修改后的映射表
        var path = $"{path_root}/{name_attendResult}{extension}";
        ExcelUtil.SaveExcelWithHtmlFormatting(path, name_attendResultSheet, dic_result);
    }

    [MenuItem("Tool/解析时间")]
    private static void Test_ParserTime()
    {
        string text1 = "正常（未排班）";
        var result1 = TimeParser.ParsePunchTime(text1);

            LogTool.Log($"上班时间: {result1.MorningHour}点{result1.MorningMinute}分"); 
            LogTool.Log($"下班时间: {result1.EveningHour}点{result1.EveningMinute}分");
   
 
    }
}
