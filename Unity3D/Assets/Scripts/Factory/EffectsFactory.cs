using UnityEngine;
using System.Collections;

public class EffectsFactory : IFactory
{

    private AssetLoaderSystem assetLoader;

    //public void LoadEffects(string bundleName)
    //{
    //    assetLoader.LoadAsset("Effects/", "Effects");
    //    assetLoader.LoadPrefab("Effects/", bundleName);
    //}

    public GameObject GetEffects(string bundleName)
    {
        GameObject go = assetLoader.GetAsset(bundleName);
        return go;
    }
}
