using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Drawing;

public class RoomGeneratorEditor
{

    [MenuItem("Generate Rooms/Generate Room")]
    public static void GenerateRoom()
    {
        GameObject point = GameObject.Find("PointToGenerate");

        if (point == null)
        {
            Debug.LogError("PointToGenerate não foi encontrado, crie um GameObject com esse nome na cena.");
            return;
        }
        RoomData roomData = point.GetComponent<RoomData>();

        if (roomData == null)
        {
            Debug.LogError("O script RoomData não foi encontrado.");
            return;
        }

        Vector3 size = roomData.tamanhoDaSala;
        float thickness = roomData.espessuraDaSala;

        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.position = point.transform.position + new Vector3(0, 0, size.z / 2f);
        backWall.transform.localScale = new Vector3(size.x, size.y, thickness);

        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.name = "FrontWall";
        frontWall.transform.position = point.transform.position + new Vector3(0, 0, -size.z / 2f);
        frontWall.transform.localScale = new Vector3(size.x, size.y, thickness);

        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.transform.position = point.transform.position + new Vector3(-size.x / 2f, 0, 0);
        leftWall.transform.localScale = new Vector3(thickness, size.y, size.z);

        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.transform.position = point.transform.position + new Vector3(size.x / 2f, 0, 0);
        rightWall.transform.localScale = new Vector3(thickness, size.y, size.z);

        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.position = point.transform.position + new Vector3(0, size.y / 2f, 0);
        ceiling.transform.localScale = new Vector3(size.x, thickness, size.z);



            
        RaycastHit hit;
        Vector3 origem = point.transform.position + Vector3.up * 0.1f; // ligeiramente acima para garantir que não colida com o chão da própria sala

        bool achouObjetoAbaixo = Physics.Raycast(origem, Vector3.down, out hit, 100f);

        Vector3 floorPos;

        if (achouObjetoAbaixo)
        {
            // Coloca o chão exatamente sobre o objeto atingido
            float yDoObjeto = hit.point.y;
            float metadeDaEspessura = thickness / 2f;

            floorPos = new Vector3(point.transform.position.x, yDoObjeto + metadeDaEspessura, point.transform.position.z);
        }
        else
        {
            // Caso não encontre nada, usa o valor padrão
            floorPos = point.transform.position + new Vector3(0, -size.y / 2f, 0);
        }

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = floorPos;
        floor.transform.localScale = new Vector3(size.x, thickness, size.z);






    }
}
