using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour {

    public static string username = "default";
    
    public void OnUsernameChanged() {
        
        username = transform.GetChild(2).GetComponent<InputField>().text;
    }
    public void OnSeedChanged() {
        int newseed;
        if (int.TryParse(transform.GetChild(3).GetComponent<InputField>().text, out newseed)) {
            transform.GetChild(3).GetComponent<InputField>().textComponent.color = Color.green;
            Utils.seed = newseed;
        } else {
            transform.GetChild(3).GetComponent<InputField>().textComponent.color = Color.red;
        }
    }

    public void OnDoubleBrownianChanged() {
        if (Utils.browninanDouble) {
            Utils.browninanDouble = false;
        } else {
            Utils.browninanDouble = true;
        }
    }

    public void OnMaxDistanceChanged() {
        int newValue;
        if (int.TryParse(transform.GetChild(4).GetComponent<InputField>().text, out newValue)) {
            if(newValue < 10) {
                transform.GetChild(4).GetComponent<InputField>().textComponent.color = Color.green;
                World.radius = newValue;
            } else {
                World.radius = 3;
                transform.GetChild(4).GetComponent<InputField>().textComponent.color = Color.red;
            }

        } else {
            transform.GetChild(4).GetComponent<InputField>().textComponent.color = Color.red;
        }
    }

    public GameObject gamemanager;

    private void Start() {
        gamemanager.SetActive(true);
    }
}
