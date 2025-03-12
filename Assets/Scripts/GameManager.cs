using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance;
    public Board board;
    
    public static bool ClickedOnSquare = false;

    private static int _playerTurn = 0;
    private static bool _boardFlipped = false;
    private static bool _chainCaptureRunning = false;

    public static bool DeveloperMode = false;
    private static bool flipBoard = true;
    
    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board = Board.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DeveloperMode = !DeveloperMode;
            gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = DeveloperMode;
        }

        if (DeveloperMode && Input.GetKeyDown(KeyCode.F))
        {
            flipBoard = !flipBoard;
            gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = !flipBoard;
        }
    }

    /*
     * Switch the turn from one player's to the other's
     */
    public static void SwitchPlayerTurn()
    {
        _playerTurn = _playerTurn switch
        {
            0 => 1,
            1 => 0,
            _ => _playerTurn
        };

        // don't flip the board if spacebar is held in dev mode
        if (DeveloperMode && !flipBoard) return;
        
        _boardFlipped = !_boardFlipped;
        Board.FlipBoard();
    }

    /*
     * Get the current player whose turn it is
     */
    public static int CurrentPlayerTurn()
    {
        return _playerTurn;
    }
    
    /*
     * Check if the board is flipped. It should always be flipped if it's team 1's turn
     */
    public static bool IsBoardFlipped()
    {
        return _boardFlipped;
    }
    
    /*
     * Returns whether there is a chain capture currently running.
     * For use in Square.OnMouseUp() to ensure the piece isn't deselected
     */
    public static bool ChainCaptureRunning()
    {
        return _chainCaptureRunning;
    }
    
    public static void SetChainCaptureRunning(bool isRunning)
    {
        _chainCaptureRunning = isRunning;
    }
}
