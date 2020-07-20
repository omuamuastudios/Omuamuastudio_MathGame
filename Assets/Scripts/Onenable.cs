using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Onenable : MonoBehaviour
{
    public GameObject action;
    public int waitTime;
    private void Start()
    {
       StartCoroutine( OnActive());
    }
   IEnumerator OnActive()
    {
        yield return new WaitForSeconds(waitTime);
        action.SetActive(true);
    }
}
