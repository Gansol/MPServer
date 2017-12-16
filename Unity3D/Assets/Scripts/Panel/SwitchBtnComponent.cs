using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SwitchBtnComponent {

    #region -- MemberChk 檢查成員變動 --
    /// <summary>
    /// 檢查成員變動
    /// </summary>
    /// <param name="serverData">伺服器資料</param>
    /// <param name="clinetData">本資機料</param>
    /// <param name="loadedBtnRefs">已載入物件資料</param>
    /// <returns>true:與伺服器資料相同  false:需要修正資料</returns>
    public bool MemberChk(Dictionary<string, object> serverData, Dictionary<string, object> clinetData, Dictionary<string, GameObject> loadedBtnRefs, Transform parent)
    {
        string key = "";


        if (loadedBtnRefs.Count == serverData.Count)
        {
            // 數量相同時 
            // 資料不同時重新載入入圖檔資料
            foreach (KeyValuePair<string, object> item in serverData)
                if (!clinetData.ContainsKey(item.Key))
                    return false;
            return true;
        }
        else if (serverData.Count > loadedBtnRefs.Count)
        {
            // 新增成員時
            List<string> keys = serverData.Keys.ToList();
            key = keys[serverData.Count - 1];
        }
        else if (serverData.Count < loadedBtnRefs.Count)
        {
            // 減少成員時
            List<string> keys = loadedBtnRefs.Keys.ToList();
            for (int i = 0; loadedBtnRefs.Count > serverData.Count; i++)
            {
                key = keys[loadedBtnRefs.Count - 1];
                if (loadedBtnRefs.ContainsKey(key) && loadedBtnRefs[key].transform.childCount > 0)
                    GameObject.Destroy(loadedBtnRefs[key].transform.GetChild(0).gameObject);
                loadedBtnRefs.Remove(key);
            }

            if (loadedBtnRefs.Count != 0)
                return false;
        }



        // 伺服器與本機 資料不同步時
        if (!Global.DictionaryCompare(serverData, clinetData))
        {
            int i = 0, j = 0;

            Dictionary<string, GameObject> loadedBtnRefsBuffer = new Dictionary<string, GameObject>(loadedBtnRefs);
            Dictionary<string, GameObject> modifyIconBuffer = new Dictionary<string, GameObject>();
            List<string> loadedGameObjectKeys = loadedBtnRefsBuffer.Keys.ToList();
            List<string> serverDataKeys = serverData.Keys.ToList();

            Debug.Log("Server: " + serverData.Count + "    Client: " + loadedBtnRefs.Count);
            foreach (KeyValuePair<string, object> item in serverData)
            {
                key = loadedGameObjectKeys[i];
                if (item.Key.ToString() != key.ToString()) // child out 
                {
                    loadedBtnRefsBuffer[key].transform.GetChild(0).GetComponent<UISprite>().spriteName = item.Value.ToString() + Global.IconSuffix;
                    loadedBtnRefs[key].transform.GetChild(0).name = item.Key;
                    loadedBtnRefsBuffer[key].SendMessage("EnableBtn");
                    Global.RenameKey(loadedBtnRefs, key, "x" + i);
                    j++;
                }
                i++;

                if (i == serverData.Count && j == 0) //相同時
                    return true;
            }

            loadedBtnRefsBuffer = new Dictionary<string, GameObject>(loadedBtnRefs);
            i = 0;

            foreach (KeyValuePair<string, GameObject> item in loadedBtnRefsBuffer)
            {
                Global.RenameKey(loadedBtnRefs, item.Key, serverDataKeys[i]);
                loadedBtnRefs[serverDataKeys[i]] = parent.GetChild(i).gameObject;
                i++;
            }
        }
        return false;
    } 
    #endregion

    #region -- ActiveMice 隱藏/顯示老鼠 --
    /// <summary>
    /// 隱藏/顯示老鼠
    /// </summary>
    /// <param name="dictData">要被無效化的老鼠</param>
    public void ActiveMice(Dictionary<string, object> dictData, Dictionary<string, GameObject> dictLoadedMiceBtnRefs) // 把按鈕變成無法使用 如果老鼠已Team中
    {
        var dictEnableMice = Global.dictMiceAll.Except(dictData);

        foreach (KeyValuePair<string, object> item in dictData)
        {
            if (dictLoadedMiceBtnRefs.ContainsKey(item.Key.ToString()))
                dictLoadedMiceBtnRefs[item.Key.ToString()].SendMessage("DisableBtn");
        }

        foreach (KeyValuePair<string, object> item in dictEnableMice)
        {
            dictLoadedMiceBtnRefs[item.Key.ToString()].SendMessage("EnableBtn");
        }
    }
    #endregion

    #region -- Add2Refs 加入載入按鈕參考 --
    /// <summary>
    /// 加入載入按鈕參考
    /// </summary>
    /// <param name="loadedBtnRefs">按鈕參考字典</param>
    /// <param name="itemID">按鈕ID</param>
    /// <param name="myParent"></param>
    public void Add2Refs(Dictionary<string, GameObject> loadedBtnRefs, Dictionary<string, GameObject> dictLoadedMiceBtnRefs, Dictionary<string, GameObject> dictLoadedTeamBtnRefs, int position, string itemID, GameObject myParent)
    {
        Transform btnArea = myParent.transform.parent;
        List<string> keys = loadedBtnRefs.Keys.ToList();

        // 檢查長度 防止溢位 position 初始值0
        if (position < loadedBtnRefs.Count && loadedBtnRefs.Count > 0)
        {
            // 如果已載入按鈕有重複Key
            if (loadedBtnRefs.ContainsKey(itemID))
            {
                // 如果Key值不同 移除舊資料
                if (keys[position] != itemID)
                {
                    loadedBtnRefs.Remove(itemID);

                    // 如果小於 載入按鈕的索引長度 直接修改索引 超過則新增
                    if (position < loadedBtnRefs.Count)
                    {
                        Global.RenameKey(loadedBtnRefs, keys[position], itemID);
                        loadedBtnRefs[itemID] = myParent;
                        loadedBtnRefs[itemID].GetComponent<BtnSwitch>().init(ref dictLoadedMiceBtnRefs, ref dictLoadedTeamBtnRefs, ref btnArea);
                    }
                    else
                    {
                        //Debug.Log("T new *******Ref ID:" + itemID + "  BtnName:" + myParent + "     Local:" + myParent.transform.parent.parent.parent.parent.name+"*************");
                        loadedBtnRefs.Add(itemID, myParent);
                        //loadedBtnRefs[itemID].GetComponent<BtnSwitch>().init(ref _dictLoadedMiceBtnRefs, ref _dictLoadedTeamBtnRefs, ref btnArea);
                    }
                }
                else
                {
                    // 重新索引按鈕對應位置
                    loadedBtnRefs[itemID] = myParent;
                    loadedBtnRefs[itemID].GetComponent<BtnSwitch>().init(ref dictLoadedMiceBtnRefs, ref dictLoadedTeamBtnRefs, ref btnArea);
                }
            }
            else
            {
                // 如果小於 載入按鈕的索引長度 直接修改索引
                Global.RenameKey(loadedBtnRefs, keys[position], itemID);
                loadedBtnRefs[itemID] = myParent;
                loadedBtnRefs[itemID].GetComponent<BtnSwitch>().init(ref dictLoadedMiceBtnRefs, ref dictLoadedTeamBtnRefs, ref btnArea);
            }
        }
        else
        {
            // 大於 載入按鈕的索引長度 則新增索引
            loadedBtnRefs.Add(itemID, myParent.gameObject);
            //Debug.Log("T > *******Ref ID:" + itemID + "  BtnName:" + myParent + "     Local:" + myParent.transform.parent.parent.parent.parent.name + "*************");
        }

    }
    #endregion
}
