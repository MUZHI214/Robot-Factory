using GOAP;
using UnityEngine;

public class GOAPManager : MonoBehaviour
{
    Action[] currentPlan;
    WorldState currentState;
    // WorldState newState = new WorldState();
    Entity entity;
    int currentActionIndex = 0;

    FloatGoal items = new FloatGoal("Items", 5);
    PositionGoal minePosition = new PositionGoal("Mine", 1, Vector2.up);

    // Start is called before the first frame update
    void Start()
    {
        this.TryGetComponent<Entity>(out entity);

        // Create initial world state
        Domain factoryDomain = new Domain();
        currentState = new WorldState();
        currentState.Debug = true;

        // Add Goals
        currentState.FloatGoals.Add(items, 0);
        currentState.PositionGoals.Add(minePosition, this.gameObject.transform.position);

        // Create actions
        Action moveToMine = new Action("Move to Mine");
        moveToMine.PositionEffects.Add(minePosition, Vector2.up);

        Action mineItems = new Action("Mine");
        mineItems.PositionPreconditions.Add(minePosition, Vector2.up);
        mineItems.FloatEffects.Add(items, 1);

        factoryDomain.Actions.Add(mineItems);
        factoryDomain.Actions.Add(moveToMine);

        currentState.Domain = factoryDomain;

        Debug.Log(currentState);

        currentPlan = DFSPlan.plan(currentState, 4);
        Debug.Log("DFS Plan:");
        foreach (Action action in currentPlan)
            Debug.Log(action);
        Debug.Log("");
    }

    // Update is called once per frame
    void Update()
    {
        if (entity is null) return;

        // TODO: Figure out when to reset the index
        if (currentActionIndex >= currentPlan.Length) return;

        if (currentPlan[currentActionIndex].Name == "Move to Mine")
        {
            // Make sure entity isn't trying to mine
            if (entity.IsMining)
                entity.StopMining();

            // Move the entity
            entity.MoveTowards(minePosition.Position);// = currentPlan[currentActionIndex].PositionEffects[minePosition] + new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
            var movedAmount = new Vector2(entity.transform.position.x, entity.transform.position.y) - currentState.PositionGoals[minePosition];
            if (Vector2.Distance(movedAmount, currentPlan[currentActionIndex].PositionEffects[minePosition]) <= 0.01)
            {
                currentState.PositionGoals[minePosition] += currentPlan[currentActionIndex].PositionEffects[minePosition];
                entity.transform.position = currentState.PositionGoals[minePosition];
                currentActionIndex++;
            }
        }
        else
        {
            // Start mining
            if (!entity.IsMining)
                entity.StartMining();
            else
            {
                if (entity.items[ItemType.Wood] > currentState.FloatGoals[items])
                {
                    entity.StopMining();
                    currentState.FloatGoals[items]++;
                    currentActionIndex++;
                }
            }
        }
    }
}
