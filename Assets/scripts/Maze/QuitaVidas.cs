using UnityEngine;
using System.Collections;

public class QuitaVidas : MonoBehaviour {

    public string tagPlayer = "Player";
    public int quitarVida = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == tagPlayer)
        {
            other.gameObject.GetComponent<MovementController>().quitarVida(1);
        }
    }
}
