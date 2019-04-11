namespace MatchUp
{
    /// <summary>A Message is made up of a Command and an optional string payload</summary>
    public class Message
    {
        public Command command;
        public string payload;

        public Message(Command command, string payload)
        {
            this.command = command;
            this.payload = payload;
        }
    }
}
