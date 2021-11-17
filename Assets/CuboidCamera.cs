using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuboidCamera : MonoBehaviour
{
    [SerializeField] Vector3 dimensions;
    [SerializeField] float nearClipPlane = 0.3f;
    [SerializeField] float farClipPlane = 1000f;
    [SerializeField] bool reinitialize = false;

    private bool initialized = false;
    [SerializeField, HideInInspector] Camera[] cameras = new Camera[6];

    void OnValidate()
    {
        if(!initialized || reinitialize)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                Initialize();
                UpdateCameraValues();
            };
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
            Camera cam = cameras[i] = go.AddComponent<Camera>();
            
            switch(i) {
                case 0:
                    camTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    camTransform.name += " front";
                    break;
                case 1:
                    camTransform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    camTransform.name += " right";
                    break;
                case 2:
                    camTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    camTransform.name += " back";
                    break;
                case 3:
                    camTransform.rotation = Quaternion.Euler(0f, 270f, 0f);
                    camTransform.name += " left";
                    break;
                case 4:
                    camTransform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                    camTransform.name += " up";
                    break;
                case 5:
                    camTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    camTransform.name += " down";
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
            cam.nearClipPlane = nearClipPlane;
            cam.farClipPlane = farClipPlane;
        
            cam.usePhysicalProperties = true;
            cam.sensorSize = new Vector2(36, 36);
            cam.gateFit = Camera.GateFitMode.None;
            cam.fieldOfView = 90; // vertical FOV

            if(dimensions.x <= 0f || dimensions.y <= 0f || dimensions.z <= 0f)
            {
                continue;
            }
            Vector2 camAspect = new Vector2(1f, 1f);
            float camDepth = .5f;
            switch(i)
            {
                case 0:
                case 2: // front and back cameras pointing towards Z and -Z
                    camAspect = new Vector2(dimensions.x, dimensions.y);
                    camDepth = dimensions.z / 2f;
                    break;
                case 1:
                case 3: // left and right cameras pointing towards X and -X
                    camAspect = new Vector2(dimensions.z, dimensions.y);
                    camDepth = dimensions.x / 2f;
                    break;
                case 4:
                case 5: // up and down cameras pointing towards Y and -Y
                    camAspect = new Vector2(dimensions.x, dimensions.z);
                    camDepth = dimensions.y / 2f;
                    break;
                default:
                    Debug.Log("More then six cameras??");
                    break;
            }
            cam.nearClipPlane = camDepth;
            cam.farClipPlane = camDepth * 10f;
            cam.sensorSize = camAspect * 36f;
            cam.fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan( (camAspect.y/2f) / camDepth );
        }
    }

    void Start()
    {
        if(!initialized)
        {
            Initialize();
        }
    }
}
