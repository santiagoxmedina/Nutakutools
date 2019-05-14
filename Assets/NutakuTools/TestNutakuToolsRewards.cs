using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNutakuToolsRewards : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(NutakuTools.Instance.rewardState == NutakuTools.HAVE_REWARDS)
        {
            NutakuTools.Instance.rewardState = NutakuTools.ADDING_REWARDS;
            StartCoroutine(AddRewards());
        }
	}

    private IEnumerator AddRewards ()
    {
        yield return new WaitForSeconds(1);
        NutakuToolRecordTasks tasks = NutakuTools.Instance.GetFirstTaskUnreceived();
        if (tasks != null)
        {
            ProcessTask(tasks);
            NutakuTools.Instance.SetFirstTaskReward();
        }
    }

    private void ProcessTask (NutakuToolRecordTasks tasks)
    {
        if(tasks.reward_received == "0")
        {
            if (tasks.rewards != null && tasks.rewards.Length>0)
            {
                for (int i = 0; i < tasks.rewards.Length; i++)
                {
                    NutakuToolRecordRewards reward = tasks.rewards[i];
                    Debug.LogFormat("Rewards information name {3} reward_id: {0}, amount: {1}, type: {2}", reward.reward_id, reward.amount, reward.type, reward.name);
                }
            }
            else
            {
                Debug.Log("Rewards are empty");
            }
            
        }
        else
        {
            Debug.Log("Rewards already received");
        }
    }
}
