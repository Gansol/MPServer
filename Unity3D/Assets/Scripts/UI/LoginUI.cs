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
using UnityEngine.Networking;

/// 
/// 年齡驗證沒寫
/// 

public class LoginUI : IMPPanelUI
{
    private GameObject LoginPanel;
    private GameObject JoinPanel;
    private GameObject LicensePanel;

    private UIInput login_AccountField, login_PasswordField, join_AccountField, join_PasswordField, join_Password2Field, join_ConfrimPasswordField, join_NicknameField, join_AgeField, join_SexField;
    private UILabel LoginMessageBox;
    private UIToggle accountToggle, passwordToggle, login_AgreeLicenseField, join_AgreeLicense_Field;

    private GameObject SNSLogin, GansolLogin;
    private GameObject FBLoginBtn, GansolLoginBtn, JoinBtn, Switch_Btn, join_JoinBtn, join_ExitBtn;
    private GameObject ErrorText_Email;
    private GameObject ErrorText_Password;
    private GameObject ErrorText_Password2;
    private GameObject ErrorText_NickName;

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
    private string jFileName = "data.json";
    private string jString;
    //private string JoinRoomResult = "";
    //private bool macthing = false;
    private static bool isLoginBtn = false;
    private bool bSwitchLoginType;
    private int emailChk, passwordChk, confirmPasswordChk, equalPassword, nicknameChk;
    Dictionary<string, object> data;

    GameObject tmpPanel;

    public LoginUI(MPGame MPGame) : base(MPGame) { }

    public void OnLicenseClickOn(GameObject panel)
    {
        panel.SetActive(false);
        LicensePanel.SetActive(true);
        tmpPanel = panel;
    }

    public void OnLicenseClickOff()
    {
        tmpPanel.SetActive(true);
        LicensePanel.SetActive(false);
        tmpPanel = null;
    }

    void OnEnable()
    {
        // if (Global.LoginStatus) ShowMatchGame();
        LoginMessageBox.gameObject.SetActive(false);

        //// 讀取帳密儲存資訊
        //if ((string.IsNullOrEmpty(jString) || jString == "{}") && File.Exists(Global.dataPath + jFileName))
        //{
        //    StartCoroutine(LoadFile(Global.dataPath + "data.json"));

        //    data = Json.Deserialize(jString) as Dictionary<string, object>;

        //    accountField.value = data["Account"].ToString();
        //    passwordField.value = data["Hash"].ToString();
        //}
        //else if (!string.IsNullOrEmpty(jString) && jString != "{}")
        //{
        //    data = Json.Deserialize(jString) as Dictionary<string, object>;
        //    accountField.value = data["Account"].ToString();
        //    passwordField.value = data["Hash"].ToString();
        //}

    }
    // 在Start裡建立好Login的回應事件
    public override void Initinal()
    {

        assetLoader = MPGame.Instance.GetAssetLoader();
        Global.photonService.LoginEvent += OnLogin;
        Global.photonService.JoinMemberEvent += OnJoinMember;
        Global.photonService.LoadSceneEvent += OnExitMainGame;
        Global.photonService.ReLoginEvent += OnReLogin;
        Global.photonService.GetProfileEvent += OnGetProfile;
        // Global.photonService.ExitWaitingEvent += ShowMatchGame;
        // PlayGamesPlatform.DebugLogEnabled = true;//20200527
        // PlayGamesPlatform.Activate();//20200527


        m_RootUI = GameObject.Find("Login(Panel)");

        LoginPanel = m_RootUI.transform.Find("Login_Panel").gameObject;
        JoinPanel = m_RootUI.transform.Find("Join_Panel").gameObject;
        LicensePanel = m_RootUI.transform.Find("License_Panel").gameObject;

        // LoginPanel
        login_AccountField = LoginPanel.transform.Find("Account_Field").GetComponent<UIInput>();
        login_PasswordField = LoginPanel.transform.Find("Password_Field").GetComponent<UIInput>();
        LoginMessageBox = LoginPanel.transform.Find("LoginMessage_Label").GetComponent<UILabel>();
        accountToggle = LoginPanel.transform.Find("Account_Toggle").GetComponent<UIToggle>();
        passwordToggle = LoginPanel.transform.Find("Password_Toggle").GetComponent<UIToggle>();
        login_AgreeLicenseField = LoginPanel.transform.Find("AgreeLicense_Field").GetComponent<UIToggle>();

        GansolLogin = LoginPanel.transform.Find("GansolLogin").gameObject;
        SNSLogin = LoginPanel.transform.Find("SNSLogin").gameObject;

        FBLoginBtn = LoginPanel.transform.Find("FB_Btn").gameObject;
        GansolLoginBtn = LoginPanel.transform.Find("Login_Btn").gameObject;
        JoinBtn = LoginPanel.transform.Find("Join_Btn").gameObject;
        Switch_Btn = LoginPanel.transform.Find("Switch_Btn").gameObject;

        // JoinPanel
        join_AccountField = JoinPanel.transform.Find("Account_Label").GetComponent<UIInput>();
        join_PasswordField = JoinPanel.transform.Find("Password_label").GetComponent<UIInput>();
        join_ConfrimPasswordField = JoinPanel.transform.Find("ConfrimPassword_label").GetComponent<UIInput>();
        join_NicknameField = JoinPanel.transform.Find("Nickname_label").GetComponent<UIInput>();
        join_AgeField = JoinPanel.transform.Find("Age_Label").GetComponent<UIInput>();
        join_SexField = JoinPanel.transform.Find("Sex_Label").GetComponent<UIInput>();
        join_AgreeLicense_Field = LoginPanel.transform.Find("AgreeLicense_Field").GetComponent<UIToggle>();

        join_JoinBtn = LoginPanel.transform.Find("Join_Btn").gameObject;
        join_ExitBtn = LoginPanel.transform.Find("Exit_Btn").gameObject;
        ErrorText_Email = JoinPanel.transform.Find("AccountError_Label").gameObject;
        ErrorText_Password = JoinPanel.transform.Find("PasswordError_Label").gameObject;
        ErrorText_Password2 = JoinPanel.transform.Find("ConfrimPasswordError_Label").gameObject;
        ErrorText_NickName = JoinPanel.transform.Find("NicknameError_Label").gameObject;

        // UIEventListener.Get(FBLogin_Btn).onClick += FBLogin;
        UIEventListener.Get(GansolLoginBtn).onClick += Login;
        UIEventListener.Get(JoinBtn).onClick += ShowJoinPanel;
        UIEventListener.Get(Switch_Btn).onClick += SwitchLoginType;


        UIEventListener.Get(join_JoinBtn).onClick += JoinMember;
        UIEventListener.Get(join_ExitBtn).onClick += ShowLoginPanel;



        passwordChk = confirmPasswordChk = nicknameChk = emailChk = equalPassword = -1;

        if (!Global.connStatus)
            LoginPanel.SetActive(false);
    }


