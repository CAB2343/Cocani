// ProceduralGeneratorManager.cs
using UnityEngine;

public class ProceduralGeneratorManager : MonoBehaviour
{
    public int maxRooms = 30;
    private int currentRooms = 0;

    public bool CanCreateRoom()
    {
        return currentRooms < maxRooms;
    }

    public void RegisterRoom()
    {
        currentRooms++;
        Debug.Log("Total de salas: " + currentRooms);
    }
}
