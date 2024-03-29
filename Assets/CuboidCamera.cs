using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CuboidCamera : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("_dimensions")] Vector3 _sensorDimensions = new Vector3(3, 2, 5);
    public Vector3 sensorDimensions {
        get { return _sensorDimensions; }
        private set { _sensorDimensions = value; }
    }
    [SerializeField] bool _individualDisplays = true;
    public bool individualDisplays {
        get { return _individualDisplays; }
        private set { _individualDisplays = value; }
    }
    [SerializeField] float nearClipFactor = 0.1f;
    [SerializeField] float farClipFactor = 1000f;
    [SerializeField] bool reinitialize = false;
    [SerializeField, Range(-.5f, 0.5f)] float horizonLevel = 0f;

    [SerializeField, HideInInspector] bool initialized = false;
    [SerializeField, HideInInspector] Camera[] cameras = new Camera[6];

    void OnValidate()
    {
        if(!initialized || reinitialize)
        {
            if( UnityEditor.PrefabUtility.GetPrefabInstanceStatus(gameObject) == UnityEditor.PrefabInstanceStatus.NotAPrefab )
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    Initialize();
                    UpdateCameraValues();
                };
            }
            else
            {
                Debug.LogWarning("Cant reinitialize prefab instance. Work on prefab or unpack it.");
            }
            reinitialize = false;
        }
        else
        {
            UpdateCameraValues();
        }
        
    }
    void Initialize()
    {
        Debug.Log("Initizlizing Cuboid Cameras");

        // destroy existing gameobjects
        foreach(Camera c in cameras)
        {
            #if UNITY_EDITOR
            if(c != null && c.gameObject != null)
            {
                DestroyImmediate(c.gameObject);
            }
            #else
            if(c != null && c.gameObject != null)
                Destroy(c.gameObject);
            #endif
        }
        
        cameras = new Camera[6];
        for(int i=0; i < 6; i++)
        {
            GameObject go = new GameObject("Cuboid Cam " + (i+1));
            go.hideFlags = HideFlags.NotEditable;
            Transform camTransform = go.transform;
            camTransform.parent = transform;
            camTransform.localPosition = Vector3.zero;
            Camera cam = cameras[i] = go.AddComponent<Camera>();
            
            switch(i) {
                case 0:
                    camTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    camTransform.name += " front";
                    camTransform.gameObject.tag = "front";
                    break;
                case 1:
                    camTransform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    camTransform.name += " right";
                    camTransform.gameObject.tag = "right";
                    break;
                case 2:
                    camTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    camTransform.name += " back";
                    camTransform.gameObject.tag = "back";
                    break;
                case 3:
                    camTransform.rotation = Quaternion.Euler(0f, 270f, 0f);
                    camTransform.name += " left";
                    camTransform.gameObject.tag = "left";
                    break;
                case 4:
                    camTransform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                    camTransform.name += " up";
                    camTransform.gameObject.tag = "up";
                    break;
                case 5:
                    camTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    camTransform.name += " down";
                    camTransform.gameObject.tag = "down";
                    break;
                default:
                    Debug.Log("More then six cameras??");
                    break;
            }
        }
        initialized = true;
    }

    void UpdateCameraValues()
    {
        for(int i=0; i < 6; i++)
        {
            Camera cam = cameras[i];
            cam.nearClipPlane = nearClipFactor;
            cam.farClipPlane = farClipFactor;
        
            cam.usePhysicalProperties = true;

            if(sensorDimensions.x <= 0f || sensorDimensions.y <= 0f || sensorDimensions.z <= 0f)
            {
                continue;
            }
            Vector2 camAspect = new Vector2(1f, 1f);
            float camDepth = .5f; // focal distance?
            float camLocalYPosition =  - (sensorDimensions.y * Mathf.InverseLerp(-.5f, 0.5f, horizonLevel) - sensorDimensions.y * 0.5f);
            Vector2 lensShift = new Vector2(0f,0f);
            switch(i)
            {
                case 0:
                case 2: // front and back cameras pointing towards Z and -Z
                    camAspect = new Vector2(sensorDimensions.x, sensorDimensions.y);
                    camDepth = sensorDimensions.z / 2f;
                    lensShift.y = horizonLevel;
                    break;
                case 1:
                case 3: // left and right cameras pointing towards X and -X
                    camAspect = new Vector2(sensorDimensions.z, sensorDimensions.y);
                    camDepth = sensorDimensions.x / 2f;
                    lensShift.y = horizonLevel;
                    break;
                case 4: // up camera pointing towards Y
                    camAspect = new Vector2(sensorDimensions.x, sensorDimensions.z);
                    camDepth = sensorDimensions.y / 2f - camLocalYPosition;
                    break;
                case 5: // down cameras pointing towards -Y
                    camAspect = new Vector2(sensorDimensions.x, sensorDimensions.z);
                    camDepth = sensorDimensions.y / 2f + camLocalYPosition;
                    break;
                default:
                    Debug.Log("More then six cameras??");
                    break;
            }

            // This is confusing! todo: think of a better way to implement this.
            // Near and far clip planes are different depending on direction.
            cam.nearClipPlane = camDepth * nearClipFactor;
            cam.farClipPlane = camDepth * farClipFactor;

            cam.sensorSize = camAspect;
            cam.gateFit = Camera.GateFitMode.None; // Stretch the sensor gate to fit exactly into the resolution gate.
            cam.fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan( (camAspect.y/2f) / camDepth );
            cam.transform.localPosition = Vector3.up * camLocalYPosition;
            cam.lensShift = lensShift;
            cam.targetDisplay = individualDisplays ? i : 0;
            cam.depth = individualDisplays ? 0 : i; // should fix strange recording issue where some cameras were rendered incorrectly
        }
    }

    void Start()
    {
        if(!initialized)
        {
            Initialize();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, sensorDimensions);
        // Gizmos.DrawLine(Vector3.forward * sensorDimensions.z * .5f + Vector3.right * sensorDimensions.x * .5f, Vector3.forward * sensorDimensions.z * .5f - Vector3.right * sensorDimensions.x * .5f);
        // Gizmos.DrawLine(-Vector3.forward * sensorDimensions.z * .5f + Vector3.right * sensorDimensions.x * .5f, -Vector3.forward * sensorDimensions.z * .5f - Vector3.right * sensorDimensions.x * .5f);
    }
}
