  using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{

    private bool timerActive;

    private float maxTimeOut = 5f;
    

    private float currentTime;
    public Timer(float timeOut , bool start =false){
        maxTimeOut = timeOut;
        timerActive = start;
        currentTime = maxTimeOut;
        
    }

    public Timer(){
        timerActive = false;
    }

    public bool RunTimer(){
        if(!timerActive){
            //Debug.LogError("Timer is not active");
            return false;
        }
        if(currentTime <= 0) 
                return false;

        currentTime -=Time.deltaTime;
        return true;
    }

    public bool IsTimeOut()
    {
        if(currentTime <= 0)
        {
            return true;
        }
        return false;
    }

    public void RestTimer() {
        currentTime = maxTimeOut;
        timerActive = true;
    }

    public void StopTimer(){
        timerActive = false;
    }

    public int TimeRemaining(){
        return (int) currentTime;
    }

    public void SetTimer(float time){
        maxTimeOut = time;
        currentTime = time;
    }
    public void ActivateTimer(bool start){
        timerActive = start;
    }

    public bool IsTimerActive(){
        return timerActive;
    }
}
