using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Entities
{
    [ProtoBuf.ProtoContract]
    class Food : Entity
    {
        [ProtoBuf.ProtoMember(1)]
        public byte[] Color { get; set; }

        private Food() { }

        public Food(float x, float y, int mass)
        {
            this.X = x;
            this.Y = y;
            this.Mass = mass;
            // random
            this.Color = new byte[3];
            GameServer.RandomG.NextBytes(this.Color);
        }
    }
}
