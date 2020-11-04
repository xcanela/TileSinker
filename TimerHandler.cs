using System;
using UnityEngine.UI;
using UnityEngine;


public class TimerHandler : MonoBehaviour
{
    public float timeRemaining;
    public GameObject timerDisplay;
    
    
    void Update()
    {
        timerDisplay.GetComponent<Text>().text = DisplayTime(timeRemaining);

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = 0;
        }
    }

    string DisplayTime(float timeToDisplay)
    {
        string secondsstring;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        if (seconds < 10)
        {
            secondsstring = "0" + Convert.ToString(seconds);
        }
        else
        {
            secondsstring = Convert.ToString(seconds);
        }

        return (Convert.ToString(minutes) + ":" + secondsstring);
    }
       
}
