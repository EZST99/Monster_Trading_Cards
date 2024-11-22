using FHTW.Swen1.Swamp;

namespace SwenProject_Arslan.Models;

public class Battle
{
    private readonly User _Player1;
    private readonly User _Player2;
    public Battle(User player1, User player2)
    {
        if (player1 == null || player2 == null)
        {
            throw new ArgumentNullException("Player cannot be null");
        }
        _Player1 = player1;
        _Player2 = player2;
    }

    public void StartBattle()
    {
        
    }
}