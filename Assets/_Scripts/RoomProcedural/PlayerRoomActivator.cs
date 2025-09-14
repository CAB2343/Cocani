using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.Timeline;

public class PlayerRoomActivator : MonoBehaviour
{
    public ProceduralRoomPointGenerator CurrentRoom { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        ProceduralRoomPointGenerator room = other.GetComponent<ProceduralRoomPointGenerator>();
        if (room != null)
        {
            CurrentRoom = room;
            Debug.Log($"Entrou na sala {room.name}");
            //vai ativar a sala atual
            
            if (room.Creator != null)
                Debug.Log($"Essa sala foi criada por {room.Creator.name}");
                //ativa a sala creator
            if (room.Created != null)
                Debug.Log($"Essa sala criou {room.Created.name}");
                //ativa a sala created
        }
    }


    void OnTriggerExit(Collider other)
    {
        ProceduralRoomPointGenerator room = other.GetComponent<ProceduralRoomPointGenerator>();
        if (room != null && room == CurrentRoom)
        {
            Debug.Log($"Saiu da sala {room.name}");
            //desativa as salas anteriores
            CurrentRoom = null;
        }
    }
}
