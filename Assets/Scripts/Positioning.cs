using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Positioning : MonoBehaviour
{
   public GameObject backPos;
    void Start()
    {
        this.gameObject.transform.position = backPos.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
