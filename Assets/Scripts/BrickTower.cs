using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickTower : MonoBehaviour
{
	private List<EnemyBrick>[] quadrants = new List<EnemyBrick>[4];
    private List<EnemyBrick> popQueue = new List<EnemyBrick>();

    [SerializeField]
    private float damageGiven = 0;
    [SerializeField]
    private Transform baseTransform;
    [SerializeField]
    private Vector3 brickPositionOffset = new Vector3(0, 0.5f, 0);
    [SerializeField]
    private float popSpeed = 1;
    [SerializeField]
    private float brickPopReenableTime = 5; // Seconds
    [SerializeField]
    private GameObject roof = null;

    private Vector3 roofSize = new Vector3(1, 1, 1); // Assume roof is 1 by 1 by 1 if it doesn't have collider

    // Use this for initialization
    void Start()
    {
        // Intialize quadrants with empty lists
        for (int i = 0; i < quadrants.Length; i++)
        {
            quadrants[i] = new List<EnemyBrick>();
        }

        if (roof != null)
        {
            BoxCollider collider = roof.GetComponent<BoxCollider>();

            if (collider != null)
            {
                roofSize = collider.size;
            }

            TryMoveRoof();
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
        else { damageGiven = 0; } 
	}

    private void TryMoveRoof()
    {
        if (roof != null)
        {
            float tallestQuadrantHeight = GetQuadrantHeight(HighestQuadrantIndex());

            Vector3 roofPos = baseTransform.position;
            roofPos.y += (roofSize.y/2) + tallestQuadrantHeight - 0.25f;

            roof.transform.position = roofPos;
        }
    }

    public void DamageTower(float damage)
    {
        damageGiven += damage;
    }

    private void AddBrick(EnemyBrick brick)
    {
        int brickCount = quadrants[0].Count + quadrants[1].Count + quadrants[2].Count + quadrants[3].Count;
        if (brickCount >= 4)
        {
            roof.SetActive(true);
        }
        
        
        if (brick.IsHeld)
        {
            brick.holdable.BreakJoint();
        }
        
        int shortestQuadrantIndex = ShortestQuadrantIndex();

        Vector3 brickSize = Vector3.Scale(brick.GetComponentInChildren<BoxCollider>().size, brick.GetComponentInChildren<BoxCollider>().transform.localScale);
        EnemyBrick highestBrickInQuadrant = HighestBrickInQuadrant(shortestQuadrantIndex);
        Vector3 highestBrickInQuadrantSize;
        Vector3 highestBrickInQuadrantPositon;

        brick.Stack();
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

        TryMoveRoof();
    }

    /* Removes highest brick */
    private void PopBrick()
    {
        EnemyBrick brick = HighestBrick();
        quadrants[HighestQuadrantIndex()].Remove(brick);
        brick.GetComponent<Rigidbody>().isKinematic = false;
        brick.GetComponentInChildren<BoxCollider>().enabled = true;

        float randomSignX = Mathf.Sign(Random.Range(-1, 1));
        float randomSignZ = Mathf.Sign(Random.Range(-1, 1));

        brick.GetComponent<Rigidbody>().velocity = new Vector3(randomSignX, 1f, randomSignZ) * popSpeed;

        popQueue.Add(brick);
        Invoke("UnstackFirstBrickInQueue", brickPopReenableTime);
    }

    private void UnstackFirstBrickInQueue()
    {
        popQueue[0].Unstack();
        popQueue.RemoveAt(0);
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyBrick enemy = other.attachedRigidbody.GetComponent<EnemyBrick>();

        if (enemy != null && enemy.isDead == true && enemy.inTower == false)
        {
            AddBrick(enemy);
        }
    }

    private float GetQuadrantHeight(int index)
    {
        if (quadrants[index].Count > 0)
        {
            Vector3 topBlockColliderSize = Vector3.Scale(quadrants[index][quadrants[index].Count - 1].GetComponentInChildren<BoxCollider>().size, 
                quadrants[index][quadrants[index].Count - 1].GetComponentInChildren<BoxCollider>().transform.localScale);
            float height = quadrants[index][quadrants[index].Count - 1].transform.position.y - brickPositionOffset.y + (topBlockColliderSize.y);
            return height;
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

        if (quadrants[highestQuadrantIndex].Count > 0)
        {
            return quadrants[highestQuadrantIndex][quadrants[HighestQuadrantIndex()].Count - 1];
        }

        return null;
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

    public bool IsTowerEmpty() 
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
