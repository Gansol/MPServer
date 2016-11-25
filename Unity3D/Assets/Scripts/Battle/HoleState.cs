using UnityEngine;

public class HoleState : MonoBehaviour {

    public State holeState = State.Open;

    public   enum State   // 老鼠洞狀態
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
        if (name=="Hole5" && this.transform.childCount == 4)
        {
            holeState = State.Closed;
        }
        else if (this.transform.childCount == 2)
        {
            holeState = State.Closed;
        }
        else
        {
                holeState = State.Open;
        }
    }
}
