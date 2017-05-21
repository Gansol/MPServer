using UnityEngine;
using System.Collections;

public class SceneAnimsManager : MonoBehaviour {

    public void PlayHole(GameObject obj)
    {
        obj.GetComponent<Animation>().Play();
    }

}
