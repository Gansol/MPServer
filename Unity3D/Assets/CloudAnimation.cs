using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudAnimation : MonoBehaviour
{
    public GameObject endPos, startPos;
    [Range(0.1f, 2)]
    public float speed;
    public int maxSpeed;

    private float _speed;

    Vector3 _startRange;

    void Start()
    {
        float distance = Vector3.Distance(endPos.transform.localPosition, transform.localPosition);
        float distance2 = Vector3.Distance(endPos.transform.localPosition, transform.localPosition + Vector3.one);


        speed = (distance > distance2) ? speed : -speed;

        _startRange = startPos.GetComponent<BoxCollider>().size;
        _speed = Random.Range(1 * speed, maxSpeed * speed);


    }

    private void init()
    {
        float x = startPos.transform.localPosition.x + Random.Range(-_startRange.x, _startRange.x);
        float y = startPos.transform.localPosition.y + Random.Range(-_startRange.y, _startRange.y);
        transform.localPosition = new Vector3(x, y);
    }

    void Update()
    {
        transform.localPosition = transform.localPosition + new Vector3(_speed, 0);
            lastTime = Time.time;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject == endPos) init();    
    }

    public float lastTime { get; set; }
}
