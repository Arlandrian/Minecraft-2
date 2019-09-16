using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public World worldScript;

    public event System.Action OnGameStart;
    public event System.Action OnGameStop;

    public bool gameStarted = false;

    public List<GameObject> fenceObjectsForNonConnected;


    //username,playersetup instance
    public Dictionary<string, PlayerSetup> userDictionary;

    public Dictionary<string, Fence> fenceDictionary;

    public static GameManager instance = null;            

    void Awake() {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        fenceDictionary = new Dictionary<string, Fence>();
        userDictionary = new Dictionary<string, PlayerSetup>();
    }

    private void Start() {
        LoadUserConfig();
        fenceObjectsForNonConnected = new List<GameObject>();
    }



    // Update is called once per frame
    void Update () {
        if (!gameStarted) {
            if (Camera.main != null) {
                GameObject.FindGameObjectWithTag("Image").GetComponent<CanvasRenderer>().SetAlpha(0);
                gameStarted = true;
                OnGameStart();
                SaveUserData();

            }
        }
        else if (Camera.main == null) {
            GameObject.FindGameObjectWithTag("Image").GetComponent<CanvasRenderer>().SetAlpha(255);
            worldScript.gameObject.SetActive(false);
            gameStarted = false;
            OnGameStop();
        }

    }

    bool LoadUserConfig() {
        string filename = Application.persistentDataPath + "/UserConfig.txt";
        if (File.Exists(filename)) {
            try {
                FileStream file = File.OpenRead(filename);

                StreamReader sr = new StreamReader(file);

                string username = sr.ReadLine();
                string seed = sr.ReadLine();

                GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
                canvas.transform.GetChild(2).GetComponent<InputField>().text = username;
                canvas.transform.GetChild(3).GetComponent<InputField>().text = seed;


                sr.Close();
                file.Close();
            } catch(IOException ex) {
                Debug.LogError(ex.Message);
                return false;
            }

            return true;
        }

        return false;
    }

    void SaveUserData() {
        string filename = Application.persistentDataPath + "/UserConfig.txt";
        
        try {
            if (!File.Exists(filename)) {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }

            FileStream file = File.OpenWrite(filename);

            StreamWriter wr = new StreamWriter(file);
            GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");

            wr.WriteLine(canvas.transform.GetChild(2).GetComponent<InputField>().text);
            wr.WriteLine(canvas.transform.GetChild(3).GetComponent<InputField>().text);

            canvas.transform.GetChild(2).gameObject.SetActive(false);
            canvas.transform.GetChild(3).gameObject.SetActive(false);

            wr.Close();
            file.Close();
        } catch (IOException ex) {
            Debug.LogError(ex.Message);
                
        }

    }
}
