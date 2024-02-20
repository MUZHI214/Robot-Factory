using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Entity
{

    // Update is called once per frame
    public override void Update()
    {
        var moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        var position = this.transform.position;
        position += new Vector3(moveDirection.x, moveDirection.y) * Time.deltaTime * speed;
        this.transform.position = position;

        if (itemMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartMining();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                StopMining();
            }
        }

        base.Update();
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
