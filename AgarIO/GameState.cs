using AgarIO.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AgarIO
{

    [ProtoBuf.ProtoContract]
    class GameState
    {
        [ProtoBuf.ProtoMember(1)]
        public List<Player> Players { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public List<Food> Food { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public List<Virus> Viruses { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public long Version;

        [ProtoBuf.ProtoIgnore]
        public Player CurrentPlayer { get; set; }

        [ProtoBuf.ProtoIgnore]
        public List<Food> EatenFoodPrediction { get; set; }

        public GameState Clone()
        {
            return (GameState)MemberwiseClone();
        }

    }
}
