using UnityEngine;
using System.Collections;

public class TeamSwitchOLD : MonoBehaviour {

    [Range(0, 1)]
    public float lerpSpeed = 0.25f;
    public bool activeBtn;

    private GameObject _clone;
    private GameObject _other;
    private int _depth;

    private bool _isTrigger;
    private bool _goHome;
    private bool _isPress;
    private bool _destroy;

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
    }

    void Update()
    {
        if (transform.childCount != 0)
            activeBtn = true;

        if (Vector3.Distance(transform.localPosition, _toPos) < .5f && _other.tag == "MiceIcon")
        {
            transform.localPosition = _toPos;
            transform.GetChild(0).parent = _other.transform;
            _clone.name = gameObject.name;
            _clone.tag = gameObject.tag;
            _clone.GetComponent<MiceSwitch>().activeBtn = !activeBtn;
            _clone.GetComponent<UIDragObject>().enabled = false;
            _goHome = false;
            Destroy(gameObject);

            Debug.Log("GET IT!");
        }
        else if (Vector3.Distance(transform.localPosition, _toPos) < .5f && _goHome)
        {
            transform.localPosition = _toPos;
            _goHome = false;
        }

        if (Vector3.Distance(transform.localPosition, _toPos) < (_distance / 3) + 1 && _goHome && _destroy)
            Destroy(_clone);
        if (_goHome) LerpMotion(transform.localPosition, _toPos, lerpSpeed);
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.tag=="MiceIcon" && _isPress)
        {
            _other = other.gameObject;
            _toPos = other.transform.localPosition;
            _isTrigger = true;
            Debug.Log("Trigger! : " + gameObject.name + " " + _isTrigger);
        }
    }

    void OnTriggerExit(Collider other)
    {

        if (other.tag == "MiceIcon" && !_isPress)
        {
            _isTrigger = false;
            Debug.Log("Left! : " + gameObject.name + " " + _isTrigger);
        }
    }

    void Mice2Team()
    {
        Debug.Log("Hello");
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
            if (isPress )
            {
                TweenColor.Begin(this.gameObject, 0.1f, new Color(0.90f, 0.90f, 0.90f));
                _clone = (GameObject)Instantiate(this.gameObject);
                _clone.transform.parent = this.transform.parent;
                _originPos = _clone.transform.localPosition = this.transform.localPosition;
                _clone.transform.localScale = this.transform.localScale;
                _depth = ObjectManager.SwitchDepthLayer(gameObject, gameObject, Global.MeunObjetDepth + 1);
            }
            else if (_isTrigger && !isPress)    // 碰撞且放開時 交換物件
            {
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
                _toPos = _originPos;
                _goHome = _destroy = true;
                _distance = Vector3.Distance(transform.localPosition, _toPos);
                TweenColor.Begin(this.gameObject, 0.1f, new Color(0.95f, 0.95f, 0.95f));
                transform.GetChild(0).GetComponent<UISprite>().depth -= _depth; //有問題 UnityException: Transform child out of bounds
                if (transform.localPosition != _originPos)
                    Destroy(_clone);
                Debug.Log("return.");
            }
        }
    }

    void LerpMotion(Vector3 from, Vector3 to, float speed)
    {
        transform.localPosition = Vector3.Lerp(from, to, speed);
    }
}
