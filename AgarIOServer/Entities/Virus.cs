using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Entities
{
    [ProtoBuf.ProtoContract]
    class Virus : Entity
    {
        [ProtoBuf.ProtoIgnore]
        public float StartX { get; set; }

        [ProtoBuf.ProtoIgnore]
        public float StartY { get; set; }

        [ProtoBuf.ProtoIgnore]
        public float EndX { get; set; }

        [ProtoBuf.ProtoIgnore]
        public float EndY { get; set; }

        [ProtoBuf.ProtoIgnore]
        public long AnimationStartTime { get; set; }

        [ProtoBuf.ProtoIgnore]
        public long AnimationEndTime { get; set; }

        public Virus() { }

        public Virus(List<PlayerPart> playersParts)
        {
            Mass = GameServer.DefaultVirusSize;
            do
            {
                X = GameServer.RandomG.Next(0, GameServer.MaxLocationX);
                Y = GameServer.RandomG.Next(0, GameServer.MaxLocationY);
            } while (playersParts.Exists(p => AreInCollision(this, p)));
        }

        private bool AreInCollision(Virus virus, PlayerPart part)
        {
            var dx = part.X - virus.X;
            var dy = part.Y - virus.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance < virus.Radius + part.Radius;
        }
    }
}
