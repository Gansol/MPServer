using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using MPProtocol;

public class FriendManager : MPPanel
{
    public string iconPath, slotItemName;
    public Transform itemPanel;
    public Vector2 offset;
    private ObjectFactory objFactory;
    private bool _bFirstLoad, _bLoadedIcon, _bLoadActorState;
    private Dictionary<string, object> _actorsState;

    void Awake()
    {
        assetLoader = gameObject.AddMissingComponent<AssetLoader>();
        objFactory = new ObjectFactory();
        _actorsState = new Dictionary<string, object>();

        Global.photonService.GetOnlineActorStateEvent += OnLoadActorState;
        _bFirstLoad = true;
    }

    void Update()
    {
        if (assetLoader.loadedObj && _bLoadedIcon && _bLoadActorState)
        {
            _bLoadedIcon = !_bLoadedIcon;
            _bLoadActorState = !_bLoadActorState;
            Vector2 pos = new Vector2(0, 0);

            int i = 0;

            foreach (KeyValuePair<string, object> friend in Global.dictFriendsDetail)
            {
                Dictionary<string, object> detail = friend.Value as Dictionary<string, object>;
                InstantiateItem(slotItemName, pos);
                InstantiateICON(detail, i);

                LoadFriendData(detail, i);
                pos.y += offset.y;
                i++;
            }
            LoadFriendState();

            EventMaskSwitch.Resume();
            GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
            EventMaskSwitch.Switch(gameObject, false);
            EventMaskSwitch.lastPanel = gameObject;
        }
    }

    public override void OnLoading()
    {
        Global.photonService.LoadPlayerData(Global.Account);
        Global.photonService.LoadFriendsData(Global.dictFriends);
        Global.photonService.GetOnlineActorState(Global.dictFriends);
        Global.photonService.LoadMiceData();
    }

    protected override void OnLoadPanel()
    {
        if (Global.dictFriends.Count != 0)
        {
            object itemName;

            if (_bFirstLoad)
            {
                Dictionary<string, object> dictMice = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> item in Global.miceProperty)
                {
                    Dictionary<string, object> prop = item.Value as Dictionary<string, object>;
                    prop.TryGetValue("ItemName", out itemName);
                    dictMice.Add(item.Key, itemName);
                }
                assetLoader.LoadAsset(iconPath + "/", "MiceICON");
                _bLoadedIcon = LoadIconObject(dictMice, iconPath);
                assetLoader.LoadPrefab("Panel/", slotItemName);
            }
            else
            {
                EventMaskSwitch.Resume();
                GameObject.FindGameObjectWithTag("GM").GetComponent<PanelManager>().Panel[5].SetActive(false);
                EventMaskSwitch.Switch(gameObject, false);
                EventMaskSwitch.lastPanel = gameObject;
            }
        }
    }

    private void InstantiateItem(string bundleName, Vector2 offset)
    {
        GameObject bundle = assetLoader.GetAsset(bundleName);
        objFactory.Instantiate(bundle, itemPanel, slotItemName, offset, Vector2.one, Vector2.zero, 10);
    }

    private void InstantiateICON(Dictionary<string, object> detail, int childValue)
    {
        object imageName = "";
        detail.TryGetValue("Image", out imageName);
        GameObject bundle = assetLoader.GetAsset(imageName.ToString());
        objFactory.Instantiate(bundle, itemPanel.GetChild(childValue).Find("Image"), imageName.ToString(), Vector2.zero, Vector2.one, Vector2.zero, 10);
    }

    private void LoadFriendState()
    {
        int i = 0;
        foreach (KeyValuePair<string, object> state in _actorsState)
        {
            switch ((ENUM_MemberState)state.Value)
            {
                case ENUM_MemberState.Online:
                case ENUM_MemberState.Idle:
                    {
                        itemPanel.GetChild(i).Find("Match").gameObject.SetActive(true);
                        break;
                    }
                case ENUM_MemberState.Offline:
                case ENUM_MemberState.Playing:
                    {
                        itemPanel.GetChild(i).Find("Match").gameObject.SetActive(false);
                        break;
                    }
            }
            i++;
        }

    }

    private void LoadFriendData(Dictionary<string, object> detail, int childValue)
    {
        object rank, nickname;
        detail.TryGetValue("Rank", out rank);
        detail.TryGetValue("NickName", out nickname);
        itemPanel.GetChild(childValue).Find("Rank").GetComponent<UILabel>().text = rank.ToString();
        itemPanel.GetChild(childValue).Find("NickName").GetComponent<UILabel>().text = nickname.ToString();
    }


    private void OnLoadActorState(string jString)
    {
        _bLoadActorState = !_bLoadActorState;
        _actorsState = Json.Deserialize(jString) as Dictionary<string, object>;
    }


    public void InviteFirend()
    {
        
    }

    public void RemoveFirend()
    {

    }
}
