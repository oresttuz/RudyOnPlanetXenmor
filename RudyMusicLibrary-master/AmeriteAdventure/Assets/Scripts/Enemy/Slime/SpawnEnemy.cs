using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject floor, player;
    public int numEnemies;
    public GameObject[] enemyTypes;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numEnemies; ++i)
        {
            Vector3 spawnPt = 
                new Vector3(Random.Range((floor.transform.localScale.x * 5f * 0.5f * -1f), floor.transform.localScale.x * 5f * 0.5f), 0f, Random.Range((floor.transform.localScale.z * 5f * 0.5f * -1f), floor.transform.localScale.z * 5f * 0.5f));
            int indexForEnemyType = Mathf.RoundToInt(Random.Range(0f, enemyTypes.Length - 1)); //temp
            GameObject cloneEnemy = Instantiate(enemyTypes[indexForEnemyType] , this.transform);
            cloneEnemy.transform.position = spawnPt;
            if (indexForEnemyType == 1) //temp
            {
                cloneEnemy.GetComponentInChildren<EnemyMovement>().Player = player;
            }
        }
    }

}
