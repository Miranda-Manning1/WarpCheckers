using UnityEngine;

public class CycleButton : MonoBehaviour
{
	private static SpriteRenderer cycleButtonSprite;
	private static SpriteRenderer altSprite;

	public static bool CycleEnabled = false;

	void OnMouseUp()
	{
		GameManager.ClickedOnSquare = true;
		SetCycleEnabled(!CycleEnabled);
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
		if (CycleEnabled != enabled)
			(cycleButtonSprite.sprite, altSprite.sprite) = (altSprite.sprite, cycleButtonSprite.sprite);

		CycleEnabled = enabled;
	}
}
