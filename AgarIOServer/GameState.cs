using AgarIOServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer
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
        [ProtoBuf.ProtoMember(28)]
        public int Version;
        public GameState()
        {
            Players = new List<Player>();
            Food = new List<Food>();
            Viruses = new List<Virus>();
            //Version = 0;
        }
    }
}
