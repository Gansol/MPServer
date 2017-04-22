using UnityEngine;
using System.Collections;

public class Scorched : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<MiceBase>())
        {
            GetComponent<BoxCollider2D>().enabled = false;
            col.gameObject.GetComponent<MiceBase>().OnEffect("Scorched", null);
            if (GetComponent<ParticleSystem>()) GetComponent<ParticleSystem>().enableEmission = true;
        }
        else
        {
            Debug.Log("Effect Error!");
        }
    }
}
