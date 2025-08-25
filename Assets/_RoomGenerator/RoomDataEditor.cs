using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomData))]
public class RoomDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Mostra os campos padrão do RoomData
        DrawDefaultInspector();

        RoomData data = (RoomData)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Gerar Paredes"))
        {
            GerarParedes(data);
        }

        if (GUILayout.Button("Apagar Paredes"))
        {
            ApagarParedes(data);
        }
    }

    private void GerarParedes(RoomData data)
    {
        Vector3 center = data.transform.position;
        Vector3 size = data.tamanhoDaSala;
        float t = data.espessuraDaSala;

        if (data.gerarParedeFrente)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Frente";
            go.transform.position = center + new Vector3(0, 0, size.z / 2f);
            go.transform.localScale = new Vector3(size.x, size.y, t);
            go.transform.SetParent(data.transform);
        }

        if (data.gerarParedeAtrás)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Atrás";
            go.transform.position = center - new Vector3(0, 0, size.z / 2f);
            go.transform.localScale = new Vector3(size.x, size.y, t);
            go.transform.SetParent(data.transform);
        }

        if (data.gerarParedeEsquerda)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Esquerda";
            go.transform.position = center - new Vector3(size.x / 2f, 0, 0);
            go.transform.localScale = new Vector3(t, size.y, size.z);
            go.transform.SetParent(data.transform);
        }

        if (data.gerarParedeDireita)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Direita";
            go.transform.position = center + new Vector3(size.x / 2f, 0, 0);
            go.transform.localScale = new Vector3(t, size.y, size.z);
            go.transform.SetParent(data.transform);
        }

        if (data.gerarTeto)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Teto";
            go.transform.position = center + new Vector3(0, size.y / 2f, 0);
            go.transform.localScale = new Vector3(size.x, t, size.z);
            go.transform.SetParent(data.transform);
        }
        
        if (data.gerarChão)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Chão";
            go.transform.position = center - new Vector3(0, size.y / 2f, 0);
            go.transform.localScale = new Vector3(size.x, t, size.z);
            go.transform.SetParent(data.transform);
        }
    }

    private void ApagarParedes(RoomData data)
    {
        // Remove todos os filhos (paredes) gerados anteriormente
        for (int i = data.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(data.transform.GetChild(i).gameObject);
        }
    }
}
