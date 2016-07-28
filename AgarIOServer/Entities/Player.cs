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
                foreach (var part in Parts.Where(p => !p.IsBeingEjected))
                    x += part.X;
                return x / Parts.Where(p => !p.IsBeingEjected).Count();
            }
        }
        [ProtoBuf.ProtoIgnore]
        new public float Y
        {
            get
            {
                float y = 0;
                foreach (var part in Parts.Where(p => !p.IsBeingEjected))
                    y += part.Y;
                return y / Parts.Where(p => !p.IsBeingEjected).Count();
            }
        }
        [ProtoBuf.ProtoIgnore]
        new public int Mass
        {
            get
            {
                var mass = 0;
                foreach (var part in Parts.Where(p => !p.IsBeingEjected))
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

        public Player(string name, GameState state)
        {
            this.Name = name;
            this.Parts = new List<PlayerPart>();
            var part = new PlayerPart();
            part.Mass = GameServer.PlayerStartSize;

            List<PlayerPart> otherPlayerParts;

            lock (state.Players)
            {
                otherPlayerParts = (from player in state.Players
                                    where player.Name != name
                                    from otherPlayerPart in player.Parts
                                    select otherPlayerPart).ToList();
            }
            do
            {
                part.X = GameServer.RandomG.Next((int)(Math.Round(this.Radius)), GameServer.MaxLocationX);
                part.Y = GameServer.RandomG.Next((int)(Math.Round(this.Radius)), GameServer.MaxLocationY);
            } while (otherPlayerParts.Exists(p => !IsInSafeDistance(part, p)) || 
                     state.Viruses.Exists(v => !IsInSafeDistance(part, v)));

            part.Identifier = 1;
            Parts.Add(part);
            this.FirstMovementServerTime = Stopwatch.GetTimestamp();
            this.FirstMovementTime = 0;
            this.Color = new byte[3];
            GameServer.RandomG.NextBytes(this.Color);
        }

        private bool IsInSafeDistance(PlayerPart part1, PlayerPart part2)
        {
            var dx = part2.X - part1.X;
            var dy = part2.Y - part1.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance > 2 * (part1.Radius + part2.Radius);
        }

        private bool IsInSafeDistance(PlayerPart part, Virus virus)
        {
            var dx = virus.X - part.X;
            var dy = virus.Y - part.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            return distance > 2 * (virus.Radius + part.Radius);
        }


        public Player() { }
    }
}
