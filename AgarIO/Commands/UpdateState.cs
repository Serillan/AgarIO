using AgarIO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Commands
{
    /// <summary>
    /// Represents the update of the state command.
    /// </summary>
    /// <seealso cref="AgarIO.Commands.Command" />
    [ProtoBuf.ProtoContract]
    class UpdateState : Command
    {
        /// <summary>
        /// A New GameState for the client.
        /// </summary>
        /// <value>The state of the game.</value>
        [ProtoBuf.ProtoMember(1)]
        public GameState GameState { get; set; }

        /// <summary>
        /// Used for deserialization.
        /// </summary>
        private UpdateState() { }

        /// <summary>
        /// Processes the command received from the server.
        /// </summary>
        /// <param name="game">The Game in which the command takes place.</param>
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
            {
                if (oldGameState.EatenFoodPrediction == null)
                    game.GameState.EatenFoodPrediction = new List<Food>();
                else
                {
                    game.GameState.EatenFoodPrediction = oldGameState.EatenFoodPrediction;
                    game.GameState.Food.RemoveAll(f => game.GameState.EatenFoodPrediction.Contains(f));
                }

                currentPlayer.Parts = oldCurrentPlayer.Parts;
            }
            else
            {
                game.GameState.EatenFoodPrediction = new List<Food>();
            }

            GameState.CurrentPlayer = currentPlayer;
            game.IsPredictionValid = true;
        }
    }
}
