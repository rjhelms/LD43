using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {

    public Image OverlayBlackout;
    public GameObject MusicPlayerPrefab;
    public Color OverlayStartColor;
    public Color OverlayEndColor;

    public float overlayFadeTime;

    private float fadeStartTime;
    private int state = 0;
    // Use this for initialization
    void Start()
    {
        fadeStartTime = Time.unscaledTime;
        OverlayBlackout.color = OverlayStartColor;
        if (GameObject.FindWithTag("MusicPlayer") == null)
        {
            GameObject music = Instantiate(MusicPlayerPrefab);
            DontDestroyOnLoad(music);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (state == 0)
        {
            OverlayBlackout.color = Color.Lerp(OverlayStartColor, OverlayEndColor,
                                               (Time.unscaledTime - fadeStartTime) / overlayFadeTime);
            if (Time.unscaledTime > fadeStartTime + overlayFadeTime)
            {
                state++;
            }
        }
        else if (state == 1)
        {
            if (Input.anyKeyDown)
            {
                state = 2;
                fadeStartTime = Time.unscaledTime;
            }
        }
        else if (state == 2)
        {
            OverlayBlackout.color = Color.Lerp(OverlayEndColor, OverlayStartColor,
                                               (Time.unscaledTime - fadeStartTime) / overlayFadeTime);
            if (Time.unscaledTime > fadeStartTime + overlayFadeTime)
            {
                ScoreManager.Instance.Reset();
                SceneManager.LoadScene(1);
            }
        }
    }
}
