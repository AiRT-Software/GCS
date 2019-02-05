
public class GlobalSettings
{
    private string ip;
    private float serverTimeout;
    private static GlobalSettings instance_;  //single instance

    // constructor
    private GlobalSettings()
    {
        ip = "";
        serverTimeout = 2.0f;  // seconds
    }

    public static GlobalSettings Instance  //lets accessing to the single instance
    {
        get
        {
            if (instance_ == null)
            {
                instance_ = new GlobalSettings();
            }
            return instance_;
        }
    }

    public void setIP(string ip)
    {
        this.ip = ip;
    }

    public string getIP()
    {
        return this.ip;
    }

    public string getRequestPort()
    {
        return "tcp://" + this.ip + ":5556";
    }

    public string getSubscriptionPort()
    {
        return "tcp://" + this.ip + ":5555";
    }

    public float getServerTimeout()
    {
        return serverTimeout;
    }
}