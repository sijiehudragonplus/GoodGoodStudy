using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

public delegate void NetworkCallback<in TResponse>(TResponse response) where TResponse : IMessage;

public interface INetworkSystem : IGameSystem
{
    void                      Request(IMessage                      request);
    void                      Request<TRequest, TResponse>(TRequest request, NetworkCallback<TResponse> callback) where TRequest : IMessage where TResponse : IMessage;
    ResponseHandle<TResponse> Request<TRequest, TResponse>(TRequest request) where TRequest : IMessage where TResponse : IMessage;

    void AddListener<TResponse>(NetworkCallback<TResponse>    callback) where TResponse : IMessage;
    void RemoveListener<TResponse>(NetworkCallback<TResponse> callback) where TResponse : IMessage;
}

public struct ResponseHandle<TResponse> where TResponse : IMessage
{
    private int                       m_UUID;
    private Dictionary<int, Delegate> m_Map;

    public ResponseHandle(int uuid, Dictionary<int, Delegate> map)
    {
        m_UUID = uuid;
        m_Map  = map;
    }

    public ResponseAwaiter<TResponse> GetAwaiter() => new ResponseAwaiter<TResponse>(m_UUID, m_Map);
}

public struct ResponseAwaiter<TResponse> : INotifyCompletion where TResponse : IMessage
{
    public ResponseAwaiter(int uuid, Dictionary<int, Delegate> map) : this()
    {
        NetworkCallback<TResponse> cache = null;
        if (map.TryGetValue(uuid, out var temp))
        {
            cache = (NetworkCallback<TResponse>) temp;
        }

        map[uuid] = cache + OnResponse;
    }

    private Action    m_OnCompleted;
    private TResponse m_Response;

    public bool IsCompleted { get; private set; }

    public TResponse GetResult() => m_Response;

    public void OnResponse(TResponse response)
    {
        m_Response = response;
        m_OnCompleted();
        IsCompleted = true;
    }

    public void OnCompleted(Action continuation)
    {
        m_OnCompleted += continuation;
    }
}

internal sealed class NetworkSystem : INetworkSystem
{
    private Dictionary<Type, Delegate> m_CallbackMap = new Dictionary<Type, Delegate>();
    private Dictionary<int, Delegate>  m_ResponseMap = new Dictionary<int, Delegate>();
    private MethodInfo                 m_InvokeMethod;
    private object[]                   m_InvokeArgs = new object[1];

    public NetworkSystem()
    {
        m_InvokeMethod = GetType().GetMethod(nameof(InvokeResponse), BindingFlags.Instance | BindingFlags.NonPublic)!;
    }

    public void Request(IMessage request)
    {
        Server.Request(request);
    }

    public void Request<TRequest, TResponse>(TRequest request, NetworkCallback<TResponse> callback) where TRequest : IMessage where TResponse : IMessage
    {
        NetworkCallback<TResponse> cache = null;
        if (m_ResponseMap.TryGetValue(request.UUID, out var temp))
        {
            cache = (NetworkCallback<TResponse>) temp;
        }

        m_ResponseMap[request.UUID] = cache + callback;
        Request(request);
    }

    public ResponseHandle<TResponse> Request<TRequest, TResponse>(TRequest request) where TRequest : IMessage where TResponse : IMessage
    {
        ResponseHandle<TResponse> handle = new ResponseHandle<TResponse>(request.UUID, m_ResponseMap);
        Request(request);
        return handle;
    }

    public void AddListener<TResponse>(NetworkCallback<TResponse> callback) where TResponse : IMessage
    {
        NetworkCallback<TResponse> cache = null;
        if (m_CallbackMap.TryGetValue(typeof(TResponse), out var temp))
        {
            cache = (NetworkCallback<TResponse>) temp;
        }

        m_CallbackMap[typeof(TResponse)] = cache + callback;
    }

    public void RemoveListener<TResponse>(NetworkCallback<TResponse> callback) where TResponse : IMessage
    {
        NetworkCallback<TResponse> cache = null;
        if (m_CallbackMap.TryGetValue(typeof(TResponse), out var temp))
        {
            cache = (NetworkCallback<TResponse>) temp;
        }

        m_CallbackMap[typeof(TResponse)] = cache - callback;
    }

    public void OnStart()
    {
    }

    public void OnUpdate()
    {
        while (Server.TryGetResponse(out IMessage response))
        {
            m_InvokeArgs[0] = response;
            m_InvokeMethod.MakeGenericMethod(response.GetType()).Invoke(this, m_InvokeArgs);
            m_InvokeArgs[0] = null;
        }
    }

    public void OnDestroy()
    {
    }

    private void InvokeResponse<TResponse>(TResponse response) where TResponse : IMessage
    {
        if (m_CallbackMap.TryGetValue(typeof(TResponse), out var temp1))
        {
            ((NetworkCallback<TResponse>) temp1).Invoke(response);
        }

        if (m_ResponseMap.TryGetValue(response.UUID, out var temp2))
        {
            ((NetworkCallback<TResponse>) temp2).Invoke(response);
        }
    }
}