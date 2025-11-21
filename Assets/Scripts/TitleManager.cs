using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using JetBrains.Annotations;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starttext;
    [SerializeField] private GameObject Tutorial;
    private bool isLoading = false;

    void Start()
    {
        // カーソルを表示
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // スタートテキストを点滅させる
        StartCoroutine(BlinkStartText());
    }

    private IEnumerator BlinkStartText()
    {
        while (true)
        {
            starttext.alpha = 1;
            yield return new WaitForSeconds(0.5f);
            starttext.alpha = 0;
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// スタートボタン押下でゲームシーンへ
    /// </summary>
    public void OnClickStart()
    {
        if(isLoading)return;
        isLoading = true;
        FadeManager.instance.FadeToScene("GameScene");
    }

    /// <summary>
    /// チュートリアルを表示
    /// </summary>
    public void OnClickTutorial()
    {
        Tutorial.SetActive(true);
    }

    /// <summary>
    /// チュートリアルを閉じる
    /// </summary>
    public void OnClickBack()
    {
        Tutorial.SetActive(false);
    }

    /// <summary>
    /// ゲームの終了
    /// </summary>
    public void OnExitGame()
    {
        Debug.Log("ゲーム終了");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
