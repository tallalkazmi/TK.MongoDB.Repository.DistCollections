namespace TK.MongoDB.Distributed.Test.ViewModels
{
    public class MessageSearchParameters
    {
        public string Text { get; set; }
        public long? Caterer { get; set; }
        public long? Client { get; set; }
        public long? Order { get; set; }
    }
}
