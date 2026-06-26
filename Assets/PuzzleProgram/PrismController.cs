using System;
using UnityEngine;

public class PrismController : MonoBehaviour
{
    [Serializable]
    public class ColorBeam
    {
        public string beamName;
        public Transform exitPoint;
        public LineRenderer lineRenderer;
        public Color color = Color.white;
    }

    [Header("Puzzle")]
    public PrismPuzzleManager puzzleManager;

    [Header("Flashlight")]
    public Transform flashlight;
    public Light flashlightLight;
    public Collider prismCollider;

    [Tooltip("設為 0 時，使用 Spot Light 的一半角度。")]
    public float prismHitAngle = 0f;

    [Header("Beam")]
    public ColorBeam[] beams;
    public float maxBeamDistance = 20f;
    public float beamWidth = 0.025f;

    [Header("Rotation")]
    public float rotateSpeed = 50f;
    public bool allowKeyboardRotate = false;

    private bool isHitByFlashlight;

    void Start()
    {
        if (prismCollider == null)
        {
            prismCollider = GetComponent<Collider>();
        }

        SetupBeams();
        SetBeamsActive(false);

        if (puzzleManager != null)
        {
            puzzleManager.HideAllNumbers();
        }
    }

    void Update()
    {
        RotatePrism();

        isHitByFlashlight = IsHitByFlashlight();

        if (isHitByFlashlight)
        {
            SetBeamsActive(true);
            UpdateBeams();
        }
        else
        {
            SetBeamsActive(false);
            ClearPuzzleLight();
        }
    }

    void SetupBeams()
    {
        foreach (ColorBeam beam in beams)
        {
            if (beam.lineRenderer == null)
            {
                continue;
            }

            beam.lineRenderer.positionCount = 2;
            beam.lineRenderer.useWorldSpace = true;

            // 永遠面向攝影機，避免玩家站在側面時看不到光
            beam.lineRenderer.alignment = LineAlignment.View;

            beam.lineRenderer.startWidth = beamWidth;
            beam.lineRenderer.endWidth = beamWidth;

            beam.lineRenderer.startColor = beam.color;
            beam.lineRenderer.endColor = beam.color;

            beam.lineRenderer.numCornerVertices = 0;
            beam.lineRenderer.numCapVertices = 0;

            beam.lineRenderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;

            beam.lineRenderer.receiveShadows = false;
        }
    }

    void RotatePrism()
    {
        if (!allowKeyboardRotate)
        {
            return;
        }

        float rotateInput = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotateInput = -1f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            rotateInput = 1f;
        }

        if (rotateInput != 0f)
        {
            transform.Rotate(
                Vector3.up,
                rotateInput * rotateSpeed * Time.deltaTime,
                Space.World
            );
        }
    }

    bool IsFlashlightOn()
    {
        if (flashlightLight == null)
        {
            return false;
        }

        if (!flashlightLight.enabled)
        {
            return false;
        }

        if (!flashlightLight.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (flashlightLight.intensity <= 0.01f)
        {
            return false;
        }

        return true;
    }

    bool IsHitByFlashlight()
    {
        if (flashlight == null || flashlightLight == null)
        {
            return false;
        }

        if (!IsFlashlightOn())
        {
            return false;
        }

        if (prismCollider == null)
        {
            prismCollider = GetComponent<Collider>();
        }

        if (prismCollider == null)
        {
            return false;
        }

        Transform lightTransform = flashlightLight.transform;

        Vector3 prismPoint =
            prismCollider.ClosestPoint(lightTransform.position);

        if (prismPoint == lightTransform.position)
        {
            prismPoint = prismCollider.bounds.center;
        }

        Vector3 directionToPrism =
            prismPoint - lightTransform.position;

        float distanceToPrism = directionToPrism.magnitude;

        if (distanceToPrism < 0.001f)
        {
            return false;
        }

        if (distanceToPrism > flashlightLight.range)
        {
            return false;
        }

        float allowedAngle = prismHitAngle;

        if (allowedAngle <= 0f)
        {
            allowedAngle = flashlightLight.spotAngle * 0.5f;
        }

        float angle = Vector3.Angle(
            lightTransform.forward,
            directionToPrism.normalized
        );

        if (angle > allowedAngle)
        {
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            lightTransform.position,
            directionToPrism.normalized,
            distanceToPrism + 0.1f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        float closestDistance = float.MaxValue;
        Collider closestCollider = null;

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreFlashlightHit(
                hit.collider,
                lightTransform))
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestCollider = hit.collider;
            }
        }

        if (closestCollider == null)
        {
            return false;
        }

        bool hitPrism =
            closestCollider == prismCollider ||
            closestCollider.transform == transform ||
            closestCollider.transform.IsChildOf(transform);

        return hitPrism;
    }

    bool ShouldIgnoreFlashlightHit(
        Collider hitCollider,
        Transform lightTransform)
    {
        if (hitCollider == null)
        {
            return true;
        }

        Transform hitTransform = hitCollider.transform;

        // 忽略手電筒自己
        if (hitTransform == lightTransform ||
            hitTransform.IsChildOf(lightTransform))
        {
            return true;
        }

        // 忽略玩家、Camera、手電筒父物件
        if (lightTransform.IsChildOf(hitTransform))
        {
            return true;
        }

        return false;
    }

    void UpdateBeams()
    {
        if (puzzleManager != null)
        {
            puzzleManager.BeginBeamCheck();
        }

        for (int i = 0; i < beams.Length; i++)
        {
            FireBeam(i, beams[i]);
        }

        if (puzzleManager != null)
        {
            puzzleManager.EndBeamCheck();
        }
    }

    void FireBeam(int beamIndex, ColorBeam beam)
    {
        if (beam.exitPoint == null)
        {
            return;
        }

        Vector3 origin = beam.exitPoint.position;
        Vector3 direction = beam.exitPoint.forward.normalized;

        Vector3 endPoint =
            origin + direction * maxBeamDistance;

        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            maxBeamDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            WallNumberTarget numberTarget =
                hit.collider.GetComponent<WallNumberTarget>();

            if (numberTarget == null)
            {
                numberTarget =
                    hit.collider.GetComponentInParent<WallNumberTarget>();
            }

            if (numberTarget != null && puzzleManager != null)
            {
                puzzleManager.RecordBeamHit(
                    beamIndex,
                    numberTarget
                );
            }
        }

        if (beam.lineRenderer != null)
        {
            beam.lineRenderer.SetPosition(0, origin);
            beam.lineRenderer.SetPosition(1, endPoint);
        }
    }

    void SetBeamsActive(bool isActive)
    {
        foreach (ColorBeam beam in beams)
        {
            if (beam.lineRenderer != null)
            {
                beam.lineRenderer.enabled = isActive;
            }
        }
    }

    void ClearPuzzleLight()
    {
        if (puzzleManager != null)
        {
            // 手電筒沒照到時，直接讓所有數字隱藏
            puzzleManager.BeginBeamCheck();
        }
    }
}