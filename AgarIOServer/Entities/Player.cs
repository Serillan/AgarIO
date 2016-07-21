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
        new public float X
        {
            get
            {
                float x = 0;
                foreach (var part in Parts)
                    x += part.X;
                return x / Parts.Count;
            }
        }
        [ProtoBuf.ProtoIgnore]
        new public float Y
        {
            get
            {
                float y = 0;
                foreach (var part in Parts)
                    y += part.Y;
                return y / Parts.Count;
            }
        }
        [ProtoBuf.ProtoIgnore]
        new public int Mass
        {
            get
            {
                var mass = 0;
                foreach (var part in Parts)
                    mass += part.Mass;
                return mass;
            }
        }

        [ProtoBuf.ProtoIgnore]
        new public float Radius
        {
            get
            {
                return 10 * (float)Math.Sqrt(Mass / Math.PI);
            }
        }

        public Player(string name)
        {
            this.Name = name;

            this.Parts = new List<PlayerPart>();
            var part = new PlayerPart();
            part.Mass = 10;
            part.X = Game.RandomG.Next((int)(Math.Round(this.Radius)), Game.MaxLocationX);
            part.Y = Game.RandomG.Next((int)(Math.Round(this.Radius)), Game.MaxLocationY);
            Parts.Add(part);
        }

        public Player() { }
    }
}
