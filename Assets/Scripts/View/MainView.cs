
using UnityEngine;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{

    public Button btn_attend;
    public Button btn_openDataPath;

    // Start is called before the first frame update
    void Start()
    {
        btn_attend.onClick.AddListener(OnClickAttend);
        btn_openDataPath.onClick.AddListener(OnClickOpenDataPath);
    }

    void OnClickAttend()
    {
        ExcelTool.DoAttendanceExcel();
    }

    void OnClickOpenDataPath()
    {
        ExcelTool.OpenExcelPathRoot();
    }

}
