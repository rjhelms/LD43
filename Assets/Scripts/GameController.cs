using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    [Header("Important Objects")]
    public Transform EggParent;
    public Transform MoneyBagSpawnPoint;
    public GameObject MoneyBagPrefab;

    [Header("Game Balance")]
    [SerializeField] private float moneyBagSpawnTime;
    [SerializeField] private int moneyBagValue;
    [SerializeField] private int HappinessCostShock = 10;

    [Header("Current Gameplay Values")]
    [SerializeField] private int money;
    [SerializeField] private int GodHappiness = 100;
    [SerializeField] private int EggHappiness = 100;

    private bool moneyBagPresent;
    private float moneyBagNextSpawn;
    // Use this for initialization
    void Start () {
        moneyBagNextSpawn = Time.time + moneyBagSpawnTime;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > moneyBagNextSpawn)
        {
            moneyBagNextSpawn += moneyBagSpawnTime;
            if (!moneyBagPresent)
            {
                Instantiate(MoneyBagPrefab, MoneyBagSpawnPoint);
                moneyBagPresent = true;
            }
        }
	}

    public void GetMoney(GameObject moneyObject)
    {
        money += moneyBagValue;
        Destroy(moneyObject);
        moneyBagPresent = false;
    }

    public void RegisterShock()
    {
        EggHappiness -= HappinessCostShock;
    }
}
