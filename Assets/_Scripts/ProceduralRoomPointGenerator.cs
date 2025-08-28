using UnityEngine;
using System.Collections.Generic;

public class ProceduralRoomPointGenerator : MonoBehaviour
{
    public Vector3 localOffset = Vector3.forward; 
    public Vector3 scale = Vector3.one;
    public List<GameObject> RoomPrefabs = new List<GameObject>();

    private ProceduralGeneratorManager generatorManager;

    void Awake()
    {

        if (generatorManager == null)
        {
            generatorManager = FindObjectOfType<ProceduralGeneratorManager>();
        }


        if (generatorManager != null)
        {
            generatorManager.RegisterRoom();
        }
    }

    void Start()
    {
        if (RoomPrefabs.Count == 0) return;
        if (!generatorManager.CanCreateRoom())
            return;

        int roomIndex = Random.Range(0, RoomPrefabs.Count);
        GameObject chosenRoom = RoomPrefabs[roomIndex];

        Vector3 spawnPos = transform.TransformPoint(localOffset);
        Vector3 worldDirection = transform.TransformDirection(localOffset).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(worldDirection, Vector3.up);

        Vector3 size = scale;
        if (CanPlaceRoom(spawnPos, size))
        {
            Instantiate(chosenRoom, spawnPos, lookRotation);
        }
        else
        {
            // cria uma parede
        }

    }

    bool CanPlaceRoom(Vector3 spawnPos, Vector3 size)
    {
        Collider[] hits = Physics.OverlapBox(spawnPos, size * 0.5f, transform.rotation);
        return hits.Length == 0;
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 spawnPos = transform.TransformPoint(localOffset);
        Gizmos.DrawLine(transform.position, spawnPos);
        Gizmos.matrix = Matrix4x4.TRS(spawnPos, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, scale);
        Gizmos.matrix = Matrix4x4.identity; // reset
        Vector3 worldDirection = transform.TransformDirection(localOffset).normalized;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(spawnPos, worldDirection * 2f);
    }
}
