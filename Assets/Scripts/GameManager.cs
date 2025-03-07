using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance;
    public Board board;
    
    public static bool ClickedOnSquare = false;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board = Board.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
