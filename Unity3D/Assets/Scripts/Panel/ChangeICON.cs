using UnityEngine;
using System.Collections;

public class ChangeICON : MonoBehaviour {

    string imageName;
    void Awake()
    {
        imageName = transform.Find("Image").GetChild(0).GetComponent<UISprite>().spriteName;
    }

    void OnEnable()
    {
        UIEventListener.Get(gameObject).onClick += OnClick;
        
    }

    public void OnClick(GameObject obj)
    {
        PlayerManager.playerImage.spriteName = imageName;
        Global.photonService.UpdatePlayerData(imageName);
    }

    void OnDisable()
    {
        UIEventListener.Get(gameObject).onClick -= OnClick;
    }
}
