using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    public string itemName;
    public float[] property;

    void OnClick()
    {
       GetComponentInParent<StoreManager>().SendMessage("OnItemClick",gameObject);
    }
}
