using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Entity : MonoBehaviour
{
    public static int maxItems = 5;
    public float speed = 10.0f;

    protected ItemMine itemMine;
    protected Factory factory;

    public Dictionary<ItemType, int> items = new Dictionary<ItemType, int>();

    public bool IsMining { get { return isMining; } }

    protected bool isMining = false;
    protected float mineTimer = 0;

    [SerializeField]
    protected Slider progressBar;

    [SerializeField]
    protected Text itemText;

    // Start is called before the first frame update
    public virtual void Start()
    {
        progressBar.value = 0;
        progressBar.gameObject.SetActive(false);

        var types = Enum.GetValues(typeof(ItemType));
        foreach (ItemType iType in types)
        {
            items.Add(iType, 0);
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {

        if (itemMine)
        {
            if (items[itemMine.itemType] >= 5)
            {
                StopMining();
            }

            if (isMining)
            {
                mineTimer -= Time.deltaTime;

                if (mineTimer <= 0)
                {
                    items[itemMine.itemType] += 1;
                    Debug.Log(items[itemMine.itemType]);
                    mineTimer = itemMine.mineTime;
                    return;
                }
            }
            progressBar.value = (itemMine.mineTime - mineTimer) / itemMine.mineTime;
        }
    }

    public void StartMining()
    {
        if (items[itemMine.itemType] < maxItems && itemMine is not null)
        {
            mineTimer = itemMine.mineTime;
            isMining = true;
            progressBar.gameObject.SetActive(true);
        }
    }

    public void StopMining()
    {
        isMining = false;
        mineTimer = 0;
        progressBar.gameObject.SetActive(false);
    }

    public void RetrieveItems()
    {
        if (factory)
            items[factory.itemToProduce] += factory.GetNumProduced(Math.Min(maxItems - items[factory.itemToProduce], maxItems));
    }

    public void MoveTowards(Vector3 targetPosition)
    {
        var direction = (targetPosition - this.transform.position).normalized;
        this.transform.position = this.transform.position + direction * Time.deltaTime * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        itemMine = collision.gameObject.GetComponent<ItemMine>();
        factory = collision.gameObject.GetComponent<Factory>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        StopMining();
        itemMine = null;
        factory = null;
    }
}
