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
        [ProtoBuf.ProtoIgnore]
        new public double X
        {
            get
            {
                double x = 0;
                foreach (var part in Parts)
                    x += part.X;
                return x / Parts.Count;
            }
        }
        [ProtoBuf.ProtoIgnore]
        new public double Y
        {
            get
            {
                double y = 0;
                foreach (var part in Parts)
                    y += part.X;
                return y / Parts.Count;
            }
        }

        public Player(string name)
        {
            this.Name = name;
            this.Mass = 5;

            this.Parts = new List<PlayerPart>();
            var part = new PlayerPart();
            part.Mass = Mass;
            part.X = Game.RandomG.Next(0, Game.MaxLocationX);
            part.Y = Game.RandomG.Next(0, Game.MaxLocationY);
            Parts.Add(part);
        }

        public Player() { }
    }
}
