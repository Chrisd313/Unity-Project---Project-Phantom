using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

//TO-DO
// - Player dashes when exiting out of the menu.
// - Would it be more beneficial to use new input system?

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;
    public GameObject player;

    public GameObject pauseFirstButton, optionsFirstButton, optionsCloseButton; 

    // Update is called once per frame
    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown("joystick button 7")){
            if (GameIsPaused){
                Resume();
            } else {
                Pause();
            }
        }
    }

    public void Resume(){
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause(){
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(pauseFirstButton);
    }

    public void OpenSettings(){
        settingsMenuUI.SetActive(true);
        pauseMenuUI.SetActive(false);

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
    }

    public void CloseSettings(){
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);

        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(optionsCloseButton);
    }

    public void ToggleGamepad(){
        if (player.GetComponent<PlayerController>().useGamepad == true){
            Debug.Log("Change");
            player.GetComponent<PlayerController>().useGamepad = false;
        } else {
            Debug.Log("Change"); 
            player.GetComponent<PlayerController>().useGamepad = true;
        }
    }

    public void ResetScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    //useful resources
    //https://www.youtube.com/watch?v=SXBgBmUcTe0&t=321s

}

