using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private SpriteRenderer _devModeMarker;
    private SpriteRenderer _boardFlipMarker;

    public static GameManager Instance;
    private static EndTurnButton _endTurnButton;
    private static CycleButton _cycleButton;
    
    public static bool ClickedOnSquare = false;

    private static int _playerTurn = 0;
    private static bool _boardFlipped = false;
    private static bool _chainCaptureRunning = false;
	private static bool _cycleRunning = false;

    public static bool DeveloperMode = false;
    private static bool _flipBoard = true;
    public static int BackwardTeam = 1;
    
	public static Color[] TeamColors = new[] { Color.white, Color.cyan };

    public static Board Board;
    public static Board previousBoard;
    
    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _devModeMarker = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        _boardFlipMarker = gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>();
        _endTurnButton = gameObject.transform.GetChild(2).GetComponent<EndTurnButton>();
        _cycleButton = gameObject.transform.GetChild(3).GetComponent<CycleButton>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DeveloperMode = !DeveloperMode;
            _devModeMarker.enabled = DeveloperMode;
        }

        if (DeveloperMode && Input.GetKeyDown(KeyCode.F))
        {
            _flipBoard = !_flipBoard;
            _boardFlipMarker.enabled = !_flipBoard;
        }
    }

	public static void SetEndTurnButtonEnabled(bool enabled) {
		_endTurnButton.gameObject.SetActive(enabled);
	}

	public static void SetCycleButtonEnabled(bool enabled) {
		_cycleButton.gameObject.SetActive(enabled);
	}

	public static int GetOppositeTeam(int currentTurn) {
    	return currentTurn switch
    	{
        	0 => 1,
        	1 => 0,
        	_ => currentTurn
    	};
	}

    /*
     * Switch the turn from one player's to the other's
     */
    public static void SwitchPlayerTurn()
    {
        int oldTurn = _playerTurn;
		_playerTurn = GetOppositeTeam(_playerTurn);

		// highlight the squares the opponent moved on, while un-highlighting the new turn's squares
        Board.lastSquaresMoved[oldTurn] = Board.squaresTraveledThisTurn;
		Square.SetAllHighlighted(Board.lastSquaresMoved[_playerTurn], false);
		Square.SetAllHighlighted(Board.lastSquaresMoved[oldTurn], true);
		Board.squaresTraveledThisTurn = new List<Square> { };

        // don't flip the board if space bar is held in dev mode
        if (DeveloperMode && !_flipBoard) return;
        
        _boardFlipped = !_boardFlipped;
        BackwardTeam = GetOppositeTeam(_playerTurn);
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

    /*
     * Returns whether there is a cycle currently running.
     */
    public static bool CycleRunning()
    {
        return _cycleRunning;
    }
    
    public static void SetCycleRunning(bool isRunning)
    {
        _cycleRunning = isRunning;
    }
}
