using UnityEngine;
using System.Collections;

public class FocusExample : MonoBehaviour {

    bool isPaused = false;
    public UILabel label;
    void OnGUI()
    {
        if (isPaused)
        {
            label.text = "Paused";
            Debug.Log(label.text);
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        isPaused = !hasFocus;
        label.text = "hasFocus : " + hasFocus;
        Debug.Log(label.text);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
        label.text = "pauseStatus : " + pauseStatus;
        Debug.Log(label.text);
    }
}
