using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleButtonManager : MonoBehaviour
{
    public void ButtonMoveScene(string nextSceneName) {
        SceneManager.LoadScene(nextSceneName);
    }
    
    public void ExitGame() {
        Application.Quit();
    }
}
