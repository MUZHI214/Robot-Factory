using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

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

    private void OnGUI()
    {
        var factoryManager = FactoryManager.Instance;

        if (factoryManager is null) return;
        var t = woodUI.transform.position;
        t.y = 5;
        woodUI.transform.position = t;

        woodUI.text = factoryManager.player.items[ItemType.Wood].ToString();
        rockUI.text = factoryManager.player.items[ItemType.Rock].ToString();
        towerBaseUI.text = factoryManager.player.items[ItemType.TowerBase].ToString();
        towerBarrelUI.text = factoryManager.player.items[ItemType.TowerBarrel].ToString();
        towerProjectileUI.text = factoryManager.player.items[ItemType.TowerProjectile].ToString();
        towerUI.text = factoryManager.player.items[ItemType.Tower].ToString();
    }
}
