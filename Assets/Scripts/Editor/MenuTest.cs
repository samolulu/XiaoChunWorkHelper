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

    [MenuItem("Tool/考勤单元测试")]
    private static void Test_ParserTime()
    {
        var todayShift = "D";
        string tex_daka = "迟到-16:00;  缺卡(22:00);";
        //var result1 = TimeParser.ParsePunchTime(text1);
        ExcelTool.Test_SingleTest(tex_daka, todayShift);
    }

}
