using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class Server
{
    [RuntimeInitializeOnLoadMethod]
    private static void StartServer()
    {
        foreach (var type in typeof(MessageHandler).Assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(MessageHandler))          &&
                type.IsAbstract                           == false &&
                type.BaseType?.GetGenericTypeDefinition() == typeof(MessageHandler<>))
            {
                var requestType = type.BaseType.GetGenericArguments()[0];
                s_Handlers.Add(requestType, (MessageHandler) Activator.CreateInstance(type));
            }
        }

        Task.Run(HandleMessages);
    }

    private static readonly object          s_Locker        = new object();
    private static readonly Queue<IMessage> s_RequestQueue  = new Queue<IMessage>();
    private static readonly Queue<IMessage> s_ResponseQueue = new Queue<IMessage>();

    private static readonly Dictionary<Type, MessageHandler> s_Handlers = new Dictionary<Type, MessageHandler>();

    public static void Request(IMessage request)
    {
        lock (s_Locker)
            s_RequestQueue.Enqueue(request);
    }

    public static bool TryGetResponse(out IMessage response)
    {
        lock (s_Locker)
            return s_ResponseQueue.TryDequeue(out response);
    }

    private static void HandleMessages()
    {
        while (true)
        {
            IMessage request;
            lock (s_Locker)
                if (s_RequestQueue.TryDequeue(out request) == false)
                    continue;

            if (!s_Handlers.TryGetValue(request.GetType(), out var handler))
                continue;

            var response = handler.Handle(request);
            lock (s_Locker)
            {
                s_ResponseQueue.Enqueue(response);
            }
        }
    }
}