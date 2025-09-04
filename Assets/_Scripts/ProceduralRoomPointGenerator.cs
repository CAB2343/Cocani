using UnityEngine;
using System.Collections.Generic;

public class ProceduralRoomPointGenerator : MonoBehaviour
{
    public Vector3 localOffset = Vector3.forward; 
    public Vector3 boxMin = new Vector3(-0.5f, -0.5f, -0.5f);
    public Vector3 boxMax = new Vector3(0.5f, 0.5f, 0.5f);
    public List<GameObject> RoomPrefabs = new List<GameObject>();

    private ProceduralGeneratorManager generatorManager;

    void Awake()
    {
        if (generatorManager == null)
            generatorManager = FindObjectOfType<ProceduralGeneratorManager>();

        if (generatorManager != null)
            generatorManager.RegisterRoom();
    }

    void Start()
    {
        if (RoomPrefabs.Count == 0) return;
        if (!generatorManager.CanCreateRoom()) return;

        Vector3 spawnPos = transform.TransformPoint(localOffset);
        Vector3 worldDirection = transform.TransformDirection(localOffset).normalized;

        bool spawned = false;

        List<GameObject> shuffledPrefabs = new List<GameObject>(RoomPrefabs);
        ShuffleList(shuffledPrefabs);

        foreach (GameObject prefab in shuffledPrefabs)
        {
            Quaternion lookRotation = Quaternion.LookRotation(worldDirection, Vector3.up);

            if (CanPlaceRoom(spawnPos, worldDirection))
            {
                Instantiate(prefab, spawnPos, lookRotation);
                spawned = true;
                break;
            }
        }

        if (!spawned)
        {

            // gerar parede
        }
    }

    bool CanPlaceRoom(Vector3 spawnPos, Vector3 direction)
    {

        Vector3 halfExtents = (boxMax - boxMin) * 0.5f;
        Vector3 centerLocal = (boxMin + boxMax) * 0.5f;
        Vector3 centerWorld = spawnPos + transform.rotation * centerLocal;


        Collider[] hits = Physics.OverlapBox(centerWorld, halfExtents, transform.rotation);

        foreach (Collider col in hits)
        {
            if (col.transform.IsChildOf(transform)) 
                continue; 
            return false;
        }


        float checkDistance = (boxMax - boxMin).magnitude;
        if (Physics.Raycast(spawnPos, direction, checkDistance))
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 spawnPos = transform.TransformPoint(localOffset);

        Vector3 centerLocal = (boxMin + boxMax) * 0.5f;
        Vector3 centerWorld = spawnPos + transform.rotation * centerLocal;
        Vector3 size = (boxMax - boxMin);

        Gizmos.matrix = Matrix4x4.TRS(centerWorld, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.color = Color.red;
        Vector3 worldDirection = transform.TransformDirection(localOffset).normalized;
        Gizmos.DrawRay(spawnPos, worldDirection * 2f);
    }
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
