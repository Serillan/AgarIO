using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Entities
{
    [ProtoBuf.ProtoContract]
    class PlayerPart : Entity
    {
        [ProtoBuf.ProtoIgnore]
        public double Speed
        {
            get
            {
                return 10 / Math.Sqrt(Mass);
            }
        }
    }
}
