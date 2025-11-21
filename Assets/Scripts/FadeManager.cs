using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    /// <summary>
    /// フェード設定
    /// </summary>
    public static FadeManager instance;

    [SerializeField] private Image fadepanel; // フェードパネル
    public float fadeDuration = 1.0f; // フェード時間

    public bool IsFadeComplete { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        fadepanel.raycastTarget = false;
        SetAlpha(0f);
    }

    void Start()
    {
        // フェードイン開始
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string scenename)
    {
        // フェードアウト開始
        StartCoroutine(FadeOut(scenename));
    }

    /// <summary>
    /// フェードイン設定
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeIn()
    {
        float t = 1;
        while (t > 0)
        {
            t -= Time.unscaledDeltaTime / fadeDuration;
            SetAlpha(t);
            yield return null;
        }

        SetAlpha(0f);
        IsFadeComplete = true; // フェードイン完了
    }

    /// <summary>
    /// フェードアウト設定
    /// </summary>
    /// <param name="scenename"></param>
    /// <returns></returns>
    private IEnumerator FadeOut(string scenename)
    {
        if (fadepanel == null)
        {
            Debug.LogError("FadePanel が設定されていません！");
            yield break;
        }

        fadepanel.transform.SetAsLastSibling();
        float t = 0;
        while (t < 1)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            SetAlpha(t);
            yield return null;
        }

        IsFadeComplete = false;

        SceneManager.LoadScene(scenename);
        yield return null;

        StartCoroutine(FadeIn());
    }

    private void SetAlpha(float a)
    {
        Color c = fadepanel.color;
        c.a = a;
        fadepanel.color = c;
    }
}
