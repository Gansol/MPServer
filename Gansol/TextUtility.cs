using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * 這個檔案是用來 檢驗資料、雜湊、加解密
 * 
 * ***************************************************************/

namespace Gansol
{
    public partial class TextUtility : Component
    {
        String DefaultPassword;
        byte[] DefaultSalt;

        public TextUtility()
        {
            InitializeComponent();

            DefaultPassword = "meul4hl3h;dj4";
            DefaultSalt = new byte[] { 0x05, 0x23, 0xA6, 0xA0, 0xE5, 0xF3, 0x23, 0x6F, 0xCC, 0xAE, 0x16, 0xB6 };

        }

        public TextUtility(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            DefaultPassword = "meul4hl3h;dj4";
            DefaultSalt = new byte[] { 0x05, 0x23, 0xA6, 0xA0, 0xE5, 0xF3, 0x23, 0x6F, 0xCC, 0xAE, 0x16, 0xB6 };
        }

        // ------  字串驗證 -----

        #region 台灣身份證檢查
        /// <summary>
        /// 身份證檢查
        /// </summary>
        /// <param name="idnumber">
        /// string idnumber ： 傳入的身份證號碼(字串)
        /// </param>
        /// <returns>
        /// 1 : 合法
        /// 2 : 不為10碼
        /// 3 : 第一碼不為英文大寫
        /// 4 : 後9碼不為數字
        /// 5 : 第2碼需為1(男生)或2(女生)
        /// 6 : 檢查碼錯誤
        /// 7 : 出現允許外的字元，例如中文字或引號
        /// </returns>
        public int IDNumberChk(string idnumber)
        {
            try
            {

                // 只能有英數字
                if (SaveTextChk(idnumber) != 1)
                {
                    return 7;		// 出現允許外的字元，例如中文字或引號
                }


                char[] eng = new char[26] {(char)10,(char)11,(char)12,(char)13,(char)14,(char)15,
										  (char)16,(char)17,(char)34,(char)18,(char)19,(char)20,
										  (char)21,(char)22,(char)35,(char)23,(char)24,(char)25,
										  (char)26,(char)27,(char)28,(char)29,(char)32,(char)30,
										  (char)31,(char)33};
                char[] idchar = new char[12];
                int i, sum;

                if (idnumber.Length != 10)
                {
                    return 2;		// 必需為10個字元
                }

                idchar = idnumber.ToCharArray();	// 轉成char陣列

                if (idchar[0] < 'A' || idchar[0] > 'Z')
                {
                    return 3;		// 第一碼不為英文大寫
                }

                for (i = 1; i < 10; i++)
                {
                    if (idchar[i] < '0' || idchar[i] > '9')
                    {
                        return 4;	// 後9碼有不為數字的字元
                    }
                }

                if (!(idchar[1] == '1' || idchar[1] == '2'))
                {
                    return 5;		// 第2碼需為1(男生)或2(女生)
                }

                // 接下來檢查檢查碼
                sum = 0;
                sum = (eng[idchar[0] - 'A']) / 10 + ((eng[idchar[0] - 'A']) % 10) * 9;
                for (i = 1; i <= 8; i++)
                {
                    sum += (idchar[i] - '0') * (9 - i);
                }
                sum += idchar[9] - '0';

                if (sum % 10 != 0)
                {
                    return 6;		// 檢查碼錯誤
                }

                return 1;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion

        #region EMail字串格式是否正確
        /// <summary>
        /// EMail字串格式是否正確
        /// </summary>
        /// <param name="EMail"></param>
        /// <returns>1:正確,2:有空白,3:沒有@符號,4:@不能是第一碼,5:超過一個@或沒有@,6:沒有「.」</returns>
        public int EMailChk(string EMail)
        {
            try
            {
                // 先檢查是否有特殊字元或中文字，只允許使用@ . - _
                if (SaveTextChk(EMail, new char[] { '@', '.', '_', '-' }) != 1)
                {
                    return 7;		// 出現允許外的字元，例如中文字或引號
                }

                if (EMail.IndexOf(" ") != -1)
                {
                    return 2;		// 不能有空白
                }

                // 先檢查是否有@符號
                if (EMail.IndexOf("@") == -1)
                {
                    return 3;
                }

                // @不能是第一碼
                if (EMail.IndexOf("@") == 0)
                {
                    return 4;
                }

                string[] EMailArray = EMail.Split(new char[] { '@' });
                if (EMailArray.Length != 2)
                {
                    return 5;	// 超過一個@或沒有@
                }

                if ((EMailArray[1].IndexOf(".") == -1) || (EMailArray[1].IndexOf(".") == 0))
                {
                    return 6;	// 後半段沒有「.」
                }

                return 1;
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }

        #endregion

        #region 字串檢查正確性，只能數字
        /// <summary>
        /// 只能是數字
        /// </summary>
        /// <returns>
        /// 1: 正確
        /// 2: 有容許外的字元
        /// </returns>
        public int NumTextChk(string InputStr)
        {
            try
            {
                int i = 0;
                bool result = int.TryParse(InputStr, out i);

                if (result)
                    return 1;
                else
                    return 2;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion

        #region 字串檢查正確性，只能16進制數字
        /// <summary>
        /// 只能16進制數字
        /// </summary>
        /// <returns>
        /// 1: 正確
        /// 2: 有容許外的字元
        /// </returns>
        public int Num16TextChk(string InputStr)
        {
            try
            {
                char[] idchar = InputStr.ToCharArray();	// 轉成char陣列


                // 不能有中文
                for (int i = 0; i < InputStr.Length; i++)
                {
                    // 檢查是否是英文字或數字
                    if ((idchar[i] < 'a' || idchar[i] > 'f') && (idchar[i] < 'A' || idchar[i] > 'F') && (idchar[i] < '0' || idchar[i] > '9'))
                    {
                        return 2;		// 只能是16進制數字
                    }

                }

                return 1;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion

        #region 安全字串檢查，只能英文或數字，不能有空白或特殊符號
        /// <summary>
        /// 安全字串檢查，只能英文或數字，不能有空白
        /// </summary>
        /// <param name="InputID"></param>
        /// <returns>
        /// 1: 正確
        /// 2: 有容許外的字元
        /// </returns>
        public int SaveTextChk(string InputID)
        {
            try
            {
                char[] idchar = InputID.ToCharArray();	// 轉成char陣列


                // 不能有中文
                for (int i = 0; i < InputID.Length; i++)
                {
                    // 檢查是否是英文字或數字
                    if ((idchar[i] < 'a' || idchar[i] > 'z') && (idchar[i] < 'A' || idchar[i] > 'Z') && (idchar[i] < '0' || idchar[i] > '9'))
                    {
                        return 2;		// 只能是英文或數字
                    }

                }

                return 1;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion

        #region 安全字串檢查，只能英文或數字，不能有空白，但可以有傳入的特殊符號
        /// <summary>
        /// 安全字串檢查，只能英文或數字，不能有空白，但可以有傳入的特殊符號
        /// 使用方式：
        /// SaveTextChk( MyString, new char[] {'@', '#', '$', '%'} )
        /// </summary>
        /// <returns>
        /// 1: 正確
        /// 2: 有容許外的字元，例如中文或空白
        /// </returns>
        public int SaveTextChk(string InputID, char[] Symbol)
        {
            try
            {
                char[] idchar = InputID.ToCharArray();	// 轉成char陣列
                bool allowsymbol = false;


                for (int i = 0; i < InputID.Length; i++)
                {

                    // 檢查是否是英文字或數字
                    if ((idchar[i] < 'a' || idchar[i] > 'z') && (idchar[i] < 'A' || idchar[i] > 'Z') && (idchar[i] < '0' || idchar[i] > '9'))
                    {
                        // 檢查是否為允許的特殊符號
                        allowsymbol = false;
                        for (int j = 0; j < Symbol.Length; j++)
                        {
                            if (idchar[i] == Symbol[j])
                            {
                                allowsymbol = true;
                            }
                        }
                        if (allowsymbol == false)
                        {
                            return 2;		// 只能是英文或數字
                        }
                    }

                }

                return 1;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion


        // ------  加解密 -----

        #region 編碼成Base64
        public string EncryptBase64String(string strPlainText)
        {
            try
            {
                Encoding encoding = Encoding.UTF8;
                byte[] bytPlainText = encoding.GetBytes(strPlainText);
                return Convert.ToBase64String(bytPlainText);
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion

        #region 將Base64字串還原
        public string DecryptBase64String(string strCipherText)
        {
            try
            {
                byte[] bytCipherText;
                Encoding uEncoding = Encoding.UTF8;

                bytCipherText = Convert.FromBase64String(strCipherText);
                return uEncoding.GetString(bytCipherText);
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion

        #region 雜湊編碼
        public string EncryptHashString(string SourceString)
        {
            try
            {
                Encoding uEncoding = Encoding.UTF8;
                byte[] bytSource = uEncoding.GetBytes(SourceString);

                SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
                byte[] bytHash = sha1.ComputeHash(bytSource);

                return Convert.ToBase64String(bytHash);
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }
        #endregion

        #region SHA1雜湊編碼
        /// <summary>
        /// SHA1雜湊編碼 Notice: Broken hash string!
        /// </summary>
        /// <param name="bytesFile"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string SHA1Complier(byte[] bytesFile)
        {
            byte[] bytes = bytesFile;

            // Convert the encrypted bytes back to a string (base 16)
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] hashBytes = sha1.ComputeHash(bytes);
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }
            return hashString.PadLeft(40, '0');
        }
        #endregion

        #region SHA512雜湊編碼
        public static string SHA512Complier(byte[] bytesFile)
        {
            byte[] bytes = bytesFile;

            // Convert the encrypted bytes back to a string (base 16)
            SHA512CryptoServiceProvider sha1 = new SHA512CryptoServiceProvider();
            byte[] hashBytes = sha1.ComputeHash(bytes);
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }
            return hashString.PadLeft(64, '0');
        }
        #endregion

        #region 金錀加密後傳回加密字串
        public string EncryptDerivedKey(String SrcString, String Password, Byte[] Salt)
        {
            try
            {
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, Salt);

                byte[] iv = new byte[] { 0xA0, 0x16, 0xBC, 0xF2, 0x08, 0x3C, 0x55, 0x68 };
                byte[] key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, iv);

                // Encrypt the data.
                TripleDES encAlg = TripleDES.Create();
                encAlg.Key = key;
                encAlg.IV = new byte[] { 0x06, 0xA2, 0xCC, 0x53, 0x2B, 0x33, 0x28, 0x2F };


                MemoryStream encryptionStream = new MemoryStream();
                CryptoStream encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);


                byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(SrcString);

                encrypt.Write(utfD1, 0, utfD1.Length);
                encrypt.FlushFinalBlock();
                encrypt.Close();
                byte[] edata1 = encryptionStream.ToArray();
                pdb.Reset();

                // 以Base-64編碼傳回
                return Convert.ToBase64String(edata1);

            }
            catch (Exception EX)
            {
                throw EX;
            }
        }


        // 當不傳Salt進來用預設的Salt值
        public string EncryptDerivedKey(String SrcString, String Password)
        {
            try
            {
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, DefaultSalt);

                byte[] iv = new byte[] { 0xA0, 0x16, 0xBC, 0xF2, 0x08, 0x3C, 0x55, 0x68 };
                byte[] key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, iv);

                // Encrypt the data.
                TripleDES encAlg = TripleDES.Create();
                encAlg.Key = key;
                encAlg.IV = new byte[] { 0x06, 0xA2, 0xCC, 0x53, 0x2B, 0x33, 0x28, 0x2F };


                MemoryStream encryptionStream = new MemoryStream();
                CryptoStream encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);


                byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(SrcString);

                encrypt.Write(utfD1, 0, utfD1.Length);
                encrypt.FlushFinalBlock();
                encrypt.Close();
                byte[] edata1 = encryptionStream.ToArray();
                pdb.Reset();

                // 以Base-64編碼傳回
                return Convert.ToBase64String(edata1);
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }


        // 當不傳Key進來用預設的Key值
        public string EncryptDerivedKey(string SrcString)
        {
            try
            {
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(DefaultPassword, DefaultSalt);

                byte[] iv = new byte[] { 0xA0, 0x16, 0xBC, 0xF2, 0x08, 0x3C, 0x55, 0x68 };
                byte[] key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, iv);

                // Encrypt the data.
                TripleDES encAlg = TripleDES.Create();
                encAlg.Key = key;
                encAlg.IV = new byte[] { 0x06, 0xA2, 0xCC, 0x53, 0x2B, 0x33, 0x28, 0x2F };


                MemoryStream encryptionStream = new MemoryStream();
                CryptoStream encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write);


                byte[] utfD1 = new System.Text.UTF8Encoding(false).GetBytes(SrcString);

                encrypt.Write(utfD1, 0, utfD1.Length);
                encrypt.FlushFinalBlock();
                encrypt.Close();
                byte[] edata1 = encryptionStream.ToArray();
                pdb.Reset();

                // 以Base-64編碼傳回
                return Convert.ToBase64String(edata1);
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }
        #endregion

        #region 金錀解密後傳回字串
        public string DecryptDerivedKey(String SrcString, String Password, Byte[] Salt)
        {
            try
            {
                Byte[] edata1 = Convert.FromBase64String(SrcString);

                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, Salt);

                byte[] iv = new byte[] { 0xA0, 0x16, 0xBC, 0xF2, 0x08, 0x3C, 0x55, 0x68 };
                byte[] key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, iv);


                TripleDES decAlg = TripleDES.Create();
                decAlg.Key = key;
                decAlg.IV = new byte[] { 0x06, 0xA2, 0xCC, 0x53, 0x2B, 0x33, 0x28, 0x2F };



                MemoryStream decryptionStreamBacking = new MemoryStream();
                CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
                decrypt.Write(edata1, 0, edata1.Length);
                decrypt.Flush();
                decrypt.Close();
                pdb.Reset();
                string data2 = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

                return data2;
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }

        public string DecryptDerivedKey(String SrcString, String Password)
        {
            try
            {
                Byte[] edata1 = Convert.FromBase64String(SrcString);

                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, DefaultSalt);

                byte[] iv = new byte[] { 0xA0, 0x16, 0xBC, 0xF2, 0x08, 0x3C, 0x55, 0x68 };
                byte[] key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, iv);


                TripleDES decAlg = TripleDES.Create();
                decAlg.Key = key;
                decAlg.IV = new byte[] { 0x06, 0xA2, 0xCC, 0x53, 0x2B, 0x33, 0x28, 0x2F };



                MemoryStream decryptionStreamBacking = new MemoryStream();
                CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
                decrypt.Write(edata1, 0, edata1.Length);
                decrypt.Flush();
                decrypt.Close();
                pdb.Reset();
                string data2 = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

                return data2;
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }


        // 當不傳Key進來用預設的Key值
        public string DecryptDerivedKey(string SrcString)
        {

            try
            {
                Byte[] edata1 = Convert.FromBase64String(SrcString);

                PasswordDeriveBytes pdb = new PasswordDeriveBytes(DefaultPassword, DefaultSalt);

                byte[] iv = new byte[] { 0xA0, 0x16, 0xBC, 0xF2, 0x08, 0x3C, 0x55, 0x68 };
                byte[] key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, iv);


                TripleDES decAlg = TripleDES.Create();
                decAlg.Key = key;
                decAlg.IV = new byte[] { 0x06, 0xA2, 0xCC, 0x53, 0x2B, 0x33, 0x28, 0x2F };



                MemoryStream decryptionStreamBacking = new MemoryStream();
                CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write);
                decrypt.Write(edata1, 0, edata1.Length);
                decrypt.Flush();
                decrypt.Close();
                pdb.Reset();
                string data2 = new UTF8Encoding(false).GetString(decryptionStreamBacking.ToArray());

                return data2;
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }
        #endregion


        // ------  解析字串 -----

        #region 序列化字串
        /// <summary>
        ///  把資料轉換為Byte[]資料
        /// </summary>
        public static byte[] SerializeToStream(object UnSerializeObj)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, UnSerializeObj);
            return stream.ToArray();
        }
        #endregion

        #region 反序列化物件
        /// <summary>
        ///  把Byte[]資料還原
        /// </summary>
        public static object DeserializeFromStream(byte[] SerializeArray)
        {
            MemoryStream stream = new MemoryStream(SerializeArray);
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object UnSerializeObj = formatter.Deserialize(stream);
            return UnSerializeObj;
        }
        #endregion

    }
}
