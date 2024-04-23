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
        Debug.Log("build tower here: " + name);
        // if the place is not empty, do nothing
        if (tower != null) return;
        GameObject towerToBuild = BuildManager.main.GetSelectedTower();

        //TDManager.main.SpendResource();

        tower = Instantiate(towerToBuild, transform.position,Quaternion.identity);
    }
}
