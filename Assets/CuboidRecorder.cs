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
        CuboidCamera cuboidCamera = GetComponent<CuboidCamera>();

        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(controllerSettings);

        var mediaOutputFolder = Path.Combine(Application.dataPath, "..", recordingFolder);
        // animation output is an asset that must be created in Assets folder
        // var animationOutputFolder = Path.Combine(Application.dataPath, recordingFolder);

        // Setup Recording
        controllerSettings.AddRecorderSettings(
            createMovieRecorderSettings("front", new Vector2(cuboidCamera.dimensions.x, cuboidCamera.dimensions.y), 1080, mediaOutputFolder)
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

    MovieRecorderSettings createMovieRecorderSettings(string cameraTag, Vector2 aspect, int pixelHeight, string mediaOutputFolder)
    {
        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();

        videoRecorder.name = "Right Video Recorder";
        videoRecorder.Enabled = true;

        videoRecorder.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
        videoRecorder.VideoBitRateMode = VideoBitrateMode.Low;

        videoRecorder.ImageInputSettings = new CameraInputSettings
        {
            Source = ImageSource.TaggedCamera,
            CameraTag = cameraTag,
            OutputHeight = pixelHeight,
            OutputWidth = Mathf.RoundToInt(pixelHeight * aspect.x / aspect.y),
            FlipFinalOutput = true
        };

        videoRecorder.AudioInputSettings.PreserveAudio = false;
        videoRecorder.OutputFile = Path.Combine(mediaOutputFolder, cameraTag) + DefaultWildcard.Take;

        Debug.Log("created video recorder " + cameraTag + " - " + videoRecorder.ImageInputSettings.OutputWidth + " * " + videoRecorder.ImageInputSettings.OutputHeight);

        return videoRecorder;
    }

    void OnDisable()
    {
        m_RecorderController.StopRecording();
    }
}
#endif
