using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class LocalNetwork : NetworkBehaviour {

    public GameObject m_player;
    public SpanwManager spawnPoints;
    public int ID = -1;


    private static LocalNetwork m_instance;

    public static LocalNetwork getInstance()
    {
        return m_instance;
    }

    void Start()
    {
        if(m_instance != null)
        {
            if(m_instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
        } 
        else
        {
            m_instance = this;
        }

        getMyId();
    }
    private GameObject instantiateNetworkObject(GameObject go)
    {

        GameObject go2 = Instantiate(go, spawnPoints.spawnPoints[ID].transform.position, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(go2);
        return go2;
    }

    private void getMyId()
    {
        LocalNetwork[] lns = FindObjectsOfType<LocalNetwork>();
        bool[] avaliable = { true, true, true, true };
        for(int i = 0; i < lns.Length; i++)
        {
            if(lns[i].ID != -1)
            {
                avaliable[lns[i].ID] = false;
            }
        }
        
        for(int i = 0; i < avaliable.Length; i++)
        {
            if (avaliable[i])
            {
                ID = i;
                break;
            }
        }


    }

    public void findSpawnPoints()
    {
        spawnPoints = FindObjectOfType<SpanwManager>();
    }

    public void mazeGenerated()
    {
        findSpawnPoints();
        if (isLocalPlayer)
        {
            RpcinstantiatePlayer();
        }
    }


    [ClientRpc]
    void RpcinstantiatePlayer()
    {
        findSpawnPoints();
        instantiateNetworkObject(m_player);
        CmdPlayerInstantiated();
    }

    int cuenta = 1;//me cuento a mi mismo de inicio
    [Command]
    void CmdPlayerInstantiated()
    {
        Debug.Log("Player instantiated");
        cuenta++;
        if(cuenta >= FindObjectsOfType<LocalNetwork>().Length)
        {
            RpcStartGame();
        }
    }

    [ClientRpc]
    void RpcStartGame()
    {
        Debug.Log("STart game");
    }

    /*
    public void mazeGeneratorEnded()
    {
        //RpcinstantiatePlayer();
        //instantiateNetworkObject(m_player);
    }
    

    [ClientRpc]
    void RpcinstantiatePlayer()
    {
        instantiateNetworkObject(m_player);
        CmdPlayerInstantiated();
    }

    [Command]
    void CmdPlayerInstantiated()
    {
        Debug.Log("Player instantiated");
    }

    [ClientRpc]
    void RpcStartGame()
    {


    }
    */


}
