using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelfTimer : MonoBehaviour
{

    public float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            Destroy(this.gameObject);
        }
    }
}
