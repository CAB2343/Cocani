using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    [Header("Grid")]
    public int cols = 12;
    public int rows = 8;
    public GameObject cellPrefab;        // prefab com CellController
    public RectTransform gridParent;     // painel com GridLayoutGroup
    public float changeInterval = 0.25f; // tempo entre trocas de chars nas não-locked

    [Header("Chars")]
    public string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    [HideInInspector] public CellController[,] cells;
    [HideInInspector] public string targetString; // string alvo de 3 chars
    Vector2Int targetStart; // col,row do primeiro dos 3 (horizontal)

    void Start()
    {
        GenerateGrid();
        PlaceTargetRandom();
        StartCoroutine(RandomizeRoutine());
    }

    public void GenerateGrid()
    {
        // limpa se houver filhos (útil no editor)
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        cells = new CellController[cols, rows];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var go = Instantiate(cellPrefab, gridParent);
                var cc = go.GetComponent<CellController>();
                if (cc == null)
                {
                    Debug.LogError("CellPrefab precisa de CellController.");
                    continue;
                }
                char c = RandomChar();
                cc.SetChar(c, false);
                cells[x, y] = cc;
            }
        }
    }

    char RandomChar()
    {
        int i = Random.Range(0, allowedChars.Length);
        return allowedChars[i];
    }

    void PlaceTargetRandom()
    {
        // escolhe posição horizontal que caiba 3 células
        int r = Random.Range(0, rows);
        int c = Random.Range(0, cols - 2);

        targetStart = new Vector2Int(c, r);
        targetString = "" + RandomChar() + RandomChar() + RandomChar();

        // aplica target e bloqueia as 3 células
        for (int i = 0; i < 3; i++)
        {
            char ch = targetString[i];
            cells[c + i, r].SetChar(ch, true);
            cells[c + i, r].SetLocked(true);
        }
    }

    IEnumerator RandomizeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(changeInterval);
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    var cell = cells[x, y];
                    if (cell.locked) continue;
                    if (Random.value < 0.9f) // probabilidade de mudar
                        cell.SetChar(RandomChar(), false);
                }
            }
        }
    }

    public string GetStringAt(int startCol, int row)
    {
        if (startCol < 0 || startCol + 2 >= cols || row < 0 || row >= rows) return null;
        char a = cells[startCol, row].currentChar;
        char b = cells[startCol + 1, row].currentChar;
        char c = cells[startCol + 2, row].currentChar;
        return "" + a + b + c;
    }

    public void NewRound()
    {
        // desbloqueia tudo antes de novo target
        for (int y=0;y<rows;y++) for (int x=0;x<cols;x++) cells[x,y].SetLocked(false);
        PlaceTargetRandom();
    }
}
