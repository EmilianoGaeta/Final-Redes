using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour {

    private InputField _ip;
    private InputField _password;
    private InputField _user;

    private GameObject _restartButton;
    private GameObject _waitingForOtherPlayer;
    private Text _countdownText;
    private Text _winnerName;

    void Start()
    {
        if(SceneManager.GetActiveScene().buildIndex == SceneManager.GetSceneByName("Login").buildIndex)
        {
            _user = GameObject.Find("User").GetComponent<InputField>();
            _password = GameObject.Find("Pass").GetComponent<InputField>();
            _password.inputType = InputField.InputType.Password;
            _ip = GameObject.Find("IP").GetComponent<InputField>();

            GameObject.Find("NetworkManager").GetComponent<ClientManager>().SetupLogin(_user, _password, _ip);
        }
        else if(SceneManager.GetActiveScene().buildIndex == SceneManager.GetSceneByName("Menu").buildIndex)
        {
            GameObject.Find("NetworkManager").GetComponent<ClientManager>().SetupMenu();
        }
        else if(SceneManager.GetActiveScene().buildIndex == SceneManager.GetSceneByName("Game").buildIndex)
        {
            _waitingForOtherPlayer = GameObject.Find("WaitionfForPlayer");
            _restartButton = GameObject.Find("Restart");
            _restartButton.SetActive(false);
            _countdownText = GameObject.Find("CountdownText").GetComponent<Text>();
            _countdownText.enabled = false;
            _winnerName = GameObject.Find("WinnerName").GetComponent<Text>();
            _winnerName.enabled = false;

            GameObject.Find("NetworkManager").GetComponent<ClientManager>().StupGame(_waitingForOtherPlayer, _restartButton, _countdownText, _winnerName);
        }
    }

}
