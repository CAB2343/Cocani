using UnityEngine;

public class DisableOnS : MonoBehaviour
{
    [Tooltip("Objeto que será desativado quando apertar S.")]
    public GameObject target;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (target != null)
            {
                target.SetActive(false);
                Debug.Log($"[DisableOnS] {target.name} foi desativado.");
            }
        }
    }
}