    public override void Update()
    {
        ShowChkMsg();   // 原本在OnGUI
    }


    public void SwitchLoginType(GameObject obj)
    {
        GansolLogin.SetActive(bSwitchLoginType);
        SNSLogin.SetActive(!bSwitchLoginType);
        bSwitchLoginType = !bSwitchLoginType;
    }

    private void ShowChkMsg()
    {
        if (!Global.LoginStatus)
        {
            // 帳號檢查
            if (emailChk == 0)
            {
                ErrorText_Email.SetActive(true);
                ErrorText_Email.GetComponent<UILabel>().color = new Color(1, 0, 0);
                ErrorText_Email.GetComponent<UILabel>().text = "X  Email Error!";
            }
            else if (emailChk == 1)
            {
                ErrorText_Email.SetActive(true);
                ErrorText_Email.GetComponent<UILabel>().color = new Color(0, 1, 0);
                ErrorText_Email.GetComponent<UILabel>().text = "O  Correct!";
            }


            // 密碼檢查 1
            if (passwordChk == 0)
            {
                ErrorText_Password.SetActive(true);
                ErrorText_Password.GetComponent<UILabel>().color = new Color(1, 0, 0);
                ErrorText_Password.GetComponent<UILabel>().text = "X  Password Error!";
            }
            else if (passwordChk == 1)
            {
                ErrorText_Password.SetActive(true);
                ErrorText_Password.GetComponent<UILabel>().color = new Color(0, 1, 0);
                ErrorText_Password.GetComponent<UILabel>().text = "O  Correct!";
            }


            // 密碼檢查 2
            if (confirmPasswordChk == 0 || equalPassword == 0)
            {
                ErrorText_Password2.SetActive(true);
                ErrorText_Password2.GetComponent<UILabel>().color = new Color(1, 0, 0);
                ErrorText_Password2.GetComponent<UILabel>().text = "X  Password Error!";
            }
            else if (confirmPasswordChk == 1)
            {
                ErrorText_Password2.SetActive(true);
                ErrorText_Password2.GetComponent<UILabel>().color = new Color(0, 1, 0);
                ErrorText_Password2.GetComponent<UILabel>().text = "O  Correct!";
            }


            // 暱稱檢查
            if (nicknameChk == 0)
            {
                ErrorText_NickName.SetActive(true);
                ErrorText_NickName.GetComponent<UILabel>().color = new Color(1, 0, 0);
                ErrorText_NickName.GetComponent<UILabel>().text = "X  Nickname Error!";
            }
            else if (nicknameChk == 1)
            {
                ErrorText_NickName.SetActive(true);
                ErrorText_NickName.GetComponent<UILabel>().color = new Color(0, 1, 0);
                ErrorText_NickName.GetComponent<UILabel>().text = "O  Correct!";
            }
        }
    }

