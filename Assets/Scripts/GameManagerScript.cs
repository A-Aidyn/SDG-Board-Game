using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;



public class GameManagerScript : NetworkBehaviour
{

    public enum States
    {
        Survey,
        ChooseCategory, // async
        ChooseCard, // async
        PutCard, // with turns
        Chat, // Chat panel
        Vote, // Voting for cards
        ProblemDescription
    }
    // GENERAL DATA
    public States gameState = States.ChooseCategory;
    public int playersNumber = 2;
    public TileManager tileManager;
    public Chat chatManager;
    NetworkRoomManager networkRoomManager;
    public int donePlayers = 0;
    List<string> playersNickname = new List<string>();
    public Dictionary<string, int> nicknameCount = new Dictionary<string, int>();
    
    // --------  SURVEY DATA -------- //
    public GameObject surveyPanelPrefab;

    // ------------------------------------------ //

    // --------  CHOOSE CATEGORY DATA -------- //
    public GameObject votingPanelPrefab;
    public GameObject issueButtonPrefab;
    public GameObject waitingPanelPrefab;
    public int categoryNumber = 3;
    int currentShownCategory = -1;
    List<string>[] issues;
    int [,] issueVoteCount = new int[30, 30];

    // ------------------------------------------ //

    // --------- CHOOSE CARD DATA ----------- //
    public int currentGroup = -1;
    public int totalGroupNumber = 4;
    public bool playerWaiting = false;
    public GameObject chooseCardPanelPrefab;
    public GameObject cardButtonPrefab;
    // -------------------------------------- //

    // --------- PUT CARD DATA ----------- //
    public int currentPlayer = 0;
    // -------------------------------------- //

    // --------- CHAT DATA ----------- //
    
    // -------------------------------------- //

    // --------- VOTE DATA ----------- //
    public GameObject votePanelPrefab;
    public Vector3Int chosenTiletpos;
    public int tokenCount = 3;
    // -------------------------------------- //

    // --------  PROBLEM DESCRIPTION DATA -------- //    
    public GameObject problemDescriptionPanelPrefab;
    // -------------------------------------- //

    // OTHER DATA
    GameObject mainCanvas;
    
    void Start()
    {
        Debug.Log("GameManagerScript start!");
        mainCanvas = GameObject.Find("Canvas");
        issues = new List<string>[categoryNumber];

        for(int i = 0; i < categoryNumber; i ++) {
            string name = i.ToString();
            if(name.Length < 2) name = "0" + name;
            TextAsset textFile = Resources.Load("Texts/" + name) as TextAsset;
            string text = textFile.ToString();
            string[] lines = text.Split('\n');
            issues[i] = new List<string>();
            for(int j = 0; j < 3; j ++) {
                issues[i].Add(lines[j]);
                // issues[i].Add(i.ToString() + '.' + j.ToString() + ": Blah blah blah");
            }
        }
        tileManager = GameObject.Find("EventSystem").GetComponent<TileManager>();
        chatManager = GameObject.Find("EventSystem").GetComponent<Chat>();
        networkRoomManager = GameObject.Find("RoomManager").GetComponent<NetworkRoomManager>();
        // Setting players number    
        playersNumber = GameObject.FindGameObjectsWithTag("Player").Length;
        Debug.Log("[GameManager Start] playersNumber: " + playersNumber.ToString());
        currentGroup = -1;
    }

    void Update() {
        if(isServer && gameState == States.ProblemDescription) {
            playersNumber = GameObject.FindGameObjectsWithTag("Player").Length;
            if(playersNumber == 0) {
                gameState = States.ChooseCategory; // Starting state;
                networkRoomManager.ServerChangeScene("WaitingScene");
            }
        }
    }

    public void DestroyObject(string objectName) {
        Debug.Log("[In Destroy Object] objectName: " + objectName);
        GameObject waitingPanel = GameObject.Find(objectName);
        Destroy(waitingPanel);
    }

    void ListShuffler(List<string> alpha) {
        for (int i = 0; i < alpha.Count; i++) {
            string temp = alpha[i];
            int randomIndex = Random.Range(i, alpha.Count);
            alpha[i] = alpha[randomIndex];
            alpha[randomIndex] = temp;
        }
    }

    public void AddPlayer(string nickname) {  
        Debug.Log("[AddPlayer] nickname: " + nickname);  
        playersNickname.Add(nickname);
        // ListShuffler(playersNickname); TODO: to sync the shuffled list across all clients
    }

    public string getNickname(int index) {
        return playersNickname[index];
    }

