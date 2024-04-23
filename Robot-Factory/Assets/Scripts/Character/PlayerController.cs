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

        if (factory)
        {
            if (Input.GetMouseButtonUp(0))
            {
                foreach (var (reagant, amount) in Item.recipes[factory.itemToProduce])
                {
                    if (items[reagant] < amount)
                    {
                        return;
                    }
                }


                foreach (var (reagant, amount) in Item.recipes[factory.itemToProduce])
                {
                    if (factory.AddItems(reagant, amount))
                    {
                        items[reagant] -= amount;
                    }
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                RetrieveItems();
            }
        }

        base.Update();
    }
}
