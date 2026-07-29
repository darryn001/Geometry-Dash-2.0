using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UImanager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onPlayButtonClicked()
    {
        PlayerPrefs.SetString("Scene to go to", "GameScene");
        SceneManager.LoadScene("GameScene");
    }
}
