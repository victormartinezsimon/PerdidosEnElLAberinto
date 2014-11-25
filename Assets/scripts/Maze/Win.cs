using UnityEngine;
using System.Collections;

public class Win : MonoBehaviour 
{
    private NetworkManager manager;
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("Network").GetComponent<NetworkManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        Destroy(transform.parent.gameObject);
       // Destroy(this.gameObject);
        manager.endGame2(other.name);
        /*
        if (other.networkView.isMine)
        {
            manager.winner();
        }
        */
    }
}
