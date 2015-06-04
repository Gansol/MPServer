using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;


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
    [Range(2,5)]
    public float miceSize;
    private Vector3 _miceSize;

    void Start()
    {
        poolManger = GetComponent<PoolManager>();
        // StartCoroutine(SpawnBy1D(1, aLineL, 0.5f, 0.05f));
    }


    /// <summary>
    /// 1D 隨機產生。
    /// </summary>
    /// <param name="holeArray">1D陣列產生方式</param>
    /// <param name="spawnTime">產生間隔時間</param>
    public IEnumerator SpawnByRandom(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount,bool isSkill)
    {
        // < = > test OK
        int _tmpCount = 0;
        int[] _rndPos = new int[spawnCount];
        List<sbyte> _holeList = new List<sbyte>(holeArray);

        for (int i = 0; i < spawnCount; i++)
        {
            int rndNum = Random.Range(0, _holeList.Count);
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
       if(!isSkill)  Global.spawnFlag = true;
    }

    /// <summary>
    /// 正向產生。holeArray[]=1D陣列產生方式,spawnTime=產生間隔時間
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnBy1D(string miceName, sbyte[] holeArray, float spawnTime,float intervalTime, float lerpTime, int spawnCount, int randomPos , bool isSkill)
    {
        // < = > test OK
        int _tmpCount = 0;
        for (int item = 0; item < spawnCount; randomPos++)
        {
            if (randomPos / holeArray.Length == 1)   // =13 = 歸零
            {
                randomPos = 0;
            }

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
       if(!isSkill)  Global.spawnFlag = true;
        
    }

    /// <summary>
    /// 反向產生。holeArray[]=1D陣列產生方式,spawnTime=產生間隔時間
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator ReSpawnBy1D(string miceName, sbyte[] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos , bool isSkill)
    {
        // < = > test OK
        int _tmpCount = 0;
        for (int item = holeArray.Length; spawnCount > 0; randomPos--)  //1~11
        {
            yield return new WaitForSeconds(spawnTime);
            if (hole[randomPos - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
            {
                GameObject clone = poolManger.ActiveObject(miceName);
                if (clone != null)
                {
                    _miceSize = hole[randomPos - 1].transform.localScale / 10 * miceSize * hole[randomPos - 1].transform.localScale.x;
                    hole[randomPos - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                    clone.transform.parent = hole[randomPos - 1].transform;                      // hole[-1] 是因為起始值是0 hole[-1]
                    clone.transform.localPosition = Vector2.zero;
                    clone.transform.localScale = hole[randomPos - 1].transform.localScale - _miceSize;
                    clone.transform.GetChild(0).SendMessage("Play");
                    spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
                    _tmpCount++;
                }
                else
                {
                    Debug.Log("Object Pool hasn't Object");
                }
            }

            if (randomPos == 1)   // =13 = 歸零
            {
                randomPos = holeArray.Length;
            }

            if ((_tmpCount - spawnCount) == 0)
            {
                yield return new WaitForSeconds(intervalTime);
                goto Finish;
            }
        }
    Finish: ;
       if(!isSkill)  Global.spawnFlag = true;
        
    }


    /// <summary>
    /// 正向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnBy2D(string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos1, int randomPos2 , bool isSkill)
    {
        if (holeArray.GetLength(0) > 4)
            intervalTime /= 2;
        int _tmpCount = 0;
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


                            if ((_tmpCount - spawnCount) == 0)
                            {
                                goto Finish;
                            }
                            if (_tmpCount == holeArray.Length || i == holeArray.GetLength(0) - 1 && j == holeArray.GetLength(1)-1)
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
            //Debug.Log("intervalTime : "+intervalTime);
            yield return new WaitForSeconds(intervalTime/3);
        }
    Finish: ;
       if(!isSkill)  Global.spawnFlag = true;
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
    public IEnumerator ReSpawnBy2D(string miceName, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount, int randomPos1, int randomPos2 , bool isSkill)
    {
        // < = > test OK
        int _tmpCount = 0;

        if (holeArray.GetLength(0) > 4)
            intervalTime /= 2;
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


                            if ((_tmpCount - spawnCount) == 0)
                            {
                                goto Finish;
                            }
                            else if (_tmpCount == holeArray.Length || i==1 && j==1)
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
            yield return new WaitForSeconds(intervalTime/3);
        }
    Finish: ;
       if(!isSkill)  Global.spawnFlag = true;
    }



    /// <summary>
    /// 正向產生自訂方式。holeArray[][]=2D自訂陣列,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnByCustom(string miceName, sbyte[][] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount , bool isSkill)
    {
        // < = > test OK
        int _tmpCount = 0;
        int _tmpArrayLength = 0;

        if (holeArray.GetLength(0) > 4)
            intervalTime /= 2;

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
            yield return new WaitForSeconds(intervalTime/5);
        }
    Finish: ;   // When amount = 0 spawn Finish !
       if(!isSkill)  Global.spawnFlag = true;
    }

    /// <summary>
    /// 反向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator ReSpawnByCustom(string miceName, sbyte[][] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount , bool isSkill)
    {
        // < = > test ok
        int _tmpCount = 0;
        int _tmpArrayLength = 0;

        if (holeArray.GetLength(0) > 4)
            intervalTime /= 2;

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
            yield return new WaitForSeconds(intervalTime/5);
        }
    Finish: ;
        if(!isSkill)  Global.spawnFlag = true;
       
    }
}
