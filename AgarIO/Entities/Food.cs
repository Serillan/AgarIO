﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Entities
{
    [ProtoBuf.ProtoContract]
    class Food : Entity
    {
        [ProtoBuf.ProtoMember(1)]
        public byte[] Color { get; set; }
    }
}