    public void Login(GameObject obj)
    {
        Global.ShowMessage("登入中...", Global.MessageBoxType.NonChk, 0);
        LoginPanel.SetActive(false);
        isLoginBtn = true;
        Global.Hash = Encrypt(login_PasswordField.value);
        char[] splitChar = new char[] { '@' };
        string[] account = login_AccountField.value.Split(splitChar);
        Global.Account = account[0];
        Global.MemberType = MemberType.Gansol;
        Global.photonService.Login(Global.Account, Global.Hash, MemberType.Gansol); // 登入
    }

    public void SwichLoginType(GameObject obj)
    {

        LoginPanel.SetActive(false);
        JoinPanel.SetActive(true);
    }

    public void ShowJoinPanel(GameObject obj)
    {
        LoginPanel.SetActive(false);
        JoinPanel.SetActive(true);
    }

    public void ShowLoginPanel(GameObject obj)
    {
        LoginPanel.SetActive(true);
        JoinPanel.SetActive(false);
    }

    // public void JoinMember(UILabel email, UIInput password, UIInput confrimPassword, UILabel nickname, UILabel age, UILabel sex)
    public void JoinMember(GameObject obj)
    {
        join_AccountField = JoinPanel.transform.Find("Account_Label").GetComponent<UIInput>();
        join_PasswordField = JoinPanel.transform.Find("Password_label").GetComponent<UIInput>();
        join_ConfrimPasswordField = JoinPanel.transform.Find("ConfrimPassword_label").GetComponent<UIInput>();
        join_NicknameField = JoinPanel.transform.Find("Nickname_label").GetComponent<UIInput>();
        join_AgeField = JoinPanel.transform.Find("Age_Label").GetComponent<UIInput>();
        join_SexField = JoinPanel.transform.Find("Sex_Label").GetComponent<UIInput>();

        int sex = -1, age = -1;
        char[] sTrim = { ' ', '-', '+' };

        // 帳號檢查
        if (!String.IsNullOrEmpty(join_AccountField.value))
            emailChk = (textUtility.EMailChk(join_AccountField.value) == 1 && join_AccountField.value.Length >= 8) ? 1 : 0;

        // 密碼檢查 1
        if (!String.IsNullOrEmpty(join_PasswordField.value))
            passwordChk = (textUtility.SaveTextChk(join_PasswordField.value) == 1 && join_PasswordField.value.Length >= 8) ? 1 : 0;

        // 密碼檢查 2
        if (!String.IsNullOrEmpty(join_ConfrimPasswordField.value))
            confirmPasswordChk = (textUtility.SaveTextChk(join_ConfrimPasswordField.value) == 1 && join_ConfrimPasswordField.value.Length >= 8) ? 1 : 0;

        // 暱稱檢查
        if (!String.IsNullOrEmpty(join_NicknameField.value))
            nicknameChk = (textUtility.SaveTextChk(join_NicknameField.value) == 1 && join_NicknameField.value.Length >= 3) ? 1 : 0;

        // 性別檢查
        if (!String.IsNullOrEmpty(join_SexField.value))
            sex = SelectGender(join_SexField.value);

        // 年齡檢查
        //if (!String.IsNullOrEmpty(join_AgeField.value))
        //    age = SelectGender(join_AgeField.value);

        if ((join_PasswordField.value == join_ConfrimPasswordField.value) && emailChk == 1 && passwordChk == 1 && confirmPasswordChk == 1 && nicknameChk == 1)
        {
            ErrorText_Email.SetActive(false);
            ErrorText_Password.SetActive(false);
            ErrorText_Password2.SetActive(false);
            ErrorText_NickName.SetActive(false);



            char[] splitChar = new char[] { '@' };
            string[] account = join_AccountField.value.Split(splitChar);
            Global.Account = account[0];
            Global.Hash = Encrypt(join_PasswordField.value);
            Global.MemberType = MemberType.Gansol;

            //SaveLoginInfo(accountToggle.value, passwordToggle.value);


            Global.photonService.JoinMember(join_AccountField.value, Global.Hash, join_NicknameField.value, System.Convert.ToByte(join_AgeField.value.Trim(sTrim)), (byte)sex, GetPublicIP(), MemberType.Gansol);
            JoinPanel.SetActive(false);
            LoginPanel.SetActive(true);
        }
        else
        {
            if (join_PasswordField.value != join_ConfrimPasswordField.value) equalPassword = 0;
            join_PasswordField.value = "";
            join_ConfrimPasswordField.value = "";
        }
    }

