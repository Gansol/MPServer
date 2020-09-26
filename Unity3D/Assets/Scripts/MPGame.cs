using UnityEngine;
using System.Collections;
//預備改寫仲介者模式
public class MPGame
{

    private static MPGame _instance = null;
    private static bool _loginStatus;

    // GameSystem
    private CreatureSystem m_CreatureSystem = null;
    private MessageManager m_MessageManager = null;
    private PoolManager m_PoolManager = null;


    // UserInterface
    private BattleHUD battleHUD = null;

    private static AssetLoader m_AssetLoader = null;
    private GameLoop m_StartCoroutine = null;

    private PanelManager m_MPPanel = null;

    private LoginUI m_LoginUI = null;
    private PlayerManager m_PlayerManager = null;
    private TeamManager m_TeamManager = null;
    private StoreManager m_StoreManager = null;
    private MatchManager m_MatchManager = null;
    private PurchaseManager m_PurchaseManager = null;
    private FriendManager m_FriendManager = null;

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
        Debug.Log("MPGame Initinal.");
        Global.photonService.LoginEvent += OnLogin;
        m_StartCoroutine = gameLoop;
        m_CreatureSystem = new CreatureSystem(this);
        m_MessageManager = new MessageManager(this);
        // m_AssetBundleManager = new AssetBundleManager(this);
        m_AssetLoader = gameLoop.GetComponent<AssetLoader>();
        m_MPPanel = new PanelManager(this);
        //        Debug.Log(m_AssetLoader);

        //        MPFactory m_MPFactory = new MPFactory(this);
    }

    public void Update()
    {

    }

    public void Release()
    {
        m_CreatureSystem.Release();
    }

    public void ShowPanel(GameObject btnName)
    {
        string panelName = btnName.name.Remove(btnName.name.Length - 4); // panelName =>  Player_Btn - 4 = Player

        switch (panelName)
        {
            case "Player":
                m_StoreManager.ShowPanel(panelName);
                break;
            case "Team":
                m_StoreManager.ShowPanel(panelName);
                break;
            case "Store":
                m_StoreManager.ShowPanel(panelName);
                break;
            case "Purchase":
                m_PurchaseManager.ShowPanel(panelName);
                break;
            case "Friend":
                m_FriendManager.ShowPanel(panelName);
                break;
            case "Match":
                m_MatchManager.ShowPanel(panelName);
                break;
            default:
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
