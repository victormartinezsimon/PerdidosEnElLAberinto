using UnityEngine;
using System.Collections;

public class Teletransporte : MonoBehaviour {

    public GameObject destino;
    public string tagPlayer="Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == tagPlayer)
        {
            other.gameObject.transform.position = destino.transform.position;
        }
    }

}