    private void SaveLoginInfo(bool memAccount, bool memPD)
    {
        bool bChange = false;
        m_MPGame.StartCoroutine(LoadFile(Global.dataPath + "data.json"));

        Dictionary<string, object> data = Json.Deserialize(jString) as Dictionary<string, object>;

        if (memAccount)
        {
            if (!data.ContainsKey("Account"))
            {
                data.Add("Account", Global.Account);
            }
            else if (data["Account"].ToString() != Global.Account)
            {
                data["Account"] = Global.Account;
                bChange = true;
            }
        }

        if (memPD)
        {
            if (!data.ContainsKey("Hash"))
            {
                data.Add("Hash", Global.Hash);
            }
            else if (data["Hash"].ToString() != Global.Hash)
            {
                data["Hash"] = Global.Hash;
                bChange = true;
            }
        }

        if (bChange)
        {
            string contant = Json.Serialize(data);
            if (File.Exists(Global.dataPath + jFileName))
                File.Delete(Global.dataPath + jFileName);
            using (FileStream fs = File.Create(Global.dataPath + jFileName)) //using 會自動關閉Stream 建立檔案
            {
                fs.Write(new UTF8Encoding(true).GetBytes(contant), 0, contant.Length); //寫入檔案
                fs.Dispose(); //避免錯誤 在寫一次關閉
            }
        }
    }

