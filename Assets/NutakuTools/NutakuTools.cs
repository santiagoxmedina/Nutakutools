using AOT;
using System;
using System.Runtime.InteropServices;
using System.Text;
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
    [DllImport("__Internal")]
    private static extern void CrossPromotionTaskArchieve ();
    [DllImport("__Internal")]
    private static extern void CrossPromotionTaskConfirm ();
    [DllImport("__Internal")]
    private static extern void OnNutakuToolStart ();
#endif

    private static Action<string> m_action;
    private static Action<string> m_actionError;
    public Action<int> onSuportToolStateChange;
    public const int SUPPORT_TOOL_OPEN = 1;
    public NutakuToolRecord nutakuToolRecord;

    private void Start ()
    {
        OnSuportToolsState(0);

#if UNITY_WEBGL  && !UNITY_EDITOR
        OnNutakuToolStart();
#endif
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

    private void OnOpenCrossPromotionComplete ()
    {
        Debug.Log("OnOpenCrossPromotionComplete");
    }

    /// <summary>
    /// Set the current taks archieve.
    /// </summary>
    public void OnCrossPromotionTaskArchieve ()
    {
#if UNITY_WEBGL
        CrossPromotionTaskArchieve();
#endif
    }
    /// <summary>
    /// Set the current cross-promo task confirmed.
    /// </summary>
    public void OnCrossPromotionTaskConfirm ()
    {
#if UNITY_WEBGL
        CrossPromotionTaskConfirm();
#endif
    }
    /// <summary>
    /// Called when cross-promo recrods is completed.
    /// </summary>
    /// <param name="response"></param>
    public void OnCrossPromoRecordResponse(string response)
    {
        nutakuToolRecord = JsonUtility.FromJson<NutakuToolRecord>(response);
        Debug.Log(nutakuToolRecord.ToString());
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
    /// <summary>
    /// Call back for javascrips functions, check the response values and call actions depending on that.
    /// </summary>
    /// <param name="arg"></param>
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

    /// <summary>
    /// Active when player open suport tools
    /// </summary>
    /// <param name="active">0 if is close, 1 if is open</param>
    public void OnSuportToolsState(int active)
    {
        //m_SuportToolText.text = string.Format("SuportTool state: {0}", active);
        Debug.LogFormat("SuportTool state: {0}", active);
        if (onSuportToolStateChange != null)
        {
            onSuportToolStateChange(active);
        }
    }

    public static string GetToStringsArray<A> (A[] arrayToPrint)
    {
        if (arrayToPrint == null)
        {
            return "null";
        }
        StringBuilder result = new StringBuilder(arrayToPrint.Length+2);
        result.Append("[");
        for (int i = 0; i < arrayToPrint.Length; i++)
        {
            if (arrayToPrint[i] != null)
            {
                result.Append(arrayToPrint[i].ToString());
            }
            else
            {
                result.Append("(null)");

            }
        }
        result.Append("]");
        return result.ToString();
    }
    
}
/// <summary>
/// Call back response class.
/// </summary>
[System.Serializable]
public class NutakuToolResponse {
    /// <summary>
    /// 0 for success, other values for error.
    /// </summary>
    public int code;
    /// <summary>
    /// Result information as json.
    /// </summary>
    public string result;
}
[System.Serializable]
public class NutakuToolRecord {
    public int status;
    public int code;
    public string message;
    public NutakuToolRecordData data;
    public override string ToString ()
    {
        return string.Format("(status: {0},code: {1},message: {2},data: {3})", status, code, message, data.ToString());
    }
}
[System.Serializable]
public class NutakuToolRecordData {
    public int id;
    public int player_id;
    public int login_game_id;
    public int campaign_id;
    public int campaign_success;
    public int view_count;
    public string date;
    public string modified_date;
    public string click_token;
    public NutakuToolRecordTasks[] tasks;
    public override string ToString ()
    {
        return string.Format("(id: {0},player_id: {1},login_game_id: {2},campaign_id: {3},campaign_success: {4},view_count: {5},date: {6},modified_date: {7},click_token: {8},tasks: {9})", id, player_id, login_game_id, campaign_id, campaign_success, view_count, date, modified_date, click_token, NutakuTools.GetToStringsArray(tasks));
    }
}
[System.Serializable]
public class NutakuToolRecordTasks{
    public string campaign_id;
    public string task_key;
    public string status;
    public string reward_received;
    public string date;
    public string task_id;
    public NutakuToolRecordRewards[] rewards;
    public override string ToString ()
    {
        return string.Format("(campaign_id: {0},task_key: {1},status: {2},reward_received: {3},date: {4},task_id: {5},rewards: {6})", campaign_id, task_key,status, reward_received, date, task_id, NutakuTools.GetToStringsArray(rewards));
    }
}
[System.Serializable]
public class NutakuToolRecordRewards{
    public string id;
    public string game_id;
    public string task_id;
    public string name;
    public string reward_id;
    public string amount;
    public string type;

    public override string ToString ()
    {
        return string.Format("(id: {0},game_id: {1},task_id: {2},name: {3},reward_id: {4},amount: {5},type: {6})",id,game_id,task_id,name,reward_id,amount,type);
    }
}

