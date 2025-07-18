
using UnityEngine;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{

    public Button btn_attend;
    public Button btn_openDataPath;
    public Button btn_test;
    public InputField inputField_shift;
    public InputField inputField_daka;
    public Text text_output;

	void Awake()
	{
		#if UNITY_STANDALONE && !UNITY_EDITOR
			// 桌面平台使用垂直同步限帧
			QualitySettings.vSyncCount =  1;         
			//Application.targetFrameRate =  30;   
		#endif 
	}

	// Start is called before the first frame update
	void Start()
    {
        btn_attend.onClick.AddListener(OnClickAttend);
        btn_openDataPath.onClick.AddListener(OnClickOpenDataPath);
        btn_test.onClick.AddListener(SingleTest);
    }

    void OnClickAttend()
    {
        ExcelTool.DoAttendanceExcel();
    }

    void OnClickOpenDataPath()
    {
        ExcelTool.OpenExcelPathRoot();
    }

    void SingleTest()
    {
       text_output.text = ExcelTool.Test_SingleTest(inputField_daka.text, inputField_shift.text);
    }

}
