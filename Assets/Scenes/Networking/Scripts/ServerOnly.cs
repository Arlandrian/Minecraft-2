using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerOnly : NetworkBehaviour {

    public static int seed;

    private void Start() {
        DontDestroyOnLoad(gameObject);
        //GameManager.instance.OnGameStart += GameStarting;
    }

    void GameStarting() {
        seed = Utils.seed;
    }

}
