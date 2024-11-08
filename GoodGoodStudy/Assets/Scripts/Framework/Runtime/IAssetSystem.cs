using UnityEngine;

public interface IAssetSystem : IGameSystem
{
    T Load<T>(string asset) where T : Object;

    void Release(Object asset);
}

internal sealed class AssetSystem : IAssetSystem
{
    public T Load<T>(string asset) where T : Object
    {
        return Resources.Load<T>(asset);
    }

    public void Release(Object asset)
    {
        Resources.UnloadAsset(asset);
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