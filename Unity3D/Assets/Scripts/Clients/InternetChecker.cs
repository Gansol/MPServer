using UnityEngine;

public class InternetChecker : MonoBehaviour
{
    public bool ConnStatus { get { return Global.connStatus; } }
    private const bool allowCarrierDataNetwork = false;
    private const string pingAddress = "8.8.8.8"; // Google Public DNS server
    private const float waitingTime = 2.0f;

    private Ping ping;
    private float pingStartTime;

    public void Start()
    {
        bool internetPossiblyAvailable;
        switch (Application.internetReachability)
        {
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                internetPossiblyAvailable = true;
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                internetPossiblyAvailable = allowCarrierDataNetwork;
                break;
            default:
                internetPossiblyAvailable = false;
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
                stopCheck = false;
            else
                InternetIsNotAvailable();
            if (stopCheck)
                ping = null;
        }
        else if (Time.time > pingStartTime + waitingTime && !Global.connStatus)
        {
            pingStartTime = Time.time;
            ping = new Ping(pingAddress);
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
        //Global.connStatus = false;
        Debug.Log("Connect.");
    }
}