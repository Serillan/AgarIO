using AgarIO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Commands
{
    [ProtoBuf.ProtoContract]
    class UpdateState : Command
    {
        [ProtoBuf.ProtoMember(1)]
        public GameState GameState { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public GameStateChange GameStateChange { get; set; }

        public UpdateState(GameState state)
        {
            this.GameState = state;
        }

        /// <summary>
        /// Used for deserialization.
        /// </summary>
        private UpdateState() { }

        public override void Process(Game game)
        {
            if (GameState != null)
                ProcessGameState(game);
            else
                ProcessGameStateChange(game);
        }

        private void ProcessGameState(Game game)
        {
            var oldGameState = game.GameState;
            if (oldGameState != null && oldGameState.Version > GameState.Version)
                return;

            game.GameState = GameState;
            Player oldCurrentPlayer = null;
            Player currentPlayer = GameState.Players.Find(p => p.Name == game.PlayerName);

            if (oldGameState?.CurrentPlayer != null)
                oldCurrentPlayer = oldGameState.CurrentPlayer;

            if (oldGameState != null && oldGameState.Version > GameState.Version)
                return;

            // prediction
            if (oldCurrentPlayer != null && game.IsPredictionValid)
                currentPlayer.Parts = oldCurrentPlayer.Parts;

            GameState.CurrentPlayer = currentPlayer;
            game.IsPredictionValid = true;
        }

        private void ProcessGameStateChange(Game game)
        {
            var oldGameState = game.GameState;
            if (oldGameState != null && oldGameState.Version > GameState.Version)
                return;

            var newGameState = new GameState();
            newGameState.Players = GameStateChange.Players;
            newGameState.Viruses = GameStateChange.Viruses;

            // food
            if (game.GameState?.Food != null)
            {
                newGameState.Food = game.GameState.Food;
                GameStateChange.RemovedFood.ForEach(f => newGameState.Food.Remove(f));
                GameStateChange.AddedFood.ForEach(f => newGameState.Food.Add(f));
            }
            else
                newGameState.Food = GameStateChange.AddedFood;

            Player oldCurrentPlayer = null;
            Player currentPlayer = GameState.Players.Find(p => p.Name == game.PlayerName);

            if (oldGameState?.CurrentPlayer != null)
                oldCurrentPlayer = oldGameState.CurrentPlayer;

            if (oldGameState != null && oldGameState.Version > GameState.Version)
                return;

            // prediction
            if (oldCurrentPlayer != null && game.IsPredictionValid)
                currentPlayer.Parts = oldCurrentPlayer.Parts;

            GameState.CurrentPlayer = currentPlayer;
            game.IsPredictionValid = true;
        }
    }
}
