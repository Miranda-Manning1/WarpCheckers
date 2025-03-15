using UnityEngine;

public class CycleButton : MonoBehaviour
{
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
        gameObject.SetActive(false);
		cycleButtonSprite = this.gameObject.GetComponent<SpriteRenderer>();
		altSprite = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

	/*
	 * set whether cycling is active or not
	 */
	public static void SetCycleEnabled(bool enabled)
	{
		// switch button sprite if enabled status changes
		if (cycleEnabled != enabled)
			(cycleButtonSprite.sprite, altSprite.sprite) = (altSprite.sprite, cycleButtonSprite.sprite);

		cycleEnabled = enabled;

		if (cycleEnabled) {
			Queen = Board.SelectedSquare.GetPiece();
			Board.CycleSquares.Add(Queen.square);
		} else {
			Queen = null;
			Board.CycleSquares.Clear();
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
