using UnityEngine;

public class Minimap2D : MonoBehaviour
{
    public enum Mode { StaticMap_MoveIcon, ScrollingMap_CenterPlayer }

    [Header("References")]
    public Transform player;          // Player 3D
    public RectTransform viewport;    // MinimapViewport (tem RectMask2D)
    public RectTransform mapImage;    // Imagem do mapa
    public RectTransform playerIcon;  // Flecha/ícone

    [Header("World bounds (XZ)")]
    public Vector2 worldMinXZ = new Vector2(-50, -50);
    public Vector2 worldMaxXZ = new Vector2(50, 50);
    public BoxCollider worldBoundsCollider; // opcional: sobrescreve min/max

    [Header("Behavior")]
    public Mode mode = Mode.StaticMap_MoveIcon;
    public bool rotateIconWithPlayer = true;
    public bool clampToViewport = true;

    Vector2 MapLocalFromWorld(Vector3 world)
    {
        float nx = Mathf.InverseLerp(worldMinXZ.x, worldMaxXZ.x, world.x);
        float nz = Mathf.InverseLerp(worldMinXZ.y, worldMaxXZ.y, world.z);

        float lx = (nx - 0.5f) * mapImage.rect.width;
        float ly = (nz - 0.5f) * mapImage.rect.height;
        return new Vector2(lx, ly);
    }

    void Awake()
    {
        if (worldBoundsCollider)
        {
            var b = worldBoundsCollider.bounds;
            worldMinXZ = new Vector2(b.min.x, b.min.z);
            worldMaxXZ = new Vector2(b.max.x, b.max.z);
        }
    }

    void LateUpdate()
    {
        if (!player || !viewport || !mapImage || !playerIcon) return;

        Vector2 local = MapLocalFromWorld(player.position);

        if (mode == Mode.StaticMap_MoveIcon)
        {
            // Mapa parado; ícone se move
            mapImage.anchoredPosition = Vector2.zero;

            var p = local;
            if (clampToViewport)
            {
                var half = viewport.rect.size * 0.5f;
                p.x = Mathf.Clamp(p.x, -half.x, half.x);
                p.y = Mathf.Clamp(p.y, -half.y, half.y);
            }
            playerIcon.anchoredPosition = p;
        }
        else // ScrollingMap_CenterPlayer
        {
            // Player fixo no centro; mapa se move
            playerIcon.anchoredPosition = Vector2.zero;

            var p = -local;
            if (clampToViewport)
            {
                var halfV = viewport.rect.size * 0.5f;
                var halfM = mapImage.rect.size * 0.5f;
                float minX = -(halfM.x - halfV.x);
                float maxX = (halfM.x - halfV.x);
                float minY = -(halfM.y - halfV.y);
                float maxY = (halfM.y - halfV.y);
                p.x = Mathf.Clamp(p.x, minX, maxX);
                p.y = Mathf.Clamp(p.y, minY, maxY);
            }
            mapImage.anchoredPosition = p;
        }

        if (rotateIconWithPlayer)
        {
            playerIcon.localEulerAngles = new Vector3(0, 0, -player.eulerAngles.y);
        }
    }
}
