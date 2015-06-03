using UnityEngine;
using System.Collections;
using System;

public class test2 : MonoBehaviour
{
    private string _default = "name";
    private string _player_name = "";

    private void OnGUI()
    {
        GUI.SetNextControlName("player_name");
        _player_name = GUI.TextField(new Rect(0, 0, 200, 30), _player_name);

        if (UnityEngine.Event.current.type == EventType.Repaint)
        {
            if (GUI.GetNameOfFocusedControl() == "player_name")
            {
                if (_player_name == _default) _player_name = "";
            }
            else
            {
                if (_player_name == "") _player_name = _default;
            }
        }
    }
}