using UnityEngine;
using System.Collections;

public class BtnSwticher : MonoBehaviour
{

    private bool _isPress;

    enum enum_BtnMethod
    {
        Return = 0, // 返回原位
        Delete, // 移除Clone
        Switch, // 交換位置
        Add,    // 增加隊員
        Change, // 改變老鼠
    }


    void OnPress(bool press)
    {
        if (press)
        {
            GetComponent<BoxCollider>().isTrigger = true;
            _isPress = true;
        }
        else
        {
            GetComponent<BoxCollider>().isTrigger = false;
            _isPress = false;
        }

        Debug.Log(name);
    }

    void OnTriggerEnter(Collider col)
    {
        if (_isPress)
        {
            Debug.Log(col.name + "  Enter");
            transform.GetChild(0).GetComponent<UISprite>().spriteName = col.transform.GetChild(0).GetComponent<UISprite>().spriteName;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (_isPress)
        {
            Debug.Log(col.name + "  Stay");
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (_isPress)
        {
            Debug.Log(col.name + "  Exit");
        }
    }



    private void BtnMethod(enum_BtnMethod method)
    {

        switch (method)
        {
            case enum_BtnMethod.Return:
                {
                    break;
                }
            case enum_BtnMethod.Delete:
                {
                    break;
                }
            case enum_BtnMethod.Switch:
                {
                    break;
                }
            case enum_BtnMethod.Add:
                {
                    break;
                }
            case enum_BtnMethod.Change:
                {
                    break;
                }
            default:
                {
                    Debug.LogWarning("Btn Switch Bug!");
                    break;
                }
        }
    }


    private void ReturnOriginPosition()
    {

    }

    private void RemoveTeamMember()
    {

    }

    private void SwitchIconLocation()
    {

    }

    private void AddTeamMember()
    {

    }

    private void ChangeMember()
    {

    }
}
