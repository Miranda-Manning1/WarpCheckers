using UnityEngine;

public class NextFragmentButton : MonoBehaviour
{
    private static SpriteRenderer nextFragmentButtonSprite;
    
    void OnMouseUp()
    {
        GameManager.ClickedOnSquare = true;
    }

    void Start()
    {
        gameObject.SetActive(false);
        nextFragmentButtonSprite = this.gameObject.GetComponent<SpriteRenderer>();
    }

}