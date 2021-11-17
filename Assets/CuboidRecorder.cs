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
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(controllerSettings);

        var mediaOutputFolder = Path.Combine(Application.dataPath, "..", recordingFolder);
        // animation output is an asset that must be created in Assets folder
        // var animationOutputFolder = Path.Combine(Application.dataPath, recordingFolder);

        // Video
        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        videoRecorder.name = "My Video Recorder";
        videoRecorder.Enabled = true;

        videoRecorder.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
        videoRecorder.VideoBitRateMode = VideoBitrateMode.Low;

        videoRecorder.ImageInputSettings = new CameraInputSettings
        {
            CameraTag = "right"//,
            // Source = ?? 
        };

        // videoRecorder.ImageInputSettings = new GameViewInputSettings
        // {
        //     OutputWidth = 1920,
        //     OutputHeight = 1080
        // };

        videoRecorder.AudioInputSettings.PreserveAudio = true;
        videoRecorder.OutputFile = Path.Combine(mediaOutputFolder, "video_v") + DefaultWildcard.Take;


        // Setup Recording
        controllerSettings.AddRecorderSettings(videoRecorder);

        controllerSettings.SetRecordModeToManual();
        controllerSettings.FrameRate = 30.0f;

        RecorderOptions.VerboseMode = false;
        m_RecorderController.PrepareRecording();
        m_RecorderController.StartRecording();
    }

    void OnDisable()
    {
        m_RecorderController.StopRecording();
    }
}
#endif
