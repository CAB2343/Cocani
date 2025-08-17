using System.Collections.Generic;
using UnityEngine;

public class ProceduralRoomPointGenerator : MonoBehaviour
{
    [Header("Instanciação")]
    public Vector3 localOffset = Vector3.forward; 
    public Vector3 scale = Vector3.one;

    [Header("Prefabs")]
    public List<GameObject> RoomPrefabs = new List<GameObject>();

    private int roomIndex;

    void Start()
    {
        if (RoomPrefabs.Count == 0) return;

        roomIndex = Random.Range(0, RoomPrefabs.Count);
        GameObject chosenRoom = RoomPrefabs[roomIndex];

        // Posição relativa no mundo
        Vector3 spawnPos = transform.TransformPoint(localOffset);

        // Rotação para olhar na direção do offset
        Vector3 worldDirection = transform.TransformDirection(localOffset).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(worldDirection, Vector3.up);

        // Instancia com posição + rotação
        GameObject instance = Instantiate(chosenRoom, spawnPos, lookRotation);
    }

    // Desenhar no editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 spawnPos = transform.TransformPoint(localOffset);

        // Linha até o ponto
        Gizmos.DrawLine(transform.position, spawnPos);

        // Desenha cubo representando posição
        Gizmos.DrawWireCube(spawnPos, scale);

        // Desenha setinha mostrando a frente do objeto
        Vector3 worldDirection = transform.TransformDirection(localOffset).normalized;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(spawnPos, worldDirection * 2f); // seta vermelha mostrando "frente"
    }
}
