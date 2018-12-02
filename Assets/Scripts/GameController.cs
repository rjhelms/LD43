using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    [Header("Important Objects")]
    public Transform EggParent;
    public Transform MoneyBagSpawnPoint;
    public GameObject MoneyBagPrefab;
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
    

    [Header("Current Gameplay Values")]
    [SerializeField] private int money;
    [SerializeField] private int GodHappiness = 100;
    [SerializeField] private int EggHappiness = 100;

    private float nextGodHappinessTick;
    private bool moneyBagPresent;
    private float moneyBagNextSpawn;
    // Use this for initialization
    void Start () {
        moneyBagNextSpawn = Time.time + moneyBagSpawnTime;
        nextGodHappinessTick = Time.time + godHappinessTickGrace;
	}
	
	// Update is called once per frame
	void Update () {
        GodHappiness = Mathf.Clamp(GodHappiness, 0, 100);
        EggHappiness = Mathf.Clamp(EggHappiness, 0, 100);
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
	}

    public void EggDied()
    {
        GodHappiness += godHappinessBoostSacrifice;
        nextGodHappinessTick = Time.time + godHappinessTickGrace;
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
}
