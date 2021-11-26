#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

[RequireComponent(typeof(CuboidCamera))]
public class CuboidRecorder : MonoBehaviour
{

    [System.Flags]
    enum CameraDirections
    {
        front = 1,
        right = 2,
        back = 4,
        left = 8,
        up = 16,
        down = 32
    }
    [SerializeField] CameraDirections recordDirections = (CameraDirections)31; //render all but down by default

    RecorderController m_RecorderController;

    [SerializeField] string outputFolder = "CuboidRecordings";
    [SerializeField] bool sceneSubfolder = true;
    [SerializeField] float frameRate = 30;
    [SerializeField] Vector3Int outputDimensions = new Vector3Int(0, 1080, 0);
    [SerializeField] Vector3Int finalOutputDimensions;

    void OnEnable()
    {
        CuboidCamera cuboidCamera = GetComponent<CuboidCamera>();

        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(controllerSettings);

        var mediaOutputFolder = Path.Combine(Application.dataPath, "..", outputFolder);
        if(sceneSubfolder) mediaOutputFolder = Path.Combine(mediaOutputFolder, SceneManager.GetActiveScene().name);

        // Setup Recording
        if(recordDirections.HasFlag(CameraDirections.front))
            controllerSettings.AddRecorderSettings(
                createMovieRecorderSettings("front", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.y),
                                                            finalOutputDimensions.x, finalOutputDimensions.y, mediaOutputFolder) );
        if(recordDirections.HasFlag(CameraDirections.right))
            controllerSettings.AddRecorderSettings(
                createMovieRecorderSettings("right", new Vector2(cuboidCamera.sensorDimensions.z, cuboidCamera.sensorDimensions.y),
                                                            finalOutputDimensions.z, finalOutputDimensions.y, mediaOutputFolder)
        );
        if(recordDirections.HasFlag(CameraDirections.back))
            controllerSettings.AddRecorderSettings(
                createMovieRecorderSettings("back", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.y),
                                                            finalOutputDimensions.x, finalOutputDimensions.y, mediaOutputFolder)
        );
        if(recordDirections.HasFlag(CameraDirections.left))
            controllerSettings.AddRecorderSettings(
                createMovieRecorderSettings("left", new Vector2(cuboidCamera.sensorDimensions.z, cuboidCamera.sensorDimensions.y),
                                                            finalOutputDimensions.z, finalOutputDimensions.y, mediaOutputFolder)
        );
        if(recordDirections.HasFlag(CameraDirections.up))
            controllerSettings.AddRecorderSettings(
                createMovieRecorderSettings("up", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.z),
                                                            finalOutputDimensions.x, finalOutputDimensions.z, mediaOutputFolder)
        );
        if(recordDirections.HasFlag(CameraDirections.down))
            controllerSettings.AddRecorderSettings(
                createMovieRecorderSettings("down", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.z),
                                                            finalOutputDimensions.x, finalOutputDimensions.z, mediaOutputFolder)
        );

        controllerSettings.SetRecordModeToManual();
        controllerSettings.FrameRate = frameRate;

        RecorderOptions.VerboseMode = false;
        m_RecorderController.PrepareRecording();
        m_RecorderController.StartRecording();
    }

    void Start()
    {
        CuboidCamera cam = GetComponent<CuboidCamera>();
        if(cam.individualDisplays)
        {
            Debug.LogWarning("Unset Individual Displays checkbox and select Display 1 in Game View before recording.");
        }
    }

    void OnValidate()
    {
        CuboidCamera cam = GetComponent<CuboidCamera>();
        if(outputDimensions.x < 0) outputDimensions.x = 0;
        if(outputDimensions.y < 0) outputDimensions.y = 0;
        if(outputDimensions.z < 0) outputDimensions.z = 0;
        finalOutputDimensions = calculateFinalOutputDimensions(cam.sensorDimensions, outputDimensions);
    }
    Vector3Int calculateFinalOutputDimensions(Vector3 sd, Vector3Int od)
    {
        Vector3Int finalOD = od;
        if(finalOD.x == 0)
        {
            if(od.y != 0)
            {
                finalOD.x = Mathf.RoundToInt(od.y * sd.x / sd.y);
            }
            else if(od.z != 0)
            {
                finalOD.x = Mathf.RoundToInt(od.z * sd.x / sd.z);
            }
            else
            {
                Debug.LogWarning("undefined output dimension");
                return Vector3Int.one * 256;
            }
        }
        if(finalOD.z == 0)
        {
            if(od.x != 0)
            {
                finalOD.z = Mathf.RoundToInt(od.x * sd.z / sd.x);
            }
            else if(od.y != 0)
            {
                finalOD.z = Mathf.RoundToInt(od.y * sd.z / sd.y);
            }
            else
            {
                Debug.LogWarning("undefined output dimension");
                return Vector3Int.one * 256;
            }
        }
        if(finalOD.y == 0)
        {
            if(od.x != 0)
            {
                finalOD.y = Mathf.RoundToInt(od.x * sd.y / sd.x);
            }
            else if(od.z != 0)
            {
                finalOD.y = Mathf.RoundToInt(od.z * sd.y / sd.z);
            }
            else
            {
                Debug.LogWarning("undefined output dimension");
                return Vector3Int.one * 256;
            }
        }
        return finalOD;
    }

    RecorderSettings createMovieRecorderSettings(string cameraTag, Vector2 aspect, int width, int height, string mediaOutputFolder)
    {
        var recorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();

        recorder.name = cameraTag + " Video Recorder";
        recorder.Enabled = true;

        recorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        recorder.CaptureAlpha = false;

        recorder.imageInputSettings = new CameraInputSettings
        {
            Source = ImageSource.TaggedCamera,
            CameraTag = cameraTag,
            OutputHeight = height,
            OutputWidth = width,
            CaptureUI = false,
            FlipFinalOutput = cameraTag == "down" ? false : true
        };

        string cameraFolderName;
        switch(cameraTag)
        {
            case "front":
                cameraFolderName = "1";
            break;
            case "right":
                cameraFolderName = "2";
            break;
            case "back":
                cameraFolderName = "3";
            break;
            case "left":
                cameraFolderName = "4";
            break;
            case "up":
                cameraFolderName = "5";
            break;
            case "down":
                cameraFolderName = "6";
            break;
            default:
                cameraFolderName = "";
                break;
        }
        cameraFolderName += "_" + cameraTag;

        recorder.OutputFile = Path.Combine(mediaOutputFolder, cameraFolderName, cameraTag + "_") + DefaultWildcard.Frame;

        // Debug.Log("created video recorder " + cameraTag + " - " + recorder.imageInputSettings.OutputWidth + " * " + recorder.imageInputSettings.OutputHeight);

        return recorder;
    }

    void OnDisable()
    {
        m_RecorderController.StopRecording();
    }
}
#endif