    IEnumerator LoadFile(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(filePath))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                throw new Exception(www.error);
            }
            else if (www.isDone)
            {
                jString = www.downloadHandler.text; // 儲存 下載好的檔案版本
            }
            www.Dispose();
        }
        //    WWW www = new WWW(filePath);
        //yield return www;
        //jString = www.text;
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
        EventMaskSwitch.PrevToFirst();
        // Global.photonService.Login(Global.Account, Global.Hash, Global.MemberType);
        LoginMessageBox.gameObject.SetActive(true);
        LoginMessageBox.color = Color.green;
        LoginMessageBox.text = "O  " + message;
    }

    private void OnLogin(bool loginStatus, string message, string returnCode)
    {
        if (loginStatus) // 若登入成功，將會員資料存起來
        {
            //  ShowMatchGame();
            LoginPanel.SetActive(false);
            Global.ShowMessage("登入成功！", Global.MessageBoxType.Yes, 0);
            //EventMaskSwitch.PrevToFirst();
            Global.photonService.LoadItemData();
            LoginPanel.SetActive(false);
        }
        else // 若登入失敗，取得錯誤回傳字串
        {
            isLoginBtn = false;
            LoginPanel.SetActive(true);
            LoginMessageBox.gameObject.SetActive(true);
            Global.ShowMessage(message, Global.MessageBoxType.Yes, 0);
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
                //((PlayGamesPlatform)Social.Active).SignOut();
                break;
            case (byte)MemberType.Facebook:
                break;
            case (byte)MemberType.Twitter:
                break;
        }
    }

    //20200527
    #region GoogleLogin
    public void GoogleLogin(GameObject obj)
    {

        if (!isLoginBtn)
        {
            Global.ShowMessage("登入中...", Global.MessageBoxType.NonChk, 0);
            LoginPanel.SetActive(false);
            Debug.Log("Google Logining...");
            isLoginBtn = true;
            if (!Social.localUser.authenticated)
                PlayGamesPlatform.Activate();
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    Debug.Log("You've successfully logged in" + Social.localUser.id);
                    if (!Global.photonService.ServerConnected) m_RootUI.GetComponentInParent<PhotonConnect>().ConnectToServer();


                    Global.MemberType = MemberType.Google;
                    Debug.Log(Global.Account);


                    // Debug.Log("Local user's email is " + ((PlayGamesLocalUser)Social.localUser).Email);
                    Global.Account = ((PlayGamesLocalUser)Social.localUser).id;
                    Global.Hash = Encrypt(Global.Account);
                    Global.Nickname = ((PlayGamesLocalUser)Social.localUser).userName;

                    string email = ((PlayGamesLocalUser)Social.localUser).Email;
                    bool underage = ((PlayGamesLocalUser)Social.localUser).underage;
                    byte age = (underage) ? (byte)88 : (byte)6;

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
    }
    #endregion

    //#region  FBLogin
    //public void FaceBookLogin()
    //{
    //    FB.Init(SetInit, OnHideUnity);
    //}

    //void SetInit()
    //{
    //    Debug.Log("FB init done.");
    //    if (FB.IsLoggedIn)
    //    {
    //        Debug.Log("login");
    //        FB.API("/me?fields=id,name,gender,email,birthday", Facebook.HttpMethod.GET, GetFBProfiler);
    //    }
    //    else
    //    {
    //        FBLogin();
    //    }
    //}

    //void OnHideUnity(bool isGameShow)
    //{
    //    if (!isGameShow)
    //    {

    //    }
    //}
    //void FBLogin()
    //{
    //    FB.Login("email", AuthCallback);
    //}

    //void AuthCallback(FBResult result)
    //{
    //    if (FB.IsLoggedIn)
    //    {
    //        Debug.Log("FB Login work");
    //        FB.API("/me?fields=id,name,gender,email,birthday", Facebook.HttpMethod.GET, GetFBProfiler);
    //    }
    //    else
    //    {
    //        Debug.Log("FB don't work!");
    //    }
    //}

    //// 普通登入時使用
    //// FB Login
    //void GetFBProfiler(FBResult result)
    //{

    //    if (result.Error != null)
    //    {
    //        Debug.Log("Get FB profiler error!");
    //        FB.API("/me?fields=id,name,gender,email,birthday", Facebook.HttpMethod.GET, GetFBProfiler);
    //        return;
    //    }
    //    //Global.MemberType = MemberType.Facebook;
    //    //if (!Global.photonService.ServerConnected) 
    //    //    gameObject.GetComponent<PhotonConnect>().ConnectToServer();

    //    FBProfiler = Json.Deserialize(result.Text) as Dictionary<string, object>;
    //    Debug.Log(FBProfiler["id"].ToString());
    //    Debug.Log(FBProfiler["name"].ToString());
    //    Debug.Log(FBProfiler["email"].ToString());
    //    Debug.Log(FBProfiler["gender"].ToString());
    //    Debug.Log(FBProfiler["birthday"].ToString());

    //    string[] tmp = FBProfiler["email"].ToString().Split('@');
    //    Global.Account = tmp[0];
    //    Global.Hash = Encrypt(Global.Account);
    //    Global.MemberType = MemberType.Facebook;
    //    Global.photonService.Login(Global.Account, Global.Hash, MemberType.Facebook); // 登入

    //    foreach (var item in FBProfiler)
    //    {
    //        Debug.Log("KEY:" + item.Key.ToString() + "Value:" + item.Value.ToString());
    //    }

    //    LoginPanel.SetActive(false);
    //    Global.ShowMessage("登入中...", Global.MessageBoxType.NonChk,0);
    //}
    //#endregion
    //20200527

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
        LoginPanel.SetActive(true);
        LoginMessageBox.gameObject.SetActive(true);
        LoginMessageBox.color = Color.red;
        LoginMessageBox.text = "X  " + "重複登入！";
    }

    // 加入會員後 再度取得資料並登入
    void OnGetProfile()
    {
        Global.ShowMessage("登入中...", Global.MessageBoxType.NonChk, 0);
        LoginPanel.SetActive(false);
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
                        // string tmpPD = "12345678";
                        Global.Account = account;
                        Global.Hash = Encrypt(Global.Account);
                        Global.MemberType = MemberType.Facebook;
                        Global.photonService.JoinMember(email, Global.Hash, name, Convert.ToByte(age), sex, GetPublicIP(), MemberType.Facebook);
                        Debug.Log("HAHA3 " + Convert.ToByte(age) + "  " + sex);
                    }
                    catch
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
            if (gender == "female" || gender == "Female" || gender == "女")
                return 0;
            if (gender == "male" || gender == "Male" || gender == "男")
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

    public override void Release()
    {
        throw new NotImplementedException();
    }

    protected override void OnLoading()
    {
        throw new NotImplementedException();
    }

    protected override void OnLoadPanel()
    {
        throw new NotImplementedException();
    }

    protected override void GetMustLoadAsset()
    {
        throw new NotImplementedException();
    }

    protected override int GetMustLoadedDataCount()
    {
        throw new NotImplementedException();
    }

    public override void OnClosed(GameObject obj)
    {
        throw new NotImplementedException();
    }
}
