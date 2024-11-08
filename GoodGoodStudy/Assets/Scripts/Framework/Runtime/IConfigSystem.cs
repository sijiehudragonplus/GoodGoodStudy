using System;
using System.Collections.Generic;

public interface IConfigSystem : IGameSystem
{
    T GetConfig<T>() where T : new();
}

internal sealed class ConfigSystem : IConfigSystem
{
    private Dictionary<Type, object> m_Configs = new Dictionary<Type, object>();

    public T GetConfig<T>() where T : new()
    {
        if (m_Configs.TryGetValue(typeof(T), out object cache))
        {
            return (T) cache;
        }

        T config = new T();
        m_Configs.Add(typeof(T), config);
        return config;
    }
    
    public void OnStart()
    {
    }

    public void OnUpdate()
    {
    }

    public void OnDestroy()
    {
    }
}