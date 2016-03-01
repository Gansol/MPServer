using UnityEngine;
using System.Collections;

public class DataUI : MonoBehaviour {

    private bool doOnce;
	// Use this for initialization
	void Start () {
        doOnce = true;
	}
	
	// Update is called once per frame
	void Update () {
 
        if (Global.LoginStatus && !Global.isPlayerDataLoaded && doOnce)
        {
            doOnce = false;
            Global.photonService.LoadPlayerData(Global.Account);
            Global.photonService.LoadCurrency(Global.Account);
            Global.photonService.LoadMiceData();
        }
       

        //Debug.Log(Global.isPlayerDataLoaded);
	}

    void OnGUI()
    {
        if (Global.isPlayerDataLoaded)
        {
            GUI.Label(new Rect(200, 40, 400, 20), "Your Rank : " + Global.Rank);            // 顯示等級
            GUI.Label(new Rect(200, 60, 400, 20), "Your EXP : " + Global.EXP);              // 顯示經驗
            GUI.Label(new Rect(200, 80, 400, 20), "Your MaxCombo : " + Global.MaxCombo);    // 顯示Combo
            GUI.Label(new Rect(200, 100, 400, 20), "Your MaxScore : " + Global.MaxScore);    // 顯示最高分
            GUI.Label(new Rect(200, 120, 400, 20), "Your MiceAll : " + Global.MiceAll);      // 顯示全部老鼠
            GUI.Label(new Rect(200, 140, 400, 20), "Your Friend : " + Global.Friend);        // 顯示朋友列表
           // Debug.Log("ITEM:" + Global.Item);
            if (GUI.Button(new Rect(200, 160, 100, 20), "Update Rnak")) // 88 是測試資料 有問題 需要改掉
            {
                Global.photonService.UpdatePlayerData(Global.Account, 88, Global.EXP, Global.MaxCombo, Global.MaxScore, Global.SumScore, Global.MiceAll, Global.Team, Global.MiceAmount,Global.Friend);
            }

            if (GUI.Button(new Rect(200, 180, 100, 20), "ReLoad Data"))
            {
                Global.photonService.LoadPlayerData(Global.Account);
            }

        }

        if (Global.isCurrencyLoaded)
        {
            GUI.Label(new Rect(200, 300, 400, 20), "Your Rice : " + Global.Rice);            // 顯示遊戲幣
            GUI.Label(new Rect(200, 320, 400, 20), "Your Gold : " + Global.Gold);              // 顯示金幣
        }

        if (Global.isMiceLoaded)
        {
            GUI.Label(new Rect(200, 340, 400, 200), "Your Mice : " + Global.miceProperty.Count);            // 顯示老鼠資料 dict<string,object>
        }
    }
}
