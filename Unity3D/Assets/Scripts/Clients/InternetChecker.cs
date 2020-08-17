using UnityEngine;

public class InternetChecker : MonoBehaviour
{
    public bool ConnStatus { get { return Global.connStatus; } }
    private const bool allowCarrierDataNetwork = true;  // 同意使用 MOBILE 網路
    private const string pingAddress = "8.8.8.8"; // Google Public DNS server
    private const float waitingTime = 2.0f;

    private Ping ping;
    private float pingStartTime;

    public void Start()
    {
        bool internetPossiblyAvailable;
        switch (Application.internetReachability)
        {
            case NetworkReachability.ReachableViaLocalAreaNetwork:      // WIFI or USB NETWORK
                internetPossiblyAvailable = true;
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork:    // MOBILE NETWORK
                internetPossiblyAvailable = allowCarrierDataNetwork;
                break;
            default:
                internetPossiblyAvailable = false;                      // 沒網路
                break;
        }
        if (!internetPossiblyAvailable)
        {
            InternetIsNotAvailable();
            return;
        }
        ping = new Ping(pingAddress);
        pingStartTime = Time.time;
    }

    public void Update()
    {
        if (ping != null)
        {
            bool stopCheck = true;
            if (ping.isDone)
            {
                if (ping.time >= 0)
                    InternetAvailable();
                else
                    InternetIsNotAvailable();
            }
            else if (Time.time - pingStartTime < waitingTime)
            {
                stopCheck = false;
            }
            else
            {
                InternetIsNotAvailable();
            }

            if (stopCheck)
                ping = null;
        }
        else if (Time.time > pingStartTime + waitingTime && !Global.connStatus)
        {
            ping = new Ping(pingAddress);
            pingStartTime = Time.time;
        }
    }


    private void InternetIsNotAvailable()
    {
        Global.connStatus = false;
        Debug.Log("Disconnect.");
    }

    private void InternetAvailable()
    {
        Global.connStatus = true;
        Debug.Log("Connect.");
    }
}