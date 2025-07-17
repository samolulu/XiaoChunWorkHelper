
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ExcelTool
{
    static string extension = ".xlsx";
    static string path_root = $"{Application.streamingAssetsPath}/Excel";

    static string name_attendance = "考勤";
    static string name_attendResult = "汇总";
    static string name_dakaSheet = "打卡表";
    static string name_stuffSheet = "人员表";
    static string name_shiftTimeSheet = "班次表";
    static string name_stuffShiftSheet = "排班表";
    static string name_attendResultSheet = "汇总表";

    public static void OpenExcelPathRoot()
    {
        Common.OpenDialogDir.OpenWinFolder(Path.GetDirectoryName(path_root));
    }

    public static void DoAttendanceExcel()
    {
        //选路径
 		var tempPath = ExcelUtil.SelectSaveExcleFile(name_attendResult, "选择表格导出目录");
        if (string.IsNullOrEmpty(tempPath)) return;
		
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

            //输出的汇总表
            StuffMonthData stuffMonthResult = new();
            stuffMonthResult.Name = name_stuff;
            //记录每日考勤
            List<AttendInfo> attendInfos = new();

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
                WorkShiftTime workShiftTime = null;
                var todayShift = dailyShift[i];
                if (todayShift == null) continue;
                LogTool.Log($"{name_stuff}的{i + 1}号排班:", todayShift);
                if (todayShift == WorkShiftTime.OffShiftName)
                {

                }
                else if (!dic_shiftTime.TryGetValue(todayShift, out workShiftTime))
                {
                    LogTool.Log($"!!班次时间没有定义:{todayShift}班,请检查{name_stuff}的{i + 1}号排班");
                    break;
                }

                //根据打卡时间和排班得出考勤结果的
                AttendInfo attendInfo = dayDaka.GenAttendInfo(workShiftTime);
                LogTool.Log($"!!{name_stuff}的{i + 1}号考勤汇总:", attendInfo);
                stuffMonthResult.SetDailyValue(i + 1, attendInfo);
                attendInfos.Add(attendInfo);
            }
            stuffMonthResult.SetMonthValue(attendInfos);
            dic_result.Add(name_stuff, stuffMonthResult);
        }

 
        ExcelUtil.SaveExcelWithHtmlFormatting(tempPath, name_attendResultSheet, dic_result);
        //打开目录  
        Common.OpenDialogDir.OpenWinFolder(Path.GetDirectoryName(tempPath), Path.GetFileName(tempPath));

    }

    public static WorkShiftTime GetWorkShiftTime(string name)
    {
        Dictionary<string, WorkShiftTime> dic_shiftTime = ExcelUtil.GetDictionary<string, WorkShiftTime>(path_root, name_attendance, name_shiftTimeSheet, keyName: "Name");
        dic_shiftTime.TryGetValue(name, out WorkShiftTime workShiftTime);
        return workShiftTime;
    }

    public static string Test_SingleTest(string tex_daka, string todayShift)
    {
        var dayDaka = DayDaka.Get(tex_daka);

        if(dayDaka.Mdaka != null)LogTool.Log($"上班时间: {dayDaka.Mdaka.H}点{dayDaka.Mdaka.M}分, 是否缺卡:{dayDaka.Mdaka.Missing}");
        if(dayDaka.Edaka != null)LogTool.Log($"下班时间: {dayDaka.Edaka.H}点{dayDaka.Edaka.M}分, 是否缺卡:{dayDaka.Edaka.Missing}");


        WorkShiftTime workShiftTime = GetWorkShiftTime(todayShift);

        AttendInfo attendInfo = dayDaka.GenAttendInfo(workShiftTime);
        var output = attendInfo.OutPut();
        LogTool.Log($"!!测试考勤汇总:", output);
        return output;
    }
}
