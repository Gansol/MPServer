//using UnityEngine;
//using System.Collections;
//using GoogleMobileAds.Api;
//using System;
//using System.Text;
//using System.Security.Cryptography;
//using UnityEngine.UI;

//public class ADTest : MonoBehaviour {
//   // public UILabel label;
//    InterstitialAd interstitial;
//    BannerView bannerView;
//    private bool ADFlag;

//    public GameObject go;
//    // AD ID
//#if UNITY_ANDROID
//    string adUnitId = "ca-app-pub-1499314998394670~3252631348";
//#elif UNITY_IPHONE
//        string adUnitId = "ca-app-pub-1499314998394670~3252631348";
//#else
//        string adUnitId = "unexpected_platform";
//#endif


//    void Start()
//    {
//        RequestBanner();
//    }

//    // 插頁廣告
//    private void RequestInterstitial()
//    {
//        // Initialize an InterstitialAd.
//         interstitial = new InterstitialAd(adUnitId);
//        // Create an empty ad request.
//        //AdRequest request = new AdRequest.Builder().Build();
//        // Load the interstitial with the request.


//         // 廣告投放類型 加入使用者資訊
//         AdRequest request = new AdRequest.Builder()

//         // 測試機
//         .AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
//         .AddTestDevice(AdCommon.DeviceIdForAdmob)  // My test iPod Touch 5.

//         //// 使用者資訊
//         //.SetGender(Gender.Male)
//         //.SetBirthday(new DateTime(1985, 1, 1))
//         //.TagForChildDirectedTreatment(true)
//         //.AddExtra("excl_cat", "cars,sports") // Category exclusions for DFP.
//         .Build();

//        interstitial.LoadAd(request);
        
//    }


//    // 小廣告
//    private void RequestBanner()
//    {
//       // label.text = adUnitId.ToString();
//        // Create a 320x50 banner at the top of the screen.
//        AdSize adSize = new AdSize(250, 250);
//        bannerView = new BannerView(adUnitId, adSize, AdPosition.Bottom);    //ID 大小 位置
//        bannerView.OnAdFailedToLoad+=Failed;
//        // Create an empty ad request.
//        //AdRequest request = new AdRequest.Builder().Build();
//        // Load the banner with the request.

//        // 廣告投放類型 加入使用者資訊
//        AdRequest request = new AdRequest.Builder()

//        // 測試機
//        .AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
//        .AddTestDevice(AdCommon.DeviceIdForAdmob)  // My test iPod Touch 5.

        
//        //// 使用者資訊
//            //.SetGender(Gender.Male)
//            //.SetBirthday(new DateTime(1985, 1, 1))
//            //.TagForChildDirectedTreatment(true)
//            //.AddExtra("excl_cat", "cars,sports") // Category exclusions for DFP.
//        .Build();
//        Debug.LogError(AdCommon.DeviceIdForAdmob);
//        go.GetComponent<Text>().text = AdCommon.DeviceIdForAdmob;
//        bannerView.LoadAd(request);
        
//    }

//    private void Failed(object sender, AdFailedToLoadEventArgs args)
//    {
//        go.GetComponent<Text>().text = AdCommon.DeviceIdForAdmob + " Failed";
//    }

//    // 顯示 插頁廣告
//    private void GamePause()
//    {
//        if (interstitial.IsLoaded())
//        {
//            interstitial.Show();
//        }
//    }

//    public void ADTurn(){
        
//        if(ADFlag){
//            bannerView.Hide();
//        }else{
//            bannerView.Show();
//        }

//        ADFlag = !ADFlag;
//    }


//public class AdCommon {

//    private static string Md5Sum(string strToEncrypt){

//        UTF8Encoding ue = new UTF8Encoding();

//        byte[] bytes = ue.GetBytes(strToEncrypt);

//        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
//        byte[] hashBytes = md5.ComputeHash(bytes);

//        string hashString = "";
//        for (int i = 0; i < hashBytes.Length; i++) {

//            hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2 , '0');
//        }

//        return hashString.PadLeft(32, '0');
//    }

//    public static string DeviceIdForAdmob{

//        get{
//#if UNITY_EDITOR
//            return SystemInfo.deviceUniqueIdentifier;
//#elif UNITY_ANDROID
//            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//            AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
//            AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
//            AndroidJavaObject secure = new AndroidJavaObject("android.provider.Settings$Secure");
//            string deviceID = secure.CallStatic<string>("getString" , contentResolver, "android_id");
//            return Md5Sum(deviceID).ToUpper();
//#elif UNITY_IOS
//            return Md5Sum(UnityEngine.iOS.Device.advertisingIdentifier);
//#else
//            return SystemInfo.deviceUniqueIdentifier;
//#endif
//        }
//    }
//}
//}
