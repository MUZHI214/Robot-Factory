using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plot : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    public GameObject tower;
    private Color startColor;

    public bool nextToPath;

    private void Start()
    {
        startColor = sr.color;
    }

    // player touch the plot
    private void OnMouseEnter()
    {
        sr.color = hoverColor;
    }
    private void OnMouseExit()
    {
        sr.color = startColor;
    }

    // player build tower on the plot
    private void OnMouseDown()
    {
        var player = FactoryManager.Instance.player;

        if (tower != null) return;
        if (player.items[ItemType.Tower] <= 0) return;

        Debug.Log("build tower here: " + name);
        player.items[ItemType.Tower]--;
        FactoryManager.Instance.towerPlots.Remove(this);
        GameObject towerToBuild = BuildManager.main.GetSelectedTower();
        tower = Instantiate(towerToBuild, transform.position, Quaternion.identity);
    }
}
