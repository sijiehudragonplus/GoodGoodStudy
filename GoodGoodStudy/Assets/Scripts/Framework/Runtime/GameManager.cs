using UnityEngine;

[DefaultExecutionOrder(-1)]
public sealed class GameManager : MonoBehaviour
{
    public static readonly IAssetSystem   AssetSystem   = new AssetSystem();
    public static readonly IConfigSystem  ConfigSystem  = new ConfigSystem();
    public static readonly INetworkSystem NetworkSystem = new NetworkSystem();
    public static readonly IUISystem      UISystem      = new UISystem();

    private static GameStage     s_CurrentStage;
    private static IGameSystem[] s_Systems;

    public static void GotoStage(GameStage stage)
    {
        s_CurrentStage?.OnExit();
        s_CurrentStage = stage;
        s_CurrentStage.OnEnter();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        s_Systems = new IGameSystem[]
        {
            AssetSystem,
            ConfigSystem,
            NetworkSystem,
            UISystem,
        };
    }

    private void Start()
    {
        for (var i = 0; i < s_Systems.Length; i++)
        {
            s_Systems[i].OnStart();
        }
    }

    private void Update()
    {
        for (var i = 0; i < s_Systems.Length; i++)
        {
            s_Systems[i].OnUpdate();
        }

        s_CurrentStage?.OnUpdate();
    }

    private void OnDestroy()
    {
        for (var i = 0; i < s_Systems.Length; i++)
        {
            s_Systems[i].OnDestroy();
        }
    }
}

public interface IGameSystem
{
    void OnStart();
    void OnUpdate();
    void OnDestroy();
}

public abstract class GameStage
{
    protected internal abstract void OnEnter();
    protected internal abstract void OnUpdate();
    protected internal abstract void OnExit();
}