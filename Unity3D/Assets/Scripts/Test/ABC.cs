using UnityEngine;
using System.Collections;

public class ABC : MonoBehaviour  {

public void fuck(){
    Global.photonService.LoadPlayerData(Global.Account);
}
}
