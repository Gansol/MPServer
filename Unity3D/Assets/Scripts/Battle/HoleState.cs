using UnityEngine;

public class HoleState : MonoBehaviour
{
    public State holeState = State.Open;

    public enum State   // 老鼠洞狀態
    {
        Open,
        Closed,
        Moving,
    }

    void Awake()
    {
        holeState = State.Open;
    }

    void Update()
    {

        if (gameObject.name != "Hole5")
        {
            if (transform.childCount > 1)
            {
                GetComponent<BoxCollider>().enabled = false;
                holeState = State.Closed;
            }
            else
            {
                GetComponent<BoxCollider>().enabled = true;
                holeState = State.Open;
            }
        }
        else
        {
            if (transform.childCount > 3)
            {
                GetComponent<BoxCollider>().enabled = false;
                holeState = State.Closed;
            }
            else
            {
                GetComponent<BoxCollider>().enabled = true;
                holeState = State.Open;
            }
        }
    }
}
