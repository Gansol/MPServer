using UnityEngine;
using System.Collections;

public class ChangeICON : MonoBehaviour {

    string imageName;
    void Start()
    {
        UIEventListener.Get(gameObject).onClick += OnClick;
        imageName = transform.Find("Image").GetChild(0).GetComponent<UISprite>().spriteName;
    }

    public void OnClick(GameObject obj)
    {

        PlayerManager.playerImage.spriteName = imageName;
        Global.photonService.UpdatePlayerData(imageName);
    }
}
