using UnityEngine;

public class CycleButton : Button
{
	public static CycleButton Instance;
	
	private static SpriteRenderer cycleButtonSprite;
	private static SpriteRenderer altSprite;

	private static bool cycleEnabled = false;

	public static Piece Queen;

	void OnMouseUp()
	{
		GameManager.ClickedOnSquare = true;
		SetCycleEnabled(!cycleEnabled);
	}

    void Start()
    {
	    Instance = this;
        gameObject.SetActive(false);
		cycleButtonSprite = this.gameObject.GetComponent<SpriteRenderer>();
		altSprite = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

	/*
	 * set whether cycling is active or not
	 */
	public static void SetCycleEnabled(bool enabled)
	{
		Board board = GameManager.Board;
		
		// switch button sprite if enabled status changes
		if (cycleEnabled != enabled)
			(cycleButtonSprite.sprite, altSprite.sprite) = (altSprite.sprite, cycleButtonSprite.sprite);

		cycleEnabled = enabled;

		if (cycleEnabled) {
			Queen = board.selectedSquare.GetPiece();
			board.cycleSquares.Add(Queen.square);
		} else {
			Queen = null;
			board.cycleSquares.Clear();
		}
	}

	/*
	 * return whether cycling is active or not
	 */
	public static bool CycleEnabled()
	{
		return cycleEnabled;
	}
}
