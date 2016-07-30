using DarkAgar.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DarkAgar
{

    /// <summary>
    /// Represents the game state.
    /// </summary>
    [ProtoBuf.ProtoContract]
    class GameState
    {
        /// <summary>
        /// Gets or sets the players.
        /// </summary>
        /// <value>The players.</value>
        [ProtoBuf.ProtoMember(1)]
        public List<Player> Players { get; set; }

        /// <summary>
        /// Gets or sets the food.
        /// </summary>
        /// <value>The food.</value>
        [ProtoBuf.ProtoMember(2)]
        public List<Food> Food { get; set; }

        /// <summary>
        /// Gets or sets the viruses.
        /// </summary>
        /// <value>The viruses.</value>
        [ProtoBuf.ProtoMember(3)]
        public List<Virus> Viruses { get; set; }

        /// <summary>
        /// The version of the game state.
        /// </summary>
        [ProtoBuf.ProtoMember(4)]
        public long Version;

        /// <summary>
        /// Gets or sets the current player.
        /// </summary>
        /// <value>The current player.</value>
        [ProtoBuf.ProtoIgnore]
        public Player CurrentPlayer { get; set; }

        /// <summary>
        /// Gets or sets the eaten food prediction.
        /// </summary>
        /// <value>The eaten food prediction.</value>
        [ProtoBuf.ProtoIgnore]
        public List<Food> EatenFoodPrediction { get; set; }

        /// <summary>
        /// Returns shallow copy of GameState with current player parts and food being deep copied.
        /// </summary>
        /// <returns>Shallow copy of GameState with current player parts and food being deep copied.</returns>
        public GameState DeepClonePrediction()
        {
            var copy = (GameState)MemberwiseClone();

            var currentPlayerPartsCopy = new List<PlayerPart>();
            foreach (var part in CurrentPlayer.Parts)
                currentPlayerPartsCopy.Add(part.Clone());

            var foodCopy = new List<Food>();
            foreach (var food in Food)
                foodCopy.Add(food);

            copy.Food = foodCopy;
            copy.CurrentPlayer.Parts = currentPlayerPartsCopy;

            return copy;
        }
    }
}
