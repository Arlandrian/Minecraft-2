using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnections : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnConnectedToServer()
    {
        Debug.Log("CONNECTED TO SERVER");
        CmdSetSeed();
    }

    /*
    private void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("NEW PLAYER CONNECTED -- IP : " + player.externalIP);
        CmdSetSeed();
    }
    */

    [Command]
    void CmdSetSeed()
    {
        Debug.Log("COMMAND IS WORKING");
        RpcSetSeed(Utils.seed);
    }

    [ClientRpc]
    void RpcSetSeed(int seed)
    {
        Debug.Log("RPC IS WORKING");
        Utils.seed = seed;
    }
    



}
