﻿using UnityEngine;
using System.Collections;

public class Trampa : MonoBehaviour 
{
    public float porcentajeTrampa = 0.3f;
    public float velocidadAnimacion = 1.0f;

    void Start()
    {
        float value = Random.value;
        if (porcentajeTrampa >= value)
        {
            this.enabled = false;
        }
        animation["UpDown"].speed = velocidadAnimacion;

    }



    void OnColliderEnter(Collider other)
    {
        animation.Play("UpDown");
        Debug.Log("damage player");
    }
	

}
