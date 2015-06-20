﻿using UnityEngine;
using System.Collections;
using MPProtocol;

public class PlayerInput : MonoBehaviour
{
    RaycastHit2D hit;
    Vector3 pos;
    MissionManager missionManager;

    // Use this for initialization
    void Start()
    {
        missionManager = GetComponent<MissionManager>();
    }

    // Update is called once per frame
    void Update()
    {


#if UNITY_EDITOR

        pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hit = Physics2D.Raycast(pos, Vector2.zero);
        if (hit && hit.collider != null)
        {

            if (Input.GetMouseButtonDown(0))
            {
                //            Debug.Log(hit.transform.name);
                if (hit.transform.name == "anims") hit.transform.SendMessage("OnHit");

                if (hit.transform.name == "Skill1" || hit.transform.name == "Skill2" || hit.transform.name == "Skill3" || hit.transform.name == "Skill4" || hit.transform.name == "Skill5")
                    hit.transform.SendMessage("OnHit");

                if (hit.transform.name == "Exit_Btn") Global.photonService.ExitRoom();
                    
            }
        }



#elif UNITY_ANDROID || UNITY_IPHONE

    if(Input.touchCount>0){

        for(int i=0; i<Input.touchCount; i++)
        {
            if(Input.GetTouch(0).phase == TouchPhase.Ended){
                pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                hit = Physics2D.Raycast(pos, Vector2.zero);

                if (hit && hit.collider != null)
                {
                    if (hit.transform.name == "anims")
                        hit.transform.SendMessage("OnHit");

                    if (hit.transform.name == "Skill1" || hit.transform.name == "Skill2" || hit.transform.name == "Skill3" || hit.transform.name == "Skill4" || hit.transform.name == "Skill5")
                        hit.transform.SendMessage("OnHit");

                    if (hit.transform.name == "Exit_Btn")
                     Global.photonService.ExitRoom();
                }
            }
        }
    }
                
#endif

    }
}
