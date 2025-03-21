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
    private SplitButton _splitButton;
    private NextFragmentButton _nextFragmentButton;
    
    public bool clickedOnSquare = false;

    public int playerTurn = 0;
    public bool boardFlipped = false;
    public bool chainCaptureRunning = false;
    public bool cycleRunning = false;
    public bool splitRunning = false;

    public bool developerMode = false;
    public bool flipBoard = true;
    public int backwardTeam = 1;
    
	public Color[] teamColors = new[] { Color.white, Color.cyan };

    public Board board;
    //public GameState savedGameState;
    
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
        _splitButton = gameObject.transform.GetChild(4).GetComponent<SplitButton>();
        _nextFragmentButton = gameObject.transform.GetChild(5).GetComponent<NextFragmentButton>();
    }

    // Update is called once per frame
    private void Update()
    {
        // enable dev mode
        if (Input.GetKeyDown(KeyCode.D))
        {
            developerMode = !developerMode;
            _devModeMarker.enabled = developerMode;
        }

        // dev mode features
        if (developerMode)
        {
            // toggle board flipping
            if (Input.GetKeyDown(KeyCode.F))
            {
                flipBoard = !flipBoard;
                _boardFlipMarker.enabled = !flipBoard;
            }

            // save the current state of the board
            if (Input.GetKeyDown(KeyCode.S))
            {
                //savedGameState = new GameState();
                //savedGameState.SaveState(board);
            }
        }

    }

	public void SetEndTurnButtonEnabled(bool enabled) {
		_endTurnButton.gameObject.SetActive(enabled);
	}

	public void SetCycleButtonEnabled(bool enabled) {
		_cycleButton.gameObject.SetActive(enabled);
	}

    public void SetSplitButtonEnabled(bool enabled)
    {
        _splitButton.gameObject.SetActive(enabled);
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
        int oldTurn = playerTurn;
		playerTurn = GetOppositeTeam(playerTurn);

		// highlight the squares the opponent moved on, while un-highlighting the new turn's squares
        board.lastSquaresMoved[oldTurn] = board.squaresTraveledThisTurn;
		Square.SetAllHighlighted(board.lastSquaresMoved[playerTurn], false);
		Square.SetAllHighlighted(board.lastSquaresMoved[oldTurn], true);
		board.squaresTraveledThisTurn = new List<Square> { };
        
        _splitButton.SetSplitEnabled(board, false);

        // don't flip the board if space bar is held in dev mode
        if (developerMode && !flipBoard) return;
        
        boardFlipped = !boardFlipped;
        backwardTeam = GetOppositeTeam(playerTurn);
        board.FlipBoard();
    }

    /*
     * Get the current player whose turn it is
     */
    public int CurrentPlayerTurn()
    {
        return playerTurn;
    }
    
    /*
     * Check if the board is flipped. It should always be flipped if it's team 1's turn
     */
    public bool IsBoardFlipped()
    {
        return boardFlipped;
    }
    
    /*
     * Returns whether there is a chain capture currently running.
     * For use in Square.OnMouseUp() to ensure the piece isn't deselected
     */
    public bool ChainCaptureRunning()
    {
        return chainCaptureRunning;
    }
    
    public void SetChainCaptureRunning(bool isRunning)
    {
        chainCaptureRunning = isRunning;
    }

    /*
     * Returns whether there is a cycle currently running.
     */
    public bool CycleRunning()
    {
        return cycleRunning;
    }
    
    public void SetCycleRunning(bool isRunning)
    {
        cycleRunning = isRunning;
    }

    public bool SplitRunning()
    {
        return splitRunning;
    }
    
    public void SetSplitRunning(bool isRunning)
    {
        splitRunning = isRunning;
    }

    public CycleButton GetCycleButton()
    {
        return _cycleButton;
    }

    public SplitButton GetSplitButton()
    {
        return _splitButton;
    }

    public NextFragmentButton GetNextFragmentButton()
    {
        return _nextFragmentButton;
    }
}
