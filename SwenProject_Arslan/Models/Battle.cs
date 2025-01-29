using SwenProject_Arslan.DataAccess;
using SwenProject_Arslan.Handlers.DbHandlers;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.Models
{
    public class Battle
    {
        private User _Player1;
        private User _Player2;
        private List<Card> _DeckPlayer1;
        private List<Card> _DeckPlayer2;
        private List<string> _BattleLog = new(); 

        public Battle() {}

        public async Task<string[]> StartBattle(string firstPlayerName, string secondPlayerName)
        {
            var userDbHandler = DbHandlerFactory.GetUserDbHandler();
            _Player1 = await userDbHandler.GetUserByUserNameAsync(firstPlayerName);
            _Player2 = await userDbHandler.GetUserByUserNameAsync(secondPlayerName);
            _DeckPlayer1 = await User.GetUserDeck(_Player1.UserName);
            _DeckPlayer2 = await User.GetUserDeck(_Player2.UserName);

            _BattleLog.Add($"The battle between {_Player1.UserName} and {_Player2.UserName} has started!");
            
            // Mandatory Unique Feature
            Random random = new();
            int result = random.Next(2); 
            if (result == 0)
            {
                _Player1.ELO += 1;
                _BattleLog.Add($"{_Player1.UserName} gets 1 extra ELO point!");
            }
            else
            {
                _Player2.ELO += 1;
                _BattleLog.Add($"{_Player2.UserName} gets 1 extra ELO point!");
            }
            int round = 1;
            while (_DeckPlayer1.Count > 0 && _DeckPlayer2.Count > 0 && round <= 100)
            {
                _BattleLog.Add($"Round {round}: ");
                await StartRound();
                round++;
            }

            _BattleLog.Add("<---- Battle Over ---->");
            if (_DeckPlayer2.Count <= 0)
            {
                _BattleLog.Add($"{_Player1.UserName} wins!");
                _Player1.ELO += 5;
                _Player2.ELO -= 3;
            }
            else if (_DeckPlayer1.Count <= 0)
            {
                _BattleLog.Add($"{_Player2.UserName} wins!");
                _Player2.ELO += 5;
                _Player1.ELO -= 3;
            }
            else
            {
                _BattleLog.Add($"The battle ends with a tie!");
            }

            await _Player1.Save(_Player1.UserName, null, null, null, _Player1.ELO, null);
            await _Player2.Save(_Player2.UserName, null, null, null, _Player2.ELO, null);

            return _BattleLog.ToArray();
        }

        private async Task StartRound()
        {
            List<Card> currentCards = PickRandomCards();
            float player1damage = currentCards[0].GetDamage(currentCards[1]);
            float player2damage = currentCards[1].GetDamage(currentCards[0]);

            _BattleLog.Add($"{_Player1.UserName} plays {currentCards[0].Name} dealing {player1damage} damage.");
            _BattleLog.Add($"{_Player2.UserName} plays {currentCards[1].Name} dealing {player2damage} damage.");

            if (player1damage > player2damage)
            {
                _BattleLog.Add($"{_Player1.UserName} wins the round!");
                _DeckPlayer1.Add(currentCards[1]);
                _DeckPlayer2.Remove(currentCards[1]);
            }
            else if (player2damage > player1damage)
            {
                _BattleLog.Add($"{_Player2.UserName} wins the round!");
                _DeckPlayer2.Add(currentCards[0]);
                _DeckPlayer1.Remove(currentCards[0]);
            }
            else
            {
                _BattleLog.Add("It's a tie!");
            }
        }

        private List<Card> PickRandomCards()
        {
            Random rnd = new();
            return new List<Card>
            {
                _DeckPlayer1[rnd.Next(_DeckPlayer1.Count)],
                _DeckPlayer2[rnd.Next(_DeckPlayer2.Count)]
            };
        }
    }
}
