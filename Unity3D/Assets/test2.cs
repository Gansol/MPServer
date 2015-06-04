using UnityEngine;
using System.Collections.Generic;
using MiniJSON;

public class test2 : MonoBehaviour
{
    public Rigidbody projectile;
    bool flag = true;
    void LaunchProjectile()
    {
        Rigidbody instance = (Rigidbody)Instantiate(projectile);
        instance.velocity = Random.insideUnitSphere * 5;
    }
    void Update()
    {
        if (flag)
        {
            flag = false;
            InvokeRepeating("LaunchProjectile", 0, 1F);
        }
    }
}