using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    [Header("Important Objects")]
    public Transform EggParent;
    public Transform MoneyBagSpawnPoint;
    public GameObject MoneyBagPrefab;
    public GameObject[] EggPrefabs;

    [SerializeField] private GameObject[] eggSpawnPoints;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Color happyGodColor;
    [SerializeField] private Color angryGodColor;

    [Header("Game Balance")]
    [SerializeField] private float moneyBagSpawnTime;
    [SerializeField] private int moneyBagValue;
    [SerializeField] private int eggHappinessCostShock;
    [SerializeField] private int godHappinessCostTick;
    [SerializeField] private int eggHappinessBoostCoin;
    [SerializeField] private int godHappinessBoostSacrifice;
    [SerializeField] private float godHappinessTickLength;
    [SerializeField] private float godHappinessTickGrace;
    [SerializeField] private float eggNumberTarget;
    [SerializeField] private float eggSpawnChanceMin;
    [SerializeField] private float eggSpawnChanceMax;
    [SerializeField] private float eggSpawnTickLength;
    [SerializeField] private float coinTimeout;

    [Header("UI Elements")]
    [SerializeField] private RectTransform godHappinessBar;
    [SerializeField] private RectTransform eggHappinessBar;
    [SerializeField] private Text coinText;
    [SerializeField] private Text timeText;
    [SerializeField] private float happinessBarScaleFactor = 0.25f;
    [SerializeField] private Image overlayBlackout;
    [SerializeField] private Color overlayStartColor;
    [SerializeField] private Color overlayEndColor;
    [SerializeField] private float overlayFadeTime;

    private float fadeStartTime;

    [Header("Current Gameplay Values")]
    [SerializeField] private int money;
    [SerializeField] private int GodHappiness = 100;
    [SerializeField] private int EggHappiness = 100;
    [SerializeField] private int currentEggs;

    private float nextEggSpawnTick;
    private float nextGodHappinessTick;
    private bool moneyBagPresent;
    private float moneyBagNextSpawn;
    private float state = 0;

    public float CoinTimeout
    {
        get
        {
            return coinTimeout;
        }

        private set
        {
            coinTimeout = value;
        }
    }

    // Use this for initialization
    void Start () {
        moneyBagNextSpawn = Time.time; // spawn money bag on first tick?
        nextGodHappinessTick = Time.time + godHappinessTickGrace;
        nextEggSpawnTick = Time.time + eggSpawnTickLength;
        eggSpawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        currentEggs = EggParent.childCount;
        Time.timeScale = 0;
        fadeStartTime = Time.unscaledTime;
        overlayBlackout.color = overlayStartColor;
	}

    // Update is called once per frame
    void Update()
    {
        if (state == 0)
        {
            overlayBlackout.color = Color.Lerp(overlayStartColor, overlayEndColor,
                                               (Time.unscaledTime - fadeStartTime) / overlayFadeTime);
            if (Time.unscaledTime > fadeStartTime + overlayFadeTime)
            {
                state++;
                Time.timeScale = 1;
            }
        }
        else if (state == 1)
        {
            if (Input.GetButtonDown("Reset"))
            {
                SceneManager.LoadScene("main");
            }
            GodHappiness = Mathf.Clamp(GodHappiness, -10, 100);
            EggHappiness = Mathf.Clamp(EggHappiness, -10, 100);
            worldCamera.backgroundColor = Color.Lerp(angryGodColor, happyGodColor, GodHappiness / 100f);
            if (Time.time > moneyBagNextSpawn)
            {
                moneyBagNextSpawn += moneyBagSpawnTime;
                if (!moneyBagPresent)
                {
                    Instantiate(MoneyBagPrefab, MoneyBagSpawnPoint);
                    moneyBagPresent = true;
                }
            }
            if (Time.time > nextGodHappinessTick)
            {
                GodHappiness -= godHappinessCostTick;
                nextGodHappinessTick = Time.time + godHappinessTickLength;
            }
            if (Time.time > nextEggSpawnTick)
            {
                nextEggSpawnTick = Time.time + eggSpawnTickLength;
                float eggSpawnChance = eggSpawnChanceMin;
                float eggSpawnRange = eggSpawnChanceMax - eggSpawnChanceMin;
                eggSpawnChance += eggSpawnRange * Mathf.Clamp01((eggNumberTarget - currentEggs) / eggNumberTarget);
                if (Random.value < eggSpawnChance)
                {
                    EggSpawn();
                }
            }
        } else if (state > 1)
        {
            overlayBlackout.color = Color.Lerp(overlayEndColor, overlayStartColor,
                                   (Time.unscaledTime - fadeStartTime) / overlayFadeTime);
            if (Time.unscaledTime > fadeStartTime + overlayFadeTime)
            {
                if (state == 2)
                    SceneManager.LoadScene("eggLose");
                if (state == 3)
                    SceneManager.LoadScene("godLose");
            }
        }
    }

    private void LateUpdate()
    {
        if (state < 2)
        {
            eggHappinessBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(EggHappiness, 0, 100) * happinessBarScaleFactor);
            godHappinessBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(GodHappiness, 0, 100) * happinessBarScaleFactor);
            coinText.text = string.Format("{0}", money);
            timeText.text = string.Format("{0:F0}", Mathf.Floor(Time.timeSinceLevelLoad));
            if (EggHappiness <= -10)
            {
                EggLose();
            }
            else if (GodHappiness <= -10)
            {
                GodLose();
            }
        }
    }

    private void EggLose()
    {
        ScoreManager.Instance.Score = (int)Mathf.Floor(Time.timeSinceLevelLoad);
        if (ScoreManager.Instance.Score >= ScoreManager.Instance.HiScore)
            ScoreManager.Instance.HiScore = ScoreManager.Instance.Score;
        Debug.Log("Egg lose!");
        Time.timeScale = 0;
        fadeStartTime = Time.unscaledTime;
        state = 2;
    }

    private void GodLose()
    {
        ScoreManager.Instance.Score = (int)Mathf.Floor(Time.timeSinceLevelLoad);
        if (ScoreManager.Instance.Score >= ScoreManager.Instance.HiScore)
            ScoreManager.Instance.HiScore = ScoreManager.Instance.Score;
        Debug.Log("God lose!");
        Time.timeScale = 0;
        fadeStartTime = Time.unscaledTime;
        state = 3;
    }

    private void EggSpawn()
    {
        Transform spawnPoint = eggSpawnPoints[Random.Range(0, eggSpawnPoints.Length)].transform;
        GameObject prefab = EggPrefabs[Random.Range(0, EggPrefabs.Length)];
        GameObject newEgg = Instantiate(prefab, spawnPoint.position, Quaternion.identity, EggParent);
        newEgg.GetComponent<Egg>().SetRandomDirection();
        currentEggs++;
    }

    public void EggDied()
    {
        GodHappiness += godHappinessBoostSacrifice;
        nextGodHappinessTick = Time.time + godHappinessTickGrace;
        currentEggs--;
    }

    public void EggAppeased(GameObject eggObject, GameObject coinObject)
    {
        Coin coin = coinObject.GetComponent<Coin>();
        if (coin.Active)
        {
            coin.Active = false; // deactivate the coin so it's only counted once
            EggHappiness += eggHappinessBoostCoin;
        }
    }

    public void GetMoney(GameObject moneyObject)
    {
        money += moneyBagValue;
        Destroy(moneyObject);
        moneyBagPresent = false;
        moneyBagNextSpawn += moneyBagSpawnTime;
    }

    public void RegisterShock()
    {
        EggHappiness -= eggHappinessCostShock;
    }

    public bool TryThrowCoin()
    {
        if (money > 0)
        {
            money--;
            return true;
        } else
        {
            return false;
        }
    }
}
