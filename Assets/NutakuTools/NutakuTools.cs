using AOT;
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NutakuTools : Singleton<NutakuTools> {
    /// <summary>
    /// Cross promotion key to save cd time.
    /// </summary>
    public const string KEY_BANNER_CLIC_TIME = "nutakucrosspromobannertime";
    /// <summary>
    ///  Cross Promotion cooldown time in hours to show banner again.
    /// </summary>
    private const int X_PROMO_CD = 24;
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void CheckGuestPlayer (Action<string> action);
    [DllImport("__Internal")]
    private static extern void OpenGuestPlayerFrom ();
    [DllImport("__Internal")]
    private static extern void OpenCrossPromotionBanner ();
    [DllImport("__Internal")]
    private static extern void CrossPromotionTaskArchieve ();
    [DllImport("__Internal")]
    private static extern void CrossPromotionTaskConfirm (string task_id);

    

    [DllImport("__Internal")]
    private static extern void OnNutakuToolStart ();
#endif

    private static Action<string> m_action;
    private static Action<string> m_actionError;
    public Action<int> onSuportToolStateChange;
    public const int SUPPORT_TOOL_OPEN = 1;
    public NutakuToolRecord nutakuToolRecord;
    private bool _canCheckCrossPromo;
    //No tiene rewards
    public const int NO_REWARDS = 0;
    //Have rewards pending
    public const int HAVE_REWARDS = 1;
    //Adding rewards
    public const int ADDING_REWARDS = 2;
    //Rewards already give.
    public const int FIRST_REWARD_WERE_ADDED = 3;
    //Confirm rewards 
    public const int REWARD_CONFIRMATION = 4;
    //Confirm request success
    public const int REWARDS_CONFIRMATION_COMPLETED = 5;
    public int rewardState;

    public const int NOT_RECORD = 0;
    public const int RECORD_UPDATE = 1;
    public const int RECORD_COMPLETE = 2;
    public int recordState = 0;

    private bool activeCrossPromotionBanner;
    private string userID;
    private bool isShowingRewardsBanner;

    protected override void OnAwake ()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start ()
    {
        rewardState = NO_REWARDS;
        recordState = NOT_RECORD;
        isShowingRewardsBanner = false;
        OnSuportToolsState(0);
        /*OnCrossPromoRecordResponse("{\"status\":200,\"code\":1,\"message\":\"OK!\",\"data\":{\"tasks\":[{\"campaign_id\":\"461\",\"task_key\":\"ABqlqKkkPk\",\"status\":\"1\",\"reward_received\":\"0\",\"date\":\"2019-05-09 20:52:16\",\"task_id\":\"351\",\"rewards\":[{\"id\":\"431\",\"game_id\":\"251\",\"task_id\":\"351\",\"name\":\"Reward A\",\"reward_id\":\"1\",\"amount\":\"1000\",\"type\":\"1\"}]}]}}");*/

#if UNITY_WEBGL  && !UNITY_EDITOR
        OnNutakuToolStart();
#endif
    }

    private void Update ()
    {
        if (!string.IsNullOrEmpty(userID))
        {
            if (recordState == RECORD_UPDATE)
            {
                recordState = RECORD_COMPLETE;
                CheckRewardsFromRecord();
            }
            else if (recordState == RECORD_COMPLETE)
            {
                if (rewardState == NO_REWARDS)
                {
                    if (activeCrossPromotionBanner)
                    {
                        CheckCrossPromotionBanner();
                    }
                }
                else if (rewardState == FIRST_REWARD_WERE_ADDED)
                {
                    rewardState = REWARD_CONFIRMATION;
                    OnCrossPromotionTaskConfirm();
                }
                else if (rewardState == REWARDS_CONFIRMATION_COMPLETED)
                {
                    rewardState = NO_REWARDS;
                    OpenRewardsBanner();
                }
            }
        }
        
    }

    public void SetFirstTaskReward ()
    {
        rewardState = FIRST_REWARD_WERE_ADDED;
    }

    public void SetUserId (string _userID)
    {
        Debug.LogFormat("Set userid: {0}",_userID);
        userID = _userID;
    }

    public void OnActivateCrossPromotionBanner ()
    {
        activeCrossPromotionBanner = true;
        Debug.Log("OnActivateCrossPromotionBanner");
    }

    /// <summary>
    /// Open rewards banner
    /// </summary>
    private void OpenRewardsBanner ()
    {
        Debug.Log("OpenRewardsBanner");
        isShowingRewardsBanner = true;
        OnOpenCrossPromotionBanner();
    }

    /// <summary>
    /// Called when cross-promo recrods is completed.
    /// </summary>
    /// <param name="response"></param>
    public void OnCrossPromoRecordResponse (string response)
    {
        nutakuToolRecord = JsonUtility.FromJson<NutakuToolRecord>(response);
        Debug.Log(nutakuToolRecord.ToString());
        if (nutakuToolRecord != null)
        {
            if (nutakuToolRecord.status == 200)
            {
                recordState = RECORD_UPDATE;
            }
            else
            {
                recordState = NOT_RECORD;
            }
        }
        else
        {
            recordState = NOT_RECORD;
        }

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
        Debug.Log("OnOpenGuestPlayerFrom");
#if UNITY_WEBGL
        OpenGuestPlayerFrom();
#endif
    }

    /// <summary>
    /// Open Cross promotion banner
    /// </summary>
    public void OnOpenCrossPromotionBanner ()
    {
        Debug.Log("OnOpenCrossPromotionBanner");
#if UNITY_WEBGL
        OpenCrossPromotionBanner();
#endif
    }

    /// <summary>
    /// Cross promotion banner completed
    /// </summary>
    private void OnOpenCrossPromotionShowBannerModalComplete ()
    {
        Debug.LogFormat("OnOpenCrossPromotionShowBannerModalComplete userid: {0}", userID);
        if (!isShowingRewardsBanner)
        {
            PlayerPrefs.SetString(GetKeyFroCrossPromoBannerTime(), System.DateTime.Now.ToBinary().ToString());
            Debug.Log("Saved cross promotion baner time.");
        }
        else
        {
            isShowingRewardsBanner = false;
        }
    }
    private string GetKeyFroCrossPromoBannerTime ()
    {
        return string.Format("{0}{1}", KEY_BANNER_CLIC_TIME, userID);
    }

    /// <summary>
    /// Set the current taks archieve.
    /// </summary>
    public void OnCrossPromotionTaskArchieve ()
    {
        Debug.Log("OnCrossPromotionTaskArchieve");
#if UNITY_WEBGL
        CrossPromotionTaskArchieve();
#endif
    }

    /// <summary>
    /// Set the current cross-promo task confirmed.
    /// </summary>
    /// <returns> true if send request, false if a error happen</returns>
    public void OnCrossPromotionTaskConfirm ()
    {
        Debug.Log("OnCrossPromotionTaskConfirm");
#if UNITY_WEBGL
           CrossPromotionTaskConfirm(GetFirstTaskUnreceived().task_key);
#endif

    }
    /// <summary>
    /// Return the first taks unreceived.
    /// </summary>
    /// <returns> unreceived task or null if not found any.</returns>
    public NutakuToolRecordTasks GetFirstTaskUnreceived ()
    {
        if (nutakuToolRecord != null)
        {
            if (nutakuToolRecord.data != null)
            {
                if (nutakuToolRecord.data.tasks != null)
                {
                    for (int i = 0; i < nutakuToolRecord.data.tasks.Length; i++)
                    {
                        if (nutakuToolRecord.data.tasks[i].reward_received == "0")
                        {
                            return nutakuToolRecord.data.tasks[i];
                        }
                    }
                }
            }
        }
                   
        return null;
    }
    /// <summary>
    /// Rewards Confirmation taks
    /// </summary>
    /// <param name="response"></param>
    public void OnCrossPromotionTaskConfirmResponse (string response)
    {
        NutakuToolRecord nutakuToolRecordConfirmResponse = JsonUtility.FromJson<NutakuToolRecord>(response);
        Debug.Log(nutakuToolRecordConfirmResponse.ToString());
        if (nutakuToolRecordConfirmResponse.status == 200)
        {
            rewardState = REWARDS_CONFIRMATION_COMPLETED;
        }
        else
        {
            rewardState = NO_REWARDS;
        }

    }
    /// <summary>
    /// Rewards Confirmation taks
    /// </summary>
    /// <param name="response"></param>
    public void OnCrossPromotionTaskArchieveResponse (string response)
    {
        Debug.LogFormat("OnCrossPromotionTaskArchieveResponse {0}", response);
    }

    /// <summary>
    /// Check the rewards from records task
    /// </summary>
    private void CheckRewardsFromRecord ()
    {
        if (nutakuToolRecord != null)
        {
            if (nutakuToolRecord.data != null)
            {
                if (nutakuToolRecord.data.tasks != null)
                {
                    if (nutakuToolRecord.data.tasks.Length > 0)
                    {
                        rewardState = HAVE_REWARDS;
                    }
                }
            }
        }
    }

    private void CheckCrossPromotionBanner ()
    {
        activeCrossPromotionBanner = false;
        string currentKeyValue = PlayerPrefs.GetString(GetKeyFroCrossPromoBannerTime(), string.Empty);
        if (!string.IsNullOrEmpty(currentKeyValue))
        {
            long temp = Convert.ToInt64(currentKeyValue);
            DateTime oldDate = DateTime.FromBinary(temp);
            TimeSpan difference = System.DateTime.Now.Subtract(oldDate);
            Debug.LogFormat("Cross promotion could down time {0}/{1} ", difference.Hours, X_PROMO_CD);
            if (difference.Hours >= X_PROMO_CD)
            {
                PlayerPrefs.SetString(GetKeyFroCrossPromoBannerTime(), string.Empty);

                OnOpenCrossPromotionBanner();
            }
        }
        else
        {
            OnOpenCrossPromotionBanner();
        }
    }

    #region test
    public void OnCheckGuestPlayerTest ()
    {
        OnCheckGuestPlayer(OnTestResult, OnTestResultError);
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
        if (nutakuToolResponse.code == 0)
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
        m_action = null;
    }

    /// <summary>
    /// Active when player open suport tools
    /// </summary>
    /// <param name="active">0 if is close, 1 if is open</param>
    public void OnSuportToolsState (int active)
    {
        //m_SuportToolText.text = string.Format("SuportTool state: {0}", active);
        Debug.LogFormat("SuportTool state: {0}", active);
        if (onSuportToolStateChange != null)
        {
            onSuportToolStateChange(active);
        }
    }
    /// <summary>
    /// Get tostring from array.
    /// </summary>
    /// <typeparam name="A"> Array type</typeparam>
    /// <param name="arrayToPrint"> Array to print</param>
    /// <returns> tostring from array</returns>
    public static string GetToStringsArray<A> (A[] arrayToPrint)
    {
        if (arrayToPrint == null)
        {
            return "null";
        }
        StringBuilder result = new StringBuilder(arrayToPrint.Length + 2);
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
public class NutakuToolRecordTasks {
    public string campaign_id;
    public string task_key;
    public string status;
    public string reward_received;
    public string date;
    public string task_id;
    public NutakuToolRecordRewards[] rewards;
    public override string ToString ()
    {
        return string.Format("(campaign_id: {0},task_key: {1},status: {2},reward_received: {3},date: {4},task_id: {5},rewards: {6})", campaign_id, task_key, status, reward_received, date, task_id, NutakuTools.GetToStringsArray(rewards));
    }
}
[System.Serializable]
public class NutakuToolRecordRewards {
    public string id;
    public string game_id;
    public string task_id;
    public string name;
    public string reward_id;
    public string amount;
    public string type;

    public override string ToString ()
    {
        return string.Format("(id: {0},game_id: {1},task_id: {2},name: {3},reward_id: {4},amount: {5},type: {6})", id, game_id, task_id, name, reward_id, amount, type);
    }
}

