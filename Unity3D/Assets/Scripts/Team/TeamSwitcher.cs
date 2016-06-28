using UnityEngine;
using System.Collections;

// 錯的很嚴重 應改用SendMessage 太多GetComponet了
// 233行開始全錯
public class TeamSwitcher : MonoBehaviour
{
    [Range(0, 1)]
    public float lerpSpeed = 0.25f;
    [Range(0, 1)]
    public float tweenColorSpeed = 0.1f;
    [Range(0, 1)]
    public float hoverColor = 0.95f;
    [Range(0, 1)]
    public float clickColor = 0.85f;
    [Range(0, 1)]
    public float disableColor = 0.5f;
    public float teamLeftDist = 3f;
    public bool _activeBtn;

    private GameObject _clone, _other, _miceTarget;
    private int _depth;
    private float _distance;
    private bool _isTrigger, _goTarget, _isPress, _destroy;
    private Vector3 _originPos, _toPos;

    // Use this for initialization
    void Start()
    {
        _originPos = transform.localPosition;
        _goTarget = false;
        _activeBtn = true;
        _other = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount == 0)
        {
            GetComponent<TeamSwitcher>().enabled = false;
            GetComponent<UIDragObject>().enabled = false;
        }

        if (_other != null)
        {
            if (Vector3.Distance(transform.localPosition, _toPos) < 3f && _goTarget && _other.tag == "TeamIcon" && tag == "MiceIcon")
            {
                transform.localPosition = _toPos;
                transform.GetChild(0).parent = _other.transform;
                _other.GetComponent<TeamSwitcher>().enabled = true;
                _other.GetComponent<UIDragObject>().enabled = true;
                _clone.GetComponent<BoxCollider>().enabled = true;
                _clone.GetComponent<BoxCollider>().isTrigger = false;
                _clone.name = gameObject.name;
                _clone.tag = gameObject.tag;
                _goTarget = false;
                Destroy(gameObject);
            }
            if (Vector3.Distance(transform.localPosition, _toPos) < 3f && _goTarget && _other.tag == "TeamIcon")
            {
                // team to team 會錯
                transform.localPosition = _toPos;
                _originPos = _toPos;
                _goTarget = false;
            }
            if (Vector3.Distance(transform.localPosition, _toPos) < 3f && _goTarget)
            {
                transform.localPosition = _toPos;
                _originPos = _toPos;
                _goTarget = false;
            }
        }
        if (Vector3.Distance(transform.localPosition, _toPos) <= (_distance / 3) + 1 && _goTarget && _destroy) Destroy(_clone);
        if (_goTarget)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _toPos, lerpSpeed);
            Debug.Log("myName:" + name + "Pos:" + _originPos);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isPress && other != gameObject)
        {
            _other = other.gameObject;
            _toPos = other.transform.localPosition;
            _isTrigger = true;
            Debug.Log("Trigger! : " + gameObject.name + " " + _isTrigger + "   other:" + _other.name);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.isTrigger == false)
            _other = other.gameObject;
        Debug.Log("OnTriggerStay:" + other.name + "  myName:" + name);
    }

    void OnTriggerExit(Collider other)
    {
        _isTrigger = false;
        _other = gameObject;    // team to team 會錯
        // Debug.Log("TriggerExit:" + _other.name);
    }

    void OnHover(bool isOver)
    {
        if (_activeBtn)
        {
            if (isOver)
                TweenColor.Begin(this.gameObject, tweenColorSpeed, new Color(0.9f, 0.9f, 0.9f));
            else if (!isOver)
                TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
        }
    }

    void OnPress(bool isPress)
    {
        if (_activeBtn)
        {
            _isPress = isPress;

            if (isPress)
            {
                GetComponent<BoxCollider>().isTrigger = true;
                TweenColor.Begin(gameObject, tweenColorSpeed, new Color(clickColor, clickColor, clickColor));
                Move2Clone();
            }
            else if (_isTrigger && !isPress && _other != gameObject && _other != null)    // 碰撞且放開時 交換物件
            {
                if (_other.name != "_clone")    // 等等可以把 Switcher裡全部 _other.name != "_clone" 砍掉
                    Switcher();
                else
                    Debug.LogError("ERROR: isTrigger bug 01.");
                Debug.Log("Collision");
            }
            else if (!_isTrigger && !isPress && _other != gameObject && _other.tag == "TeamIcon" && _other != null)    // 碰撞且放開時 交換物件
            {
                if (_other.name != "_clone")    // 等等可以把 Switcher裡全部 _other.name != "_clone" 砍掉
                    Switcher();
                else
                    RetOrigin();
                Debug.Log("Collision");
            }
            else if (!_isTrigger && !isPress)  // 無碰撞且放開時 返回
            {
                RetOrigin();
                Debug.Log("Not collision");
            }
            else
            {
                Debug.LogError("ERROR: Update Trigger bug 02.");
            }
        }
    }

    void RetOrigin()
    {
        _toPos = _originPos;
        _distance = Vector3.Distance(transform.localPosition, _toPos);

        if (tag == "MiceIcon")
        {
            _other = gameObject;
            _goTarget = _destroy = true;
            TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
            transform.GetChild(0).GetComponent<UISprite>().depth -= _depth; //有問題 UnityException: Transform child out of bounds
            Debug.Log("return.");
        }
        else if (tag == "TeamIcon")
        {
            if (_distance <= teamLeftDist)
            {
                if (_other.tag == "TeamIcon" && _other != gameObject && _other.transform.childCount != 0)
                {
                    Debug.Log("to do switch  B1>B2");
                    // to do switch  B1>B2
                }
                else
                {
                    _other = gameObject;
                    _goTarget = _destroy = true;
                    TweenColor.Begin(this.gameObject, tweenColorSpeed, Color.white);
                    transform.GetChild(0).GetComponent<UISprite>().depth -= _depth; //有問題 UnityException: Transform child out of bounds
                    Debug.Log("return.");
                }
            }
            else if (_distance > teamLeftDist)
            {
                if (_other.tag == "TeamIcon" && _other != gameObject && _other.transform.childCount != 0)
                {
                    Debug.Log("to do switch  B1>B2");
                    // to do switch  B1>B2
                }
                else
                {
                    _other = gameObject;

                    TweenColor.Begin(_miceTarget, tweenColorSpeed, Color.white);
                    _miceTarget.GetComponent<TeamSwitcher>().enabled = true;
                    _miceTarget.GetComponent<UIDragObject>().enabled = true;
                    _miceTarget.GetComponent<TeamSwitcher>()._activeBtn = true;
                    transform.GetChild(0).GetComponent<UISprite>().depth -= _depth; //有問題 UnityException: Transform child out of bounds
                    Debug.Log("return. Pos: " + _originPos);
                    gameObject.GetComponent<BoxCollider>().isTrigger = false;
                    gameObject.transform.localPosition = _originPos;
                    Destroy(transform.GetChild(0).gameObject);
                    Destroy(_clone);
                }
            }
        }
    }

    void Switcher()
    {
        if (_other != null)     // switch
        {
            if (tag == "MiceIcon" && _other.tag == "TeamIcon" && _other != gameObject)    // A > B
            {
                // 如果移動到Team的位置"沒有"Mice 移至Team
                _other.GetComponent<TeamSwitcher>()._miceTarget = _clone;
                GetComponent<BoxCollider>().isTrigger = false;
                _isTrigger = !_isTrigger;
                _goTarget = true;
                _destroy = false;
                _clone.GetComponent<TeamSwitcher>()._activeBtn = false;     // 灰色clone mice按鈕失效

                _distance = Vector3.Distance(transform.localPosition, _toPos);
                TweenColor.Begin(_clone, tweenColorSpeed, new Color(disableColor, disableColor, disableColor));
                transform.GetChild(0).GetComponent<UISprite>().depth -= _depth; // 恢復深度值

                if (_other.transform.childCount != 0)   // 如果移動到Team的位置有Mice移至Team Team老鼠返回
                {
                    Destroy(_other.transform.GetChild(0).gameObject);               // 錯的很嚴重 應改用SendMessage 太多GetComponet了
                    TweenColor.Begin(_other.GetComponent<TeamSwitcher>()._miceTarget, tweenColorSpeed, Color.white);
                    _other.GetComponent<TeamSwitcher>()._miceTarget.GetComponent<TeamSwitcher>().enabled = true;
                    _other.GetComponent<TeamSwitcher>()._miceTarget.GetComponent<UIDragObject>().enabled = true;
                    _other.GetComponent<TeamSwitcher>()._miceTarget.GetComponent<TeamSwitcher>()._activeBtn = true;
                }

            }
            else if (tag == "TeamIcon" && _other.tag == "TeamIcon") // B1 > B2
            {
                if (_other.transform.childCount == 1)  // 有物件作交換
                {
                    if (_other.name != "_clone") // if B1 != B1 Clone
                    {
                        /*
                        transform.GetChild(0).parent = _other.transform;
                        _other.transform.GetChild(0).parent = transform;
                        transform.GetChild(0).localPosition = Vector3.zero;
                        _other.transform.GetChild(0).localPosition = Vector3.zero;
                         * */
                        string tmpName;
                        _other.GetComponent<TeamSwitcher>()._toPos = _originPos;
                        _toPos = _other.transform.localPosition;
                        _other.GetComponent<TeamSwitcher>()._goTarget = true;
                        transform.GetChild(0).GetComponent<UISprite>().depth -= _depth;
                        tmpName = _other.name;
                        _other.name = name;
                        name = tmpName;
                        _goTarget = true;
                        Destroy(_clone);
                    }
                }
                else
                {
                    RetOrigin();
                }
            }
            else
            {
                RetOrigin();
            }
        }
    }

    void Move2Clone()   // 當按下且移動時，產生Clone
    {
        _clone = (GameObject)Instantiate(gameObject);
        _clone.GetComponent<TeamSwitcher>().enabled = false;
        _clone.GetComponent<UIDragObject>().enabled = false;
        _clone.GetComponent<BoxCollider>().enabled = false;
        _clone.name = "_clone";
        _clone.transform.parent = transform.parent;
        _clone.transform.localPosition = _originPos;
        _clone.transform.localScale = transform.localScale;
        _depth = ObjectManager.SwitchDepthLayer(gameObject, gameObject, Global.MeunObjetDepth); // 移動時深度提到最高防止遮擋
        // 還要設定無作用 與 恢復作用
    }

    void TeamQueue()
    {
        // B1 B2 B3. if B2 remove. B3 to B2
    }

    void RetOrSwitch()
    {

    }
}
