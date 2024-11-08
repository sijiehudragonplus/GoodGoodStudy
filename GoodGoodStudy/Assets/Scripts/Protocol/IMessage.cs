public interface IMessage
{
    int UUID { get; set; }
}

public class Message : IMessage
{
    public int UUID { get; set; }
}