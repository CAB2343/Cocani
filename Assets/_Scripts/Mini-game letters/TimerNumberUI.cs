using UnityEngine;
using TMPro;

/// <summary>
/// Mostra apenas o número do timer.
/// - Enquanto o mini-game roda mostra o tempo em segundos (2 casas decimais).
/// - Ao completar (player vence) congela no tempo em que foi completado.
/// - Ao falhar por timeout mostra "0.00s".
/// - hideWhenNotRunning controla se o texto some quando não há mini-game ativo;
///   mesmo com hideWhenNotRunning=true o valor congelado (sucesso/fracasso) permanece visível.
/// </summary>
public class TimerNumberUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerTrioController playerTrio; // arrasta aqui no Inspector
    public TMP_Text timeText;               // arrasta o TMP do UI

    [Header("Opções")]
    public bool hideWhenNotRunning = true;

    // estado interno
    bool prevRunning = false;
    bool frozenSuccess = false;
    bool frozenFail = false;
    float frozenTime = 0f;

    void Update()
    {
        if (playerTrio == null || timeText == null) return;

        bool running = playerTrio.IsMiniGameRunning();

        // transição: começou a rodar
        if (running && !prevRunning)
        {
            // reset estados congelados quando iniciar novo mini-game
            frozenSuccess = false;
            frozenFail = false;
            frozenTime = 0f;
        }

        // transição: parou de rodar agora
        if (!running && prevRunning)
        {
            float rem = Mathf.Max(0f, playerTrio.GetTimeRemaining());

            if (rem > 0f)
            {
                // parou antes de zerar -> sucesso: congelar no tempo atual
                frozenSuccess = true;
                frozenFail = false;
                frozenTime = rem;
            }
            else
            {
                // tempo esgotou -> falha: mostrar 0.00
                frozenFail = true;
                frozenSuccess = false;
                frozenTime = 0f;
            }
        }

        prevRunning = running;

        // visibilidade do texto
        if (hideWhenNotRunning)
        {
            // se congelado por sucesso/falha mostramos mesmo com hideWhenNotRunning = true
            timeText.enabled = running || frozenSuccess || frozenFail;
        }
        else
        {
            timeText.enabled = true;
        }

        // atualização do texto
        if (running)
        {
            float rem = Mathf.Max(0f, playerTrio.GetTimeRemaining());
            timeText.text = rem.ToString("F2");
        }
        else if (frozenSuccess)
        {
            timeText.text = frozenTime.ToString("F2");
        }
        else if (frozenFail)
        {
            timeText.text = "0.00";
        }
    }
}