    //  <----------------- Survey Scripts ---------------------> // 

    public void ShowSurvey() {
        gameState = States.Survey;
        GameObject surveyPanel = Instantiate(surveyPanelPrefab, new Vector2(0, 0), Quaternion.identity);
        surveyPanel.transform.SetParent(mainCanvas.transform, false);
    }

    public string GetNicknameInputText() {
        Debug.Log("[GetNicknameInputText]");
        GameObject nicknameInputField = GameObject.Find("NicknameInputField");
        return nicknameInputField.GetComponent<InputField>().text;
    }

    public void DestroySurvey() {
        Debug.Log("In Destroy Survey!");
        GameObject votingPanel = GameObject.Find("SurveyPanel(Clone)");
        Destroy(votingPanel);
    }

    // ---------------------------------------------------------------------------- //

    //  <----------------- Category Choosing Scripts ---------------------> // 
    public void ShowChooseCategory(int currentCategory) {
        if(currentShownCategory >= currentCategory)
            return;
        gameState = States.ChooseCategory;
        Debug.Log("In Choose Category!");
        GameObject votingPanel = Instantiate(votingPanelPrefab, new Vector2(0, 0), Quaternion.identity);
        GameObject issuesPanel = votingPanel.transform.GetChild(1).gameObject;
        for(int j = 0; j < 3; j ++) {
            GameObject issueButton = Instantiate(issueButtonPrefab, new Vector2(0, 0), Quaternion.identity);
            issueButton.GetComponentInChildren<Text>().text = issues[currentCategory][j];
            issueButton.transform.SetParent(issuesPanel.transform, false);
        }
        votingPanel.transform.SetParent(mainCanvas.transform, false);
        currentShownCategory = currentCategory;
    }
    
    public void DestroyChooseCategory() {
        Debug.Log("In Destroy Choose Category!");
        GameObject votingPanel = GameObject.Find("VotingPanel(Clone)");
        Destroy(votingPanel);
    }

    public void VoteForIssue(int category, int issue) {
        issueVoteCount[category, issue] ++;
        Debug.Log("[IN GAME MANAGER] Vote for " + category.ToString() + issue.ToString());
        Debug.Log("[IN GAME MANAGER] votes number for it " + issueVoteCount[category, issue].ToString());
    }

    public void ShowWaitingPanel() {
        playerWaiting = true;
        GameObject waitingPanel = Instantiate(waitingPanelPrefab, new Vector2(0, 0), Quaternion.identity);
        waitingPanel.transform.SetParent(mainCanvas.transform, false);
    }

    public void DestroyWaitingPanel() {
        playerWaiting = false;
        Debug.Log("In Destroy Waiting Panel!");
        GameObject waitingPanel = GameObject.Find("WaitingPanel(Clone)");
        Destroy(waitingPanel);
        
        int curVote = 0, curCategory = -1;
        for(int i = 0; i < categoryNumber; i ++) {
            for(int j = 0; j < 3; j ++) {
                if(issueVoteCount[i, j] > curVote) {
                    curVote = issueVoteCount[i, j];
                    curCategory = i;
                }
            }
        }
        Debug.Log("Current category: " + curCategory.ToString());
        tileManager.setCentralTile(curCategory);
    }

    // ---------------------------------------------------------------------------- //

    //  <----------------- Card Choosing Scripts ---------------------> //
    public void ShowChooseCard() {
        currentGroup ++;
        Debug.Log("[In ShowChooseCard!] curGroup: " + currentGroup.ToString());
        if(currentGroup >= totalGroupNumber) {
            // TODO: add choosing another 3 top categories and put them on the board. implement FINAL STEPS
            ShowProblemDescription();
            return;
        }
        playerWaiting = false;
        donePlayers = 0;
        gameState = States.ChooseCard;
        GameObject chooseCardPanel = Instantiate(chooseCardPanelPrefab, new Vector2(0, 0), Quaternion.identity);
        GameObject cardsListPanel = chooseCardPanel.transform.GetChild(1).gameObject;
        for(int i = 0; i < 3; i ++) {
            Sprite cardImage = Resources.Load<Sprite>("Sprites/" + currentGroup.ToString() + "/0" + i.ToString()) as Sprite;
            GameObject cardButton = Instantiate(cardButtonPrefab, new Vector2(0, 0), Quaternion.identity);
            cardButton.GetComponentInChildren<Image>().sprite = cardImage;
            cardButton.transform.SetParent(cardsListPanel.transform, false);
        }
        chooseCardPanel.transform.SetParent(mainCanvas.transform, false);
    }

