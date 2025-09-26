using UnityEngine;
using TMPro;

public class TimerNumberUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerTrioController playerTrio; // arrasta aqui no Inspector
    public TMP_Text timeText;               // arrasta o TMP do UI

    [Header("Opções")]
    public bool hideWhenNotRunning = true;

    void Update()
    {
        if (playerTrio == null || timeText == null) return;

        bool running = playerTrio.IsMiniGameRunning();
        if (hideWhenNotRunning) timeText.enabled = running;

        if (!running) return;

        float rem = Mathf.Max(0f, playerTrio.GetTimeRemaining());
        // formatação: >=10 -> inteiro, <10 -> 1 casa decimal
        if (rem >= 10f)
            timeText.text = Mathf.CeilToInt(rem).ToString() + "s";
        else
            timeText.text = rem.ToString("F1") + "s";
    }
}
