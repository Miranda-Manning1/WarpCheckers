using UnityEngine;

public class NextFragmentButton : Button
{
    private static SpriteRenderer nextFragmentButtonSprite;
    public static NextFragmentButton Instance;
    
    void OnMouseUp()
    {
        _gameManager.clickedOnSquare = true;
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
        nextFragmentButtonSprite = this.gameObject.GetComponent<SpriteRenderer>();
    }
}