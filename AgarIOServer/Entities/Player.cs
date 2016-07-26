using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public int LastMovementTime { get; set; }

        [ProtoBuf.ProtoIgnore]
        public int FirstMovementTime { get; set; }

        [ProtoBuf.ProtoIgnore]
        public long FirstMovementServerTime { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public byte[] Color { get; set; }

        [ProtoBuf.ProtoIgnore]
        public long CreationTime { get; set; }

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
            part.X = GameServer.RandomG.Next((int)(Math.Round(this.Radius)), GameServer.MaxLocationX);
            part.Y = GameServer.RandomG.Next((int)(Math.Round(this.Radius)), GameServer.MaxLocationY);
            part.Identifier = 1;
            Parts.Add(part);
            this.FirstMovementServerTime = Stopwatch.GetTimestamp();
            this.FirstMovementTime = 0;
            this.Color = new byte[3];
            GameServer.RandomG.NextBytes(this.Color);
        }

        public Player() { }
    }
}
