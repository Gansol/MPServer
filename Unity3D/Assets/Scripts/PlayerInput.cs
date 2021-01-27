using UnityEngine;
using System.Collections;
using MPProtocol;

public class PlayerInput : MonoBehaviour
{
    MPGame m_MPGame;
    ICreature creature;
    RaycastHit2D raycast2D;
    Vector3 worldPos;
    Vector3 mousePos;
    //    float i = 0;

    private void Start()
    {
        m_MPGame = MPGame.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Global.ShowMessage("確定要離開遊戲嗎?", Global.MessageBoxType.YesNo, 0);
            Global.ExitGame();
        }

#if UNITY_EDITOR || UNITY_STANDALONE

        mousePos = Input.mousePosition;
        mousePos.z = 0.67f;

        worldPos = (Camera.main.orthographic) ? UICamera.mainCamera.ScreenToWorldPoint(Input.mousePosition) : UICamera.mainCamera.ScreenToWorldPoint(mousePos);
        raycast2D = (Camera.main.orthographic) ? Physics2D.Raycast(worldPos, Vector2.zero) : Physics2D.Raycast(worldPos, Vector2.zero);
        //        Debug.Log("pos:" + pos + "XX" + Input.mousePosition);

        if (raycast2D && raycast2D.collider != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Debug.Log("Hit" + hit.transform.name);

                // 如果 敲到老鼠
                if (raycast2D.transform.childCount != 0)
                {
                    if (raycast2D.transform.GetChild(0).name == "anims" || raycast2D.transform.name == "anims")
                    {
                        // 取得老鼠 觸發 死亡動畫
                        creature = m_MPGame.GetCreatureSystem().GetActiveHoleMice(raycast2D.transform.parent);
                        if(creature!=null)
                            creature.OnHit();
                    }
                }

                // 如果按下 技能按扭 觸發技能
                if (raycast2D.transform.parent.name == "Skill1" || raycast2D.transform.name == "Skill2" || raycast2D.transform.name == "Skill3" || raycast2D.transform.name == "Skill4" || raycast2D.transform.name == "Skill5")
                    raycast2D.transform.SendMessage("OnHit");

                // 如果按下 離開按扭 離開遊戲
                if (raycast2D.transform.name == "Exit_Btn")
                    Global.photonService.ExitRoom();
            }
        }



#elif UNITY_ANDROID || UNITY_IPHONE

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount && i <= 2; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Ended)
                {    // GetTouch(i) 越多 觸控點越多
                    worldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                    raycast2D = Physics2D.Raycast(worldPos, Vector2.zero);

                    if (raycast2D && raycast2D.collider != null)
                    {
                        if (raycast2D.transform.childCount != 0)
                        {
                            if (raycast2D.transform.GetChild(0).name == "anims" || raycast2D.transform.name == "anims")
                            {
                                creature = m_MPGame.GetCreatureSystem().GetActiveHoleMice(raycast2D.transform.parent);
                                if(creature!=null)
                                    creature.OnHit();
                                //raycast2D.transform.SendMessage("OnHit");
                            }
                          }

                        if (raycast2D.transform.name == "Skill1" || raycast2D.transform.name == "Skill2" || raycast2D.transform.name == "Skill3" || raycast2D.transform.name == "Skill4" || raycast2D.transform.name == "Skill5")
                            raycast2D.transform.SendMessage("OnHit");

                        if (raycast2D.transform.name == "Exit_Btn")
                            Global.photonService.ExitRoom();
                    }
                }
            }
        }

#endif

    }
}