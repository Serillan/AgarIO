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
    }
}
