using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    public string[] property;

    void OnClick()
    {
       GetComponentInParent<StoreManager>().SendMessage("OnItemClick",gameObject);
    }
}
