using UnityEngine;

public class NextFragmentButton : Button
{
    private static SpriteRenderer nextFragmentButtonSprite;
    public static NextFragmentButton Instance;
    
    void OnMouseUp()
    {
        GameManager.ClickedOnSquare = true;
    }

    void Start()
    {
        Instance = this;
        gameObject.SetActive(false);
        nextFragmentButtonSprite = this.gameObject.GetComponent<SpriteRenderer>();
    }
}