using UnityEngine;
using System.Collections;

public class ShowVision : MonoBehaviour
{

    public UILabel vision;
    private float lastTime;

    void Awake()
    {
        lastTime = 0;
        Global.photonService.ActorOnlineEvent += OnGetActorOnline;
    }

    void Start()
    {
        vision.text = "線上玩家數:" + Global.OnlineActor + "  Ver." + Global.gameVersion + " build " + Global.bundleVersion;
    }

    private void OnGetActorOnline()
    {
        vision.text = "線上玩家數:" + Global.OnlineActor + "  Ver." + Global.gameVersion + " build " + Global.bundleVersion;
    }
}
