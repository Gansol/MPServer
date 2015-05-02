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
        if (this.transform.childCount == 3)
        {
            holeState = State.Closed;
        }
        else
        {
            holeState = State.Open;
        }
    }
}
