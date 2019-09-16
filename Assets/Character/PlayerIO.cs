using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerIO : NetworkBehaviour {

	public static PlayerIO currentPlayerIO;
	public float maxInteractDistance = 8;
	public byte selectedInventory = 0;
	public bool resetCamera = false;
	public Vector3 campos;
	public Animator playerAnimator;
    GameObject fpsController;
    private FPSInputControllerC inputController;
	// Use this for initialization
	void Start() {
		currentPlayerIO = this;
        fpsController = GameObject.FindWithTag("FPSController");
        inputController = GetComponentInParent<FPSInputControllerC>();
        /*
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;
        */
    }
	
	// Update is called once per frame
	void Update() {
        if (!inputController.hasAuthority) {
            return;
        }
		playerAnimator.SetBool("walking", Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D));
		if(fpsController.transform.position.y < -20) {
			Debug.Log("Player fell through world, resetting!");
            fpsController.transform.position = new Vector3(fpsController.transform.position.x, 60, fpsController.transform.position.z);
		}

		if(Input.GetKeyDown(KeyCode.F5)) {
			if(!resetCamera) {
				Camera.main.transform.localPosition -= Vector3.forward * 3.14159f;
			} else {
				Camera.main.transform.position = transform.position;
			}
			resetCamera = !resetCamera;
		}
		if(Input.GetKey(KeyCode.Escape) && Input.GetKey(KeyCode.F1)) {
			Application.Quit();
		}
	}
}