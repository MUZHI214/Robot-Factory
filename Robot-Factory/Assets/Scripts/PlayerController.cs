using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 10.0f;

    ItemMine itemMine;
    public List<Item> items = new List<Item>();

    bool isMining = false;
    float mineTimer = 0;

    [SerializeField]
    Slider progressBar;

    // Start is called before the first frame update
    void Start()
    {
        progressBar.value = 0;
        progressBar.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        var position = this.transform.position;
        position += new Vector3(moveDirection.x, moveDirection.y) * Time.deltaTime * speed;
        this.transform.position = position;

        if (itemMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mineTimer = itemMine.mineTime;
                isMining = true;
                progressBar.gameObject.SetActive(true);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isMining = false;
                mineTimer = itemMine.mineTime;
                progressBar.gameObject.SetActive(false);
            }

            if (isMining)
            {
                mineTimer -= Time.deltaTime;

                if (mineTimer <= 0)
                {
                    items.Add(new Item(itemMine.itemType));
                    mineTimer = itemMine.mineTime;
                    return;
                }
            }
            progressBar.value = (itemMine.mineTime - mineTimer) / itemMine.mineTime;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        itemMine = collision.gameObject.GetComponent<ItemMine>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        itemMine = null;
        progressBar.gameObject.SetActive(false);
        isMining = false;
        mineTimer = 0;
    }
}
