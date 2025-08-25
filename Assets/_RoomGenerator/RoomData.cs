using UnityEngine;

public class RoomData : MonoBehaviour
{
    public Vector3 tamanhoDaSala = new Vector3(10f, 3f, 10f);
    public float espessuraDaSala = 0.5f;
    public bool gerarParedeAtrás = true;
    public bool gerarParedeFrente = true;
    public bool gerarParedeEsquerda = true;
    public bool gerarParedeDireita = true;
    public bool gerarTeto = true;
    public bool gerarChão = true;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(transform.position, tamanhoDaSala);

        Gizmos.color = Color.red;

        Vector3 center = transform.position;
        Vector3 size = tamanhoDaSala;
        float t = espessuraDaSala;

        // Frente
        Gizmos.DrawWireCube(center + new Vector3(0, 0, size.z / 2f), new Vector3(size.x, size.y, t));
        // Trás
        Gizmos.DrawWireCube(center - new Vector3(0, 0, size.z / 2f), new Vector3(size.x, size.y, t));
        // Esquerda
        Gizmos.DrawWireCube(center - new Vector3(size.x / 2f, 0, 0), new Vector3(t, size.y, size.z));
        // Direita
        Gizmos.DrawWireCube(center + new Vector3(size.x / 2f, 0, 0), new Vector3(t, size.y, size.z));

    }

}



