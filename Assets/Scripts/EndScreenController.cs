using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndScreenController : MonoBehaviour {

    public Text HiScore;
    public Text CurrentScore;
    public Image OverlayBlackout;

    public Color OverlayStartColor;
    public Color OverlayEndColor;

    public float overlayFadeTime;

    private float fadeStartTime;
    private int state = 0;
    // Use this for initialization
    void Start()
    {
        CurrentScore.text = string.Format("{0}", ScoreManager.Instance.Score);
        HiScore.text = string.Format("{0}", ScoreManager.Instance.HiScore);
        fadeStartTime = Time.unscaledTime;
        OverlayBlackout.color = OverlayStartColor;
	}
	
	// Update is called once per frame
	void Update () {
        if (state == 0)
        {
            OverlayBlackout.color = Color.Lerp(OverlayStartColor, OverlayEndColor,
                                               (Time.unscaledTime - fadeStartTime) / overlayFadeTime);
            if (Time.unscaledTime > fadeStartTime + overlayFadeTime)
            {
                state++;
            }
        } else if (state == 1)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            } else if (Input.anyKeyDown)
            {
                state = 2;
                fadeStartTime = Time.unscaledTime;
            }
        } else if (state == 2)
        {
            OverlayBlackout.color = Color.Lerp(OverlayEndColor, OverlayStartColor,
                                               (Time.unscaledTime - fadeStartTime) / overlayFadeTime);
            if (Time.unscaledTime > fadeStartTime + overlayFadeTime)
            {
                ScoreManager.Instance.Reset();
                SceneManager.LoadScene("main");
            }
        }
    }
}
