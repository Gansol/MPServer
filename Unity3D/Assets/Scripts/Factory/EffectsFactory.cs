using UnityEngine;
using System.Collections;

public class EffectsFactory : FactoryBase
{

    private AssetLoader assetLoader;

    public void LoadEffects(string bundleName)
    {
        assetLoader.LoadAsset("Effects/", "Effects");
        assetLoader.LoadPrefab("Effects/", bundleName);
    }

    public GameObject GetEffects(string bundleName)
    {
        GameObject obj = assetLoader.GetAsset(bundleName);
        return obj;
    }
}
