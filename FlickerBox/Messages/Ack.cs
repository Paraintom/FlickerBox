namespace FlickerBox.Messages
{
    public class Ack
    {
        public Ack()
        {
            this.Type = this.GetType().Name;
        }
        public string Type { get; set; }
        public string Id { get; set; }
        //See AckStates
        public string State { get; set; }
    }
}
