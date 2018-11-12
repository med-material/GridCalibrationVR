using System;

public static class AppConstants
{

    #region General Constants

    public static string DefaultEyeTrackingFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Aalborg\\Projet\\GridCalibrationVR\\LogData";
    #endregion

    #region LoggerBehavior Constants

    public const string CsvFirstRow = "local_time;test_name;gaze_world_x;gaze_world_y;left_confidence;right_confidence;" +
    "first_time_entry;target_x;target_y;radius_percent";

    #endregion
}
