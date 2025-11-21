using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework.Interfaces;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("生存時間")]
    [SerializeField] private TMP_Text timetext;
    public float survivaltime = 0f; // 生存時間
    private bool isGameOver = false;
    private bool isStarted = false; // ゲーム開始済みフラグ

    [Header("スタートUI")]
    [SerializeField] private TextMeshProUGUI starttext;

    [Header("敵速度の上昇")]
    public float NowEnemySpeed = 1f; // 現在の速度倍率
    public float SpeedRate = 0.05f; // 1秒あたりでどれだけ速くなるか
    public float MaxSpeed = 7f; // 最大速度

    private Coroutine startRoutine;

    private void Awake()
    {
        if (instance != null && instance == null)
        {
            Destroy(gameObject);
        }
        instance = this;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            StartCoroutine(WaitForFadeThenInit());
        }
    }

    private IEnumerator WaitForFadeThenInit()
    {
        // フェード完了を待つ
        if (FadeManager.instance != null)
        {
            while (!FadeManager.instance.IsFadeComplete) yield return null;
        }
        InitGame();
    }

    /// <summary>
    ///  ゲームの初期化
    /// </summary>
    private void InitGame()
    {
        // 現在のシーンをチェック
        if (SceneManager.GetActiveScene().name != "GameScene") return;

        isGameOver = false;
        isStarted = false;

        UpdateTimeUI();

        if (starttext != null)
        {
            starttext.gameObject.SetActive(true);
            StartCoroutine(ShowStarTextRoutine());
        }

        // ゲーム停止中
        Time.timeScale = 0f;
    }

    private void OnStartButtonPressed()
    {
        if (isStarted) return;
        isStarted = true;

        // マウスロック
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ボタン・テキスト非表示
        starttext?.gameObject.SetActive(false);

        // UI有効化
        timetext?.gameObject.SetActive(true);

        // ゲーム再開
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (isGameOver) return;

        // ==== スペースキーでゲームスタート ====
        if (!isStarted && Input.GetKeyUp(KeyCode.Space))
        {
            OnStartButtonPressed();
        }

        if (!isStarted) return;
        
        survivaltime += Time.deltaTime;

        // 敵速度上昇
        NowEnemySpeed += SpeedRate * Time.deltaTime;
        NowEnemySpeed = Mathf.Min(NowEnemySpeed, MaxSpeed);

        UpdateTimeUI();
    }

    /// <summary>
    /// スタートテキストを表示、スペースキーが押されるまで待機
    /// </summary>
    private IEnumerator ShowStarTextRoutine()
    {
        if (starttext != null)
        {
            starttext.gameObject.SetActive(true);
        }

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        // 押されたらスタートする
        OnStartButtonPressed();

        yield break;
    }

    // ==== 時間表示 ====
    private void UpdateTimeUI()
    {
        if (timetext != null)
        {
            timetext.text = "Time:" + survivaltime.ToString("F1");
        }
    }

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("ゲームオーバー");
        Time.timeScale = 1f;

        if (FadeManager.instance != null)
        {
            Debug.Log("FadeManager 経由でリザルトへ遷移します");
            FadeManager.instance.FadeToScene("Result");
        }
        else
        {
            Debug.LogWarning("FadeManager が見つからないため、直接シーン遷移します");
            SceneManager.LoadScene("Result");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    // ==== シーンリセット ====
    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            timetext = GameObject.Find("Timer")?.GetComponent<TMP_Text>();
            starttext = GameObject.Find("StartText")?.GetComponent<TextMeshProUGUI>();
            StartCoroutine(WaitForFadeThenInit());
        }
        else
        {
            if (timetext != null) timetext.gameObject.SetActive(false);
            if (starttext != null) starttext.gameObject.SetActive(false);
            Time.timeScale = 1;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
