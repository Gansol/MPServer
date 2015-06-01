using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using MiniJSON;

public class PoolManager : MonoBehaviour
{
    public static int acitveCount = 0;

    private Dictionary<string, List<GameObject>> dictObjectPool;
    private Dictionary<int, string> dictMice;
    private HashSet<int> _myMice;
    private HashSet<int> _otherMice;
    private List<GameObject> Pool;
    private GameObject clone;
    private int _miceCount;
    private byte miceID;
    private string miceName;
    private int[] _miceIDArray;
    private GameObject[] _dynamicPoolName;
    private int index;      // _dynamicPoolName index
    private static int miceAllCount;

    public GameObject Panel;
    public GameObject ObjectPool;
    public GameObject[] miceDeck;
    public int spawnCount = 5;
    public int clearLimit = 5;
    public int reserveCount = 2;
    public int objHeight = 0;
    public int objWidth = 0;

    public bool mergeFlag = false;       // 合併老鼠完成
    public bool poolingFlag = false;      // 初始化物件池

    void Awake()
    {

    }

    void Start()
    {
        miceAllCount = 0;
        poolingFlag = false;      // 初始化物件池
        Debug.Log("START!!!!!!!!!!!!!!!!");
        Pool = new List<GameObject>();
        dictObjectPool = new Dictionary<string, List<GameObject>>();
        dictMice = new Dictionary<int, string>();
        _miceCount = 0;
        _myMice = new HashSet<int>();
        _otherMice = new HashSet<int>();
        index = 0;
        MergeMice();    // 將雙方的老鼠合併 剔除相同的老鼠 ＊＊＊＊＊＊這裡測試時才注掉　要還原
        _dynamicPoolName = new GameObject[dictMice.Count];
        // 生出 預設數量的物件
        foreach (KeyValuePair<int, string> item in dictMice)
        {
            clone = new GameObject();
            clone.name = item.Value;
            clone.transform.parent = ObjectPool.transform;
            clone.transform.localScale = Vector3.one;
            _dynamicPoolName[index] = clone;

            for (int i = 1; i <= spawnCount; i++)
            {
                clone = (GameObject)Instantiate(miceDeck[item.Key - 1]);   //等傳老鼠ID名稱近來這要改
                clone.name = item.Value + i.ToString();//等傳老鼠ID名稱近來這要改
                clone.transform.parent = ObjectPool.transform.FindChild(item.Value).transform;
                clone.transform.localScale = Vector3.one;
                clone.GetComponent<UISprite>().width = objWidth;
                clone.GetComponent<UISprite>().height = objHeight;
                clone.tag = "EggMice";
                Pool.Add(clone);
                clone.SetActive(false);
                _miceCount = i;
            }
            dictObjectPool.Add(item.Value, Pool);
            index++;
        }
        /*
        for (int i = 0; i < Pool.Count; i++)
        {
            Pool[i].SetActive(false);
            // 地一個被設定ACTIVE 應該多了1 然後 POSION錯誤
        }
       */
        poolingFlag = true;
    }


    /// <summary>
    /// 每一次顯示一個GameObject。如果GameObject不足，Spawn一個物件並顯示。回傳 ( GameObject / null )
    /// </summary>
    /// <param name="miceID"></param>
    /// <returns></returns>
    public GameObject ActiveObject(int miceID)
    {
        dictMice.TryGetValue(miceID, out miceName);//等傳老鼠ID名稱近來這要改miceName

        if (ObjectPool.transform.FindChild(miceName).childCount == 0)
        {
            clone = (GameObject)Instantiate(miceDeck[miceID - 1], Vector3.zero, Quaternion.identity);
            clone.name = miceName + (_miceCount+1).ToString();
            clone.transform.parent = ObjectPool.transform.FindChild(miceName).transform;
            clone.transform.localScale = Vector3.one;
            clone.GetComponent<UISprite>().width = 260;
            clone.GetComponent<UISprite>().height = 290;

            dictObjectPool[miceName].Insert(dictObjectPool[miceName].Count, clone);     // 把新的物件加入字典的最後面(dictObjectPool[miceName].Count)

            _miceCount++;
            acitveCount++;
            Debug.Log("FUCK!");
            return clone;
        }

        for (int i = 1; i <= dictObjectPool[miceName].Count; i++)
        {
            GameObject mice;
            Debug.Log(i);
            mice = dictObjectPool[miceName].Find(delegate(GameObject obj) { return obj.name == miceName + i.ToString(); });
            if (mice.name == (miceName + i.ToString()) && !mice.activeInHierarchy)//等傳老鼠ID名稱近來這要改 nicename
            {
                mice.SetActive(true);
                acitveCount++;
                return mice;
            }

        }

        Debug.Log("miceAllCount : "+miceAllCount+"miceName : " + miceName + " acitveCount : " + acitveCount + "FindGameObjectsWithTag(miceName).Length : " + GameObject.FindGameObjectsWithTag(miceName).Length);





        Debug.Log("_miceCount "+_miceCount);
        return null;
    }


