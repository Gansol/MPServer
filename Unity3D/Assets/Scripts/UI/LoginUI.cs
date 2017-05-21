﻿using UnityEngine;
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

public class LoginUI : MonoBehaviour
{
    public GameObject LoginPanel;
    public GameObject JoinPanel;
    public GameObject MatchGame;
    public GameObject LicensePanel;
    public UIInput accountField, passwordField;
    public UILabel LoginMessageBox;
    public UIToggle accountToggle, passwordToggle;

    public GameObject[] LoginType;
    AssetLoader assetLoader;
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
    private string jFileName = "data.json";
    private string jString;
    //private string JoinRoomResult = "";
    //private bool macthing = false;
    private static bool isLoginBtn = false;
    private bool bSwitchLoginType;
    private int emailChk, passwordChk, confirmPasswordChk, equalPassword, nicknameChk;
    Dictionary<string, object> data;

    GameObject tmpPanel;

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
    void Start()
    {
        assetLoader = gameObject.AddMissingComponent<AssetLoader>();
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


    void OnGUI()
    {
        //if (Global.photonService.ServerConnected)  // 若已連線成功才顯示登入對話盒
        //{
        //    GUI.Label(new Rect(130, 10, 100, 20), "Connecting . . ."); // 已連線
        //}

        ShowChkMsg();
    }


    public void SwitchLoginType()
    {
        if (bSwitchLoginType)
        {
            LoginType[0].SetActive(true);
            LoginType[1].SetActive(false);
        }
        else
        {
            LoginType[0].SetActive(false);
            LoginType[1].SetActive(true);
        }
        bSwitchLoginType = !bSwitchLoginType;
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
        Global.ShowMessage("登入中...", Global.MessageBoxType.NonChkBtn);
        LoginPanel.SetActive(false);
        isLoginBtn = true;
        Global.Hash = Encrypt(password.value);
        char[] splitChar = new char[] { '@' };
        string[] account = email.text.Split(splitChar);
        Global.Account = account[0];
        Global.MemberType = MemberType.Gansol;
        Global.photonService.Login(Global.Account, Global.Hash, MemberType.Gansol); // 登入
    }

    public void OpenJoinPanel(GameObject myPanel, GameObject joinPanel)
    {
        EventMaskSwitch.Switch(myPanel, true);
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


            char[] splitChar = new char[] { '@' };
            string[] account = email.text.Split(splitChar);
            Global.Account = account[0];
            Global.Hash = Encrypt(password.value);
            Global.MemberType = MemberType.Gansol;

            //SaveLoginInfo(accountToggle.value, passwordToggle.value);


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

    private void SaveLoginInfo(bool memAccount, bool memPD)
    {
        bool bChange = false;
        StartCoroutine(LoadFile(Global.dataPath + "data.json"));

        Dictionary<string, object> data = Json.Deserialize(jString) as Dictionary<string, object>;

        if (memAccount)
        {
            if (!data.ContainsKey("Account"))
            {
                data.Add("Account", Global.Account);
            }
            else if (data["Account"] != Global.Account)
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
            else if (data["Hash"] != Global.Hash)
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
        WWW www = new WWW(filePath);
        yield return www;
        jString = www.text;
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
            Global.ShowMessage("登入成功！", Global.MessageBoxType.Default);
            //EventMaskSwitch.PrevToFirst();
            Global.photonService.LoadItemData();
            LoginPanel.SetActive(false);
        }
        else // 若登入失敗，取得錯誤回傳字串
        {
            isLoginBtn = false;
            LoginPanel.SetActive(true);
            LoginMessageBox.gameObject.SetActive(true);
            Global.ShowMessage(message, Global.MessageBoxType.Default);
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
            Global.ShowMessage("登入中...",Global.MessageBoxType.NonChkBtn);
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

                    Global.ShowMessage("登入中...",Global.MessageBoxType.NonChkBtn);
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
            FB.API("/me?fields=id,name,gender,email,birthday", Facebook.HttpMethod.GET, GetFBProfiler);
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

    // 普通登入時使用
    // FB Login
    void GetFBProfiler(FBResult result)
    {

        if (result.Error != null)
        {
            Debug.Log("Get FB profiler error!");
            FB.API("/me?fields=id,name,gender,email,birthday", Facebook.HttpMethod.GET, GetFBProfiler);
            return;
        }
        //Global.MemberType = MemberType.Facebook;
        //if (!Global.photonService.ServerConnected) 
        //    gameObject.GetComponent<PhotonConnect>().ConnectToServer();

        FBProfiler = Json.Deserialize(result.Text) as Dictionary<string, object>;
        Debug.Log(FBProfiler["id"].ToString());
        Debug.Log(FBProfiler["name"].ToString());
        Debug.Log(FBProfiler["email"].ToString());
        Debug.Log(FBProfiler["gender"].ToString());
        Debug.Log(FBProfiler["birthday"].ToString());

        string[] tmp = FBProfiler["email"].ToString().Split('@');
        Global.Account = tmp[0];
        Global.Hash = Encrypt(Global.Account);
        Global.MemberType = MemberType.Facebook;
        Global.photonService.Login(Global.Account, Global.Hash, MemberType.Facebook); // 登入

        foreach (var item in FBProfiler)
        {
            Debug.Log("KEY:" + item.Key.ToString() + "Value:" + item.Value.ToString());
        }

        LoginPanel.SetActive(false);
        Global.ShowMessage("登入中...",Global.MessageBoxType.NonChkBtn);
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
        LoginPanel.SetActive(true);
        LoginMessageBox.gameObject.SetActive(true);
        LoginMessageBox.color = Color.red;
        LoginMessageBox.text = "X  " + "重複登入！";
    }

    // 加入會員後 再度取得資料並登入
    void OnGetProfile()
    {
        Global.ShowMessage("登入中...",Global.MessageBoxType.NonChkBtn);
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
}
