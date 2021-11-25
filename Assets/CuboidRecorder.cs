#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

[RequireComponent(typeof(CuboidCamera))]
public class CuboidRecorder : MonoBehaviour
{
    RecorderController m_RecorderController;

    const string recordingFolder = "CuboidRecordings";

    void OnEnable()
    {
        // Bildsequenzen, nicht video
        // ca 4000 px Breit bei der langen Wand
        // lägnge breite: 260 * 285
        // länge breite ungefähr quadratisch, höhe ca 3m
        // 1920px breite für die kürzeren, doppelt so viel 
        // aegypten: schnellere Fahrt
        // Europa: keine Rotationen, tages loop
        // alle scenen in BuildSettings
        // 13.6m * 15m * 6m 

        // todo: render auflösung unabhänging von camera seitenverhältnissen

        CuboidCamera cuboidCamera = GetComponent<CuboidCamera>();

        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(controllerSettings);

        var mediaOutputFolder = Path.Combine(Application.dataPath, "..", recordingFolder);
        // animation output is an asset that must be created in Assets folder
        // var animationOutputFolder = Path.Combine(Application.dataPath, recordingFolder);

        // Setup Recording
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("front", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.y), 1080, mediaOutputFolder)
        );
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("left", new Vector2(cuboidCamera.sensorDimensions.z, cuboidCamera.sensorDimensions.y), 1080, mediaOutputFolder)
        );
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("back", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.y), 1080, mediaOutputFolder)
        );
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("right", new Vector2(cuboidCamera.sensorDimensions.z, cuboidCamera.sensorDimensions.y), 1080, mediaOutputFolder)
        );
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("up", new Vector2(cuboidCamera.sensorDimensions.x, cuboidCamera.sensorDimensions.z), 1080, mediaOutputFolder)
        );

        // controllerSettings.AddRecorderSettings(
        //     createMovieRecorderSettings("right", new Vector2(cuboidCamera.dimensions.z, cuboidCamera.dimensions.y), 1080, mediaOutputFolder)
        // );

        controllerSettings.SetRecordModeToManual();
        controllerSettings.FrameRate = 30.0f;

        RecorderOptions.VerboseMode = false;
        m_RecorderController.PrepareRecording();
        m_RecorderController.StartRecording();
    }

    RecorderSettings createMovieRecorderSettings(string cameraTag, Vector2 aspect, int pixelHeight, string mediaOutputFolder)
    {
        var recorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();

        recorder.name = "Right Video Recorder";
        recorder.Enabled = true;

        recorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        recorder.CaptureAlpha = false;

        recorder.imageInputSettings = new CameraInputSettings
        {
            Source = ImageSource.TaggedCamera,
            CameraTag = cameraTag,
            OutputHeight = pixelHeight,
            OutputWidth = Mathf.RoundToInt(pixelHeight * aspect.x / aspect.y),
            CaptureUI = false,
            FlipFinalOutput = true
        };

        // recorder.OutputFile = Path.Combine(mediaOutputFolder, "_png", "image_v") + DefaultWildcard.Take + "." + DefaultWildcard.Frame;
        
        recorder.OutputFile = Path.Combine(mediaOutputFolder, cameraTag) + DefaultWildcard.Take + "_" + DefaultWildcard.Frame;

        Debug.Log("created video recorder " + cameraTag + " - " + recorder.imageInputSettings.OutputWidth + " * " + recorder.imageInputSettings.OutputHeight);

        return recorder;
    }

    void OnDisable()
    {
        m_RecorderController.StopRecording();
    }
}
#endif
