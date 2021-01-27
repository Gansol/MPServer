using UnityEngine;
using System.Collections;
/* ***************************************************************
 * ------------  Copyright © 2021 Gansol Studio.  All Rights Reserved.  ------------
 * ----------------                                CC BY-NC-SA 4.0                                ----------------
 * ----------------                @Website:  EasyUnity@blogspot.com      ----------------
 * ----------------                @Email:    GansolTW@gmail.com               ----------------
 * ----------------                @Author:   Krola.                                             ----------------
 * ***************************************************************
 *                                                       Description
 * ***************************************************************
 *                                                          主系統
 *   1.單例、仲介者模式
 * ***************************************************************
 *                                                       ChangeLog
 *  20210107 v 1.0.0 完成
 * ****************************************************************/
public class MPGame
{
    // Panel開啟狀態
    private static bool _bLoadPlayerPanel, _bLoadTeamPanel, _bLoadStorePanel, _bLoadMatchPanel, _bloadMainScene, _bLoadFriendPanel, _bLoadPurchasePanel, _bLoadTutorialPanel, _bLoadBattlelPanel;
    private static bool _loginStatus;                    // 登入狀態
    private static MPGame _instance = null;     // 單例實體化

    // GameSystem
    private AssetLoaderSystem m_AssetLoaderSystem = null;
    private CreatureSystem m_CreatureSystem = null;
    private MessageSystem m_MessageSystem = null;
    private MissionSystem m_MissionSystem = null;
    private BattleSystem m_BattleSystem = null;
    private AudioSystem m_AudioSystem = null;
    private PoolSystem m_PoolSystem = null;

    // GameUI
    private LoginUI m_LoginUI = null;
    private TeamUI m_TeamUI = null;
    private StoreUI m_StoreUI = null;
    private MenuUI m_MenuUI = null;
    private BattleUI m_BattleUI = null;
    private PlayerUI m_PlayerUI = null;
    private MatchUI m_MatchUI = null;
    private FriendUI m_FriendUI = null;
    private TutorialUI m_TutorialUI = null;
    private PurchaseUI m_PurchaseUI = null;

    private GameLoop m_StartCoroutine = null;


