using System;
using System.Diagnostics;

class HRTimer
{
    public Stopwatch timer;

    public HRTimer()
    {
        timer = new Stopwatch();
    }

    public void Start()
    {
        timer.Start();
    }

    public void Stop()
    {
        timer.Stop();
    }

    public void Reset()
    {
        timer.Reset();
    }

    public long getElapsedMS()
    {
        return timer.ElapsedMilliseconds;
    }

    public string getProperties()
    {
        string prop = "";
        if (Stopwatch.IsHighResolution)
        {
            prop += "\nUsing system's high-resolution performance-counter";
        }
        else
        {
            prop += "\nUsing DateTime class";
        }

        long frequency = Stopwatch.Frequency;
        prop += "\nFrequency (ticks/seconds) = " + frequency.ToString();

        long nanosecsPerTick = (1000L * 1000L * 1000L) / frequency;
        prop += "\nNanosecs per tick = " + nanosecsPerTick.ToString();

        return prop;
    }
}
