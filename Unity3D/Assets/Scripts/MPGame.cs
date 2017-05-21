using UnityEngine;
using System.Collections;
//預備改寫仲介者模式
public class MPGame {

    private static MPGame _instance = null;
    private PoolManager m_PoolManager = null;
    private AssetLoader assetLoader = null;
    private BattleHUD battleHUD = null;
    private CreatureSystem m_CreatureSystem = null;

    public static MPGame Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MPGame();
            return _instance;
        }
    }

    public void Initinal()
    {
        CreatureSystem CreatureSystem = new CreatureSystem(this);
    }

}
