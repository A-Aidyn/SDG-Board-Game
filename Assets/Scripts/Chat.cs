using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Chat : NetworkBehaviour
{
    Text chatText = null;
    InputField inputField = null;
    public GameObject chatPanelPrefab;
    Text timerText;
    GameManagerScript gameManager;

    GameObject mainCanvas;
    public float timeToWait = 30.0f;
    float timeLeft = 30.0f;
    bool timerStarted = false;

    private static event Action<string> OnMessage;

    void Update() {
        if(!timerStarted)
            return;
        timeLeft -= Time.deltaTime;
        if(timeLeft < 0) {
            timerText.text = "0.0";
            OnStopChat();
            return;
        }
        timerText.text = timeLeft.ToString();
    }

    // Called when the a client is connected to the server
    public void OnStartChat()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        mainCanvas = GameObject.Find("Canvas");
        if(mainCanvas)
            Debug.Log("[Chat Start]: Canvas was set!");
        timerText = GameObject.Find("TimerText").GetComponent<Text>();
        if(timerText)
            Debug.Log("[Chat Start]: Timer text was set!");
        
        GameObject chatPanel = Instantiate(chatPanelPrefab, new Vector2(0, 0), Quaternion.identity);
        if(chatPanel)
            Debug.Log("[Chat Start]: Chat panel was set!");
        
        chatText = GameObject.Find("ChatText").GetComponent<Text>();
        inputField = GameObject.Find("InputField").GetComponent<InputField>();
        
        chatPanel.transform.SetParent(mainCanvas.transform, false);

        OnMessage += HandleNewMessage;
        timerStarted = true;
        timeLeft = (float)timeToWait;
        timerText.text = timeLeft.ToString();
    }

    // Called when a client has exited the server
    public void OnStopChat()
    {
        Debug.Log("[Chat Start] Stopping chat");
        OnMessage -= HandleNewMessage;
        timerStarted = false;
        Destroy(GameObject.Find("ChatPanel(Clone)"));
        gameManager.StartVote();
    }

    // When a new message is added, update the Scroll View's Text to include the new message
    private void HandleNewMessage(string message)
    {
        chatText.text += message;
    }

    public string GetMessage() {
        if(!Input.GetKeyDown(KeyCode.Return)) 
            return "";
        if (string.IsNullOrWhiteSpace(inputField.text))
            return "";
        string tmp = inputField.text;
        inputField.text = string.Empty;
        return tmp;
    }

    public void HandleMessage(string message) {
        OnMessage?.Invoke($"\n{message}");
    }

}