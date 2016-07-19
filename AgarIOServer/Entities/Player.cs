using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Entities
{
    [ProtoBuf.ProtoContract]
    class Player : Entity
    {
        [ProtoBuf.ProtoMember(1)]
        public string Name { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public List<PlayerPart> Parts { get; set; }

        public Player(string name)
        {
            this.Name = name;
            this.Mass = 5;
            this.X = new Random().Next(0, Game.MaxLocationX);
            this.Y = new Random().Next(0, Game.MaxLocationY);

            this.Parts = new List<PlayerPart>();
            var part = new PlayerPart();
            part.Mass = Mass;
            part.X = X;
            part.Y = Y;
            Parts.Add(part);
        }

        public Player() { }
    }
}
