using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class NutakuTools : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void CheckGuestPlayer (Action<string> action);
    [DllImport("__Internal")]
    private static extern void OpenGuestPlayerFrom ();
    [DllImport("__Internal")]
    private static extern void OpenCrossPromotion ();

    private static Action<string> m_action;
    private static Action<string> m_actionError;

    /// <summary>
    /// Check the player nutaku grade
    /// </summary>
    /// <param name="action"> return the grade value</param>
    /// <param name="actionError">return error message</param>
    public void OnCheckGuestPlayer (Action<string> action, Action<string> actionError)
    {
        m_action += action;
        m_actionError += actionError;
        CheckGuestPlayer(Callback);
    }

    /// <summary>
    /// Open Guest form
    /// </summary>
    public void OnOpenGuestPlayerFrom ()
    {

        OpenGuestPlayerFrom();
    }
    /// <summary>
    /// Open Cross promotion banner
    /// </summary>
    public void OnOpenCrossPromotion()
    {
        OpenCrossPromotion();
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
}
[System.Serializable]
public class NutakuToolResponse {
    public int code;
    public string result;
}
