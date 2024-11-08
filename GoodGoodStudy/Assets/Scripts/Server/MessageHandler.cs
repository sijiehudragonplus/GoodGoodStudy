internal abstract class MessageHandler
{
    public abstract IMessage Handle(IMessage request);
}

internal abstract class MessageHandler<T> : MessageHandler where T : IMessage
{
    public sealed override IMessage Handle(IMessage request) => Handle((T) request);

    protected abstract IMessage Handle(T request);
}