using UnityEngine;
using System.Collections;

public class KillPlane : MonoBehaviour 
{
    void OnTriggerEnter(Collider other)
    {
        other.gameObject.SendMessage("restart");
    }
}
