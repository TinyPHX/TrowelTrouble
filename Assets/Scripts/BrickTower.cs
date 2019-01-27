using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickTower : MonoBehaviour
{
	private List<EnemyBrick>[] quadrants = new List<EnemyBrick>[4];
    private float damageGiven = 0;

    /* Min and max distances for bricks after being popped */
    [SerializeField]
    private float brickRespawnDistanceMin = 2;
    [SerializeField]
    private float brickRespawnDistanceMax = 5;

    [SerializeField]
    private Transform baseTransform;
    [SerializeField]
    private Vector3 brickPositionOffset = new Vector3(0, 0.5f, 0);

    // Use this for initialization
    void Start() {
        // Intialize quadrants with empty lists
        for (int i = 0; i < quadrants.Length; i++)
        {
            quadrants[i] = new List<EnemyBrick>();
        }
    }
	
	/* Update is called once per frame */
	void Update() {
        if (!IsTowerEmpty())
        {
            // Check if highest brick should be removed
            if (damageGiven >= HighestBrick().maxHealth)
            {
                damageGiven -= HighestBrick().maxHealth;
                PopBrick();
            }
        }

        //Time.timeScale = 0.8f;
	}

    public void DamageTower(float damage)
    {
        damageGiven += damage;
    }

    private void AddBrick(EnemyBrick brick)
    {
        if (brick.IsHeld)
        {
            brick.holdable.BreakJoint();
        }
        
        int shortestQuadrantIndex = ShortestQuadrantIndex();

        Vector3 brickSize = Vector3.Scale(brick.GetComponentInChildren<BoxCollider>().size, brick.GetComponentInChildren<BoxCollider>().transform.localScale);
        EnemyBrick highestBrickInQuadrant = HighestBrickInQuadrant(shortestQuadrantIndex);
        Vector3 highestBrickInQuadrantSize;
        Vector3 highestBrickInQuadrantPositon;

        brick.inTower = true;
        brick.GetComponent<Rigidbody>().isKinematic = true;
        brick.GetComponentInChildren<BoxCollider>().enabled = false;

        float verticalPosition = (brickSize.y / 2);

        if (highestBrickInQuadrant == null)
        {
            highestBrickInQuadrantSize = new Vector3(0, 0, 0);
            highestBrickInQuadrantPositon = baseTransform.position;
            verticalPosition += baseTransform.position.y + brickPositionOffset.y;
        } else
        {
            highestBrickInQuadrantSize = Vector3.Scale(highestBrickInQuadrant.GetComponentInChildren<BoxCollider>().size, highestBrickInQuadrant.GetComponentInChildren<BoxCollider>().transform.localScale);
            highestBrickInQuadrantPositon = highestBrickInQuadrant.transform.position;
            verticalPosition += highestBrickInQuadrantPositon.y + (highestBrickInQuadrantSize.y / 2);
        }

        //Debug.Log(shortestQuadrantIndex);

        switch (shortestQuadrantIndex) // Note that if a brick is rotated by 90 degrees, the x and z positions (relative to the tower) need to be switched
        {
            case 0: // 1, 1
                brick.transform.eulerAngles = new Vector3(0, 0, 0); // 0 degrees y
                brick.transform.position = new Vector3(
                    brickPositionOffset.x + baseTransform.position.x + (brickSize.x / 2) * 1,
                    verticalPosition,
                    brickPositionOffset.z + baseTransform.position.z + (brickSize.z / 2) * 1);
                break;
            case 1: // 1, -1
                brick.transform.eulerAngles = new Vector3(0, 0, 0); // 0 degrees y
                brick.transform.position = new Vector3(
                    brickPositionOffset.x + baseTransform.position.x + (brickSize.x / 2) * 1,
                    verticalPosition,
                    brickPositionOffset.z + baseTransform.position.z + (brickSize.z / 2) * -1);
                break;
            case 2: // -1, -1
                brick.transform.eulerAngles = new Vector3(0, 0, 0); // 0 degrees y
                brick.transform.position = new Vector3(
                    brickPositionOffset.x + baseTransform.position.x + (brickSize.x / 2) * -1,
                    verticalPosition,
                    brickPositionOffset.z + baseTransform.position.z + (brickSize.z / 2) * -1);
                break;
            case 3: // -1, 1
                brick.transform.eulerAngles = new Vector3(0, 0, 0); // 0 degrees y
                brick.transform.position = new Vector3(
                    brickPositionOffset.x + baseTransform.position.x + (brickSize.x / 2) * -1,
                    verticalPosition,
                    brickPositionOffset.z + baseTransform.position.z + (brickSize.z / 2) * 1);
                break;
        }

        quadrants[shortestQuadrantIndex].Add(brick);
    }

    /* Removes highest brick */
    private void PopBrick()
    {
        HighestBrick().inTower = false;
        quadrants[HighestQuadrantIndex()].Remove(HighestBrick());
        // TODO: Move brick away from tower in random direction, use raycasting to set brick close to ground
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyBrick enemy = other.attachedRigidbody.GetComponent<EnemyBrick>();

        if (enemy != null && enemy.health <= 0 && enemy.inTower == false) // Check if EnemyBrick script was found, the brick is dead, and the brick isn't in the tower
        {
            AddBrick(enemy);
        }
    }

    private float GetQuadrantHeight(int index)
    {
        if (quadrants[index].Count > 0)
        {
            return quadrants[index][quadrants[index].Count-1].transform.position.y + (quadrants[index][quadrants[index].Count-1].GetComponentInChildren<BoxCollider>().size.y / 2);
        }

        return 0;
    }

    /* Returns index of shortest quadrant */
    private int ShortestQuadrantIndex()
    {
        float lowestHeight = GetQuadrantHeight(0);
        int lowestHeightIndex = 0;

        for (int i = 1; i < quadrants.Length; i++)
        {
            if (GetQuadrantHeight(i) < lowestHeight)
            {
                lowestHeight = GetQuadrantHeight(i);
                lowestHeightIndex = i;
            }
        }

        return lowestHeightIndex;
    }

    /* Returns index of highest quadrant */
    private int HighestQuadrantIndex()
    {
        float highestHeight = GetQuadrantHeight(0);
        int highestHeightIndex = 0;

        for (int i = 1; i < quadrants.Length; i++)
        {
            if (GetQuadrantHeight(i) > highestHeight)
            {
                highestHeight = GetQuadrantHeight(i);
                highestHeightIndex = i;
            }
        }

        return highestHeightIndex;
    }

    /* Returns brick (in tower) with highest upper bound */
    private EnemyBrick HighestBrick()
    {
        int highestQuadrantIndex = HighestQuadrantIndex();

        return quadrants[highestQuadrantIndex][quadrants[HighestQuadrantIndex()].Count - 1];
    }

    /* Returns the highest brick in a certain quadrant */
    private EnemyBrick HighestBrickInQuadrant(int index)
    {
        if (quadrants[index].Count > 0)
        {
            return quadrants[index][quadrants[index].Count - 1];
        }

        return null;
    }

    private bool IsTowerEmpty()
    {
        bool empty = true;

        foreach (List<EnemyBrick> quadrant in quadrants)
        {
            if (quadrant.Count != 0)
            {
                empty = false;
            }
        }

        return empty;
    }
}
