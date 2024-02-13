using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemMine : MonoBehaviour
{
    public Item item;
    public GameObject miner;

    public float mineTime = 1.0f;

    float mineTimer;
    bool isBeingMined = false;

    [SerializeField]
    Slider progressBar;

    // Start is called before the first frame update
    void Start()
    {
        mineTimer = mineTime;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isBeingMined = true;
            progressBar.gameObject.SetActive(true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isBeingMined = false;
            mineTimer = mineTime;
            progressBar.gameObject.SetActive(false);
        }

        if (isBeingMined)
        {
            mineTimer -= Time.deltaTime;

            if (mineTimer <= 0)
            {
                Instantiate(item, miner ? miner.transform : null);
                mineTimer = mineTime;
                return;
            }
        }

        progressBar.value = (mineTime - mineTimer) / mineTimer;
    }
}
