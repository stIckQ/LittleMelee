using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    [Header("Controlled UI")]
    public GameObject health;
    public GameObject score;
    public GameObject gameOverUI;
    public GameObject messageBoard;

    [Header("GameState")]
    public bool isPlaying;
    public bool canGameOver;

    private string dateTime;
    private int scoreNum;
    private GameOverBoardManager gameOverBoardManager;
    private Animator gameOverBoardAnimator;
    private Animator messageBoardAnimator;

    // Use this for initialization
    void Start () {
        isPlaying = true;
        gameOverBoardManager = gameOverUI.GetComponent<GameOverBoardManager>();
        gameOverBoardAnimator = gameOverBoardManager.GetComponent<Animator>();

        StartCoroutine(ShowMessageBoard(0));
    
        Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {

        if(canGameOver)
        {        
            GameOver();
            canGameOver = false;
        }
	}

    void GameOver()
    {
        //set game state
        isPlaying = false;
        // Time.timeScale = 0;
        
        //save score
        scoreNum = PlayerManager.instance.player.GetComponent<PlayerStates>().scoreNum;
        dateTime = System.DateTime.Now.ToString();
        ScoreManager.SaveScoreDate(scoreNum, dateTime);

        //show and disappear UI and cursor
        Cursor.visible = true;
        health.SetActive(false);
        score.SetActive(false);
        gameOverBoardManager.ShowBoard(scoreNum);
        gameOverBoardAnimator.SetBool("ShowGameOverBoard", true);


    }

    private IEnumerator ShowMessageBoard(int num)
    {
        messageBoardAnimator = messageBoard.GetComponent<Animator>();
        messageBoardAnimator.SetBool("ShowMessageBoard", true);

        yield return new WaitForSeconds(10);

        messageBoardAnimator.SetBool("ShowMessageBoard", false);
    }
}
