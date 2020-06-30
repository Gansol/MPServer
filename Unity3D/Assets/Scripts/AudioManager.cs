using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    public AudioClip BGSound;
    public AudioSource[] SE;
    float audio1Volume = 0f, minVolume=.1f,maxVolume = .5f;

    bool flag;

    void Awake()
    {
        GetComponent<AudioSource>().volume = audio1Volume;
        GetComponent<AudioSource>().loop = true;
    }

    void Start()
    {
        GetComponent<AudioSource>().clip = BGSound;
        GetComponent<AudioSource>().Stop();
        flag = true;
//        Debug.Log(audio.name);

    }

    void OnGUI()
    {
        //if (GUI.Button(new Rect(0, 300, 100, 50), "Audio UP"))
        //{
        //    audio.volume += 0.5f;
        //}
        //if (GUI.Button(new Rect(100, 300, 100, 50), "Audio ON/OFF"))
        //{
        //    if (flag)
        //    {
        //        audio.volume = 0.0f;
        //    }
        //    else
        //    {
        //        bFadeOut = false;
        //        audio.Play();
        //        audio.volume = 0.5f;
        //    }
        //}
        //if (GUI.Button(new Rect(200, 300, 100, 50), "Audio Down"))
        //{
        //    audio.volume -= 0.1f;
        //}
    }

    void Update()
    {
        FadeIn();

        if (GetComponent<AudioSource>().isPlaying == false)
        {
            GetComponent<AudioSource>().volume = 0;
            GetComponent<AudioSource>().Play();
            Debug.Log("PLAY Sound");
        }

    }

    void FadeIn()
    {

        if (GetComponent<AudioSource>().volume < maxVolume)
        {
            GetComponent<AudioSource>().volume += 0.1f * Time.deltaTime;
        }

    }

    void FadeOut()
    {

        if (GetComponent<AudioSource>().volume > minVolume)
            {
                GetComponent<AudioSource>().volume -= 0.1f * Time.deltaTime;
            }

    }

    public bool bFadeOut { get; set; }
}
