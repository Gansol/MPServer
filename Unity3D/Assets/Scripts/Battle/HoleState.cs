using UnityEngine;

public class HoleState : MonoBehaviour
{
    public State holeState = State.Open;
    BoxCollider collider;

    public enum State   // 老鼠洞狀態
    {
        Open,
        Closed,
        Moving,
    }

    void Start()
    {
        collider = GetComponent<BoxCollider>();
        holeState = State.Open;
    }

    void Update()
    {

        if (gameObject.name != "Hole5")
        {
            if (transform.childCount > 1)
            {
                collider.enabled = false;
                holeState = State.Closed;
            }
            else
            {
                collider.enabled = true;
                holeState = State.Open;
            }
        }
        else
        {
            if (transform.childCount > 3)
            {
                collider.enabled = false;
                holeState = State.Closed;
            }
            else
            {
                collider.enabled = true;
                holeState = State.Open;
            }
        }
    }
}
