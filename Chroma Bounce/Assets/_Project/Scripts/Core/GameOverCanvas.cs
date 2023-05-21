using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverCanvas : MonoBehaviour{
    public static GameOverCanvas instance;
    [SerializeField] TextMeshProUGUI restartButtonTxt;
    [SerializeField] TextMeshProUGUI scoreDescTxt;
    [SerializeField] TextMeshProUGUI scoreTxt;
    [SerializeField] TextMeshProUGUI highscoreDescTxt;
    [SerializeField] TextMeshProUGUI highscoreTxt;
    [HideInInspector]public bool gameOver;
    void Awake(){instance=this;}
    public void OpenGameOverCanvas(bool open=true){
        gameOver=open;
        transform.GetChild(0).gameObject.SetActive(open);
    }
}
