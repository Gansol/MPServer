using UnityEngine;
using System.Collections;

public class MPIAPManager : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        IabWrapper iapWrapper = new IabWrapper();

        IabWrapper.init(
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAgWeiYWDdYS5bXOGAKhXVoS2SK7D8c8Iuvl0Qn0yuIFwmVIE+J44+VO8NNwd8ma+wYdLO7Cx8QanI27ad3tUrf14a70qOWgzqdNKp/lh0s6FEg6fo4/o9c2ZYZOz4vq1zccFh7Y3jFSdu1uSkZqEL/caxihvJIOgbHr3yrAQe17KV3EMfXoWdpC7nBqS34I0NnAAM0OGWmJRNvtUQ08PMtfaPzkv0G3mQREsXOXMsVZdAee5HeGtCq9O4DwzAAjMrLm7T5Cs7wQ/EKcFxAoEq9Fp8RUezznebAi6zLroy9i94i1r4lYt+HHtYzySuHsRaAgOmHCPTDAkLIIy20Tlg1wIDAQAB",
            delegate(object[] ret)
            {
                if (true == (bool)ret[0])
                {
                    Debug.Log("iab successfully initialized");
                }
                else
                {
                    Debug.Log("failed to initialize iab");
                }
            });
    }

    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(0, 0, 100, 100), "purchase"))
    //    {
    //        IabWrapper.purchase("micepow_00000", 10001, "PRODUCT_SKU_AND_USER_ID_AND_DATE",
    //            delegate(object[] ret)
    //            {
    //                if (false == (bool)ret[0])
    //                {
    //                    Debug.Log("purchase cancelled");
    //                }
    //                else
    //                {
    //                    string purchaseinfo = (string)ret[1];
    //                    string signature = (string)ret[2];
    //                    IabWrapper.ConsumeInApp(purchaseinfo, signature,
    //                        delegate(object[] ret2)
    //                        {
    //                            if (false == (bool)ret2[0])
    //                            {
    //                                Debug.Log("failed to consume product");
    //                            }
    //                        });
    //                }
    //            });
    //    }
    //}

    void OnApplicationQuit()
    {
        IabWrapper.dispose();
    }
}