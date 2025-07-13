using System.Collections;
using System.Collections.Generic;
using UnityEditor;
 

public class MenuTest
{ 

    [MenuItem("Tool/汇总考勤表")]
    private static void Test_DoAttendanceExcel()
    {
        ExcelTool.DoAttendanceExcel();
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
