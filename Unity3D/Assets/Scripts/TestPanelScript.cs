using UnityEngine;
using System.Collections;
using MPProtocol;
public class TestPanelScript : MonoBehaviour
{

    public GameObject testPanel;
    public bool bTSPanel;
    public UILabel lb_status;
    public UILabel lb_spawnLerp;
    public UILabel lb_betweenLerp;
    public UILabel lb_spawnTime;
    public UILabel lb_spawnCount;
    public UIScrollBar scrollBar;

    private BattleSystem battleManager;
    // Use this for initialization
    void Start()
    {
        battleManager = MPGame.Instance.GetBattleSystem();

       // lb_status.text = battleManager.spawnStatus.ToString();
        lb_spawnLerp.text = battleManager.GetBattleAIState().GetIntervalTime().ToString();
        lb_betweenLerp.text = battleManager.GetBattleAIState().GetLerpTime().ToString();
        lb_spawnTime.text = battleManager.GetBattleAIState().GetSpawnOffset().ToString();
        lb_spawnCount.text = battleManager.GetBattleAIState().GetSpawnCount().ToString();

    }

    // Update is called once per frame
    void Update()
    {
      //  lb_status.text = battleManager.spawnStatus.ToString();
        lb_spawnLerp.text = battleManager.GetBattleAIState().GetIntervalTime().ToString();
        lb_betweenLerp.text = battleManager.GetBattleAIState().GetLerpTime().ToString();
        lb_spawnCount.text = battleManager.GetBattleAIState().GetSpawnCount().ToString();
        lb_spawnTime.text = battleManager.GetBattleAIState().GetSpawnTime().ToString();
    }

    public void OnScroll()
    {
        battleManager.GetBattleAIState().SetSpeed(scrollBar.value * 2);
    }

    public void OnOFF()
    {
        battleManager.SpawnFlag = !battleManager.SpawnFlag;
    }

    public void OnOpenTestPanel()
    {
        if (!bTSPanel)
            testPanel.SetActive(true);
        else
            testPanel.SetActive(false);

        bTSPanel = !bTSPanel;
    }

    // 現在無法調整了 spawnStatus 已由Spawner控制
    public void OnStatus(GameObject go)
    {
        if (go.name == "+")
        {
            Debug.Log("已無法調整");
            // battleManager.spawnStatus = (SpawnStatus)((int)battleManager.GetBattleAIState().GetSpawnStatus() + 1);
        }
        else
        {
            Debug.Log("已無法調整");
            // battleManager.spawnStatus = (SpawnStatus)((int)battleManager.GetBattleAIState().GetSpawnStatus() - 1);
        }

        // lb_status.text = battleManager.spawnStatus.ToString();
    }

    public void OnSpawnLerp(GameObject go)
    {
        if (go.name == "+")
        {
            battleManager.SetValue(0, 0, battleManager.GetBattleAIState().GetIntervalTime() + 0.5f, 0);
        }
        else
        {
            battleManager.SetValue(0, 0, battleManager.GetBattleAIState().GetIntervalTime() - 0.5f, 0);
        }
        lb_spawnLerp.text = battleManager.GetBattleAIState().GetIntervalTime().ToString();
    }

    public void OnMiceLerp(GameObject go)
    {
        if (go.name == "+")
        {
            battleManager.GetBattleAIState().SetValue(battleManager.GetBattleAIState().GetLerpTime() + .05f, 0, 0, 0);
        }
        else
        {
            battleManager.GetBattleAIState().SetValue(battleManager.GetBattleAIState().GetLerpTime() - .05f, 0, 0, 0);
        }
        lb_betweenLerp.text = battleManager.GetBattleAIState().GetLerpTime().ToString();
    }

    public void OnCount(GameObject go)
    {
        if (go.name == "+")
        {
            battleManager.GetBattleAIState().SetValue(0, 0, 0, battleManager.GetBattleAIState().GetSpawnCount() + 1);
        }
        else
        {
            battleManager.GetBattleAIState().SetValue(0, 0, 0, battleManager.GetBattleAIState().GetSpawnCount() - 1);
        }

        lb_spawnCount.text = battleManager.GetBattleAIState().GetSpawnCount().ToString();
    }

    public void OnMiceSpawnTime(GameObject go)
    {
        if (go.name == "+")
        {
            battleManager.GetBattleAIState().SetValue(0, battleManager.GetBattleAIState().GetSpawnTime() + .05f, 0, 0);
        }
        else
        {
            battleManager.GetBattleAIState().SetValue(0, battleManager.GetBattleAIState().GetSpawnTime() - .05f, 0, 0);
        }
        lb_spawnTime.text = battleManager.GetBattleAIState().GetSpawnTime().ToString();
    }
}
