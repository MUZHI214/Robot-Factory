using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Diagnostics;

public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI woodUI;
    [SerializeField] TextMeshProUGUI rockUI;
    [SerializeField] TextMeshProUGUI towerBaseUI;
    [SerializeField] TextMeshProUGUI towerBarrelUI;
    [SerializeField] TextMeshProUGUI towerProjectileUI;
    [SerializeField] TextMeshProUGUI towerUI;
    [SerializeField] TextMeshProUGUI robotUI;
    [SerializeField] TextMeshProUGUI timeUI;

    Stopwatch stopwatch;

    void Start()
    {
        stopwatch = new Stopwatch();
        stopwatch.Start();
    }

    private void OnGUI()
    {
        var factoryManager = FactoryManager.Instance;

        if (factoryManager is null) return;

        woodUI.text = (factoryManager.player.items[ItemType.Wood]
            + factoryManager.robots.Aggregate(0, (acc, robot) => acc + robot.items[ItemType.Wood])).ToString();
        rockUI.text = (factoryManager.player.items[ItemType.Rock]
            + factoryManager.robots.Aggregate(0, (acc, robot) => acc + robot.items[ItemType.Rock])).ToString();
        towerBaseUI.text = (factoryManager.player.items[ItemType.TowerBase]
            + factoryManager.robots.Aggregate(0, (acc, robot) => acc + robot.items[ItemType.TowerBase])).ToString();
        towerBarrelUI.text = (factoryManager.player.items[ItemType.TowerBarrel]
            + factoryManager.robots.Aggregate(0, (acc, robot) => acc + robot.items[ItemType.TowerBarrel])).ToString();
        towerProjectileUI.text = (factoryManager.player.items[ItemType.TowerProjectile]
            + factoryManager.robots.Aggregate(0, (acc, robot) => acc + robot.items[ItemType.TowerProjectile])).ToString();
        towerUI.text = (factoryManager.player.items[ItemType.Tower]
            + factoryManager.robots.Aggregate(0, (acc, robot) => acc + robot.items[ItemType.Tower])).ToString();
        robotUI.text = factoryManager.robots.Count.ToString();
        timeUI.text = stopwatch.Elapsed.Seconds.ToString() + " sec";
    }
}
