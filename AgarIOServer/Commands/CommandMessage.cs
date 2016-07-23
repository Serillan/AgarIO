using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;

namespace AgarIOServer.Commands
{
    [ProtoContract]
    class CommandMessage
    {
        [ProtoMember(1)]
        public CommandType CommandType { get; set; }

        [ProtoMember(2)]
        public byte[] SerializedCommand { get; set; }

        public CommandMessage(byte[] serializedCommand, CommandType commandType)
        {
            this.SerializedCommand = serializedCommand;
            this.CommandType = commandType;
        }

        public CommandMessage(Command command)
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Serialize(stream, command);
            SerializedCommand = stream.ToArray();
            CommandType = command.CommandType;
        }

        private CommandMessage() { }

        public Command GetCommand()
        {
            var stream = new MemoryStream(SerializedCommand);
            return (Command)Serializer.Deserialize(typeof(Command), stream);
        }
    }
}
