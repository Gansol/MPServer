using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* ***************************************************************
 * -----Copyright © 2020 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 使用前需要由AssetLoader載入Bundle後才能使用
 * 
 * ***************************************************************
 *                           ChangeLog       
 * 20201123 v1.0.0  未測試
 * ****************************************************************/
public class AudioSystem : IGameSystem
{
    static GameObject musics_root;//这个就是根节点
    Dictionary<string, AudioSource> musics;
    Dictionary<string, AudioSource> sounds;
    static bool is_music_mute = false;//存放当前全局背景音乐是否静音的变量
    static bool is_effect_mute = false;//存放当前音效是否静音的变量

    public AudioSystem(MPGame mPGame) : base(mPGame)
    {
        Debug.Log("--------------- AudioSystem Created ----------------");
        musics = new Dictionary<string, AudioSource>();
        sounds = new Dictionary<string, AudioSource>();
        Initialize();
    }

    public override void Initialize()
    {
        Debug.Log("--------------- AudioSystem Initialize ----------------");
        musics_root = new GameObject("music_root");         // 聲音物件
        GameObject.DontDestroyOnLoad(musics_root);    // 不刪除聲音

        // 取得是否聲音靜音
        if (PlayerPrefs.HasKey("music_mute"))
        {
            int value = PlayerPrefs.GetInt("music_mute");
            is_music_mute = (value == 1);
        }

        // 取得是否音效靜音
        if (PlayerPrefs.HasKey("effect_mute"))
        {
            int value = PlayerPrefs.GetInt("effect_mute");
            is_effect_mute = (value == 1);
        }
    }

    public void PlayMusic( string name, bool bLoop = true)
    {
        AudioSource audio_source;

        if (musics.ContainsKey(name))
        {
            musics.TryGetValue(name, out audio_source);
        }
        else
        {
            GameObject bundle = m_MPGame.GetAssetLoaderSystem().GetAsset(name);
            GameObject music = MPGFactory.GetObjFactory().Instantiate(bundle, musics_root.transform, name, Vector3.zero, Vector3.zero, Vector2.zero, -1);
            audio_source = music.GetComponent<AudioSource>();
            musics.Add(name, audio_source);
        }

        audio_source.mute = is_music_mute;
        audio_source.loop = bLoop;
        audio_source.enabled = true;
        audio_source.Play();
    }

    public void PauseAllMusic()
    {
        foreach (AudioSource s in musics.Values)
        {
            s.Pause();
        }
    }

    public void StopAllMusic()
    {
        foreach (AudioSource s in musics.Values)
        {
            s.Stop();
        }
    }


    public void PlaySound(string name, bool bLoop = false)
    {
        AudioSource audio_source;

        if (sounds.ContainsKey(name))
        {
            sounds.TryGetValue(name, out audio_source);
        }
        else
        {
            GameObject bundle = m_MPGame.GetAssetLoaderSystem().GetAsset(name);
            GameObject sound = MPGFactory.GetObjFactory().Instantiate(bundle, musics_root.transform, name, Vector3.zero, Vector3.zero, Vector2.zero, -1);
            audio_source = sound.GetComponent<AudioSource>();
            sounds.Add(name, audio_source);
        }

        audio_source.mute = is_music_mute;
        audio_source.loop = bLoop;
        audio_source.enabled = true;
        audio_source.Play();
    }

    public void PauseAllSound()
    {
        foreach (AudioSource s in sounds.Values)
        {
            s.Pause();
        }
    }

    public void StopAllSound()
    {
        foreach (AudioSource s in sounds.Values)
        {
            s.Stop();
        }
    }

    public void ClearSound(string name)
    {
        AudioSource audio_source = null;
        if (!sounds.ContainsKey(name)) //判斷是否已經在背景音樂表裡面了
        {
            return; //沒有這個背景音樂就直接返回
        }

        audio_source = sounds[name]; //有就把audio_source直接賦值過去
        GameObject.Destroy(audio_source.gameObject); //刪除掉掛載指定audio_source組件的節點
        sounds[name] = null; //指定audio_source組件清空
        sounds.Remove(name);
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
