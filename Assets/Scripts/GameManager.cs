using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance;
    public Board board;
    
    public static bool ClickedOnSquare = false;

    private static int _playerTurn = 0;

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
        return _playerTurn == 1;
    }
}
