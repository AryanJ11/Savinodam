using UnityEditor;

namespace GPUInstancer.CrowdAnimations
{
    [InitializeOnLoad]
    public class GPUICrowdDefines
    {
#if GPU_INSTANCER
        static GPUICrowdDefines()
        {
            EditorApplication.update -= SetVersionNo;
            EditorApplication.update += SetVersionNo;
        }

        static void SetVersionNo()
        {
            GPUICrowdSettings gpuiCrowdSettings = GPUICrowdSettings.GetDefaultGPUICrowdSettings();
            if (gpuiCrowdSettings.extensionVersionNo != GPUICrowdEditorConstants.GPUI_CA_VERSION_NO)
            {
                UpdateVersion(gpuiCrowdSettings.extensionVersionNo, GPUICrowdEditorConstants.GPUI_CA_VERSION_NO);
                gpuiCrowdSettings.extensionVersionNo = GPUICrowdEditorConstants.GPUI_CA_VERSION_NO;
                EditorUtility.SetDirty(gpuiCrowdSettings);
            }
            EditorApplication.update -= SetVersionNo;
        }

        public static bool IsVersionUpdateRequired(float previousVersion, float newVersion)
        {
            return false;
        }

        public static void UpdateVersion(float previousVersion, float newVersion)
        {
        }
#else //GPU_INSTANCER
        static GPUICrowdDefines()
        {
            UnityEngine.Debug.LogError(GPUICrowdConstants.ERROR_GPUI_Dependency);
        }
#endif //GPU_INSTANCER
    }
}