    /// <summary>
    /// 單例+仲介者模式 主系統
    /// </summary>
    public static MPGame Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MPGame();
            return _instance;
        }
    }

    public void Initialize(GameLoop gameLoop)
    {
        // Init Event
        Global.photonService.LoginEvent += OnLogin;

        // Init Compoment
        m_StartCoroutine = gameLoop;

        // Iint GameSystem
        m_AssetLoaderSystem = new AssetLoaderSystem(this);
        m_CreatureSystem = new CreatureSystem(this);
        m_BattleSystem = new BattleSystem(this);
        m_PoolSystem = new PoolSystem(this);
        m_MessageSystem = new MessageSystem(this);
        m_MissionSystem = new MissionSystem(this);
        m_AudioSystem = new AudioSystem(this);

        // Init GameUI
        m_MenuUI = new MenuUI(this);
        m_LoginUI = new LoginUI(this);
        m_BattleUI = new BattleUI(this);
        m_PlayerUI = new PlayerUI(this);
        m_TeamUI = new TeamUI(this);
        m_StoreUI = new StoreUI(this);
        m_MatchUI = new MatchUI(this);
        m_FriendUI = new FriendUI(this);
        m_TutorialUI = new TutorialUI(this);
        m_PurchaseUI = new PurchaseUI(this);
        Debug.Log("MPGame Initialize.");
    }

    public void OnGUI()
    {
        if (_bloadMainScene)
            m_LoginUI.OnGUI();
        if (_bLoadBattlelPanel)
            m_BattleUI.OnGUI();
    }

    public void Update()
    {
        m_AssetLoaderSystem.Update();

        if (_bloadMainScene)
            m_MenuUI.Update();
        if (_bLoadPlayerPanel)
            m_PlayerUI.Update();
        if (_bLoadTeamPanel)
            m_TeamUI.Update();
        if (_bLoadStorePanel)
            m_StoreUI.Update();
        if (_bLoadMatchPanel)
            m_MatchUI.Update();
        if (_bLoadPurchasePanel)
            m_PurchaseUI.Update();
        if (_bLoadFriendPanel)
            m_FriendUI.Update();
        if (_bLoadTutorialPanel)
            m_TutorialUI.Update();

        if (_bLoadBattlelPanel)
        {
            m_BattleUI.Update();
            m_BattleSystem.Update();
            m_PoolSystem.Update();
            m_CreatureSystem.Update();
            m_MissionSystem.Update();
        }
    }
    public void FixedUpdate()
    {
        if (_bLoadBattlelPanel)
        {
            m_BattleSystem.FixedUpdate();
        }
    }


    public void ShowPanel(GameObject panel_btn)
    {
        m_AudioSystem.PlaySound("se_click001");

        if (Global.LoginStatus)
        {
            string panelName = panel_btn.name.Replace("_Btn", "");//.Remove(panel_btn.name.Length - 4) ; // panelName =>  Player_Btn - 4 = Player

            switch (panelName)
            {
                case "Player":
                    m_PlayerUI.ShowPanel(panelName);
                    _bLoadPlayerPanel = true;
                    break;
                case "Team":
                    m_TeamUI.ShowPanel(panelName);
                    _bLoadTeamPanel = true;
                    break;
                case "Store":
                    m_StoreUI.ShowPanel(panelName);
                    _bLoadStorePanel = true;
                    break;
                case "Purchase":
                    m_PurchaseUI.ShowPanel(panelName);
                    _bLoadPurchasePanel = true;
                    break;
                case "Friend":
                    m_FriendUI.ShowPanel(panelName);
                    _bLoadFriendPanel = true;
                    break;
                case "Match":
                    m_MatchUI.ShowPanel(panelName);
                    _bLoadMatchPanel = true;
                    break;
                case "Tutorial":
                    m_TutorialUI.ShowPanel(panelName);
                    _bLoadTutorialPanel = true;
                    break;
                default:
                    break;
            }
        }
    }

    public void InitScene(GameObject panelName)
    {
        switch (panelName.name)
        {
            case "menuui":
                m_BattleUI.Release();
                m_BattleSystem.Release();
                m_PoolSystem.Release();
                m_MissionSystem.Release();
                m_CreatureSystem.Release();

                m_MenuUI.Initialize();
                m_LoginUI.Initialize();
                _bLoadBattlelPanel = false;
                _bloadMainScene = true;
                break;

            case "gameui":
                m_LoginUI.Release();
                m_TeamUI.Release();
                m_StoreUI.Release();
                m_MenuUI.Release();
                m_PlayerUI.Release();
                m_MatchUI.Release();
                m_FriendUI.Release();
                m_TutorialUI.Release();
                m_PurchaseUI.Release();

                m_BattleUI.Initialize();
                m_BattleSystem.Initialize();
                m_PoolSystem.Initialize();
                m_MissionSystem.Initialize();
                _bLoadBattlelPanel = true;
                _bloadMainScene = false;
                break;
        }
    }

    public MessageSystem GetMessageSystem()
    {
        if (m_MessageSystem == null)
            m_MessageSystem = new MessageSystem(this);
        return m_MessageSystem;
    }

    public AssetLoaderSystem GetAssetLoaderSystem()
    {
        if (m_AssetLoaderSystem == null)
            m_AssetLoaderSystem = new AssetLoaderSystem(this);
        return m_AssetLoaderSystem;
    }

    public AudioSystem GeAudioSystem()
    {
        if (m_AudioSystem == null)
            m_AudioSystem = new AudioSystem(this);
        return m_AudioSystem;
    }

    public BattleSystem GetBattleSystem()
    {
        if (m_BattleSystem == null)
            m_BattleSystem = new BattleSystem(this);
        return m_BattleSystem;
    }
    public MissionSystem GetMissionSystem()
    {
        if (m_MissionSystem == null)
            m_MissionSystem = new MissionSystem(this);
        return m_MissionSystem;
    }

    public PoolSystem GetPoolSystem()
    {
        if (m_PoolSystem == null)
            m_PoolSystem = new PoolSystem(this);
        return m_PoolSystem;
    }

    public CreatureSystem GetCreatureSystem()
    {
        if (m_CreatureSystem == null)
            m_CreatureSystem = new CreatureSystem(this);
        return m_CreatureSystem;
    }

    public BattleUI GetBattleUI()
    {
        if (m_BattleUI == null)
            m_BattleUI = new BattleUI(this);
        return m_BattleUI;
    }

    public Coroutine StartCoroutine(IEnumerator IEnumerator)
    {
        return m_StartCoroutine.StartCoroutine(IEnumerator);
    }

    private void OnLogin(bool loginStatus, string message, string returnCode)
    {
        _loginStatus = loginStatus;
    }

    public bool GetLoginStatus()
    {
        return _loginStatus;
    }

    public void Release()
    {
        m_AssetLoaderSystem.Release();
        m_CreatureSystem.Release();
        m_MessageSystem.Release();
        m_MissionSystem.Release();
        m_AudioSystem.Release();

        m_LoginUI.Release();
        m_MenuUI.Release();
        m_BattleUI.Release();
        m_PlayerUI.Release();
        m_TeamUI.Release();
        m_StoreUI.Release();
        m_MatchUI.Release();
        m_FriendUI.Release();
        m_TutorialUI.Release();
        m_PurchaseUI.Release();
    }

    ~MPGame()
    {
        Release();
        Global.photonService.LoginEvent -= OnLogin;
    }
}
