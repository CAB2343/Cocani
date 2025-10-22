using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Components")]
    public Canvas mainCanvas;
    public EventSystem eventSystem;
    
    [Header("Raycaster Settings")]
    public Camera raycastCamera;
    
    private PhysicsRaycaster physicsRaycaster;
    private GraphicRaycaster graphicRaycaster;
    
    void Start()
    {
        InitializeRaycasters();
    }
    
    void InitializeRaycasters()
    {
        // Inicializar PhysicsRaycaster se não existir
        if (raycastCamera != null)
        {
            physicsRaycaster = raycastCamera.GetComponent<PhysicsRaycaster>();
            if (physicsRaycaster == null)
            {
                physicsRaycaster = raycastCamera.gameObject.AddComponent<PhysicsRaycaster>();
            }
        }
        
        // Inicializar GraphicRaycaster no Canvas se não existir
        if (mainCanvas != null)
        {
            graphicRaycaster = mainCanvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        
        // Verificar se EventSystem existe
        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }
        }
    }
    
    // Método para verificar se um ponto está sobre UI
    public bool IsPointerOverUI()
    {
        if (eventSystem == null) return false;
        
        PointerEventData eventDataCurrentPosition = new PointerEventData(eventSystem);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        
        return results.Count > 0;
    }
    
    // Método para fazer raycast físico
    public bool PhysicsRaycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = Mathf.Infinity)
    {
        return Physics.Raycast(origin, direction, out hit, maxDistance);
    }
    
    // Método para fazer raycast de UI
    public List<RaycastResult> UIRaycast(Vector2 screenPosition)
    {
        if (eventSystem == null) return new List<RaycastResult>();
        
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.position = screenPosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        return results;
    }
    
    void Update()
    {
        // Verificar se os componentes ainda existem
        if (physicsRaycaster == null && raycastCamera != null)
        {
            physicsRaycaster = raycastCamera.GetComponent<PhysicsRaycaster>();
        }
        
        if (graphicRaycaster == null && mainCanvas != null)
        {
            graphicRaycaster = mainCanvas.GetComponent<GraphicRaycaster>();
        }
    }
    
    // Método para obter o PhysicsRaycaster atual
    public PhysicsRaycaster GetPhysicsRaycaster()
    {
        return physicsRaycaster;
    }
    
    // Método para obter o GraphicRaycaster atual
    public GraphicRaycaster GetGraphicRaycaster()
    {
        return graphicRaycaster;
    }
}