    public void MergeMice()
    {
        Dictionary<int, string> _tmpDict = new Dictionary<int, string>();
        //_tmpDict = Json.Deserialize(Global.Team) as Dictionary<int, string>;

        _tmpDict.Add(1, "EggMice");
        _tmpDict.Add(2, "BggMice");
        //把自己的老鼠存入HashSet中等待比較，再把老鼠存入合併好的老鼠Dict中
        foreach (KeyValuePair<int, string> item in _tmpDict)
        {
            _myMice.Add(item.Key);
            dictMice.Add(item.Key, item.Value);
        }

        //_tmpDict = Json.Deserialize(Global.OtherData.Team) as Dictionary<int, string>;
        _tmpDict.Add(3, "CggMice");
        //把對手的老鼠存入HashSet中等待比較
        foreach (KeyValuePair<int, string> item in _tmpDict)
        {
            _otherMice.Add(item.Key);
        }

        _otherMice.ExceptWith(_myMice); // 把對手重複的老鼠丟掉

        if (_otherMice.Count != 0)      // 如果對手老鼠有不重複的話
        {
            foreach (int item in _otherMice)    // 加入合併好的老鼠Dict
            {
                string miceName;
                _tmpDict.TryGetValue(item, out miceName);

                dictMice.Add(item, miceName);
            }
        }
        mergeFlag = true;
        Debug.Log("miceProperty.Count" + dictMice.Count);
    }



    void Update()
    {
        
        foreach (GameObject item in _dynamicPoolName)       // 跑遍動態池
        {
            
            if (ObjectPool.transform.FindChild(item.name).childCount > clearLimit)
            {
                Debug.Log("AYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY");
                // int count = Pool.FindAll(delegate(GameObject obj) { return obj.name == item; }).Count;   // 很酷的寫法

                foreach (KeyValuePair<string, List<GameObject>> item2 in dictObjectPool)
                {
                    item2.Value.RemoveRange(0, item2.Value.Count - reserveCount);
                    Debug.Log("Pool.Count : " + item2.Value.Count);
                }

                for (int i = 0; i < (clearLimit - reserveCount); i++)
                {
                    if (!ObjectPool.transform.FindChild(item.name).GetChild(i).gameObject.activeInHierarchy)
                    {
                        Destroy(ObjectPool.transform.FindChild(item.name).GetChild(i).gameObject);
                        _miceCount--;
                        Debug.Log("Destroy");
                    }
                }

                //Pool.FindAll(x => x.name == item.name).RemoveRange(0, clearLimit);
            }
        }

        /*
         * if (_miceCount - acitveCount > clearLimit)       // 如果沒有Active的老鼠大於需要清除的數量
        {
            int tmp = _miceCount;
            for (int i = 0; i < tmp - 1; i++)
            {
                //Pool.FindAll(delegate(GameObject p) { return p.name == item; });
                if (!Pool[_miceCount - 1].activeInHierarchy)
                {
                    Destroy(Pool[_miceCount - 1]);
                    Pool.RemoveAt(_miceCount - 1);
                    _miceCount--;
                }
            }
        }
         * */
    }
}
