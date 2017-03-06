using UnityEngine;
using System.Collections;

public class Scorched : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<Mice>())
        {
            col.gameObject.GetComponent<Mice>().OnEffect("Scorched", null);
        }
        else
        {
            Debug.Log("Effect Error!");
        }
    }
}
