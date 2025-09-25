using Steamworks;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    private static SteamManager s_instance;
    private bool m_bInitialized = false;
    
    public bool IsInitialized => m_bInitialized;
    
    public static SteamManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                return new GameObject("SteamManager").AddComponent<SteamManager>();
            }
            return s_instance;
        }
    }
    
    void Awake()
    {
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (!Packsize.Test())
        {
            Debug.LogError("Steamworks.NET Packsize Test failed!");
            return;
        }
        
        try
        {
            if (SteamAPI.Init())
            {
                m_bInitialized = true;
                Debug.Log("Steam 초기화 성공!");
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("Steam이 설치되지 않았습니다: " + e);
        }
    }
    
    void Update()
    {
        if (m_bInitialized)
        {
            SteamAPI.RunCallbacks();
        }
    }
    
    void OnApplicationQuit()
    {
        if (m_bInitialized)
        {
            SteamAPI.Shutdown();
        }
    }
}