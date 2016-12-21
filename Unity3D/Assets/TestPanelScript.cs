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

    private SpawnController sc;
    // Use this for initialization
    void Start()
    {
        sc = GameObject.FindGameObjectWithTag("GM").GetComponent<SpawnController>();

        lb_status.text = sc.spawnStatus.ToString();
        lb_spawnLerp.text = sc._spawnIntervalTime.ToString();
        lb_betweenLerp.text = sc.lerpTime.ToString();
        lb_spawnCount.text = sc.spawnCount.ToString();
        lb_spawnTime.text = sc.spawnTime.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnOFF()
    {
        sc.tSpawnFlag = !sc.tSpawnFlag;
    }

    public void OnOpenTestPanel()
    {
        if (!bTSPanel)
            testPanel.SetActive(true);
        else
            testPanel.SetActive(false);

        bTSPanel = !bTSPanel;
    }

    public void OnExit()
    {
        Global.photonService.KickOther();
        Global.photonService.ExitRoom();
    }

    public void OnStatus(GameObject obj)
    {
        if (obj.name == "+")
        {
            sc.spawnStatus = (SpawnStatus)((int)sc.spawnState.GetSpawnStatus() + 1);
        }
        else
        {
            sc.spawnStatus = (SpawnStatus)((int)sc.spawnState.GetSpawnStatus() - 1);
        }

        lb_status.text = sc.spawnStatus.ToString();
    }

    public void OnSpawnLerp(GameObject obj)
    {
        if (obj.name == "+")
        {
            sc._spawnIntervalTime += .05f;
        }
        else
        {
            sc._spawnIntervalTime -= .05f;
        }
        lb_spawnLerp.text = sc._spawnIntervalTime.ToString();
    }

    public void OnMiceLerp(GameObject obj)
    {
        if (obj.name == "+")
        {
            sc.lerpTime += .05f;
        }
        else
        {
            sc.lerpTime -= .05f;
        }
        lb_betweenLerp.text = sc.lerpTime.ToString();
    }

    public void OnCount(GameObject obj)
    {
        if (obj.name == "+")
        {
            sc.spawnCount += 1;
        }
        else
        {
            sc.spawnCount -= 1;
        }

        lb_spawnCount.text = sc.spawnCount.ToString();
    }

    public void OnMiceSpawnTime(GameObject obj)
    {
        if (obj.name == "+")
        {
            sc.spawnTime += .05f;
        }
        else
        {
            sc.spawnTime -= .05f;
        }
        lb_spawnTime.text = sc.spawnTime.ToString();
    }
}
