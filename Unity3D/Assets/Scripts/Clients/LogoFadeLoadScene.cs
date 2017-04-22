using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class LogoFadeLoadScene : MonoBehaviour
{
    InternetChecker check;
    public int seconds;
    public UITexture Logo;
    private float lastTime;
    bool flag;
    // Use this for initialization

    void Awake()
    {
        check = gameObject.AddComponent<InternetChecker>();
    }

    void Start()
    {
        StartCoroutine(WaitSeconds());
        StartCoroutine(WaitSeconds());
    }

    IEnumerator WaitSeconds()
    {
        yield return new WaitForSeconds(seconds);
    }

    void Update()
    {
        float time = Time.time;


        if (time > lastTime + 0.1f && time < seconds / 4 * 3)
        {
            Logo.color = new Color(255, 255, 255, Mathf.Lerp(Logo.color.a, 1, 0.3f));
            lastTime = time;
        }

        if (time > lastTime + 0.1f && time > seconds / 4 * 3 && time < seconds)
        {
            Logo.color = new Color(255, 255, 255, Mathf.Lerp(Logo.color.a, 0, 0.4f));
            lastTime = time;
        }

        if (time > seconds && !flag)
        {
            flag = !flag;
            

            if (check.ConnStatus)
                Application.LoadLevel("BundleCheck");
            if (!check.ConnStatus)
                Application.LoadLevel("MainGame");
        }
    }
}
