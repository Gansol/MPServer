using UnityEngine;
using System.Collections;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

public class XXXXXX : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }


    public void GoogleLogin()
    {
        Debug.Log("Google Logining...");

        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                Debug.Log("You've successfully logged in" + Social.localUser.id+"   "+ Social.localUser.userName);
            }
            else
            {
                Debug.Log("Login failed for some reason");
            }
        });
    }
}
