using UnityEngine;
using System.Collections.Generic;

public class ProceduralRoomPointGenerator : MonoBehaviour
{
    public Vector3 localOffset = Vector3.forward;
    public Vector3 boxMin = new Vector3(-0.5f, -0.5f, -0.5f);
    public Vector3 boxMax = new Vector3(0.5f, 0.5f, 0.5f);
    public List<GameObject> RoomPrefabs = new List<GameObject>();

    [SerializeField] private GameObject creator;
    [SerializeField] private GameObject created;

    public GameObject Creator { get => creator; set => creator = value; }
    public GameObject Created { get => created; set => created = value; }


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

    Debug.Log($"{name} tentando instanciar {prefab.name} em {spawnPos}");

    if (CanPlaceRoom(spawnPos, worldDirection))
    {
        GameObject newRoom = Instantiate(prefab, spawnPos, lookRotation);
        ProceduralRoomPointGenerator newRoomGenerator = newRoom.GetComponentInChildren<ProceduralRoomPointGenerator>();
        if (newRoomGenerator == null)
        {
            Debug.LogWarning($"{newRoom.name} não tem ProceduralRoomPointGenerator!");
        }

        if (newRoomGenerator != null)
        {
            newRoomGenerator.Creator = this.gameObject;
            this.Created = newRoom;
            Debug.Log($"{name} criou {newRoom.name}");
            Debug.Log($"{newRoom.name} foi criado por {newRoomGenerator.Creator.name}");
        }

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
    // calcula centro e tamanho do box
    Vector3 halfExtents = (boxMax - boxMin) * 0.5f;
    Vector3 centerLocal = (boxMin + boxMax) * 0.5f;
    Vector3 centerWorld = spawnPos + transform.rotation * centerLocal;

    // pega todos os colliders dentro do box
    Collider[] hits = Physics.OverlapBox(centerWorld, halfExtents, transform.rotation);

    foreach (Collider col in hits)
    {
        // ignora a si mesmo e seus filhos
        if (col.transform.root == transform.root)
            continue;

        Debug.Log($"{name} não pode instanciar, colisão com {col.name}");
        return false;
    }

    float checkDistance = (boxMax - boxMin).magnitude;
    if (Physics.Raycast(spawnPos, direction, checkDistance))
    {
        Debug.Log($"{name} não pode instanciar, algo bloqueia o caminho");
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
