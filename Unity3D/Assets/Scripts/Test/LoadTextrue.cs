using UnityEngine;
using System.Collections;

public class LoadTextrue : MonoBehaviour
{
    public string url = "http://192.168.88.77:58767/MicePow/good.jpg";
    IEnumerator Start()
    {
        renderer.material.mainTexture = new Texture2D(4, 4, TextureFormat.DXT1, false);

            using (WWW www = new WWW(url))
            {
                yield return www;
                www.LoadImageIntoTexture(renderer.material.mainTexture as Texture2D);
                GetComponent<UITexture>().mainTexture = www.texture;
            }
    }
}