using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using MPProtocol;
using System;

/*
 * 這腳本只負責生東西 時間和控制器要另外寫在Global
 * 現在是測試版 完成後要把Intantiate改成poolManager.ActiveObject()
 * 
 * 
 * 
 * 
 */

public class MiceSpawner : MonoBehaviour
{
    public GameObject[] hole;
    private PoolManager poolManger;
    [Range(2, 5)]
    public float miceSize;
    private Vector3 _miceSize;

    void Start()
    {
        poolManger = GetComponent<PoolManager>();
        Global.photonService.ApplyMissionEvent += OnApplyMission;
        Global.photonService.ExitRoomEvent += OnExitRoom;
    }


    /// <summary>
    /// 1D 隨機產生。
    /// </summary>
    /// <param name="holeArray">1D陣列產生方式</param>
    /// <param name="spawnTime">產生間隔時間</param>
    public IEnumerator SpawnByRandom(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
    {
        // < = > test OK
        int _tmpCount = 0;
        int[] _rndPos = new int[spawnCount];
        List<sbyte> _holeList = new List<sbyte>(holeArray);

        for (int i = 0; i < spawnCount; i++)
        {
            int rndNum = UnityEngine.Random.Range(0, _holeList.Count);
            _rndPos[i] = _holeList[rndNum];
            _holeList.RemoveAt(rndNum);
        }

        for (int item = 0; item < spawnCount; item++)
        {
            if (item / _rndPos.Length == 1)   // =13 = 歸零
            {
                spawnCount -= item;
                item = 0;
            }
            yield return new WaitForSeconds(spawnTime);
            if (hole[_rndPos[item] - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
            {
                GameObject clone = poolManger.ActiveObject(miceName);
                if (clone != null)
                {
                    _miceSize = hole[_rndPos[item] - 1].transform.localScale / 10 * miceSize * hole[_rndPos[item] - 1].transform.localScale.x;
                    hole[_rndPos[item] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                    clone.transform.parent = hole[_rndPos[item] - 1].transform;              // hole[-1]是因為起始值是0 
                    clone.transform.localPosition = Vector2.zero;
                    clone.transform.localScale = hole[_rndPos[item] - 1].transform.localScale - _miceSize;  // 公式 原始大小分為10等份 10等份在減掉 要縮小的等份*乘洞的倍率(1.4~0.9) => 1.0整份-0.2份*1(洞口倍率)=0.8份 
                    clone.transform.GetChild(0).SendMessage("Play");
                    spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
                    _tmpCount++;

                    if (_tmpCount - spawnCount == 0)
                    {
                        yield return new WaitForSeconds(intervalTime);
                        goto Finish;
                    }
                }
                else
                {
                    Debug.Log("Object Pool hasn't Object");
                }
            }
            else
            {
                Debug.Log("Closed!");
            }
        }
    Finish: ;
        if (!isSkill) Global.spawnFlag = true;
    }

    /// <summary>
    /// 正向產生。holeArray[]=1D陣列產生方式,spawnTime=產生間隔時間
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnBy1D(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos, bool isSkill)
    {
        int _tmpCount = 0;
        for (int item = 0; item < spawnCount; randomPos++)
        {
            if (randomPos / holeArray.Length == 1) randomPos = 0; // =12= 歸零

            yield return new WaitForSeconds(spawnTime);
            if (hole[holeArray[randomPos] - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
            {
                GameObject clone = poolManger.ActiveObject(miceName);
                if (clone != null)
                {
                    _miceSize = hole[holeArray[randomPos] - 1].transform.localScale / 10 * miceSize * hole[holeArray[randomPos] - 1].transform.localScale.x;
                    hole[holeArray[randomPos] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                    clone.transform.parent = hole[holeArray[randomPos] - 1].transform;              // hole[-1]是因為起始值是0 
                    clone.transform.localPosition = Vector2.zero;
                    clone.transform.localScale = hole[holeArray[randomPos] - 1].transform.localScale - _miceSize;
                    clone.transform.GetChild(0).SendMessage("Play");
                    spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
                    _tmpCount++;

                    if (_tmpCount - spawnCount == 0)
                    {
                        yield return new WaitForSeconds(intervalTime);
                        goto Finish;
                    }
                }
                else
                {
                    Debug.Log("Object Pool hasn't Object");
                }
            }
            else
            {
                Debug.Log("Closed!");
            }


        }
    Finish: ;
        if (!isSkill) Global.spawnFlag = true;

    }

    /// <summary>
    /// 反向產生。1D陣列產生方式
    /// </summary>
    /// <param name="miceName"></param>
    /// <param name="holeArray">1D陣列</param>
    /// <param name="spawnTime">產生間隔時間</param>
    /// <param name="intervalTime"></param>
    /// <param name="lerpTime"></param>
    /// <param name="spawnCount"></param>
    /// <param name="randomPos">隨機位置(最大值為陣列長度)</param>
    /// <param name="isSkill">是否為技能</param>
    /// <returns></returns>
    public IEnumerator ReSpawnBy1D(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos, bool isSkill)
    {
        int _tmpCount = 0;

        for (int item = randomPos; spawnCount > 0; item--)  //1~11
        {
            yield return new WaitForSeconds(spawnTime);
            if (hole[item - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
            {
                GameObject clone = poolManger.ActiveObject(miceName);
                if (clone != null)
                {
                    _miceSize = hole[item - 1].transform.localScale / 10 * miceSize * hole[item - 1].transform.localScale.x;
                    hole[item - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                    clone.transform.parent = hole[item - 1].transform;                      // hole[-1] 是因為起始值是0 hole[-1]
                    clone.transform.localPosition = Vector2.zero;
                    clone.transform.localScale = hole[item - 1].transform.localScale - _miceSize;
                    clone.transform.GetChild(0).SendMessage("Play");
                    spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
                    _tmpCount++;
                }
                else
                {
                    Debug.Log("Object Pool hasn't Object");
                }
            }

            if (item == 1) item = holeArray.Length; // =12 = 歸零

            if (_tmpCount - spawnCount == 0)
            {
                yield return new WaitForSeconds(intervalTime);
                goto Finish;
            }
        }
    Finish: ;
        if (!isSkill) Global.spawnFlag = true;

    }


    /// <summary>
    /// 正向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnBy2D(string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos1, int randomPos2, bool isSkill)
    {
        int _tmpCount = 0;

        if (holeArray.GetLength(0) >= 4)
        {
            intervalTime /= 3;
            lerpTime *= 2;
            spawnTime /= 2;
        }

        for (int i = randomPos1; i < holeArray.GetLength(0); i++)    // 1D陣列
        {
            for (int j = randomPos2; j < holeArray.GetLength(1); j++)    // 2D陣列
            {
                if (spawnCount > 0)
                {
                    yield return new WaitForSeconds(spawnTime);
                    if (hole[holeArray[i, j] - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
                    {
                        GameObject clone = poolManger.ActiveObject(miceName);
                        if (clone != null)
                        {
                            _miceSize = hole[holeArray[i, j] - 1].transform.localScale / 10 * miceSize * hole[holeArray[i, j] - 1].transform.localScale.x;
                            hole[holeArray[i, j] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                            clone.transform.parent = hole[holeArray[i, j] - 1].transform;                           //hole[-1] 是因為起始值是0
                            clone.transform.localPosition = Vector2.zero;
                            clone.transform.localScale = hole[holeArray[i, j] - 1].transform.localScale - _miceSize;
                            clone.transform.GetChild(0).SendMessage("Play");
                            _tmpCount++;

                            if (_tmpCount - spawnCount == 0) goto Finish;

                            if (_tmpCount == holeArray.Length || i == holeArray.GetLength(0) - 1 && j == holeArray.GetLength(1) - 1)
                            {
                                spawnCount -= _tmpCount;
                                _tmpCount = i = 0;
                                j = -1;                         // 因為 j++是迴圈跑完才會+1 所以在跑一次會變 0+1=1不是0 所以要=-1+1=0
                            }
                        }
                        else
                        {
                            Debug.Log("Object Pool hasn't Object");
                        }
                    }
                }
            }
            intervalTime = Mathf.Lerp(intervalTime, 0f, lerpTime);
            yield return new WaitForSeconds(intervalTime / 3);
        }
    Finish: ;
        if (!isSkill) Global.spawnFlag = true;
    }

    /// <summary>
    /// 反向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="miceName">老鼠ID</param>
    /// <param name="holeArray">產生陣列</param>
    /// <param name="spawnTime">產生老鼠間隔</param>
    /// <param name="intervalTime">間隔時間</param>
    /// <param name="lerpTime">加速度</param>
    /// <param name="spawnCount">產生數量</param>
    /// <param name="randomPos1">1D陣列隨機值</param>
    /// <param name="randomPos2">2D陣列隨機值</param>
    /// <returns></returns>
    public IEnumerator ReSpawnBy2D(string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos1, int randomPos2, bool isSkill)
    {
        int _tmpCount = 0;

        if (holeArray.GetLength(0) >= 4)
        {
            intervalTime /= 3;
            lerpTime *= 2;
            spawnTime /= 2;
        }

        for (int i = randomPos1; i > 0; i--)    // 1D陣列
        {
            for (int j = randomPos2; j > 0; j--)    // 2D陣列
            {
                yield return new WaitForSeconds(spawnTime);
                if (hole[holeArray[i - 1, j - 1] - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
                {
                    if (spawnCount > 0)
                    {
                        GameObject clone = poolManger.ActiveObject(miceName);
                        if (clone != null)
                        {
                            _miceSize = hole[holeArray[i - 1, j - 1] - 1].transform.localScale / 10 * miceSize * hole[holeArray[i - 1, j - 1] - 1].transform.localScale.x;
                            hole[holeArray[i - 1, j - 1] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                            clone.transform.parent = hole[holeArray[i - 1, j - 1] - 1].transform;                       // hole[-1] 和 holeArray[-1,-1]是因為起始值是0 
                            clone.transform.localPosition = Vector2.zero;
                            clone.transform.localScale = hole[holeArray[i - 1, j - 1] - 1].transform.localScale - _miceSize;
                            clone.transform.GetChild(0).SendMessage("Play");
                            _tmpCount++;

                            if (_tmpCount - spawnCount == 0)
                            {
                                goto Finish;
                            }
                            else if (_tmpCount == holeArray.Length || i == 1 && j == 1)
                            {
                                spawnCount -= _tmpCount;
                                _tmpCount = 0;
                                i = holeArray.GetLength(0);
                                j = holeArray.GetLength(1) + 1;   // 因為 j--是迴圈跑完才會-1 所以在跑一次會變 holeArray.GetLength(1)-1 不是 holeArray.GetLength(1) 0 所以要+1
                            }
                        }
                        else
                        {
                            Debug.Log("Object Pool hasn't Object");
                        }
                    }
                }
            }
            intervalTime = Mathf.Lerp(intervalTime, 0f, lerpTime);
            yield return new WaitForSeconds(intervalTime / 3);
        }
    Finish: ;
        if (!isSkill) Global.spawnFlag = true;
    }



    /// <summary>
    /// 正向產生自訂方式。holeArray[][]=2D自訂陣列,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnByCustom(string miceName, sbyte[][] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
    {
        int _tmpCount = 0;
        int _tmpArrayLength = 0;

        if (holeArray.GetLength(0) >= 4)
        {
            intervalTime /= 2;
            lerpTime *= 2;
            spawnTime /= 3;
        }

        foreach (sbyte[] item in holeArray)
        {
            _tmpArrayLength += item.Length;
        }

        for (int i = 0; i < holeArray.GetLength(0); i++)    // 1D陣列
        {
            for (int j = 0; j < holeArray[i].Length; j++)    // 2D陣列
            {
                if (spawnCount > 0)
                {
                    yield return new WaitForSeconds(spawnTime);
                    if (hole[holeArray[i][j] - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
                    {
                        GameObject clone = poolManger.ActiveObject(miceName);

                        if (clone != null)
                        {
                            _miceSize = hole[holeArray[i][j] - 1].transform.localScale / 10 * miceSize * hole[holeArray[i][j] - 1].transform.localScale.x;
                            hole[holeArray[i][j] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                            clone.transform.parent = hole[holeArray[i][j] - 1].transform;                           //hole[-1] 是因為起始值是0
                            clone.transform.localPosition = Vector2.zero;
                            clone.transform.localScale = hole[holeArray[i][j] - 1].transform.localScale - _miceSize;
                            clone.transform.GetChild(0).SendMessage("Play");
                            _tmpCount++;

                            if ((_tmpCount - spawnCount) == 0)
                            {
                                goto Finish;
                            }
                            else if (_tmpCount == _tmpArrayLength)
                            {
                                spawnCount -= _tmpCount;
                                _tmpCount = i = 0;
                                j = -1;
                            }
                        }
                        else
                        {
                            Debug.Log("Object Pool hasn't Object");
                        }
                    }
                }
            }
            intervalTime = Mathf.Lerp(intervalTime, 0f, lerpTime);
            yield return new WaitForSeconds(intervalTime / 5);
        }
    Finish: ;   // When amount = 0 spawn Finish !
        if (!isSkill) Global.spawnFlag = true;
    }

    /// <summary>
    /// 反向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator ReSpawnByCustom(string miceName, sbyte[][] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, bool isSkill)
    {
        int _tmpCount = 0;
        int _tmpArrayLength = 0;

        if (holeArray.GetLength(0) >= 4)
        {
            intervalTime /= 2;
            lerpTime *= 2;
            spawnTime /= 3;
        }

        foreach (sbyte[] item in holeArray)
        {
            _tmpArrayLength += item.Length;
        }


        for (int i = holeArray.GetLength(0); i > 0; i--)    // 1D陣列
        {
            for (int j = holeArray[i - 1].Length; j > 0; j--)    // 2D陣列      holeArray[i-1]是因為起始值0
            {
                yield return new WaitForSeconds(spawnTime);
                if (hole[holeArray[i - 1][j - 1] - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
                {
                    GameObject clone = poolManger.ActiveObject(miceName);
                    if (clone != null)
                    {
                        _miceSize = hole[holeArray[i - 1][j - 1] - 1].transform.localScale / 10 * miceSize * hole[holeArray[i - 1][j - 1] - 1].transform.localScale.x;
                        hole[holeArray[i - 1][j - 1] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                        clone.transform.parent = hole[holeArray[i - 1][j - 1] - 1].transform;                       // hole[-1] 和 holeArray[-1][-1]是因為起始值是0 
                        clone.transform.localPosition = Vector2.zero;
                        clone.transform.localScale = hole[holeArray[i - 1][j - 1] - 1].transform.localScale - _miceSize;
                        clone.transform.GetChild(0).SendMessage("Play");
                        _tmpCount++;

                        if ((_tmpCount - spawnCount) == 0)
                        {
                            goto Finish;
                        }
                        else if (_tmpCount == _tmpArrayLength)
                        {
                            spawnCount -= _tmpCount;
                            _tmpCount = 0;
                            i = holeArray.GetLength(0);
                            j = holeArray[i - 1].Length + 1;
                        }
                    }
                    else
                    {
                        Debug.Log("Object Pool hasn't Object");
                    }
                }
            }
            intervalTime = Mathf.Lerp(intervalTime, 0f, lerpTime);
            yield return new WaitForSeconds(intervalTime / 5);
        }
    Finish: ;
        if (!isSkill) Global.spawnFlag = true;
    }

    void Update()
    {
        /*
        Animator anims = hole[4].GetComponent<Animator>() as Animator;
        AnimatorStateInfo animState = anims.GetCurrentAnimatorStateInfo(0);
        if (animState.nameHash == Animator.StringToHash("Layer1.HoleScale"))
        {
            if (animState.normalizedTime > 1)
            {

            }
            }
         * */
    }

    public void SpawnBoss(string miceName,float hp){

        if (hole[4].GetComponent<HoleState>().holeState == HoleState.State.Closed)
        {
            hole[4].transform.GetChild(2).GetChild(0).SendMessage("OnDied", 0.0f);
        }
        hole[4].GetComponent<Animator>().enabled = true;
        hole[4].GetComponent<Animator>().Play("HoleScale");
        
        // 要等待 動畫結速再出現
        GameObject clone = poolManger.ActiveObject(miceName);
        clone.transform.parent = hole[4].transform;
        clone.transform.localScale = new Vector3(1.2f, 1.2f, 0f);
        clone.transform.localPosition = new Vector3(0,0,0);
        clone.transform.GetChild(0).SendMessage("AsBoss", true);
        clone.transform.GetChild(0).transform.gameObject.AddComponent<BossPorperty>();
        
        clone.transform.GetChild(0).transform.GetComponent<BossPorperty>().hp = hp;
        clone.transform.GetChild(0).transform.GetComponent<BossPorperty>().hpMax = hp;
    }


    void OnApplyMission(Mission mission, Int16 missionScore)
    {
        if (mission == Mission.WorldBoss)
        {
            SpawnBoss("EggMice", missionScore);    //missionScore這裡是HP
        }
    }

    void OnExitRoom()
    {
        Global.photonService.ApplyMissionEvent -= OnApplyMission;
        Global.photonService.ExitRoomEvent -= OnExitRoom;
    }
}
