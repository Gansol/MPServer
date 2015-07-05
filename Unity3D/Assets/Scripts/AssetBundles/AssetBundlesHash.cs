using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
public class AssetBundlesHash : MonoBehaviour
{
    public string MD5Complier(byte[] bytesFile)
    {
//        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = bytesFile;

        // encrypt bytes
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);
        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }
        return hashString.PadLeft(32, '0'); //如不滿32字填補0至32字元
    }

    public string SHA1Complier(byte[] bytesFile)
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
}
