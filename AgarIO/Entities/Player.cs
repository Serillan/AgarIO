using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Entities
{
    [ProtoBuf.ProtoContract]
    class Player : Entity
    {
        [ProtoBuf.ProtoMember(1)]
        public string Name { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public List<PlayerPart> Parts { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public byte[] Color { get; set; }

        [ProtoBuf.ProtoIgnore]
        public int LastMovementTime { get; set; }

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
    }
}
