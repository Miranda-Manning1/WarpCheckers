using UnityEngine;

public class SplitButton : MonoBehaviour
{
    private static SpriteRenderer splitButtonSprite;

    private static bool splitEnabled = false;
    
    void OnMouseUp()
    {
        GameManager.ClickedOnSquare = true;
    }

    void Start()
    {
        gameObject.SetActive(false);
        splitButtonSprite = this.gameObject.GetComponent<SpriteRenderer>();
    }

}
