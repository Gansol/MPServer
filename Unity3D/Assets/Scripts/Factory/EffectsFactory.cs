using UnityEngine;
using System.Collections;

public class EffectsFactory : IFactory
{
    //public void LoadEffects(string bundleName)
    //{
    //    assetLoader.LoadAsset("Effects/", "Effects");
    //    assetLoader.LoadPrefab("Effects/", bundleName);
    //}

    public GameObject GetEffects(string bundleName)
    {
        return MPGame.Instance.GetAssetLoaderSystem().GetAsset(bundleName);
    }
}
