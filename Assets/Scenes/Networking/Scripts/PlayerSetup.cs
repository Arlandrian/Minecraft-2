using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] componentsToDisable;

    [SyncVar]
    public int seed;

    [SyncVar]
    public string username;

    public Fence fence;

    public List<GameObject> fenceObjects;

    public GameManager gm;
	// Use this for initialization
	void Start () {

        gm = GameManager.instance;
        fenceObjects = new List<GameObject>();

        if (!isLocalPlayer) {
            for(int i = 0; i < componentsToDisable.Length; i++) {
                componentsToDisable[i].enabled = false;
            }
            return;
        }


        StartCoroutine(StartingQueue());

        gm.OnGameStart += GameStarting;
        gm.OnGameStop += GameStopped;

    }
    
    IEnumerator StartingQueue() {

        if (isServer) {

            username = UIScript.username;

            InitFenceForUser();

            seed = Utils.seed;
            WorldStart();
            GetAllFencesServer();

            foreach (Fence item in GameManager.instance.fenceDictionary.Values) {
                PlayerSetup ps;
                if (IsThisUserInGame(item.ownerName,out ps)) {
                    if(item.ownerName == username) {
                        DrawFence(item, this);
                    } else {
                        ps.DrawFence(item, ps);
                    }
                } else {
                    DrawFenceGM(item, gm);
                }
            }
        } else {
            username = UIScript.username;

            CmdSetUsername(username);
            yield return new WaitWhile(() => !isUsernameSet);

            InitFenceForUser();

            Debug.Log("SEED: "+Utils.seed);
            CmdSetSeed();

            //yield return new WaitForSeconds(0.5f);
            yield return new WaitWhile(() => !isSeedSet);
            WorldStart();

            CmdGetAllFencesClient(username, GetComponent<NetworkIdentity>());
            /*
            yield return new WaitWhile(() => !isFenceDatasReceived);
            */
            this.CmdDrawFence();
            
        }
    }

    #region Seed


     public override void OnStartClient() {
         //Runs on every PlayerSetup when a client connects JUST WHAT I NEED
         
         base.OnStartClient();

     }

    private void OnConnectedToServer() {
        Debug.Log("-----------------CONNECTED TO SERVER-----------------");
        Debug.Log("-----------------CONNECTED TO SERVER-----------------");
        Debug.Log("-----------------CONNECTED TO SERVER-----------------");
        Debug.Log("-----------------CONNECTED TO SERVER-----------------");
        CmdSetSeed();

    }

    [Command]
    void CmdSetSeed() {
        Debug.Log("COMMAND SET SEED");
        this.seed = Utils.seed;
        TargetRpcSetSeed(connectionToClient, Utils.seed);
    }
    bool isSeedSet = false;

    [TargetRpc]
    void TargetRpcSetSeed(NetworkConnection target, int seed) {
        Debug.Log("Setting targets seed to "+seed);
        Utils.seed = seed;
        this.isSeedSet = true;
    }

    #endregion

    #region Fence Initialize

    [Command]
    void CmdDrawFence() {
        Debug.Log("Cmd Draw Fence 231");
        RpcDrawFence();
    }

    [ClientRpc]
    void RpcDrawFence() {
        Debug.Log("Rpc Draw Fence 232");
        ClearFenceFromWorld(this);
        ClearFenceFromWorldGM(gm);
        Debug.Log("Rpc Draw Fence 233");

        foreach (Fence item in GameManager.instance.fenceDictionary.Values) {
            PlayerSetup ps;
            if (IsThisUserInGame(item.ownerName, out ps)) {
                if (item.ownerName == username) {
                    DrawFence(item, this);
                } else {
                    ps.DrawFence(item, ps);
                }
            } else {
                DrawFenceGM(item, gm);
            }
        }
        Debug.Log("Rpc Draw Fence 234");

    }

    void InitFenceForUser() {
        fence = new Fence(username);
        GameManager.instance.fenceDictionary.Add(username, fence);
    }

    [Command]
    void CmdGetAllFencesClient(string username, NetworkIdentity target) {
        Fence fi;
        if(!GameManager.instance.fenceDictionary.TryGetValue(username,out fi))
            GameManager.instance.fenceDictionary.Add(username, new Fence(username));
        Debug.Log("--------CMD Serializition--START-");
        byte[] serializedFenceDic = SerializeDictionary(GameManager.instance.fenceDictionary);
        Debug.Log("--------CMD Serializition--FINE-");

        TargetSetSerializedDictionary(target.connectionToClient, serializedFenceDic);
    }

    byte[] SerializeDictionary(Dictionary<string, Fence> dict) {
        BinaryFormatter bf = new BinaryFormatter();

        FenceData[] fenceData = new FenceData[dict.Count];
        int i = 0;
        foreach (Fence item in dict.Values) {
            fenceData[i] = Fence.FenceToData(item);
            i++;
        }
        using (MemoryStream ms = new MemoryStream()) {
            bf.Serialize(ms, fenceData);
            return ms.ToArray(); 
        }
    }
    bool isFenceDatasReceived = false;
    [TargetRpc]
    void TargetSetSerializedDictionary(NetworkConnection target,byte [] serializedDic) {
        
        Debug.Log("--------Target DE Serializition--START-");
        GameManager.instance.fenceDictionary = DesirializeDictionary(serializedDic);
        Debug.Log("--------Target DE Serializition--FINE-");

        isFenceDatasReceived = true;
    }

    Dictionary<string, Fence> DesirializeDictionary(byte[] serializedDic) {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream mStream = new MemoryStream(serializedDic);
        FenceData[] fenceDatas = bf.Deserialize(mStream) as FenceData[];
        Dictionary<string, Fence> dictionary = new Dictionary<string, Fence>();
        foreach(FenceData fData in fenceDatas) {
            Fence fi = Fence.DataToFence(fData);
            dictionary.Add(fi.ownerName, fi);
        }
        return dictionary;
    }

    ////Dosyadan fenceleri fence dictionary e okur
    void GetAllFencesServer() {
        string foldername = Application.persistentDataPath + "/savedata/" + seed + "/fence/";
        if (!Directory.Exists(foldername))
            Directory.CreateDirectory(foldername);
        try {
            DirectoryInfo di = new DirectoryInfo(foldername);
            FileInfo[] files = di.GetFiles();

            BinaryFormatter bf = new BinaryFormatter();

            foreach (FileInfo file in files) {

                if (File.Exists(file.ToString())) {

                    FileStream fileStream = File.Open(file.ToString(), FileMode.Open);
                    FenceData fenceData = new FenceData();
                    fenceData = (FenceData)bf.Deserialize(fileStream);

                    Fence fence = new Fence(fenceData.ownerName);

                    for (int i = 0; i < fenceData.polygon.Length; i++) {

                        fence.polygon.Add(new Vector3(fenceData.polygon[i].x, fenceData.polygon[i].y, fenceData.polygon[i].z));
                    }

                    Fence temp;
                    if (gm.fenceDictionary.TryGetValue(fence.ownerName,out temp)) {
                        this.fence = fence;
                        gm.fenceDictionary[fence.ownerName] = fence;
                    } else {
                        this.fence = fence;
                        gm.fenceDictionary.Add(fence.ownerName, fence);
                    }

                    fileStream.Close();

                }

            }
        } catch (IOException ex) {

            Debug.LogError(ex.Message);
        }
    }
    #endregion

    #region Fence Synchronization

    [Command]
    public void CmdFenceSync(string usrname) {
        Fence fi;
        if (GameManager.instance.fenceDictionary.TryGetValue(usrname,out fi)) {
            fi.Save();
        }
    }
  
    [ClientRpc]
    public void RpcFenceSync(bool isAdding,Vector3 blockWorldPos,string usr) {
        
        Fence fenceT;
        Debug.Log("RPC Fence Sync");
        if (GameManager.instance.fenceDictionary.TryGetValue(username, out fenceT)) {
            Debug.Log("RPC Fence Sync user: "+username);

            if (isAdding) {
                
                fenceT.polygon.Add(blockWorldPos);
                DrawFence(fenceT,this);

            } else {
                fenceT.polygon.Remove(blockWorldPos);
                DrawFence(fenceT, this);
            }
            if(username == usr) {
                this.CmdFenceSync(username);
            }
        } else {
            Debug.LogError(username + " fence could not found in Dictionary");
        }
    }

    #endregion

    #region Game Start Stop
    private void OnDestroy() {

    }

    void GameStopped() {
        //gm.
    }

    void GameStarting() {
        GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
        canvas.transform.GetChild(4).gameObject.SetActive(false);
        canvas.transform.GetChild(5).gameObject.SetActive(false);

    }

    void WorldStart() {
        GameManager.instance.worldScript.gameObject.SetActive(true);
        gm.worldScript.player = gameObject;
    }

    private void OnApplicationQuit() {
        OnDestroy();
    }

    #endregion

    #region Fence Drawing

    //returns two nearest fence
    public void ClearFenceFromWorld(PlayerSetup pSetup) {
        foreach (GameObject item in pSetup.fenceObjects) {
            DestroyImmediate(item);
        }
        pSetup.fenceObjects.Clear();
        
    }

    public void DrawFence(Fence fence, PlayerSetup pSetup) {

        //fence = fi;
        ClearFenceFromWorld(pSetup);

        if (fence.polygon.Count <= 1)
            return;

        for (int i = 0; i < fence.polygon.Count - 1; i++) {
            FenceObject fObj = new FenceObject(fence.polygon[i], fence.polygon[i + 1], this);
            pSetup.fenceObjects.Add(fObj.gameObj);
            //fObj.gameObj.transform.parent = this.gameObject.transform;

        }

        FenceObject fObj2 = new FenceObject(fence.polygon[0], fence.polygon[fence.polygon.Count - 1], this);
        pSetup.fenceObjects.Add(fObj2.gameObj);
        //fObj2.gameObj.transform.parent = this.gameObject.transform;
        Debug.Log(fence.polygon.Count + " ----Fences Created-----");
        
    }

    public void ClearFenceFromWorldGM(GameManager gm) {
        foreach (GameObject item in gm.fenceObjectsForNonConnected) {
            DestroyImmediate(item);
        }
        gm.fenceObjectsForNonConnected.Clear() ;

    }

    public void DrawFenceGM(Fence fence, GameManager gm) {

        //fence = fi;
        ClearFenceFromWorldGM(gm);

        if (fence.polygon.Count <= 1)
            return;

        for (int i = 0; i < fence.polygon.Count - 1; i++) {
            FenceObject fObj = new FenceObject(fence.polygon[i], fence.polygon[i + 1], this);
            gm.fenceObjectsForNonConnected.Add(fObj.gameObj);
            //fObj.gameObj.transform.parent = this.gameObject.transform;

        }

        FenceObject fObj2 = new FenceObject(fence.polygon[0], fence.polygon[fence.polygon.Count - 1], this);
        gm.fenceObjectsForNonConnected.Add(fObj2.gameObj);
        //fObj2.gameObj.transform.parent = this.gameObject.transform;
        Debug.Log(fence.polygon.Count + " ----Fences Created-----");

    }

    public void DrawFences() {

        if(fence== null) {
            if(!GameManager.instance.fenceDictionary.TryGetValue(username,out fence)) {
                fence = new Fence(username);
            }
        }
        if (fence.polygon.Count <= 1) 
            return;

        for (int i = 0; i < fence.polygon.Count - 1; i++) {
            FenceObject fObj = new FenceObject(fence.polygon[i], fence.polygon[i + 1], this);
            fenceObjects.Add(fObj.gameObj);
            //fObj.gameObj.transform.parent = this.transform;
        }
        
        FenceObject fObj2 = new FenceObject(fence.polygon[0], fence.polygon[fence.polygon.Count - 1], this);
        fenceObjects.Add(fObj2.gameObj);
        //fObj2.gameObj.transform.parent = this.transform;
        
    }

    #endregion

    #region Boundary Controls
    
    public bool CheckFences( Vector3 blockWorldPos,string tryingUser) {
        //PlayerSetup [] pSetups = FindObjectsOfType<PlayerSetup>();

        foreach(Fence fi in GameManager.instance.fenceDictionary.Values) {
            if (fi.ownerName != tryingUser) {
                if (RedStoneControl(fi.polygon, blockWorldPos, tryingUser)) {

                    return true;
                } else if (fi.IsInside(blockWorldPos)) {
                    return true;
                } 
            }
        }
        return false;
    }

    bool RedStoneControl(List<Vector3> pol,Vector3 blockWorldPos, string tryingUser) {

        for (int i = 0; i < pol.Count; i++) {

            if(pol[i] == blockWorldPos) {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Username Synch

    bool isUsernameSet = false;
    [Command]
    void CmdSetUsername(string usr) {
        this.username = usr;
        TargetSetUsername(connectionToClient);
    }

    [TargetRpc]
    void TargetSetUsername(NetworkConnection target) {
        isUsernameSet = true;
    }

    bool IsThisUserInGame(string username, out PlayerSetup ps) {
        GameObject[] users = GameObject.FindGameObjectsWithTag("FPSController");
        for (int i = 0; i < users.Length; i++) {
            PlayerSetup tmp = users[i].GetComponent<PlayerSetup>();
            if (tmp.username == username) {
                ps = tmp;
                return true;
            }
        }
        ps = null;
        return false;
    }
    #endregion
}
