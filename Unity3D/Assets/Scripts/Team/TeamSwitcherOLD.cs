using UnityEngine;
using System.Collections;

public class TeamSwitcherOLD : MonoBehaviour
{
    [Range(0, 1)]
    public float lerpSpeed = 0.25f;
    public bool activeBtn;
    public float teamLeftDist = 10;
    private GameObject _clone;
    private GameObject _other;
    private int _depth;

    private bool _isTrigger;
    private bool _goHome;
    private bool _isPress;
    private bool _destroy;
    private bool _disable;  // set active disable

    private Vector3 _originPos;
    private Vector3 _toPos;

    private float _distance;

    // Use this for initialization
    void Start()
    {
        _originPos = transform.localPosition;
        _goHome = false;
        activeBtn = false;
        _other = gameObject;
        _disable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount != 0 && !_disable) activeBtn = true;

        if (Vector3.Distance(transform.localPosition, _toPos) < .5f && _other.tag == "TeamIcon")    // 有撞到TEAM當兩方距離幾乎等於0時 進行交換
        {
            Switch();
            Debug.Log("Swtich to Team.");
        }
        else if (Vector3.Distance(transform.localPosition, _toPos) < .5f && _goHome)    // *可能有BUG(其他tag)   沒撞到時，返回到原始位置
        {
            if (tag == "MiceIcon")
            {
                transform.localPosition = _toPos;
                _goHome = false;
            }
            else if (tag == "TeamIcon")
            {
                //to do team return mice.
            }
        }

        if (Vector3.Distance(transform.localPosition, _toPos) < (_distance / 3) + 1 && _goHome && _destroy) // 彈回時 <1/3 距離時 刪除clone eg:Mice彈回時、Mice彈至Team時、Team彈至Team時
            Destroy(_clone);
        if (_goHome) LerpMotion(transform.localPosition, _toPos, lerpSpeed);    // motion動畫
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.tag == "TeamIcon" && _isPress)    //按下並Trigger進入時且撞到Team 記錄雙方位置 _isTrigger = true
        {
            _other = other.gameObject;
            _toPos = other.transform.localPosition;
            _isTrigger = true;
            Debug.Log("Trigger! : " + gameObject.name + " " + _isTrigger);
        }
    }

    void OnTriggerExit(Collider other)  //放開並Trigger離開時且撞到Team _isTrigger = false
    {
        if (other.tag == "TeamIcon" && !_isPress)
        {
            _isTrigger = false;
            Debug.Log("Left! : " + gameObject.name + " " + _isTrigger);
        }
    }

    void Mice2Team()
    {
        Debug.Log("Hello");
        if (tag == "MiceIcon")
        {

        }
        else if (tag == "TeamIcon")
        {

        }
    }

    void OnHover(bool isOver)
    {
        if (activeBtn)
        {
            if (isOver && _originPos == transform.localPosition)
                TweenColor.Begin(this.gameObject, 0.1f, new Color(0.95f, 0.95f, 0.95f));
            else if (!isOver && _originPos == transform.localPosition)
                TweenColor.Begin(this.gameObject, 0.1f, Color.white);
        }
    }

    void OnPress(bool isPress)
    {
        if (activeBtn)
        {
            _isPress = isPress;

            if (isPress)
            {
                GetComponent<BoxCollider>().isTrigger = true;
                TweenColor.Begin(this.gameObject, 0.1f, new Color(0.90f, 0.90f, 0.90f));
                _clone = (GameObject)Instantiate(this.gameObject);
                _clone.transform.parent = this.transform.parent;
                _originPos = _clone.transform.localPosition = this.transform.localPosition;
                _clone.transform.localScale = this.transform.localScale;
                _depth = ObjectManager.SwitchDepthLayer(gameObject, gameObject, Global.MeunObjetDepth + 1);
            }
            else if (_isTrigger && !isPress)    // 碰撞且放開時 交換物件
            {
                GetComponent<BoxCollider>().isTrigger = false;
                _isTrigger = !_isTrigger;
                _goHome = true;
                _destroy = false;
                _distance = Vector3.Distance(transform.localPosition, _toPos);
                TweenColor.Begin(_clone, 0.1f, new Color(0.5f, 0.5f, 0.5f));
                transform.GetChild(0).GetComponent<UISprite>().depth -= _depth;
                Debug.Log("collision and switch.");
            }
            else if (!_isTrigger && !isPress)  // 無碰撞且放開時 返回
            {


                GetComponent<BoxCollider>().isTrigger = false;
                _toPos = _originPos;
                _goHome = _destroy = true;
                _distance = Vector3.Distance(transform.localPosition, _toPos);
                TweenColor.Begin(this.gameObject, 0.1f, new Color(0.95f, 0.95f, 0.95f));
                transform.GetChild(0).GetComponent<UISprite>().depth -= _depth; //有問題 UnityException: Transform child out of bounds
                if (transform.localPosition != _originPos)
                    Destroy(_clone);
                Debug.Log("return Mice.");
                if (tag == "TeamIcon")
                    ReturnMice();

            }
        }
    }

    void LerpMotion(Vector3 from, Vector3 to, float speed)
    {
        transform.localPosition = Vector3.Lerp(from, to, speed);
    }

    void Switch()
    {
        if (tag == "MiceIcon")
        {
            transform.localPosition = _toPos;
            transform.GetChild(0).parent = _other.transform;
            _clone.name = gameObject.name;
            _clone.tag = gameObject.tag;
            _goHome = false;
            _clone.GetComponent<TeamSwitcherOLD>()._disable = !_disable;
            _clone.GetComponent<TeamSwitcherOLD>().activeBtn = !activeBtn;
            _clone.GetComponent<UIDragObject>().enabled = false;
            Destroy(gameObject);
        }
        else if (tag == "TeamIcon")
        {
            // to do team switch
            Debug.Log("to do team switch.");
            if (transform.GetChild(0).name != _other.transform.GetChild(0).name)
            {
                transform.GetChild(0).parent = _other.transform.GetChild(0).parent;
                _other.transform.GetChild(0).parent = transform;
            }
            else
            {
                if (_clone != null)
                {
                    Destroy(_clone.GetComponent<TeamSwitcherOLD>());
                    Destroy(_clone);
                }
            }
        }

    }

    void ReturnMice()
    {
        Debug.Log("ReturnMice");
        _distance = Vector3.Distance(transform.localPosition, _toPos);
        if (_distance > teamLeftDist)
        {
            string miceName = "Mice" + gameObject.name.Remove(0, 4);
            GameObject miceObject = GameObject.Find(miceName).transform.gameObject;
            TweenColor.Begin(miceObject, lerpSpeed, Color.white);
            miceObject.GetComponent<TeamSwitcherOLD>()._disable = false;
            Destroy(gameObject);
        }
    }

}
