using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.IO;
using MPProtocol;
using GooglePlayGames;
using MiniJSON;
using Gansol;
using System.Data;
using System.Text;
using GooglePlayGames.BasicApi;

public class del2 :MonoBehaviour
{
    public GameObject[] btn;
    public UILabel label;

    void Start()
    {
        Global.photonService.ActorOnlineEvent += OnGetActorOnline;
        label.text = "IAB START!";
    }

    private void OnGetActorOnline()
    {
        Debug.Log("Online: 1");
        label.text = "Online: 1" ;
    }

    public void GoogleLogin()
    {

            if (!Social.localUser.authenticated)
                PlayGamesPlatform.Activate();
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    Debug.Log("You've successfully logged in" + Social.localUser.id);
                    if (!Global.photonService.ServerConnected) gameObject.GetComponent<PhotonConnect>().ConnectToServer();


                    Global.MemberType = MemberType.Google;
                    Debug.Log(Global.Account);


                    // Debug.Log("Local user's email is " + ((PlayGamesLocalUser)Social.localUser).Email);
                    Global.Account = ((PlayGamesLocalUser)Social.localUser).id;
                    Global.Hash = Encrypt(Global.Account);
                    Global.Nickname = ((PlayGamesLocalUser)Social.localUser).userName;

                    //string email = ((PlayGamesLocalUser)Social.localUser).Email;
                    bool underage = ((PlayGamesLocalUser)Social.localUser).underage;
                    int age = (underage) ? 88 : 6;

                    //if (String.IsNullOrEmpty(email))
                    //    email = "example@example.com";

                    Global.ShowMessage("登入中...", Global.MessageBoxType.NonChk, 0);
                    Global.photonService.LoginGoogle(Global.Account, Global.Hash, Global.Nickname, age, "example@example.com", MemberType.Google); // 登入
                }
                else
                {
                    Debug.Log("Login failed for some reason");
                }
            });
        }


    public void Subscription()
    {
        Debug.Log("On Subscription");
        label.text  = "On Subscription";

        IabWrapper.purchase("micepow_00000", 10001, "PRODUCT_SKU_AND_USER_ID_AND_DATE",
                delegate(object[] ret)
                {
                    if (false == (bool)ret[0])
                    {
                        Debug.Log("purchase cancelled");
                    }
                    else
                    {
                        string purchaseinfo = (string)ret[1];
                        string signature = (string)ret[2];

                        Debug.Log("Subscription > ConsumeInApp ");
                        IabWrapper.ConsumeInApp(purchaseinfo, signature,
                            delegate(object[] ret2)
                            {
                                if (false == (bool)ret2[0])
                                {
                                    Debug.Log("failed to consume product");
                                }
                            });
                    }
                });
    }

    public void UnSubscription()
    {
        Debug.Log("On UnSubscription");
        label.text  = "On UnSubscription";

        IabWrapper.purchase("micepow_00000", 10001, "PRODUCT_SKU_AND_USER_ID_AND_DATE",
                        delegate(object[] ret)
                        {
                            if (false == (bool)ret[0])
                            {
                                Debug.Log("purchase cancelled");
                            }
                            else
                            {
                                string purchaseinfo = (string)ret[1];
                                string signature = (string)ret[2];

                                Debug.Log("Subscription > ConsumeInApp ");
                                IabWrapper.ConsumeInApp(purchaseinfo, signature,
                                    delegate(object[] ret2)
                                    {
                                        if (false == (bool)ret2[0])
                                        {
                                            Debug.Log("failed to consume product");
                                        }
                                    });
                            }
                        });

    }



    private string Encrypt(string data)
    {
        string tmpString = TextUtility.SHA512Complier(Gansol.TextUtility.SerializeToStream(data));
        return TextUtility.SHA1Complier(Gansol.TextUtility.SerializeToStream(tmpString));
    }


    public void OnInit()
    {
        label.text = "Init";
    }

       public void OnBuySuccess()
    {
        label.text = "Buy Success!"+(++i).ToString();
    }

       public int i = 0;

}


