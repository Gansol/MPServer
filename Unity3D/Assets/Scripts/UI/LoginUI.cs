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

public class LoginUI : MonoBehaviour
{
    public GameObject LoginPanel;
    public GameObject JoinPanel;
    public GameObject MatchGame;
    public UILabel LoginMessageBox;

    public GameObject[] ErrorText;

    TextUtility textUtility = new TextUtility();

    private Dictionary<string, object> FBProfiler = null;
    //private string _defaultAccout = "請輸入帳號(8~16英文數字)";
    //private string _defaultPassowrd = "請輸入密碼(8~16英文數字)";

    //private string getAccount = "";
    //private string getPassowrd = "";
    //private string getNickname = "";
    //private string getAge = "0";
    //private string getSex = "0";
    private string getIP = "";

    //private string JoinRoomResult = "";
    //private bool macthing = false;
    private static bool isLoginBtn = false;

    private int emailChk, passwordChk, confirmPasswordChk, equalPassword, nicknameChk;

    void OnEnable()
    {
        //  if (Global.LoginStatus) ShowMatchGame();
        LoginMessageBox.gameObject.SetActive(false);
    }
    // 在Start裡建立好Login的回應事件
    void Start()
    {
        Global.photonService.LoginEvent += OnLogin;
        Global.photonService.JoinMemberEvent += OnJoinMember;
        Global.photonService.LoadSceneEvent += OnExitMainGame;
        Global.photonService.ReLoginEvent += OnReLogin;
        Global.photonService.GetProfileEvent += OnGetProfile;
        // Global.photonService.ExitWaitingEvent += ShowMatchGame;
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        passwordChk = confirmPasswordChk = nicknameChk = emailChk = equalPassword = -1;

        if (!Global.connStatus)
            LoginPanel.SetActive(false);
    }

    void Update()
    {

    }

    void OnGUI()
    {
        if (Global.photonService.ServerConnected)  // 若已連線成功才顯示登入對話盒
        {
            GUI.Label(new Rect(130, 10, 100, 20), "Connecting . . ."); // 已連線
        }

        ShowChkMsg();
    }



    private void ShowChkMsg()
    {
        if (!Global.LoginStatus)
        {
            // 帳號檢查
            if (emailChk == 0)
            {
                ErrorText[0].SetActive(true);
                ErrorText[0].GetComponent<UILabel>().color = new Color(1, 0, 0);
                ErrorText[0].GetComponent<UILabel>().text = "X  Email Error!";
            }
            else if (emailChk == 1)
            {
                ErrorText[0].SetActive(true);
                ErrorText[0].GetComponent<UILabel>().color = new Color(0, 1, 0);
                ErrorText[0].GetComponent<UILabel>().text = "O  Correct!";
            }


            // 密碼檢查 1
            if (passwordChk == 0)
            {
                ErrorText[1].SetActive(true);
                ErrorText[1].GetComponent<UILabel>().color = new Color(1, 0, 0);
                ErrorText[1].GetComponent<UILabel>().text = "X  Password Error!";
            }
            else if (passwordChk == 1)
            {
                ErrorText[1].SetActive(true);
                ErrorText[1].GetComponent<UILabel>().color = new Color(0, 1, 0);
                ErrorText[1].GetComponent<UILabel>().text = "O  Correct!";
            }


            // 密碼檢查 2
            if (confirmPasswordChk == 0 || equalPassword == 0)
            {
                ErrorText[2].SetActive(true);
                ErrorText[2].GetComponent<UILabel>().color = new Color(1, 0, 0);
                ErrorText[2].GetComponent<UILabel>().text = "X  Password Error!";
            }
            else if (confirmPasswordChk == 1)
            {
                ErrorText[2].SetActive(true);
                ErrorText[2].GetComponent<UILabel>().color = new Color(0, 1, 0);
                ErrorText[2].GetComponent<UILabel>().text = "O  Correct!";
            }


            // 暱稱檢查
            if (nicknameChk == 0)
            {
                ErrorText[3].SetActive(true);
                ErrorText[3].GetComponent<UILabel>().color = new Color(1, 0, 0);
                ErrorText[3].GetComponent<UILabel>().text = "X  Nickname Error!";
            }
            else if (nicknameChk == 1)
            {
                ErrorText[3].SetActive(true);
                ErrorText[3].GetComponent<UILabel>().color = new Color(0, 1, 0);
                ErrorText[3].GetComponent<UILabel>().text = "O  Correct!";
            }
        }
    }

