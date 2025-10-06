using UnityEngine;
using System.Collections.Generic;

public class ProceduralGeneratorManager : MonoBehaviour
{
    public int maxRooms = 30;
    private int currentRooms = 0;

    private List<ProceduralRoomPointGenerator> allPoints = new List<ProceduralRoomPointGenerator>();

    public bool CanCreateRoom()
    {
        return currentRooms < maxRooms;
    }

    public void RegisterRoom()
    {
        currentRooms++;

        if (currentRooms >= maxRooms)
        {
            Debug.Log("Limite de salas atingido. Finalizando geração...");
            FinalizeGeneration();
        }
    }

    public void RegisterPoint(ProceduralRoomPointGenerator point)
    {
        if (!allPoints.Contains(point))
            allPoints.Add(point);
    }

    public void FinalizeGeneration()
    {
        Debug.Log("Finalizando geração: fechando aberturas vazias...");

        foreach (var point in allPoints)
        {
            point.TryFinalize();
        }
    }
}
