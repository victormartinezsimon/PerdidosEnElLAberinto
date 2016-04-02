using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MyNetworkLobbyPlayer : NetworkLobbyPlayer
{


    [SyncVar(hook = "OnMyName")]
    public string playerName = "";

    public void OnMyName(string newName)
    {
        playerName = newName;
    }

    public void OnReadyClicked()
    {
        SendReadyToBeginMessage();
    }

}
