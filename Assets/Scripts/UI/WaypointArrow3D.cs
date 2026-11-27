using UnityEngine;
using UnityEngine.UI;

namespace Rimbara.UI
{
    /// <summary>
    /// Renders an animated borderless 3D directional arrow into a transparent RawImage
    /// pointing smoothly toward the player's active objective.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public sealed class WaypointArrow3D : MonoBehaviour
    {
        private const int StudioLayer = 30;
        private const float SmoothTurnSpeed = 10f;

        private RawImage targetImage;
        private RenderTexture studioTexture;
        private Camera studioCamera;
        private Light keyLight;
        private Light rimLight;
        private Transform arrowPivot;
        private Transform arrowMeshTransform;
        private Mesh arrowMesh;
        private Material arrowMaterial;
        private float currentYaw;
        private bool hasTarget;
        private Transform lastExcludedCamera;

        private void Awake()
        {
            targetImage = GetComponent<RawImage>();
            targetImage.color = Color.white;
            targetImage.raycastTarget = false;
            CreateStudioRig();
        }

        public void UpdateDirection(Vector3 originPosition, Transform cameraTransform, Transform destination)
        {
            if (arrowPivot == null) return;
            hasTarget = destination != null && cameraTransform != null;
            if (targetImage != null) targetImage.enabled = hasTarget;
            if (!hasTarget) return;

            if (lastExcludedCamera != cameraTransform)
            {
                if (cameraTransform.TryGetComponent(out Camera mainCam))
                    mainCam.cullingMask &= ~(1 << StudioLayer);
                lastExcludedCamera = cameraTransform;
            }

            Vector3 toTarget = destination.position - originPosition;
            toTarget.y = 0f;
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;

            if (toTarget.sqrMagnitude > 0.001f && forward.sqrMagnitude > 0.001f)
            {
                float targetYaw = Vector3.SignedAngle(forward.normalized, toTarget.normalized, Vector3.up);
                currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, 1f - Mathf.Exp(-SmoothTurnSpeed * Time.unscaledDeltaTime));
            }

            float bobY = Mathf.Sin(Time.unscaledTime * 2.6f) * 0.045f;
            float pulseZ = Mathf.Sin(Time.unscaledTime * 3.8f) * 0.07f;
            arrowPivot.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
            arrowMeshTransform.localPosition = new Vector3(0f, bobY, pulseZ);
            studioCamera.Render();
        }

        private void CreateStudioRig()
        {
            studioTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4
            };
            studioTexture.Create();
            targetImage.texture = studioTexture;

            var root = new GameObject("WaypointArrow3D_Studio");
            root.hideFlags = HideFlags.HideAndDontSave;
            root.transform.position = new Vector3(0f, -600f, 0f);

            var camGo = new GameObject("StudioCamera");
            camGo.transform.SetParent(root.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.38f, -2.15f);
            camGo.transform.LookAt(root.transform.position);
            studioCamera = camGo.AddComponent<Camera>();
            studioCamera.enabled = false;
            studioCamera.clearFlags = CameraClearFlags.SolidColor;
            studioCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            studioCamera.cullingMask = 1 << StudioLayer;
            studioCamera.fieldOfView = 30f;
            studioCamera.nearClipPlane = 0.1f;
            studioCamera.farClipPlane = 15f;
            studioCamera.targetTexture = studioTexture;

            var keyGo = new GameObject("StudioKeyLight");
            keyGo.transform.SetParent(root.transform, false);
            keyGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            keyLight = keyGo.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(1f, 0.92f, 0.72f);
            keyLight.intensity = 2.0f;
            keyLight.cullingMask = 1 << StudioLayer;

            var rimGo = new GameObject("StudioRimLight");
            rimGo.transform.SetParent(root.transform, false);
            rimGo.transform.rotation = Quaternion.Euler(-15f, 145f, 0f);
            rimLight = rimGo.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = new Color(1f, 0.68f, 0.25f);
            rimLight.intensity = 1.25f;
            rimLight.cullingMask = 1 << StudioLayer;

            arrowPivot = new GameObject("ArrowPivot").transform;
            arrowPivot.SetParent(root.transform, false);

            var meshGo = new GameObject("ArrowMesh", typeof(MeshFilter), typeof(MeshRenderer));
            meshGo.layer = StudioLayer;
            meshGo.transform.SetParent(arrowPivot, false);
            arrowMeshTransform = meshGo.transform;

