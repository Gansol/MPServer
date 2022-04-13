using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachBtn_LoginUI : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject joinPanel;
    public GameObject licensePanel;

  public UILabel loginMessageBox;
    public UILabel login_AccountField, login_PasswordField, join_AccountField, join_PasswordField, join_ConfrimPasswordField, join_NicknameField, join_AgeField, join_SexField;

    public UIToggle accountToggle, passwordToggle, login_AgreeLicenseToggle, join_AgreeLicenseToggle;

    public GameObject snsLogin, gansolLogin;
    public GameObject facebookLoginBtn, gansolLoginBtn, joinBtn, switch_Btn, join_JoinBtn, join_ExitBtn;
    public UILabel errorText_Email, errorText_Password, errorText_Password2, errorText_NickName;

}
