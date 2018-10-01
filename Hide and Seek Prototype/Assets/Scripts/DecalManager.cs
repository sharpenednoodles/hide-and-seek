using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handle decals from bullets and other damage such as melee or explosives
/// Uses object pooling to minimise overhead
/// To do, randomise rotation in local y space
/// </summary>

namespace HideSeek.WeaponController
{
    public class DecalManager : MonoBehaviour
    {
        [System.Serializable]
        public class Pool
        {
            [Tooltip("Tag of the object used to spawn")]
            public string tag;
            [Tooltip("Game Object prefab to spawn")]
            public GameObject prefab;
            [Tooltip("Max number of object instances allowed in world")]
            public int size;
        }

        public List<Pool> pools;
        public Dictionary<string, Queue<GameObject>> poolDictionary;

        private Queue<GameObject> decalsInWorld;

        void Start()
        {
            decalsInWorld = new Queue<GameObject>();
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, new Vector3(0,0,0), Quaternion.Euler(0, Random.Range(0, 360), 0), gameObject.transform.GetChild(1));
                    obj.SetActive(false);
                    //NEED TO SET AS AN OCCLUDER
                    objectPool.Enqueue(obj);
                }
                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        //Spawn decal from defined pool
        public void SpawnFromPool(string tag, Vector3 position, Quaternion rotation, GameObject parent)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("tag " +tag +" does not exist in current dictionary");
                return;
            }
                
            GameObject objToSpawn = poolDictionary[tag].Dequeue();
            objToSpawn.SetActive(true);
            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;
            objToSpawn.transform.SetParent(parent.transform);

            poolDictionary[tag].Enqueue(objToSpawn);
        }

        //overload for decal spawn for no parent
        public void SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("tag " + tag + " does not exist in current dictionary");
                return;
            }
        
            GameObject objToSpawn = poolDictionary[tag].Dequeue();
            objToSpawn.SetActive(true);
            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;

            poolDictionary[tag].Enqueue(objToSpawn);
        }
    }
}
