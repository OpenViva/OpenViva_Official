using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;

public class XRStartupBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    private static bool s_OriginalInitOnStartState;

    public void OnPreprocessBuild(BuildReport report)
    {
        BuildTargetGroup targetGroup = report.summary.platformGroup;

        XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);

        if (settings != null)
        {
            s_OriginalInitOnStartState = settings.InitManagerOnStart;

            if (s_OriginalInitOnStartState)
            {
                settings.InitManagerOnStart = false;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();

                UnityEngine.Debug.Log($"[XR Build Processor] Temporarily disabled 'Initialize XR on Startup' for target group: {targetGroup}");
            }
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        BuildTargetGroup targetGroup = report.summary.platformGroup;
        XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);

        if (settings != null && s_OriginalInitOnStartState)
        {
            settings.InitManagerOnStart = true;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            UnityEngine.Debug.Log($"[XR Build Processor] Restored 'Initialize XR on Startup' to true for target group: {targetGroup}");
        }
    }
}
