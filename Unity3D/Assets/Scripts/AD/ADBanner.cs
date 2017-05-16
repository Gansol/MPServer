using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using System;

public class ADBanner : MonoBehaviour
{

    public enum BannerSize
    {

        BANNER,
        MEDIUM_RECTANGLE,
        FULL_BANNER,
        LEADERBOARD,
        SMART_BANNER
    }

    public static ADBanner ins;

    public string unitId;
    public BannerSize size = BannerSize.BANNER;
    public AdPosition position = AdPosition.Top;

    private BannerView bannerView;
    private Dictionary<BannerSize, AdSize> adSize = new Dictionary<BannerSize, AdSize>(){
        {BannerSize.BANNER , AdSize.Banner},
        {BannerSize.MEDIUM_RECTANGLE , AdSize.MediumRectangle},
        {BannerSize.FULL_BANNER , AdSize.IABBanner},
        {BannerSize.LEADERBOARD , AdSize.Leaderboard},
    {BannerSize.SMART_BANNER , AdSize.SmartBanner}
    };

    void Awake()
    {

        if (ins == null)
        {

            ins = this;
            DontDestroyOnLoad(gameObject);

        }
        else if (ins != this)
        {

            Destroy(gameObject);
        }
    }

    void Start()
    {

        this.RequestBanner();
    }

    private void RequestBanner()
    {

        if (!string.IsNullOrEmpty(this.unitId))
        {

            this.bannerView = new BannerView(this.unitId, this.adSize[this.size], this.position);

            AdRequest.Builder _builder = new AdRequest.Builder();

            if (Debug.isDebugBuild) _builder.AddTestDevice(AdCommon.DeviceIdForAdmob);

            this.bannerView.LoadAd(_builder.Build());
        }
    }

    public void ShowBanner()
    {

        if (this.bannerView != null)
        {

            this.bannerView.Show();
        }
    }

    public void HideBanner()
    {

        if (this.bannerView != null)
        {

            this.bannerView.Hide();
        }
    }

    public class AdCommon
    {

        private static string Md5Sum(string strToEncrypt)
        {

            UTF8Encoding ue = new UTF8Encoding();

            byte[] bytes = ue.GetBytes(strToEncrypt);

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);

            string hashString = "";
            for (int i = 0; i < hashBytes.Length; i++)
            {

                hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }

        public static string DeviceIdForAdmob
        {

            get
            {
#if UNITY_EDITOR
                return SystemInfo.deviceUniqueIdentifier;
#elif UNITY_ANDROID
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaObject secure = new AndroidJavaObject("android.provider.Settings$Secure");
            string deviceID = secure.CallStatic<string>("getString" , contentResolver, "android_id");
            return Md5Sum(deviceID).ToUpper();
#elif UNITY_IOS
            return Md5Sum(UnityEngine.iOS.Device.advertisingIdentifier);
#else
            return SystemInfo.deviceUniqueIdentifier;
#endif
            }
        }
    }
}