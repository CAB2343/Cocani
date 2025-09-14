using UnityEngine;
using System.Collections.Generic;

public class ProceduralRoomPointGenerator : MonoBehaviour
{
    public Vector3 localOffset = Vector3.forward;
    public List<GameObject> RoomPrefabs = new List<GameObject>();
    public GameObject WallPrefab; // adicione isso no inspetor para definir sua parede

    [SerializeField] private GameObject creator;
    [SerializeField] private GameObject created;

    public GameObject Creator { get => creator; set => creator = value; }
    public GameObject Created { get => created; set => created = value; }
    public GameObject RoomRoot => transform.root.gameObject;

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

        // embaralhar lista
        List<GameObject> shuffledPrefabs = new List<GameObject>(RoomPrefabs);
        ShuffleList(shuffledPrefabs);

        foreach (GameObject prefab in shuffledPrefabs)
        {
            Quaternion lookRotation = Quaternion.LookRotation(worldDirection, Vector3.up);

            Debug.Log($"{name} tentando instanciar {prefab.name} em {spawnPos}");

            if (CanPlaceRoom(spawnPos, lookRotation, prefab))
            {
                GameObject newRoom = Instantiate(prefab, spawnPos, lookRotation);

                ProceduralRoomPointGenerator newRoomGenerator = newRoom.GetComponentInChildren<ProceduralRoomPointGenerator>();
                if (newRoomGenerator == null)
                {
                    Debug.LogWarning($"{newRoom.name} não tem ProceduralRoomPointGenerator!");
                }
                else
                {
                    newRoomGenerator.Creator = this.gameObject;
                    this.Created = newRoom;
                    Debug.Log($"{name} criou {newRoom.name}");
                    Debug.Log($"{newRoom.name} foi criado por {newRoomGenerator.Creator.name}");
                }

                spawned = true;
                break; // achou sala válida, para
            }
        }

        // se nenhuma coube, gera parede (sem contar como "room")
        if (!spawned)
        {
            if (WallPrefab != null)
            {
                Quaternion wallRotation = Quaternion.LookRotation(worldDirection, Vector3.up);
                Instantiate(WallPrefab, spawnPos, wallRotation, transform.parent);

                Debug.Log($"{name} não conseguiu instanciar nenhuma sala, gerando parede.");
            }
            else
            {
                Debug.Log($"{name} não conseguiu instanciar nenhuma sala e não há WallPrefab configurado.");
            }
        }
    }

    bool CanPlaceRoom(Vector3 spawnPos, Quaternion rotation, GameObject prefab)
    {
        Bounds prefabBounds = GetPrefabBounds(prefab);

        if (prefabBounds.size == Vector3.zero)
        {
            Debug.LogWarning($"{prefab.name} não tem colliders!");
            return false;
        }

        Vector3 halfExtents = prefabBounds.extents;
        Vector3 center = spawnPos + rotation * prefabBounds.center;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation);

        foreach (Collider col in hits)
        {
            if (col.transform.root == transform.root) 
                continue;

            Debug.Log($"{name} não pode instanciar {prefab.name}, colisão com {col.name}");
            return false;
        }

        return true;
    }

    Bounds GetPrefabBounds(GameObject prefab)
    {
        Collider[] colliders = prefab.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        Bounds bounds = colliders[0].bounds;
        foreach (Collider c in colliders)
        {
            bounds.Encapsulate(c.bounds);
        }

        bounds.center = prefab.transform.InverseTransformPoint(bounds.center);
        return bounds;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 spawnPos = transform.TransformPoint(localOffset);
        Gizmos.DrawWireSphere(spawnPos, 0.25f);

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
