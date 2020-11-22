using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    //public string[] itemProperty;
    //public string[] storeInfo;
    //public string[] etcInfo;

    void OnClick()
    {
       GetComponentInParent<StoreUI>().OnItemClick(gameObject);
    //    GetComponentInParent<StoreManager>()..SendMessage("OnItemClick", gameObject);
    }
}
