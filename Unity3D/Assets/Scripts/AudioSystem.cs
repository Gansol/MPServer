using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : GameSystem
{
    Dictionary<string, AudioSource> musics;
    Dictionary<string, AudioSource> sounds;

    public AudioSystem(MPGame mPGame) : base(mPGame)
    {

    }

    public override void Initinal()
    {
        musics = new Dictionary<string, AudioSource>();
        sounds = new Dictionary<string, AudioSource>();
    }

    public override void Update()
    {
        
    }

    public void PlayMusic(GameObject obj, string name , bool bLoop = true)
    {
        AudioSource objAudio = null;
        
        objAudio =  obj.AddMissingComponent<AudioSource>();
        musics.TryGetValue(name, out AudioSource audio_source);
        objAudio.clip = audio_source.clip;
        objAudio.Play();
    }

    public void PauseMusic(string name)
    {
        musics.TryGetValue(name, out AudioSource audio_source);
        audio_source.Pause();
    }

    public void StopMusic(string name)
    {
        AudioSource audio_source = null;
        audio_source.Stop();
    }




    public void PlaySound(string name, bool bLoop = true)
    {
        AudioSource audio_source = null;
        audio_source.Play();
    }

    public void PauseSound(string name)
    {
        AudioSource audio_source = null;
        audio_source.Pause();
    }

    public void StopSound(string name)
    {
        AudioSource audio_source = null;
        audio_source.Stop();
    }

    public void PauseAllSound()
    {
        AudioSource audio_source = null;
        audio_source.Pause();
    }
    public void StopAllSound()
    {
        AudioSource audio_source = null;
        audio_source.Stop();
    }

    //void FadeIn()
    //{

    //    if (GetComponent<AudioSource>().volume < maxVolume)
    //    {
    //        GetComponent<AudioSource>().volume += 0.1f * Time.deltaTime;
    //    }

    //}

    //void FadeOut()
    //{

    //    if (GetComponent<AudioSource>().volume > minVolume)
    //    {
    //        GetComponent<AudioSource>().volume -= 0.1f * Time.deltaTime;
    //    }

    //}

    public bool bFadeOut { get; set; }

    public override void Release()
    {
        musics.Clear();
        sounds.Clear();
    }
}
