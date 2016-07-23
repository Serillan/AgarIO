using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace AgarIO.Commands
{
    [ProtoContract]
    enum CommandType
    {
        Stop, Move, Divide, Update, Invalidate
    }

    [ProtoContract]
    [ProtoInclude(1, typeof(Divide))]
    [ProtoInclude(2, typeof(Invalidate))]
    [ProtoInclude(3, typeof(Move))]
    [ProtoInclude(4, typeof(Stop))]
    [ProtoInclude(5, typeof(UpdateState))]
    abstract class Command
    {
        [ProtoIgnore]
        public abstract CommandType CommandType { get; }

        public static Type GetType(CommandType type)
        {
            switch (type)
            {
                case CommandType.Divide:
                    return typeof(Divide);
                case CommandType.Move:
                    return typeof(Move);
                case CommandType.Invalidate:
                    return typeof(Invalidate);
                case CommandType.Stop:
                    return typeof(Stop);
                case CommandType.Update:
                    return typeof(UpdateState);
            }
            throw new ArgumentException("Unknown binding between CommandType and the actual Type.");
        }
    }

}
