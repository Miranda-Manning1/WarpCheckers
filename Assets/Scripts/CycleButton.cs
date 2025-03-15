using UnityEngine;

public class CycleButton : MonoBehaviour
{

	void OnMouseUp()
	{
		GameManager.ClickedOnSquare = true;
	}

    void Start()
    {
        gameObject.SetActive(false);
    }
}
