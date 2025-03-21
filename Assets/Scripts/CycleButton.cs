using UnityEngine;

public class CycleButton : Button
{
	public static CycleButton Instance;
	
	public SpriteRenderer cycleButtonSprite;
	public SpriteRenderer altSprite;

	public bool cycleEnabled = false;

	public Piece queen;

	void OnMouseUp()
	{
		_gameManager.clickedOnSquare = true;
		SetCycleEnabled(_board, !cycleEnabled);

		if (cycleEnabled)
		{
			_gameManager.SetSplitButtonEnabled(false);
			_gameManager.GetSplitButton().SetSplitEnabled(_gameManager.board, false);
		}
		else
		{
			_gameManager.SetSplitButtonEnabled(true);
		}
	}

	void Awake()
	{
		Instance = this;
	}

    void Start()
    {
	    SetBoard(Board.Instance);
	    SetGameManager(GameManager.Instance);
	    
        gameObject.SetActive(false);
		cycleButtonSprite = this.gameObject.GetComponent<SpriteRenderer>();
		altSprite = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

	/*
	 * set whether cycling is active or not
	 */
	public void SetCycleEnabled(Board board, bool enabled)
	{
		// switch button sprite if enabled status changes
		if (cycleEnabled != enabled)
			(cycleButtonSprite.sprite, altSprite.sprite) = (altSprite.sprite, cycleButtonSprite.sprite);

		cycleEnabled = enabled;

		if (cycleEnabled) {
			queen = board.selectedSquare.GetPiece();
			board.cycleSquares.Add(queen.square);
		} else {
			queen = null;
			board.cycleSquares.Clear();
		}
	}

	/*
	 * return whether cycling is active or not
	 */
	public bool CycleEnabled()
	{
		return cycleEnabled;
	}
}
