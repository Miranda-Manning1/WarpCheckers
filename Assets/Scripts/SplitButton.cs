using UnityEngine;

public class SplitButton : Button
{
    public static SplitButton Instance;
    private static SpriteRenderer splitButtonSprite;

    private static bool splitEnabled = false;
    
    void OnMouseUp()
    {
        GameManager.ClickedOnSquare = true;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetBoard(Board.Instance);
        gameObject.SetActive(false);
        splitButtonSprite = this.gameObject.GetComponent<SpriteRenderer>();
    }

}
