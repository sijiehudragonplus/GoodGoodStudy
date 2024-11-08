using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject]
public sealed partial class Account : IDisposable
{
    public static void Login(string account)
    {
        string  savePath = $"{SavePath}/{account}.archive";
        string  archive  = File.ReadAllText(savePath);
        Account instance = string.IsNullOrEmpty(archive) ? new Account() : JsonConvert.DeserializeObject<Account>(archive);
        instance.Initialize(savePath);
        s_Instance = instance;
    }

    public static void Logout()
    {
        s_Instance.Dispose();
        s_Instance = null;
    }

    private static string  SavePath = $"{Application.persistentDataPath}/Archive";
    private static Account s_Instance;

    [JsonIgnore]
    private string m_SavePath;

    [JsonIgnore]
    private bool m_Active;

    private void Initialize(string savePath)
    {
        m_SavePath = savePath;
        m_Active   = true;
        Task.Run(SaveAsync);
    }

    public void Dispose()
    {
        m_Active = false;
    }

    private void SaveAsync()
    {
        while (m_Active)
        {
            Thread.Sleep(3000);
            File.WriteAllText(m_SavePath, JsonConvert.SerializeObject(this));
        }
    }
}