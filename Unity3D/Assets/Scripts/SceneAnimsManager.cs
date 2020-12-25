using UnityEngine;
using System.Collections;

public class SceneAnimsManager : MonoBehaviour {

    public void PlayHole(GameObject go)
    {
        go.GetComponent<Animation>().Play();
    }

}
