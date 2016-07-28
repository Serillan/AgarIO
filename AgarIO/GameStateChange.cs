using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Entities;

namespace AgarIO
{
    /// <summary>
    /// Used for sending Gamestate change to all connected clients.
    /// </summary>
    [ProtoBuf.ProtoContract]
    class GameStateChange
    {
        [ProtoBuf.ProtoMember(1)]
        public List<Player> Players { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public List<Food> Food { get; set; }

        public List<Virus> Viruses { get; set; }

        [ProtoBuf.ProtoMember(5)]
        public List<Food> AddedFood { get; set; }

        [ProtoBuf.ProtoMember(6)]
        public List<Food> RemovedFood { get; set; }
    }
}