            arrowMesh = BuildArrowMesh();
            meshGo.GetComponent<MeshFilter>().sharedMesh = arrowMesh;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            arrowMaterial = new Material(shader);
            arrowMaterial.SetColor("_BaseColor", new Color(0.95f, 0.72f, 0.24f, 1f));
            arrowMaterial.SetColor("_Color", new Color(0.95f, 0.72f, 0.24f, 1f));
            arrowMaterial.SetFloat("_Metallic", 0.55f);
            arrowMaterial.SetFloat("_Smoothness", 0.72f);
            arrowMaterial.EnableKeyword("_EMISSION");
            arrowMaterial.SetColor("_EmissionColor", new Color(0.45f, 0.28f, 0.05f));
            meshGo.GetComponent<MeshRenderer>().sharedMaterial = arrowMaterial;
        }

        private static Mesh BuildArrowMesh()
        {
            // Extruded 2D arrow profile in XZ with beveled Y thickness; tip points along +Z.
            Vector2[] outline =
            {
                new Vector2(0f, 0.62f),
                new Vector2(0.44f, 0.04f),
                new Vector2(0.17f, 0.04f),
                new Vector2(0.17f, -0.52f),
                new Vector2(-0.17f, -0.52f),
                new Vector2(-0.17f, 0.04f),
                new Vector2(-0.44f, 0.04f)
            };

            const float halfH = 0.10f;
            int n = outline.Length;
            var verts = new System.Collections.Generic.List<Vector3>();
            var norms = new System.Collections.Generic.List<Vector3>();
            var tris = new System.Collections.Generic.List<int>();

            // Top face (+Y): fan from center-ish split (head + shaft) via convex decomposition:
            // Head triangles: (0,1,2), (0,2,5), (0,5,6); Shaft triangles: (2,3,4), (2,4,5).
            int[] capTris = { 0, 1, 2, 0, 2, 5, 0, 5, 6, 2, 3, 4, 2, 4, 5 };

            int topOffset = verts.Count;
            for (int i = 0; i < n; i++)
            {
                verts.Add(new Vector3(outline[i].x, halfH, outline[i].y));
                norms.Add(Vector3.up);
            }
            for (int i = 0; i < capTris.Length; i += 3)
            {
                tris.Add(topOffset + capTris[i]);
                tris.Add(topOffset + capTris[i + 1]);
                tris.Add(topOffset + capTris[i + 2]);
            }

            int botOffset = verts.Count;
            for (int i = 0; i < n; i++)
            {
                verts.Add(new Vector3(outline[i].x, -halfH, outline[i].y));
                norms.Add(Vector3.down);
            }
            for (int i = 0; i < capTris.Length; i += 3)
            {
                tris.Add(botOffset + capTris[i]);
                tris.Add(botOffset + capTris[i + 2]);
                tris.Add(botOffset + capTris[i + 1]);
            }

            // Side walls with crisp flat normals for 3D edge highlights.
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                Vector3 aTop = new Vector3(outline[i].x, halfH, outline[i].y);
                Vector3 bTop = new Vector3(outline[next].x, halfH, outline[next].y);
                Vector3 bBot = new Vector3(outline[next].x, -halfH, outline[next].y);
                Vector3 aBot = new Vector3(outline[i].x, -halfH, outline[i].y);
                Vector3 sideNormal = Vector3.Cross(bTop - aTop, aBot - aTop).normalized;

                int idx = verts.Count;
                verts.Add(aTop); verts.Add(bTop); verts.Add(bBot); verts.Add(aBot);
                norms.Add(sideNormal); norms.Add(sideNormal); norms.Add(sideNormal); norms.Add(sideNormal);
                tris.Add(idx); tris.Add(idx + 2); tris.Add(idx + 1);
                tris.Add(idx); tris.Add(idx + 3); tris.Add(idx + 2);
            }

            var mesh = new Mesh { name = "WaypointArrow3D" };
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private void OnDestroy()
        {
            if (arrowPivot != null && arrowPivot.parent != null)
                Destroy(arrowPivot.parent.gameObject);
            if (studioTexture != null)
            {
                studioTexture.Release();
                Destroy(studioTexture);
            }
            if (arrowMesh != null) Destroy(arrowMesh);
            if (arrowMaterial != null) Destroy(arrowMaterial);
        }
    }
}
