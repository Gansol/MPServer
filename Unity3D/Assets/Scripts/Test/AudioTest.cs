using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    Dictionary<string, AudioSource> music;
    float lastTime = 0;
    AudioSource s;
    public AudioClip ms;
    public float speed = 1;

    private void Start()
    {
        music = new Dictionary<string, AudioSource>();


    }

    private void Update()
    {

        if (Time.time > lastTime + speed)
        {
            //music.Add("s", ms);
            s = gameObject.AddMissingComponent<AudioSource>();
            s.clip = ms;
            s.Play();
            lastTime = Time.time;
        }
    }
}
