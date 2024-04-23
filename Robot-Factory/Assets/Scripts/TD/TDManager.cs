using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDManager : MonoBehaviour
{
    public static TDManager main;

    public Transform startPoint;
    public Transform[] path;

    Entity entity = new Entity();
    FactoryManager factoryManager = new FactoryManager();

    public string wood;
    public string rock;
    public string towerBase;
    public string towerBarrel;
    public string towerProjectile;
    public string towerNum;

    private void Awake()
    {
        main = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }
    void Update()
    {
        //wood = entity.items[ItemType.Wood].ToString();
        //rock = entity.items[ItemType.Rock].ToString();
        //towerBase = entity.items[ItemType.TowerBase].ToString();
        //towerBarrel = entity.items[ItemType.TowerBarrel].ToString();
        //towerProjectile = entity.items[ItemType.TowerProjectile].ToString();
        //towerNum = entity.items[ItemType.Tower].ToString();
    }

}
