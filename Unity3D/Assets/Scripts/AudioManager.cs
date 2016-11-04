using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    public AudioClip BGSound;
    float audio1Volume = 1.0f;
    bool track2Playing = false;
    bool flag;

    void Awake()
    {
        audio.volume = 0;
        audio.loop = true;
    }

    void Start()
    {
        audio.clip = BGSound;
        audio.Stop();
        flag = true;
        Debug.Log(audio.name);

    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 300, 100, 50), "Audio UP"))
        {
            audio.volume += 0.5f;
        }
        if (GUI.Button(new Rect(100, 300, 100, 50), "Audio ON/OFF"))
        {
            if (flag)
            {
                audio.Stop();
                audio.volume = 0.0f;
            }
            else
            {
                bFadeOut = false;
                audio.Play();
                audio.volume = 0.5f;
            }
        }
        if (GUI.Button(new Rect(200, 300, 100, 50), "Audio Down"))
        {
            audio.volume -= 0.5f;
        }
    }

    void Update()
    {
        FadeIn();

        if (audio.isPlaying == false)
        {
            audio.volume = 0;
            audio.Play();
            Debug.Log("REPLAY");
        }

    }

    void FadeIn()
    {

        if (audio.volume < 0.8f)
        {
            audio.volume += 0.1f * Time.deltaTime;
        }

    }

    void FadeOut()
    {

            if (audio.volume > 0.1f)
            {
                audio.volume -= 0.1f * Time.deltaTime;
            }

    }

    public bool bFadeOut { get; set; }
}
