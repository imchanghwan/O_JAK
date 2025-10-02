using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneMangaer : MonoBehaviour
{
    public static SceneMangaer Instance;
    
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text loadingText;
    
    private string sceneToLoad;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadGameScene(string sceneName)
    {
        sceneToLoad = sceneName;
        StartCoroutine(LoadSceneCoroutine());
    }
    
    private IEnumerator LoadSceneCoroutine()
    {
        loadingPanel.SetActive(true);
        
        yield return new WaitForSeconds(0.5f);
        
        // 비동기 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false; // 자동 전환 방지
        
        // 로딩 진행
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            progressBar.value = progress;
            loadingText.text = $"로딩 중... {(int)(progress * 100)}%";
            
            if (asyncLoad.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1f);
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // 로딩 화면 숨김
        loadingPanel.SetActive(false);
    }
}