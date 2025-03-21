using UnityEngine;

public class SplitButton : Button
{
    public static SplitButton Instance;

    public SpriteRenderer splitButtonSprite;
    public SpriteRenderer altSprite;

    public bool splitEnabled = false;

    public Piece piece;
    
    void OnMouseUp()
    {
        _gameManager.clickedOnSquare = true;
        SetSplitEnabled(_board, !splitEnabled);
        
        if (splitEnabled)
        {
            _gameManager.SetCycleButtonEnabled(false);
            _gameManager.GetCycleButton().SetCycleEnabled(_gameManager.board, false);
        }
        else
        {
            CancelSplit();
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
        splitButtonSprite = this.gameObject.GetComponent<SpriteRenderer>();
        altSprite = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void CancelSplit()
    {
        if (_board.selectedSquare.GetPiece().pieceType == Piece.PieceType.Queen) _gameManager.SetCycleButtonEnabled(true);
    }

    public void SetSplitEnabled(Board board, bool enabled)
    {
        // switch button sprite if enabled status changes
        if (splitEnabled != enabled)
            (splitButtonSprite.sprite, altSprite.sprite) = (altSprite.sprite, splitButtonSprite.sprite);

        splitEnabled = enabled;
    }
}
