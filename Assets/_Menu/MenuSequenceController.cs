using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuSequenceController : MonoBehaviour
{
    [System.Serializable]
    public class AnimationStep
    {
        public Animator animatorAlvo; // O objeto que tem a animação
        
        [Tooltip("Se marcado, espera a animação acabar antes de ir para a próxima")]
        public bool esperarTerminar = true;
        
        [Tooltip("Tempo extra de espera após a animação")]
        public float delayExtra = 0f;
    }

    public List<AnimationStep> sequenciaDeAnimacao;

    void Start()
    {
        // Opcional: Garante que todos comecem desligados para a sequência funcionar
        foreach (var etapa in sequenciaDeAnimacao)
        {
            if(etapa.animatorAlvo != null)
                etapa.animatorAlvo.gameObject.SetActive(false);
        }

        StartCoroutine(ExecutarSequencia());
    }

    IEnumerator ExecutarSequencia()
    {
        foreach (var etapa in sequenciaDeAnimacao)
        {
            if (etapa.animatorAlvo != null)
            {
                // 1. Ativa o objeto. O Animator vai rodar o estado "Entry" (Laranja) automaticamente.
                etapa.animatorAlvo.gameObject.SetActive(true);

                // 2. Pequeno delay para o Animator inicializar
                yield return new WaitForSeconds(0.1f);

                if (etapa.esperarTerminar)
                {
                    // Pega a duração da animação atual (Layer 0)
                    float duracao = etapa.animatorAlvo.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(duracao + etapa.delayExtra);
                }
                else
                {
                    if (etapa.delayExtra > 0) yield return new WaitForSeconds(etapa.delayExtra);
                }
            }
        }
    }
}