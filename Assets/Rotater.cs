using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotater : MonoBehaviour
{
    public int speed;
    void Update()
    {
        transform.RotateAround(this.gameObject.transform.position, Vector3.forward, speed * Time.deltaTime);
    }
}
