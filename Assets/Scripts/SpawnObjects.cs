// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    public GameObject[] objects;

    public float spawnInterval = 2.0f;
    public float minSpawnInterval = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spawn());
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator Spawn()
    {
        while (true)
        {
            int spawnRandom = Random.Range(0, 1000) % objects.Length;
            int xRandom = Random.Range(0, 1000) % 3 - 1;
            int yRandom = Random.Range(0, 1000) % 3 - 1;

            Vector3 spawnPosition = new Vector3(xRandom, yRandom, 0);
            GameObject spawnedObject = objects[spawnRandom];

            Instantiate(spawnedObject, spawnedObject.transform.position + transform.position + spawnPosition, spawnedObject.transform.rotation);
            yield return new WaitForSeconds(spawnInterval);
            if (spawnInterval >= minSpawnInterval)
            {
                spawnInterval -= 0.01f;
            }
        }
    }
}
