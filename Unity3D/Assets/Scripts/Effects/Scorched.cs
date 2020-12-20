using UnityEngine;
using System.Collections;

public class Scorched : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<IMice>())
        {
            GetComponent<BoxCollider2D>().enabled = false;
            col.gameObject.GetComponent<IMice>().OnEffect("Scorched", null);
            if (GetComponent<ParticleSystem>()) GetComponent<ParticleSystem>().enableEmission = true;
        }
        else
        {
            Debug.Log("Effect Error!");
        }
    }
}
