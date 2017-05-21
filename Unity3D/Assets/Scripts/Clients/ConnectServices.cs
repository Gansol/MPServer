using UnityEngine;
using System.Collections;

public class ConnectServices : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // 呼叫Service()
        Global.photonService.Service();
    }

}
