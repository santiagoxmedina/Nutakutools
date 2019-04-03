using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class NutakuTools : MonoBehaviour {

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void CheckGuestPlayer (Action<string> action);
    [DllImport("__Internal")]
    private static extern void OpenGuestPlayerFrom ();
    [DllImport("__Internal")]
    private static extern void OpenCrossPromotion ();
#endif

    private static Action<string> m_action;
    private static Action<string> m_actionError;
    public Action<int> onSuportToolStateChange;
    public const int SUPPORT_TOOL_OPEN = 1;
    //public Text m_SuportToolText;

    private void Start ()
    {
        OnSuportToolsState(0);
    }

    /// <summary>
    /// Check the player nutaku grade
    /// </summary>
    /// <param name="action"> return the grade value</param>
    /// <param name="actionError">return error message</param>
    public void OnCheckGuestPlayer (Action<string> action, Action<string> actionError)
    {
        m_action += action;
        m_actionError += actionError;
        #if UNITY_WEBGL
        CheckGuestPlayer(Callback);
        #endif
    }

    /// <summary>
    /// Open Guest form
    /// </summary>
    public void OnOpenGuestPlayerFrom ()
    {
#if UNITY_WEBGL
        OpenGuestPlayerFrom();
#endif
    }
    /// <summary>
    /// Open Cross promotion banner
    /// </summary>
    public void OnOpenCrossPromotion()
    {
#if UNITY_WEBGL
        OpenCrossPromotion();
#endif
    }

    #region test
    public void OnCheckGuestPlayerTest ()
    {
        OnCheckGuestPlayer(OnTestResult,OnTestResultError);
    }

    private void OnTestResult (string result)
    {
        Debug.Log("OnTestResult  " + result);
    }

    private void OnTestResultError (string result)
    {
        Debug.Log("OnTestResultError: " + result);
    }
#endregion

    [MonoPInvokeCallback(typeof(Action<string>))]
    private static void Callback (string arg)
    {
       
        NutakuToolResponse nutakuToolResponse = JsonUtility.FromJson<NutakuToolResponse>(arg);
        if(nutakuToolResponse.code == 0)
        {
            if (m_action != null)
            {
                m_action(nutakuToolResponse.result);
            }
        }
        else
        {
            if (m_actionError != null)
            {
                m_actionError(nutakuToolResponse.result);
            }
        }
        m_actionError = null;
        m_action =null;
    }

    public void OnSuportToolsState(int active)
    {
        //m_SuportToolText.text = string.Format("SuportTool state: {0}", active);
        Debug.LogFormat("SuportTool state: {0}", active);
        if (onSuportToolStateChange != null)
        {
            onSuportToolStateChange(active);
        }
    }
}
[System.Serializable]
public class NutakuToolResponse {
    public int code;
    public string result;
}