    public void ReserveChooseCard(int cardIndex) {
        // If other player have chosen a card but this player is in waiting mode then do nothin (because gameobjects were destroyed)
        if(playerWaiting)
            return;
        Debug.Log("[In ReserveChooseCard!] donePlayers: " + donePlayers);
        GameObject cardsListPanel = GameObject.Find("CardsListPanel");
        Button cardButton = cardsListPanel.transform.GetChild(cardIndex).gameObject.GetComponent<Button>();
        cardButton.interactable = false;
    }

    public void DestroyChooseCard() {
        Debug.Log("[In Destroy Choose Card]! donePlayers: " + donePlayers);
        GameObject chooseCardPanel = GameObject.Find("ChooseCardPanel(Clone)");
        Destroy(chooseCardPanel);
    }

    // ---------------------------------------------------------------------------- //

    //  <----------------- Card Put Scripts ---------------------> //
    public void StartCardPut() {
        Debug.Log("[StartCardPut] currentPlayer: " + currentPlayer.ToString());
        Debug.Log("[StartCardPut] currentPlayer nickname: " + playersNickname[currentPlayer]);
        Text whosTurnText = GameObject.Find("StatusText").GetComponent<Text>();
        // if current players turn
        if(playersNickname[currentPlayer] == NetworkClient.connection.identity.GetComponent<PlayerManager>().nickname) {
            whosTurnText.text = "It's your turn!";
        } else {
            whosTurnText.text = "It's " + playersNickname[currentPlayer] + " turn!";
        }
    }
    // ---------------------------------------------------------------------------- //

    //  <----------------- Start Chat Scripts ---------------------> //
    public void StartChat() {
        Debug.Log("[Starting chat]");
        gameState = States.Chat;
        // TODO: start chat panel! With timer!
        chatManager.OnStartChat();
    }
    // ---------------------------------------------------------------------------- //


    //  <----------------- Start Vote Scripts ---------------------> //
    // Showing token number on status text
    public void UpdateTokenCountDisplay(int token) {
        GameObject.Find("StatusText").GetComponent<Text>().text = "Tokens lefts: " + token.ToString();
    }
    
    public void StartVote() {
        Debug.Log("[Starting vote]");
        gameState = States.Vote;
        donePlayers = 0;
        // Setting clients tokens to tokenCount
        NetworkClient.connection.identity.GetComponent<PlayerManager>().tokens = tokenCount;
        UpdateTokenCountDisplay(tokenCount);
    }

    public void ShowVote(BoardTile tile, Vector3Int tpos) {
        Debug.Log("[ShowVote]");
        tileManager.cannotClick = true;
        GameObject votePanel = Instantiate(votePanelPrefab, new Vector2(0, 0), Quaternion.identity);
        chosenTiletpos = tpos;
        Debug.Log("Clicked tpos: " + chosenTiletpos.ToString());
        votePanel.transform.GetChild(0).GetComponent<Image>().sprite = tile.sprite;
        votePanel.transform.GetChild(1).GetComponent<Text>().text = "Owner: " + tile.ownerNickname;
        votePanel.transform.GetChild(2).GetComponent<Text>().text = "Vote count: " + tile.voteCount.ToString();
        votePanel.transform.SetParent(mainCanvas.transform, false);
    }

    public void DestroyVote() {
        Debug.Log("[In Destroy Vote]!");
        tileManager.cannotClick = false;
        GameObject votePanel = GameObject.Find("TileVotePanel(Clone)");
        Destroy(votePanel);
    }
    // ---------------------------------------------------------------------------- //

    //  <----------------- Problem Description Scripts ---------------------> //

    public void ShowProblemDescription() {

        // Changing game state on server
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.CmdChangeGameState(States.ProblemDescription);

        gameState = States.ProblemDescription;
        GameObject problemDescriptionPanel = Instantiate(problemDescriptionPanelPrefab, new Vector2(0, 0), Quaternion.identity);
        problemDescriptionPanel.transform.SetParent(mainCanvas.transform, false);
    }


    public void DestroyProblemDescription() {
        Debug.Log("In Destroy Problem Description!");
        GameObject problemDescriptionPanel = GameObject.Find("ProblemDescriptionPanel(Clone)");
        Destroy(problemDescriptionPanel);
        networkRoomManager.OnClientDisconnect(NetworkClient.connection);
        SceneManager.LoadScene("MainMenuScene");
    }

    // ---------------------------------------------------------------------------- //
}
