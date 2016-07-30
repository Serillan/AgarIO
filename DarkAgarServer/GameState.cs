using DarkAgarServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DarkAgarServer
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
        /// Gets or sets the game state lock.
        /// It is used as the global lock for all GameState manipulation. <para />
        /// 
        /// While you have reading lock, you can access GameState properties only while you have
        /// smaller locks. <para />
        /// 
        /// For each property you need to use lock on that property and for accessing each
        /// Player, you need to use lock on that player for accessing it's properties. <para />
        /// 
        /// While you have writer lock you don't need to use smaller locks.
        /// </summary>
        /// <value>The game state lock.</value>
        [ProtoBuf.ProtoIgnore]
        public ReaderWriterLockSlim GameStateLock { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        public GameState()
        {
            Players = new List<Player>();
            Food = new List<Food>();
            Viruses = new List<Virus>();
            GameStateLock = new ReaderWriterLockSlim();
        }
    }
}
