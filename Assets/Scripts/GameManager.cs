using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private SpriteRenderer _devModeMarker;
    private SpriteRenderer _boardFlipMarker;

    public static GameManager Instance;
    private EndTurnButton _endTurnButton;
    private CycleButton _cycleButton;
    
    public bool clickedOnSquare = false;

    private int _playerTurn = 0;
    private bool _boardFlipped = false;
    private bool _chainCaptureRunning = false;
	private bool _cycleRunning = false;

    public bool developerMode = false;
    private bool _flipBoard = true;
    public int backwardTeam = 1;
    
	public Color[] teamColors = new[] { Color.white, Color.cyan };

    public Board board;
    public Board previousBoard;
    
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
            developerMode = !developerMode;
            _devModeMarker.enabled = developerMode;
        }

        if (developerMode && Input.GetKeyDown(KeyCode.F))
        {
            _flipBoard = !_flipBoard;
            _boardFlipMarker.enabled = !_flipBoard;
        }
    }

	public void SetEndTurnButtonEnabled(bool enabled) {
		_endTurnButton.gameObject.SetActive(enabled);
	}

	public void SetCycleButtonEnabled(bool enabled) {
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
    public void SwitchPlayerTurn()
    {
        int oldTurn = _playerTurn;
		_playerTurn = GetOppositeTeam(_playerTurn);

		// highlight the squares the opponent moved on, while un-highlighting the new turn's squares
        board.lastSquaresMoved[oldTurn] = board.squaresTraveledThisTurn;
		Square.SetAllHighlighted(board.lastSquaresMoved[_playerTurn], false);
		Square.SetAllHighlighted(board.lastSquaresMoved[oldTurn], true);
		board.squaresTraveledThisTurn = new List<Square> { };

        // don't flip the board if space bar is held in dev mode
        if (developerMode && !_flipBoard) return;
        
        _boardFlipped = !_boardFlipped;
        backwardTeam = GetOppositeTeam(_playerTurn);
        board.FlipBoard();
    }

    /*
     * Get the current player whose turn it is
     */
    public int CurrentPlayerTurn()
    {
        return _playerTurn;
    }
    
    /*
     * Check if the board is flipped. It should always be flipped if it's team 1's turn
     */
    public bool IsBoardFlipped()
    {
        return _boardFlipped;
    }
    
    /*
     * Returns whether there is a chain capture currently running.
     * For use in Square.OnMouseUp() to ensure the piece isn't deselected
     */
    public bool ChainCaptureRunning()
    {
        return _chainCaptureRunning;
    }
    
    public void SetChainCaptureRunning(bool isRunning)
    {
        _chainCaptureRunning = isRunning;
    }

    /*
     * Returns whether there is a cycle currently running.
     */
    public bool CycleRunning()
    {
        return _cycleRunning;
    }
    
    public void SetCycleRunning(bool isRunning)
    {
        _cycleRunning = isRunning;
    }
}
