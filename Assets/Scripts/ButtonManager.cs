using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class ButtonManager : NetworkBehaviour
{
    public PlayerManager playerManager;

    public void SubmitSurvey()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.SubmitSurvey();
    }

    public void SendChosenIssue()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.SendChosenIssue();
    }

    public void SendChosenCard()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.SendChosenCard();
    }

    public void SendMessage()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.SendMessage();
    }

    public void SendVote()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.SendVote();
        CloseVotePanel();
    }

    public void CloseVotePanel()
    {
        GameManagerScript gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        gameManager.DestroyVote();
    }

    public void CloseProblemDescriptionPanel()
    {
        GameManagerScript gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        gameManager.DestroyProblemDescription();
    }

}
