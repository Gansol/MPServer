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

public class LoginUI : MonoBehaviour
{
    public GameObject[] LoginObject;

    private Dictionary<string, object> FBProfiler = null;
    private string _defaultAccout = "請輸入帳號(8~16英文數字)";
    private string _defaultPassowrd = "請輸入密碼(8~16英文數字)";

    private string getAccount = "";
    private string getPassowrd = "";
    private string getNickname = "";
    private string getAge = "0";
    private string getSex = "0";
    private string getIP = "";

    private string joinResult = "";
    private string loginResult = "";
    //private string JoinRoomResult = "";
    //private bool macthing = false;
    private float ckeckTime;
    private bool checkFlag = true;
    private static bool isLoginBtn = false;

    void Awake()
    {

    }
    // 在Start裡建立好Login的回應事件
    void Start()
    {
        Global.photonService.LoginEvent += OnLogin;
        Global.photonService.JoinMemberEvent += OnJoinMember;
        Global.photonService.LoadSceneEvent += OnExitMainGame;
        Global.photonService.ReLoginEvent += OnReLogin;
        Global.photonService.GetProfileEvent += OnGetProfile;
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    void Update()
    {
        if (Global.isMatching)
        {
            if (ckeckTime > 10 && checkFlag)
            {
                Global.photonService.ExitWaitingRoom();
                ckeckTime = 0;
                checkFlag = false;
            }
            else
            {
                checkFlag = true;
                ckeckTime += Time.deltaTime;
            }
        }
    }
    void OnGUI()
    {
        # region Draw Default 預設值
        if (UnityEngine.Event.current.type == EventType.Repaint)
        {
            if (GUI.GetNameOfFocusedControl() == "Account")
            {
                if (getAccount == _defaultAccout) getAccount = "";
            }
            else
            {
                if (getAccount == "") getAccount = _defaultAccout;
            }

            if (GUI.GetNameOfFocusedControl() == "Password")
            {
                if (getPassowrd == _defaultPassowrd) getPassowrd = "";
            }
            else
            {
                if (getPassowrd == "") getPassowrd = _defaultPassowrd;
            }
        }
        #endregion

        try
        {
            GUI.Label(new Rect(30, 10, 100, 20), "MicePow Test");

            if (Global.photonService.ServerConnected)  // 若已連線成功才顯示登入對話盒
            {
                GUI.Label(new Rect(130, 10, 100, 20), "Connecting . . ."); // 已連線

                if (Global.LoginStatus) // 若已登入
                {
                    foreach (GameObject item in LoginObject)
                    {
                        if (item!=null)
                            item.SetActive(false);
                    }

                    GUI.Label(new Rect(30, 40, 400, 20), "Your Nickname : " + Global.Nickname); // 顯示暱稱
                    GUI.Label(new Rect(30, 60, 400, 20), "Your Sex : " + Global.Sex); // 顯示性別
                    GUI.Label(new Rect(30, 80, 400, 20), "Your Age : " + Global.Age); // 顯示性別
                    GUI.Label(new Rect(100, 150, 200, 40), "Matching : " + Global.Age); // 顯示性別

                    if (!Global.isMatching)
                    {
                        if (GUI.Button(new Rect(100, 200, 250, 100), "Matching Game"))
                        {
                            Global.photonService.MatchGame(Global.PrimaryID, Global.Team);
                            //Debug.Log("click");
                            Global.isMatching = true;
                        }
                    }
                }
                else if (Global.isJoinMember)
                {
                    // 顯示登入視窗
                    GUI.Label(new Rect(30, 40, 200, 20), "Please Login");


                    GUI.Label(new Rect(30, 70, 80, 20), "Account:");
                    GUI.SetNextControlName("Account");
                    getAccount = GUI.TextField(new Rect(110, 70, 400, 50), getAccount);

                    GUI.Label(new Rect(30, 150, 80, 20), "Passowrd:");
                    GUI.SetNextControlName("Password");
                    getPassowrd = GUI.TextField(new Rect(110, 150, 400, 50), getPassowrd, 16);


                    if (GUI.Button(new Rect(30, 230, 200, 50), "Login") && !isLoginBtn)
                    {
                        isLoginBtn = true;
                        Global.photonService.Login(getAccount, getPassowrd, MemberType.Gansol); // 登入
                    }

                    if (GUI.Button(new Rect(250, 230, 200, 50), "Join"))
                        Global.isJoinMember = false;

                    GUI.Label(new Rect(30, 160, 600, 20), loginResult); // 顯示登入回傳
                    GUI.Label(new Rect(30, 190, 600, 20), joinResult); // 顯示登入回傳
                }
                else
                {
                    GUI.Label(new Rect(30, 70, 80, 20), "Account:");
                    getAccount = GUI.TextField(new Rect(110, 70, 100, 20), getAccount, 16);

                    GUI.Label(new Rect(30, 100, 80, 20), "Passowrd:");
                    getPassowrd = GUI.PasswordField(new Rect(110, 100, 100, 20), getPassowrd, '*');

                    GUI.Label(new Rect(30, 130, 80, 20), "Nickname:");
                    getNickname = GUI.TextField(new Rect(110, 130, 100, 20), getNickname, 12);

                    GUI.Label(new Rect(30, 160, 80, 20), "Age:");
                    getAge = GUI.TextField(new Rect(110, 160, 100, 20), getAge, 2);

                    GUI.Label(new Rect(30, 190, 80, 20), "Sex:");
                    getSex = GUI.TextField(new Rect(110, 190, 100, 20), getSex, 1);

                    //Global.photonService.JoinMember(getAccount, getPassowrd, getNickname, getAge, getSex);

                    GUI.Label(new Rect(30, 210, 600, 20), joinResult); // 顯示登入回傳

                    if (GUI.Button(new Rect(250, 230, 200, 50), "Join"))
                    {
                        // Debug.Log(getAge);
                        byte age = Convert.ToByte(getAge);
                        byte sex = Convert.ToByte(getSex);
                        Debug.Log(getPassowrd);
                        Global.photonService.JoinMember(getAccount, getPassowrd, getNickname, age, sex, MemberType.Gansol);
                    }
                }



            }
            else
            {
                GUI.Label(new Rect(130, 10, 200, 20), "Disconnect");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void GetPublicIP()
    {
        WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
        using (WebResponse response = request.GetResponse())
        using (StreamReader stream = new StreamReader(response.GetResponseStream()))
        {
            getIP = stream.ReadToEnd();
        }

        //Search for the ip in the html
        int first = getIP.IndexOf("Address: ") + 9;
        int last = getIP.LastIndexOf("</body>");
        getIP = getIP.Substring(first, last - first);
    }

    public void GetHostIP()
    {

        String strHostName = Dns.GetHostName();

        /*

        // 取得本機的 IpHostEntry 類別實體
        IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

        // 取得所有 IP 位址
        int num = 1;
        foreach (IPAddress ipaddress in iphostentry.AddressList)
        {
            Console.WriteLine("IP #" + num + ": " + ipaddress.ToString());
            num = num + 1;
            getIP = ipaddress.ToString();
        }
        */
        IPAddress ip = System.Net.Dns.GetHostEntry(strHostName).AddressList[0];
        getIP = ip.ToString();
    }
    // Login Event
    private void OnJoinMember(bool joinStatus, string returnCode, string message)
    {
        joinResult = message;
        Global.isJoinMember = joinStatus;
        Global.Ret = returnCode;
    }

    private void OnLogin(bool loginStatus, string message, string returnCode, int primaryID, string account, string nickname, byte sex, byte age, MemberType memberType)
    {
        if (loginStatus) // 若登入成功，將會員資料存起來
        {
            Global.Ret = returnCode;
            Global.PrimaryID = primaryID;
            Global.Account = account;
            Global.Nickname = nickname;
            Global.Sex = sex;
            Global.Age = age;
            Global.LoginStatus = true;
            Global.MemberType = memberType;
        }
        else // 若登入失敗，取得錯誤回傳字串
        {
            isLoginBtn = false;
            Global.Ret = returnCode;
            Global.Account = "";
            Global.LoginStatus = false;
            loginResult = message;
        }
    }
    public void Logout(MemberType memberType)
    {
        switch ((byte)memberType)
        {
            case (byte)MemberType.Gansol:
                // to do
                break;
            case (byte)MemberType.Google:
                ((PlayGamesPlatform)Social.Active).SignOut();
                break;
            case (byte)MemberType.Facebook:
                break;
            case (byte)MemberType.Twitter:
                break;
        }
    }

    public void GoogleLogin()
    {
        if (!isLoginBtn)
        {
            Debug.Log("Google Logining...");
            isLoginBtn = true;
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    Debug.Log("You've successfully logged in" + Social.localUser.id);

                    Global.Account = Social.localUser.id;
                    Global.Nickname = Social.localUser.userName;
                    Debug.Log(Global.Account);
                    Global.photonService.Login(Global.Account, "12345678", MemberType.Google); // 登入
                }
                else
                {
                    Debug.Log("Login failed for some reason");
                }
            });
        }
    }

    public void FaceBookLogin()
    {
        FB.Init(SetInit, OnHideUnity);
    }

    void SetInit()
    {
        Debug.Log("FB init done.");
        if (FB.IsLoggedIn)
        {
            Debug.Log("login");
        }
        else
        {
            FBLogin();
        }
    }

    void OnHideUnity(bool isGameShow)
    {
        if (!isGameShow)
        {

        }
    }
    void FBLogin()
    {
        FB.Login("email", AuthCallback);
    }

    void AuthCallback(FBResult result)
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("FB Login work");
            FB.API("/me?fields=id,name,gender,email,birthday", Facebook.HttpMethod.GET, GetFBProfiler);
        }
        else
        {
            Debug.Log("FB don't work!");
        }
    }

    void GetFBProfiler(FBResult result)
    {
        if (result.Error != null)
        {
            Debug.Log("Get FB profiler error!");
            FB.API("/me?fields=id,name,gender,email,birthday", Facebook.HttpMethod.GET, GetFBProfiler);
            return;
        }
        Global.MemberType = MemberType.Facebook;
        

        FBProfiler = Json.Deserialize(result.Text) as Dictionary<string, object>;

        Global.Account = FBProfiler["id"].ToString();
        Global.photonService.Login(Global.Account, "12345678", MemberType.Facebook); // 登入

        foreach (var item in FBProfiler)
        {
            Debug.Log("KEY:" + item.Key.ToString() + "Value:" + item.Value.ToString());
        }
    }

    void OnExitMainGame()
    {
        Global.photonService.LoginEvent -= OnLogin;
        Global.photonService.JoinMemberEvent -= OnJoinMember;
        Global.photonService.LoadSceneEvent -= OnExitMainGame;
        Global.photonService.ReLoginEvent -= OnReLogin;

    }
    void OnReLogin()
    {
        Global.LoginStatus = false;
        Global.isMatching = false;
        isLoginBtn = false;
    }

    void OnGetProfile()
    {
        Debug.Log("HAHA1");
        switch ((byte)Global.MemberType)
        {
            case (byte)MemberType.Facebook:
                {
                    try
                    {
                        Debug.Log("HAHA2");
                        string account = FBProfiler["id"].ToString();
                        string name = FBProfiler["name"].ToString();
                        string gender = FBProfiler["gender"].ToString();
                        string email = FBProfiler["email"].ToString();
                        Debug.Log("account:" + account + "name:" + name + "gender:" + gender + "email:" + email);

                        DateTime birthday = Convert.ToDateTime(FBProfiler["birthday"]);
                        TimeSpan ts = DateTime.Now.Subtract(birthday);
                        byte age =Convert.ToByte( Math.Floor(ts.TotalDays / 365));
                        byte sex = SelectGender(gender);
                        Debug.Log("sex" + sex);
                        Debug.Log("AGE" + age);
                        Global.photonService.JoinMember(account, "12345678", name, Convert.ToByte(age), sex, MemberType.Facebook);
                        Debug.Log("HAHA3 " + Convert.ToByte(age) +"  "  +sex);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    break;
                }

        }
    }

    byte SelectGender(string gender)
    {
        if (gender == "female")
            return 0;
        if (gender == "male")
            return 1;
        return 2;
    }
}
