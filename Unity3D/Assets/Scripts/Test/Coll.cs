using UnityEngine;
using System.Collections;

public class Coll : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    public float minimum = 0.0F;
    public float maximum = 2000.0F;
    void Update()
    {
        transform.localPosition = new Vector3(0, Mathf.Lerp(minimum, maximum, 0.2f), 0);
    }
/*
    void RetOrigin()
    {
        if (_other)
            GetComponent<BoxCollider>().isTrigger = false;
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
        else if ((tag == "TeamIcon" && _distance <= teamLeftDist)) // here 返回原本位置
        {
            if (_other != null && _other != gameObject)
            {
                if (_other.tag == "TeamIcon")
                    Debug.Log("_distance <= teamLeftDist switch Team." + "   other:" + _other.name);
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
        else if (tag == "TeamIcon" && _distance > teamLeftDist)
        {  // 返回Mice

            if (_other.tag == "TeamIcon")
            {
                Debug.Log("_distance > teamLeftDist switch Team");
            }
            else
            {
                _other = gameObject;
                _miceTarget.GetComponent<UIDragObject>().enabled = true;
                _miceTarget.GetComponent<TeamSwitcher>().enabled = true;
                TweenColor.Begin(_miceTarget, tweenColorSpeed, Color.white);
                if (_clone != null)
                {
                    Destroy(_clone);
                    Destroy(transform.GetChild(0).gameObject);  // bug
                    gameObject.transform.localPosition = _originPos;
                }
            }

        }
    }
    */
}
