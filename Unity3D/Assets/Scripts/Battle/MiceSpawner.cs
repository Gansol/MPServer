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
    private HoleState holeState;            // 地洞狀態



    void Start()
    {
        poolManger = GetComponent<PoolManager>();
        // StartCoroutine(SpawnBy1D(1, aLineL, 0.5f, 0.05f));
    }

    /// <summary>
    /// 正向產生。holeArray[]=1D陣列產生方式,spawnTime=產生間隔時間
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnBy1D(int miceID, sbyte[] holeArray, float spawnTime, float lerpTime, int spawnCount,int randomPos)
    {
        // < = > test OK
        int _tmpCount = 0;
        for (int item = randomPos; item < spawnCount; item++)
        {
            if (item / holeArray.Length == 1)   // =13 = 歸零
            {
                spawnCount -= item;
                item = 0;
            }
            yield return new WaitForSeconds(spawnTime);
            if (hole[holeArray[item] - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
            {
                GameObject clone = poolManger.ActiveObject(miceID);
                if (clone != null)
                {
                    hole[holeArray[item] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                    clone.transform.parent = hole[holeArray[item] - 1].transform;              // hole[-1]是因為起始值是0 
                    clone.transform.localPosition = Vector2.zero;
                    clone.transform.localScale = hole[holeArray[item] - 1].transform.localScale / 1.8f;
                    clone.transform.GetChild(0).SendMessage("Play");
                    spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
                    _tmpCount++;

                    if (_tmpCount - spawnCount == 0)
                    {
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
        Global.spawnFlag = true;
    }

    /// <summary>
    /// 反向產生。holeArray[]=1D陣列產生方式,spawnTime=產生間隔時間
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator ReSpawnBy1D(int miceID, sbyte[] holeArray, float spawnTime, float lerpTime, int spawnCount)
    {
        // < = > test OK
        int _tmpCount = 0;
        for (int item = holeArray.Length; spawnCount > 0; item--)
        {
            yield return new WaitForSeconds(spawnTime);
            if (hole[item - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
            {
                GameObject clone = poolManger.ActiveObject(miceID);
                if (clone != null)
                {
                    hole[item - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                    clone.transform.parent = hole[item - 1].transform;                      // hole[-1] 是因為起始值是0 hole[-1]
                    clone.transform.localPosition = Vector2.zero;
                    clone.transform.localScale = hole[item - 1].transform.localScale / 1.8f;
                    clone.transform.GetChild(0).SendMessage("Play");
                    spawnTime = Mathf.Lerp(spawnTime, 0f, lerpTime);
                    _tmpCount++;
                }
                else
                {
                    Debug.Log("Object Pool hasn't Object");
                }
            }

            if ((_tmpCount - spawnCount) == 0)
            {
                goto Finish;
            }
            if (item == 1)   // =13 = 歸零
            {
                _tmpCount = 0;
                spawnCount -= holeArray.Length;
                item = holeArray.Length + 1;    // +1是因為迴圈結束時 item-- 會少1 
            }
        }
    Finish: ;
        Global.spawnFlag = true;
    }


    /// <summary>
    /// 正向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnBy2D(int miceID, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount)
    {
        Debug.Log("IN SPAWN2D: " + intervalTime);
        // < = > test OK
        int _tmpCount = 0;
        for (int i = 0; i < holeArray.GetLength(0); i++)    // 1D陣列
        {
            for (int j = 0; j < holeArray.GetLength(1); j++)    // 2D陣列
            {
                if (spawnCount > 0)
                {
                    yield return new WaitForSeconds(spawnTime);
                    if (hole[holeArray[i, j] - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
                    {
                        GameObject clone = poolManger.ActiveObject(miceID);
                        if (clone != null)
                        {
                            hole[holeArray[i, j] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                            clone.transform.parent = hole[holeArray[i, j] - 1].transform;                           //hole[-1] 是因為起始值是0
                            clone.transform.localPosition = Vector2.zero;
                            clone.transform.localScale = hole[holeArray[i, j] - 1].transform.localScale / 1.8f;
                            clone.transform.GetChild(0).SendMessage("Play");
                            _tmpCount++;


                            if ((_tmpCount - spawnCount) == 0)
                            {
                                goto Finish;
                            }
                            if (_tmpCount == holeArray.Length)
                            {
                                _tmpCount = i = 0;
                                j = -1;                         // 因為 j++是迴圈跑完才會+1 所以在跑一次會變 0+1=1不是0 所以要=-1+1=0
                                spawnCount -= holeArray.Length;
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
            Debug.Log("intervalTime : "+intervalTime);
            yield return new WaitForSeconds(intervalTime);
        }
    Finish: ;
        Global.spawnFlag = true;
    }

    /// <summary>
    /// 反向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator ReSpawnBy2D(int miceID, sbyte[,] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount)
    {
        // < = > test OK
        int _tmpCount = 0;
        for (int i = holeArray.GetLength(0); i > 0; i--)    // 1D陣列
        {
            for (int j = holeArray.GetLength(1); j > 0; j--)    // 2D陣列
            {
                yield return new WaitForSeconds(spawnTime);
                if (hole[holeArray[i - 1, j - 1] - 1].GetComponent<HoleState>().holeState == HoleState.State.Open)
                {
                    if (spawnCount > 0)
                    {

                        GameObject clone = poolManger.ActiveObject(miceID);
                        if (clone != null)
                        {
                            hole[holeArray[i - 1, j - 1] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                            clone.transform.parent = hole[holeArray[i - 1, j - 1] - 1].transform;                       // hole[-1] 和 holeArray[-1,-1]是因為起始值是0 
                            clone.transform.localPosition = Vector2.zero;
                            clone.transform.localScale = hole[holeArray[i - 1, j - 1] - 1].transform.localScale / 1.8f;
                            clone.transform.GetChild(0).SendMessage("Play");
                            _tmpCount++;


                            if ((_tmpCount - spawnCount) == 0)
                            {

                                goto Finish;
                            }
                            else if (_tmpCount == holeArray.Length)
                            {
                                _tmpCount = 0;
                                i = holeArray.GetLength(0);
                                j = holeArray.GetLength(1) + 1;   // 因為 j--是迴圈跑完才會-1 所以在跑一次會變 holeArray.GetLength(1)-1 不是 holeArray.GetLength(1) 0 所以要+1
                                spawnCount -= holeArray.Length;
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
            yield return new WaitForSeconds(intervalTime);
        }
    Finish: ;
        Global.spawnFlag = true;
    }



    /// <summary>
    /// 正向產生自訂方式。holeArray[][]=2D自訂陣列,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator SpawnByCustom(int miceID, sbyte[][] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount)
    {
        // < = > test OK
        int _tmpCount = 0;
        int _tmpArrayLength = 0;

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
                        GameObject clone = poolManger.ActiveObject(miceID);

                        if (clone != null)
                        {
                            hole[holeArray[i][j] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                            clone.transform.parent = hole[holeArray[i][j] - 1].transform;                           //hole[-1] 是因為起始值是0
                            clone.transform.localPosition = Vector2.zero;
                            clone.transform.localScale = hole[holeArray[i][j] - 1].transform.localScale / 1.8f;
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
            yield return new WaitForSeconds(intervalTime);
        }
    Finish: ;   // When amount = 0 spawn Finish !
        Global.spawnFlag = true;
    }

    /// <summary>
    /// 反向產生。holeArray[,]=2D陣列產生方式,spawnTime=老鼠間隔時間,intervalTime=產生間隔
    /// </summary>
    /// <param name="holeArray"></param>
    /// <param name="spawnTime"></param>
    /// <returns></returns>
    public IEnumerator ReSpawnByCustom(int miceID, sbyte[][] holeArray, float spawnTime, float intervalTime, float lerpTime, int spawnCount)
    {
        // < = > test ok
        int _tmpCount = 0;
        int _tmpArrayLength = 0;

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
                    GameObject clone = poolManger.ActiveObject(miceID);
                    if (clone != null)
                    {
                        hole[holeArray[i - 1][j - 1] - 1].GetComponent<HoleState>().holeState = HoleState.State.Closed;
                        clone.transform.parent = hole[holeArray[i - 1][j - 1] - 1].transform;                       // hole[-1] 和 holeArray[-1][-1]是因為起始值是0 
                        clone.transform.localPosition = Vector2.zero;
                        clone.transform.localScale = hole[holeArray[i - 1][j - 1] - 1].transform.localScale / 1.8f;
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
            yield return new WaitForSeconds(intervalTime);
        }
    Finish: ;
        Global.spawnFlag = true;
    }
}
