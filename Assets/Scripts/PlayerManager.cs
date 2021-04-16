using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    GameManagerScript gameManager;
    TileManager tileManager;
    Chat chatManager;
    // public GameObject textObject;
    
    // Players state of current category in choosing issues
    [SyncVar]
    int currentCategory = 0;

    int chosenCardIndex;

    public int tokens;
    public string nickname;
    public override void OnStartServer()
    {
        Debug.Log("In OnStartServer!");
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        Debug.Log("In OnStartClient");
        base.OnStartClient();
        // gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    void Start() {
        Debug.Log("In start"); 
        gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        tileManager = GameObject.Find("EventSystem").GetComponent<TileManager>();
        chatManager = GameObject.Find("EventSystem").GetComponent<Chat>();
        if(gameManager) {
            // Setting players number    
            gameManager.playersNumber = GameObject.FindGameObjectsWithTag("Player").Length;
            Debug.Log("[PlayerManager Start] playersNumber: " + gameManager.playersNumber.ToString());
            if(hasAuthority)
                gameManager.ShowSurvey();
            //  Where to show? 
            //  gameManager.ShowChooseCategory(currentCategory);
        }
    }

    // ---------------------  Survey part -------------------- //

    public void SubmitSurvey() {
        // If player didn't enter nickname then do nothing
        string submittedNickname = gameManager.GetNicknameInputText();
        Debug.Log("[SubmitSurvey] submittedNickname: " + submittedNickname);
        if(submittedNickname == "")
            return;
        
        nickname = submittedNickname;
        if(gameManager.nicknameCount.ContainsKey(nickname))
            gameManager.nicknameCount[nickname] ++;
        else
            gameManager.nicknameCount.Add(nickname, 1);
        if(gameManager.nicknameCount[nickname] > 1)
            nickname += "(" + gameManager.nicknameCount[nickname].ToString() + ")";
        
        Debug.Log("[SubmitSurvey] modified nickname: " + nickname);

        CmdSubmitSurvey(nickname);
    }

    [Command]
    void CmdSubmitSurvey(string submittedNickname) {
        RpcSubmitSurvey(submittedNickname);
    }

    [ClientRpc]
    void RpcSubmitSurvey(string submittedNickname) {
        nickname = submittedNickname;
        gameManager.AddPlayer(submittedNickname);
        if(hasAuthority) {
            gameManager.DestroySurvey();
            gameManager.ShowChooseCategory(currentCategory);
        }
    }

    // ---------------------  Survey part -------------------- //


    // ---------------------  Choose Issue part -------------------- //

    public void SendChosenIssue() {

        Debug.Log("[PLAYER MANAGER] Button was pressed!");
        Debug.Log(gameObject.tag);
        // Get the pressed button gameobject
        GameObject buttonGameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if(hasAuthority && buttonGameObject) {
            int issueIndex = buttonGameObject.transform.GetSiblingIndex();
            CmdSendChosenIssue(issueIndex);
        }
    }

    [Command]
    void CmdSendChosenIssue(int issueIndex) {
        Debug.Log("Issue index: " + issueIndex.ToString());
        RpcChooseIssue(currentCategory, issueIndex);
        currentCategory ++;
        RpcUpdateActivePanel(currentCategory);
    }

    [ClientRpc]
    void RpcChooseIssue(int currentCategory, int issueIndex) {
        if(hasAuthority) {
            Debug.Log("[In RPCChooseIssue] Has Authority!");
            Debug.Log("[In RPCChooseIssue] currentCategory!" + currentCategory.ToString());
            
            gameManager.VoteForIssue(currentCategory, issueIndex);
            gameManager.DestroyChooseCategory();

        } else {
            gameManager.VoteForIssue(currentCategory, issueIndex);
        }
    }

    [ClientRpc]
    void RpcUpdateActivePanel(int currentCategory) {
        if(hasAuthority) {
            Debug.Log("[RpcUpdateActivePanel] currentCategory: " + currentCategory.ToString());
            if(currentCategory < gameManager.categoryNumber) 
                gameManager.ShowChooseCategory(currentCategory);
            else {
                gameManager.donePlayers ++;
                Debug.Log("[PlayerManager] doneplayers: " + gameManager.donePlayers.ToString());
                gameManager.ShowWaitingPanel();
                
            }
        } else {
            if(currentCategory >= gameManager.categoryNumber)
                gameManager.donePlayers ++;
        }
        if(gameManager.donePlayers == gameManager.playersNumber) {
            gameManager.DestroyWaitingPanel();
            gameManager.ShowChooseCard();
        }
    }

    // ---------------------  Choose Issue part -------------------- //

    // ---------------------  Choose Card part ------------------- //
    public void SendChosenCard() {
        // To send index of chosen card
        Debug.Log("[In SendChosenCard] Button was pressed!");
        // Get the pressed button gameobject
        GameObject buttonGameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if(hasAuthority && buttonGameObject) {
            int cardIndex = buttonGameObject.transform.GetSiblingIndex();
            Debug.Log("[In SendChosenCard] Card sibling index: " + cardIndex);    
            CmdSendChosenCard(cardIndex);
        }
    }

    [Command]
    void CmdSendChosenCard(int cardIndex) {
        Debug.Log("Card index: " + cardIndex.ToString());
        RpcChooseCard(cardIndex);
    }

    [ClientRpc]
    void RpcChooseCard(int cardIndex) {
        gameManager.donePlayers ++;
        if(hasAuthority) {
            Debug.Log("[In RPCChooseIssue] Has Authority!");
            Debug.Log("[In RPCChooseIssue] cardIndex!" + cardIndex.ToString());
            chosenCardIndex = cardIndex;
            gameManager.DestroyChooseCard();
            gameManager.ShowWaitingPanel();
        } else {
            gameManager.ReserveChooseCard(cardIndex);
        }
        if(gameManager.donePlayers == gameManager.playersNumber) {
            gameManager.DestroyObject("WaitingPanel(Clone)");
            gameManager.playerWaiting = false;
            // Starting new state!
            gameManager.gameState = GameManagerScript.States.PutCard;
            gameManager.currentPlayer = 0;
            gameManager.StartCardPut();
        }
    }

    // ---------------------  Choose Card part ------------------- //


    // ---------------------  Card Put part ------------------- //
    public void UpdateCurrentPlayer(Vector3Int tpos) {
        Debug.Log("[UpdateCurrentPlayer]");
        CmdUpdateCurrentPlayer(tpos);
    }
    [Command]
    void CmdUpdateCurrentPlayer(Vector3Int tpos) {
        Debug.Log("[CmdUpdateCurrentPlayer]");
        RpcUpdateCurrentPlayer(chosenCardIndex, tpos);
    }

    [ClientRpc]
    void RpcUpdateCurrentPlayer(int chosenCardIndex, Vector3Int tpos) {
        Debug.Log("[RpcUpdateCurrentPlayer]");
        gameManager.currentPlayer ++;
        tileManager.UpdateTile(gameManager.currentGroup, chosenCardIndex, tpos, nickname);
        if(gameManager.currentPlayer >= gameManager.playersNumber) {
            gameManager.StartChat();
            return;
        }
        gameManager.StartCardPut();
    }
    // ---------------------  Card Put part ------------------- //
    
    // ---------------------  SendMessage part ------------------- //
    public void SendMessage() {
        CmdSendMessage(chatManager.GetMessage());
    }
    
    [Command]
    void CmdSendMessage(string message) {
        if(message.Length > 0) {
            // Validate message
            RpcHandleMessage(message);
        }
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        chatManager.HandleMessage($"[{nickname}]: {message}");
    }
    // ---------------------  SendMessage part ------------------- //

    // ---------------------  SendVote part ------------------- //
    public void SendVote() {
        Debug.Log("[Player Manager] [SendVote] tpos of current opened tile: " + gameManager.chosenTiletpos.ToString());
        // If out of tokens do nothing
        if(tokens == 0) return;

        tokens --;
        gameManager.UpdateTokenCountDisplay(tokens);
        // Send this tpos
        CmdSendVote(gameManager.chosenTiletpos);
        if(tokens == 0) {
            // Maybe show waiting panel and prepare going to next step of game
            gameManager.ShowWaitingPanel();
            CmdIncrementDonePlayers();
        }
    }

    [Command]
    void CmdSendVote(Vector3Int tpos) {
        RpcSendVote(tpos);
    }

    [ClientRpc]
    private void RpcSendVote(Vector3Int tpos) {
        tileManager.IncrementVoteCount(tpos);
    }

    [Command]
    void CmdIncrementDonePlayers() {
        RpcIncrementDonePlayers();
    }

    [ClientRpc]
    void RpcIncrementDonePlayers() {
        gameManager.donePlayers ++;
        if(gameManager.donePlayers == gameManager.playersNumber) {
            gameManager.DestroyObject("WaitingPanel(Clone)");
            // Change again to choose card state
            gameManager.ShowChooseCard();
        }
    }
    // ---------------------  SendVote part ------------------- //

    // -- Reloading server part --//
    [Command]
    public void CmdChangeGameState(GameManagerScript.States toState) {
        gameManager.gameState = toState;
    }
    // ----------------------//
}
