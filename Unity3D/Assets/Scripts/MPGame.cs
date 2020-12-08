using UnityEngine;
using System.Collections;
//預備改寫仲介者模式
public class MPGame
{

    private static MPGame _instance = null;
    public static bool _loginStatus, _bLoadPlayerPanel, _bLoadTeamPanel, _bLoadStorePanel, _bLoadMatchPanel, _bloadMainScene, _bLoadFriendPanel, _bLoadPurchasePanel, _bLoadTutorialPanel, _bLoadBattlelPanel;

    // GameSystem
    private CreatureSystem m_CreatureSystem = null;
    private MessageManager m_MessageManager = null;
    private AudioSystem m_AudioSystem = null;
    private  PoolSystem m_PoolSystem = null;
    private BattleSystem m_BattleSystem = null;
    // UserInterface


    private static AssetLoader m_AssetLoader = null;
    private GameLoop m_StartCoroutine = null;


    private LoginUI m_LoginUI = null;
    private MenuUI m_MenuUI = null;
    private PlayerUI m_PlayerUI = null;
    private TeamUI m_TeamUI = null;
    private StoreUI m_StoreUI = null;
    private MatchUI m_MatchUI = null;
    private PurchaseUI m_PurchaseUI = null;
    private FriendUI m_FriendUI = null;
    private TutorialUI m_TutorialUI = null;
    private BattleUI m_BattleUI = null;

    public static MPGame Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MPGame();
            return _instance;
        }
    }

    public void Initinal(GameLoop gameLoop)
    {
        // Init Event
        Global.photonService.LoginEvent += OnLogin;

        // Init Compoment
        m_StartCoroutine = gameLoop;

        // Iint GameSystem
        m_CreatureSystem = new CreatureSystem(this);
        m_MessageManager = new MessageManager(this);
        m_AudioSystem = new AudioSystem(this);
        m_AssetLoader = gameLoop.GetComponent<AssetLoader>();

        // Init GameUI
        m_MenuUI = new MenuUI(this);

        Debug.Log("MPGame Initinal.");
    }
    public void OnGUI()
    {
        if (_bLoadBattlelPanel)
            m_BattleUI.OnGUI();
    }

    public void Update()
    {
        if (_bloadMainScene)
        {
            m_LoginUI.Update();
            m_MenuUI.Update();
            //   m_TutorialManager.Update();
        }
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
        }
    }
    public void FixedUpdate()
    {

    }

    public void Release()
    {
        m_CreatureSystem.Release();
    }

    public void ShowPanel(GameObject panel_btn)
    {
        if (Global.LoginStatus)
        {
            string panelName = panel_btn.name.Replace("_Btn", "");//.Remove(panel_btn.name.Length - 4) ; // panelName =>  Player_Btn - 4 = Player
            
            m_AudioSystem.PlaySound("se_click001");
            switch (panelName)
            {
                case "Player":
                    if (!_bLoadPlayerPanel)
                        m_PlayerUI = new PlayerUI(this);
                    m_PlayerUI.ShowPanel(panelName);
                    _bLoadPlayerPanel = true;
                    break;
                case "Team":
                    if (!_bLoadTeamPanel)
                        m_TeamUI = new TeamUI(this);
                    m_TeamUI.ShowPanel(panelName);
                    _bLoadTeamPanel = true;
                    break;
                case "Store":
                    if (!_bLoadStorePanel)
                        m_StoreUI = new StoreUI(this);
                    m_StoreUI.ShowPanel(panelName);
                    _bLoadStorePanel = true;
                    break;
                case "Purchase":
                    if (!_bLoadPurchasePanel)
                        m_PurchaseUI = new PurchaseUI(this);
                    m_PurchaseUI.ShowPanel(panelName);
                    _bLoadPurchasePanel = true;
                    break;
                case "Friend":
                    if (!_bLoadFriendPanel)
                        m_FriendUI = new FriendUI(this);
                    m_FriendUI.ShowPanel(panelName);
                    _bLoadFriendPanel = true;
                    break;
                case "Match":
                    if (!_bLoadMatchPanel)
                        m_MatchUI = new MatchUI(this);
                    m_MatchUI.ShowPanel(panelName);
                    _bLoadMatchPanel = true;
                    break;
                case "Tutorial":
                    if (!_bLoadTutorialPanel)
                        m_TutorialUI = new TutorialUI(this);
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
                m_MenuUI = new MenuUI(this);
                m_LoginUI = new LoginUI(this);
                m_AudioSystem.Initinal();
                m_MenuUI.Initinal();
                m_LoginUI.Initinal();
                break;
            case "gameui":
                if (!_bLoadBattlelPanel)
                    m_BattleUI = new BattleUI(this);
                m_BattleSystem = new BattleSystem(this);
                m_BattleUI.Initinal();
                m_BattleSystem.Initinal();
                _bLoadBattlelPanel = true;
                break;
        }
    }

    //public void InitScene(string panelName)
    //{
    //    //  Debug.Log("panelName" + panelName);
    //    switch (panelName)
    //    {
    //        case "menuui":

    //            break;
    //        case "battle":
    //            //battleUI.Initinal();
    //            break;
    //        case "Player":
    //            // m_StoreManager.ShowPanel(panelName);
    //            break;
    //        case "Team":
    //            //  m_StoreManager.ShowPanel(panelName);
    //            break;
    //        case "Store":
    //            //  m_StoreManager.ShowPanel(panelName);
    //            break;
    //        case "Purchase":
    //            //  m_PurchaseManager.ShowPanel(panelName);
    //            break;
    //        case "Friend":
    //            //   m_FriendManager.ShowPanel(panelName);
    //            break;
    //        case "Match":
    //            //m_MatchManager.ShowPanel(panelName);
    //            //if (!_bLoadMatchPanel)
    //            //    m_MatchManager = new MatchManager(this);
    //            //_bLoadMatchPanel = true;
    //            break;
    //        default:
    //            break;
    //    }
    //}

    public void LoadedScene(string sceneName)
    {
        switch (sceneName)
        {
            case (string)Global.Scene.MainGame:
                _bloadMainScene = true;
                break;
        }
    }

    public MessageManager GetMessageManager()
    {
        if (m_MessageManager == null)
            m_MessageManager = new MessageManager(this);
        return m_MessageManager;
    }

    //public MessageManager GetAssetBundleManager()
    //{
    //    if (m_AssetBundleManager == null)
    //        m_AssetBundleManager = new AssetBundleManager(this);
    //    return m_MessageManager;
    //}

    public AssetLoader GetAssetLoader()
    {
        if (m_AssetLoader == null)
            m_AssetLoader = m_StartCoroutine.GetComponent<AssetLoader>();
        return m_AssetLoader;
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
    public PoolSystem GePoolSystem()
    {
        if (m_PoolSystem == null)
            m_PoolSystem = new PoolSystem(this);
        return m_PoolSystem;
    }

    public BattleUI GetBattleUI()
    {
        if (m_BattleUI == null)
        {
            m_BattleUI = new BattleUI(this);
            m_BattleUI.Initinal();
        }
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

    ~MPGame()
    {
        Global.photonService.LoginEvent -= OnLogin;
    }
}
