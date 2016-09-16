using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    public string[] itemProperty;
    public string[] itemInfo;
    public string[] etcInfo;

    void OnClick()
    {
       GetComponentInParent<StoreManager>().SendMessage("OnItemClick",gameObject);
    }
}
