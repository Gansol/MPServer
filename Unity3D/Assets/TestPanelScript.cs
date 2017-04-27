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

    private BattleManager battleManager;
    // Use this for initialization
    void Start()
    {
        battleManager = GameObject.FindGameObjectWithTag("GM").GetComponent<BattleManager>();

        lb_status.text = battleManager.spawnStatus.ToString();
        lb_spawnLerp.text = battleManager.battleAIState.GetIntervalTime().ToString();
        lb_betweenLerp.text = battleManager.battleAIState.GetLerpTime().ToString();
        lb_spawnTime.text = battleManager.battleAIState.GetSpawnOffset().ToString();
        lb_spawnCount.text = battleManager.battleAIState.GetSpawnCount().ToString();

    }

    // Update is called once per frame
    void Update()
    {
        lb_status.text = battleManager.spawnStatus.ToString();
        lb_spawnLerp.text = battleManager.battleAIState.GetIntervalTime().ToString();
        lb_betweenLerp.text = battleManager.battleAIState.GetLerpTime().ToString();
        lb_spawnCount.text = battleManager.battleAIState.GetSpawnCount().ToString();
        lb_spawnTime.text = battleManager.battleAIState.GetSpawnTime().ToString();
    }

    public void OnScroll()
    {
        battleManager.battleAIState.SetSpeed(scrollBar.value * 2);
    }

    public void OnOFF()
    {
        battleManager.tSpawnFlag = !battleManager.tSpawnFlag;
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
    public void OnStatus(GameObject obj)
    {
        if (obj.name == "+")
        {
            Debug.Log("已無法調整");
            // battleManager.spawnStatus = (SpawnStatus)((int)battleManager.battleAIState.GetSpawnStatus() + 1);
        }
        else
        {
            Debug.Log("已無法調整");
            // battleManager.spawnStatus = (SpawnStatus)((int)battleManager.battleAIState.GetSpawnStatus() - 1);
        }

        // lb_status.text = battleManager.spawnStatus.ToString();
    }

    public void OnSpawnLerp(GameObject obj)
    {
        if (obj.name == "+")
        {
            battleManager.SetValue(0, 0, battleManager.battleAIState.GetIntervalTime() + 0.5f, 0);
        }
        else
        {
            battleManager.SetValue(0, 0, battleManager.battleAIState.GetIntervalTime() - 0.5f, 0);
        }
        lb_spawnLerp.text = battleManager.battleAIState.GetIntervalTime().ToString();
    }

    public void OnMiceLerp(GameObject obj)
    {
        if (obj.name == "+")
        {
            battleManager.battleAIState.SetValue(battleManager.battleAIState.GetLerpTime() + .05f, 0, 0, 0);
        }
        else
        {
            battleManager.battleAIState.SetValue(battleManager.battleAIState.GetLerpTime() - .05f, 0, 0, 0);
        }
        lb_betweenLerp.text = battleManager.battleAIState.GetLerpTime().ToString();
    }

    public void OnCount(GameObject obj)
    {
        if (obj.name == "+")
        {
            battleManager.battleAIState.SetValue(0, 0, 0, battleManager.battleAIState.GetSpawnCount() + 1);
        }
        else
        {
            battleManager.battleAIState.SetValue(0, 0, 0, battleManager.battleAIState.GetSpawnCount() - 1);
        }

        lb_spawnCount.text = battleManager.battleAIState.GetSpawnCount().ToString();
    }

    public void OnMiceSpawnTime(GameObject obj)
    {
        if (obj.name == "+")
        {
            battleManager.battleAIState.SetValue(0, battleManager.battleAIState.GetSpawnTime() + .05f, 0, 0);
        }
        else
        {
            battleManager.battleAIState.SetValue(0, battleManager.battleAIState.GetSpawnTime() - .05f, 0, 0);
        }
        lb_spawnTime.text = battleManager.battleAIState.GetSpawnTime().ToString();
    }
}
