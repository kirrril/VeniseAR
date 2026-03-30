using UnityEngine;
using TMPro;
using System.Linq;

public class Debugger : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI logText;
    private string logHistory = string.Empty;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void Start()
    {
        if (logText != null)
        {
            logText.text = string.Empty;
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string colorTag = "";
        string details = logString;

        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                colorTag = "<color=red>";
                if (!string.IsNullOrWhiteSpace(stackTrace))
                {
                    details = logString + "\n" + stackTrace;
                }
                break;
            case LogType.Warning:
                colorTag = "<color=orange>";
                break;
            default:
                colorTag = "<color=black>";
                break;
        }

        string formattedLog = colorTag + details + "</color>\n";
        logHistory += formattedLog;

        if (logText != null)
        {
            logText.text = logHistory;
        }
    }
}
