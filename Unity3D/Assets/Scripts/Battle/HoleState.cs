using MPProtocol;
using System;
using UnityEngine;

public class HoleState : MonoBehaviour
{
    private bool bBossSpawn;
    public State holeState = State.Open;
    BoxCollider collider;
    Mission mission;

    public enum State   // 老鼠洞狀態
    {
        Open,
        Closed,
        Event,
    }

    private void Awake()
    {
        Global.photonService.ApplyMissionEvent += OnApplyMission;
        Global.photonService.MissionCompleteEvent += OnMissionComplete;
    }

    private void OnMissionComplete(short missionScore)
    {
        if (bBossSpawn && gameObject.name == "Hole5")
        {
            bBossSpawn = !bBossSpawn;
            this.mission = Mission.None;
        }
    }

    private void OnApplyMission(Mission mission, Int16 missionScore)
    {
        Debug.Log("Hole OnApplyMission mission:" + mission);
        if (mission == Mission.WorldBoss)
        {
            Debug.Log(" mission:yes" );
            bBossSpawn =true;
            this.mission = Mission.WorldBoss;
        }
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
            else if (mission == Mission.WorldBoss)
            {
                Debug.Log(" mission:WorldBoss holeState = State.Event;");
                collider.enabled = true;
                holeState = State.Event;
            }
            else
            {
                collider.enabled = true;
                holeState = State.Open;
            }
        }
    }
}