    public void Login(UILabel email, UIInput password)
    {
        isLoginBtn = true;
       Global.Hash= Encrypt(password.value);
       Global.photonService.Login(email.text, Global.Hash, MemberType.Gansol); // 登入
    }

    public void OpenJoinPanel(GameObject myPanel, GameObject joinPanel)
    {
        myPanel.SetActive(false);
        joinPanel.SetActive(true);
    }

    public void OpenLoginPanel(GameObject myPanel, GameObject loginPanel)
    {
        myPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void JoinMember(UILabel email, UIInput password, UIInput confrimPassword, UILabel nickname, UILabel age, UILabel sex)
    {
        int getSex = -1;
        char[] sTrim = { ' ', '-', '+' };

        // 帳號檢查
        if (!String.IsNullOrEmpty(email.text))
            emailChk = (textUtility.EMailChk(email.text) == 1 && email.text.Length >= 8) ? 1 : 0;

        // 密碼檢查 1
        if (!String.IsNullOrEmpty(password.value))
            passwordChk = (textUtility.SaveTextChk(password.value) == 1 && password.value.Length >= 8) ? 1 : 0;

        // 密碼檢查 2
        if (!String.IsNullOrEmpty(confrimPassword.value))
            confirmPasswordChk = (textUtility.SaveTextChk(confrimPassword.value) == 1 && confrimPassword.value.Length >= 8) ? 1 : 0;

        // 暱稱檢查
        if (!String.IsNullOrEmpty(nickname.text))
            nicknameChk = (textUtility.SaveTextChk(nickname.text) == 1 && nickname.text.Length >= 3) ? 1 : 0;

        // 性別檢查
        if (!String.IsNullOrEmpty(sex.text))
            getSex = SelectGender(sex.text);

        // 年齡檢查
        if (!String.IsNullOrEmpty(sex.text))
            getSex = SelectGender(sex.text);

        if ((password.value == confrimPassword.value) && emailChk == 1 && passwordChk == 1 && confirmPasswordChk == 1 && nicknameChk == 1)
        {
            foreach (GameObject obj in ErrorText)
            {
                obj.SetActive(false);
            }
            Global.Hash = Encrypt(password.value.ToString());
            Global.photonService.JoinMember(email.text, Global.Hash, nickname.text, System.Convert.ToByte(age.text.TrimEnd(sTrim)), (byte)getSex, GetPublicIP(), MemberType.Gansol);
            OpenLoginPanel(JoinPanel, LoginPanel);
        }
        else
        {
            if (password.value != confrimPassword.value) equalPassword = 0;
            password.value = "";
            confrimPassword.value = "";
        }
    }

    private Dictionary<string, object> GetSkillData(DataTable dt)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        if (dt != null)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Dictionary<string, string> tmp = new Dictionary<string, string>();
                string id = dt.Rows[i]["SkillID"].ToString();
                string type = dt.Rows[i]["SkillType"].ToString();
                string name = dt.Rows[i]["SkillName"].ToString();
                string level = dt.Rows[i]["SkillLevel"].ToString();
                string time = dt.Rows[i]["SkillTime"].ToString();
                string coldDown = dt.Rows[i]["ColdDown"].ToString();
                string energy = dt.Rows[i]["Energy"].ToString();
                string dealy = dt.Rows[i]["Delay"].ToString();
                string attr = dt.Rows[i]["Attr"].ToString();

                tmp.Add("SkillType", type);
                tmp.Add("ItemName", name);
                tmp.Add("SkillLevel", level);
                tmp.Add("SkillTime", time);
                tmp.Add("ColdDown", coldDown);
                tmp.Add("Energy", dealy);
                tmp.Add("Delay", dealy);
                tmp.Add("Attr", attr);
                data.Add(id, tmp);
            }
        }
        else
        {
            Debug.LogError("DataTable is Null !");
        }

        return data;
    }

    // Login Event
    private void OnJoinMember(bool joinStatus, string returnCode, string message)
    {
        Global.isJoinMember = joinStatus;
        Global.Ret = returnCode;
        LoginMessageBox.gameObject.SetActive(true);
        LoginMessageBox.color = Color.green;
        LoginMessageBox.text = "O  " + message;
    }

    private void OnLogin(bool loginStatus, string message, string returnCode)
    {
        if (loginStatus) // 若登入成功，將會員資料存起來
        {
            //  ShowMatchGame();
            Global.photonService.LoadItemData();
            LoginPanel.SetActive(false);
        }
        else // 若登入失敗，取得錯誤回傳字串
        {
            isLoginBtn = false;
            LoginMessageBox.gameObject.SetActive(true);
            LoginMessageBox.color = Color.red;
            LoginMessageBox.text = "X  " + message;
        }
    }

    //private void ShowMatchGame()
    //{
    //    MatchGame.SetActive(!Global.isMatching);
    //}

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

    #region GoogleLogin
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
                    Global.Hash = Encrypt("12345678");
                    Global.photonService.Login(Global.Account,Global.Hash , MemberType.Google); // 登入
                }
                else
                {
                    Debug.Log("Login failed for some reason");
                }
            });
        }
    }
    #endregion

    #region  FBLogin
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

    // FB Login
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
        Global.Hash = Encrypt("12345678");
        Global.photonService.Login(Global.Account, Global.Hash, MemberType.Facebook); // 登入

        foreach (var item in FBProfiler)
        {
            Debug.Log("KEY:" + item.Key.ToString() + "Value:" + item.Value.ToString());
        }
    }
    #endregion


    void OnExitMainGame()
    {
        Global.photonService.LoginEvent -= OnLogin;
        Global.photonService.JoinMemberEvent -= OnJoinMember;
        Global.photonService.LoadSceneEvent -= OnExitMainGame;
        Global.photonService.ReLoginEvent -= OnReLogin;
        //Global.photonService.ExitWaitingEvent -= ShowMatchGame; // 注意一下是否要使用

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
                        byte age = Convert.ToByte(Math.Floor(ts.TotalDays / 365));
                        byte sex = SelectGender(gender);
                        Debug.Log("sex" + sex);
                        Debug.Log("AGE" + age);
                        string tmpPD = "12345678";
                        Global.Hash = Encrypt(tmpPD);
                        Global.photonService.JoinMember(account, Global.Hash, name, Convert.ToByte(age), sex, GetPublicIP(), MemberType.Facebook);
                        Debug.Log("HAHA3 " + Convert.ToByte(age) + "  " + sex);
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                    break;
                }

        }
    }

    byte SelectGender(string gender)
    {
        if (!String.IsNullOrEmpty(gender))
        {
            if (gender == "female" || gender == "Female")
                return 0;
            if (gender == "male" || gender == "Male")
                return 1;
        }
        return 2;
    }

    public string GetPublicIP()
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
        Debug.Log("ip:" + getIP);

        return getIP;
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
        Debug.Log("ip:" + getIP);
    }

    /// <summary>
    /// 破壞編碼 固定長度加密
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    private string Encrypt(string data)
    {
        string tmpString = TextUtility.SHA512Complier(Gansol.TextUtility.SerializeToStream(data));
        return TextUtility.SHA1Complier(Gansol.TextUtility.SerializeToStream(tmpString));
    }
}
