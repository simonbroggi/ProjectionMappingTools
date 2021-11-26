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
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("front", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.y),
                                                            finalOutputDimensions.x, finalOutputDimensions.y, mediaOutputFolder)
        );
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("right", new Vector2(cuboidCamera.sensorDimensions.z, cuboidCamera.sensorDimensions.y),
                                                            finalOutputDimensions.z, finalOutputDimensions.y, mediaOutputFolder)
        );
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("back", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.y),
                                                            finalOutputDimensions.x, finalOutputDimensions.y, mediaOutputFolder)
        );
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("left", new Vector2(cuboidCamera.sensorDimensions.z, cuboidCamera.sensorDimensions.y),
                                                            finalOutputDimensions.z, finalOutputDimensions.y, mediaOutputFolder)
        );
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("up", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.z),
                                                            finalOutputDimensions.x, finalOutputDimensions.z, mediaOutputFolder)
        );
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
            FlipFinalOutput = true
        };

        recorder.OutputFile = Path.Combine(mediaOutputFolder, cameraTag, cameraTag + "_") + DefaultWildcard.Frame;

        Debug.Log("created video recorder " + cameraTag + " - " + recorder.imageInputSettings.OutputWidth + " * " + recorder.imageInputSettings.OutputHeight);

        return recorder;
    }

    void OnDisable()
    {
        m_RecorderController.StopRecording();
    }
}
#endif
