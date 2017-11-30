using UnityEngine;
using System.Collections;
//預備改寫仲介者模式
public class MPGame {

    private static MPGame _instance = null;
    private CreatureSystem m_CreatureSystem = null;
    private MessageManager m_MessageManager = null;

    private PoolManager m_PoolManager = null;


    private BattleHUD battleHUD = null;

    private static AssetLoader m_AssetLoader = null;
    private GameLoop m_StartCoroutine = null;

    private PanelManager m_MPPanel = null;

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
        m_StartCoroutine = gameLoop;
        m_CreatureSystem = new CreatureSystem(this);
        m_MessageManager = new MessageManager(this);
        m_AssetLoader = gameLoop.GetComponent<AssetLoader>();
        m_MPPanel = new PanelManager(this);
        Debug.Log(m_AssetLoader);

//        MPFactory m_MPFactory = new MPFactory(this);
    }

    public void Update()
    {

    }

    public void Release()
    {
        m_CreatureSystem.Release();
    }

    public MessageManager GetMessageManager()
    {
        if (m_MessageManager == null)
            m_MessageManager = new MessageManager(this);
        return m_MessageManager;
    }


    public AssetLoader AssetLoader()
    {
        if (m_AssetLoader == null)
            m_AssetLoader = m_StartCoroutine.GetComponent<AssetLoader>();
        return m_AssetLoader;
    }

    public Coroutine StartCoroutine(IEnumerator IEnumerator)
    {
       return m_StartCoroutine.StartCoroutine(IEnumerator);
    }
}
