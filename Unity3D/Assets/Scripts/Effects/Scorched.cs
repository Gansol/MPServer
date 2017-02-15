using UnityEngine;
using System.Collections;

public class Scorched : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<MiceBase>())
        {
            col.gameObject.GetComponent<MiceBase>().OnEffect("Scorched", null);
        }
        else
        {
            Debug.Log("Effect Error!");
        }
    }
}
