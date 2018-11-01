using System;

public static class AppConstants
{

    #region General Constants

    public static string DefaultEyeTrackingFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Aalborg\\Projet\\GridCalibrationVR\\LogData";
    #endregion

    // not used
    #region OSCSystem constants
    public const string CalibrationCubeTag = "CCube";
    public const float CubeScaleRate = 0.75f;
    public const float CubeDistance = 2.0f;
    public const float CubeDepth = 0.1f;
    public const float TimeOutBeforeSizeUp = 1000f;
    public const int GridHeight = 3;
    public const int GridWidth = 3;
    #endregion

    #region LoggerBehavior Constants

    public const string CsvFirstRow = "local_time;gaze_hemi_x;gaze_hemi_y;gaze_world_x;gaze_world_y;left_confidence;right_confidence" +
    "first_time_entry;target_x;target_y;radius_percent";

    #endregion
}
