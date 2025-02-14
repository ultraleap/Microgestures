#define PLATFORM_SDK_WINDOWS_OPENXR
#define HAND_TRACKING_ULTRALEAP_LEAPC

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.Management;
using System.Threading;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.Build.Reporting;
#endif

namespace Ultraleap.XRTemplate
{
    public class PlatformFeatures : MonoBehaviour
    {
        public PlatformSDK CurrentPlatformSDK { get { return _platformSDK; } }
        private PlatformSDK _platformSDK;

        public HandSDK CurrentHandSDK { get { return _handSDK; } }
        private HandSDK _handSDK;

        [Tooltip("Check this to enable passthrough on supported platforms.")]
        [SerializeField] private bool _enablePassthrough = false;

        private CameraClearFlags _cameraClearFlags;
        private Color _cameraColour;

        public Action<bool> OnPassthroughSettingChanged;

        /* Set platform and hand SDK enums off of CI ifdef */
        void Awake()
        {
#if PLATFORM_SDK_WINDOWS_OPENXR
            _platformSDK = PlatformSDK.WINDOWS_OPENXR;
#elif PLATFORM_SDK_PICO_SDK
            _platformSDK = PlatformSDK.PICO_SDK;
#elif PLATFORM_SDK_PICO_OPENXR
            _platformSDK = PlatformSDK.PICO_OPENXR;
#elif PLATFORM_SDK_VIVE_OPENXR
            _platformSDK = PlatformSDK.VIVE_OPENXR;
#elif PLATFORM_SDK_LYNX_OPENXR
            _platformSDK = PlatformSDK.LYNX_OPENXR;
#elif PLATFORM_SDK_STOCK_OPENXR
            _platformSDK = PlatformSDK.STOCK_OPENXR;
#elif PLATFORM_SDK_QUALCOMM_SPACES
            _platformSDK = PlatformSDK.QUALCOMM_SPACES;
#elif PLATFORM_SDK_QUALCOMM_OPENXR
            _platformSDK = PlatformSDK.QUALCOMM_OPENXR;
#elif PLATFORM_SDK_META_OPENXR
            _platformSDK = PlatformSDK.META_OPENXR;
#elif PLATFORM_SDK_WINDOWS_VARJO
            _platformSDK = PlatformSDK.WINDOWS_VARJO;
#else
            _platformSDK = PlatformSDK.EDITOR;
#endif

#if HAND_TRACKING_ULTRALEAP_LEAPC
            _handSDK = HandSDK.ULTRALEAP_LEAPC;
#elif HAND_TRACKING_ULTRALEAP_OPENXR
            _handSDK = HandSDK.ULTRALEAP_OPENXR;
#elif HAND_TRACKING_PICO_SDK
            _handSDK = HandSDK.PICO_SDK;
#elif HAND_TRACKING_VIVE_OPENXR
            _handSDK = HandSDK.VIVE_OPENXR;
#elif HAND_TRACKING_QUALCOMM_SPACES
            _handSDK = HandSDK.QUALCOMM_SPACES;
#elif HAND_TRACKING_META_OPENXR
            _handSDK = HandSDK.META_OPENXR;
#else
            _handSDK = HandSDK.EDITOR;
#endif

            _cameraClearFlags = Camera.main.clearFlags;
            _cameraColour = Camera.main.backgroundColor;

            ResolutionScalingURP();
        }

        /* Handle platform-specific SDK stuff */
        private void Start()
        {
            EnablePassthrough(_enablePassthrough);

#if PLATFORM_SDK_PICO_SDK
            Unity.XR.PICO.TOBSupport.PXR_Enterprise.InitEnterpriseService();
            Unity.XR.PICO.TOBSupport.PXR_Enterprise.BindEnterpriseService(delegate (bool binded)
            {
                if (!binded) return;
                Unity.XR.PICO.TOBSupport.PXR_Enterprise.SwitchSystemFunction(Unity.XR.PICO.TOBSupport.SystemFunctionSwitchEnum.SFS_BASIC_SETTING_SHOW_APP_QUIT_CONFIRM_DIALOG, Unity.XR.PICO.TOBSupport.SwitchEnum.S_OFF);
            });
#endif
        }
        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                EnablePassthrough(_enablePassthrough);
        }
        void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
                EnablePassthrough(_enablePassthrough);
        }
        private void OnDestroy()
        {
#if PLATFORM_SDK_PICO_SDK
            Unity.XR.PICO.TOBSupport.PXR_Enterprise.UnBindEnterpriseService();
#endif
        }

        /* Is passthrough supported on the platform? */
        public bool PassthroughSupported
        {
            get
            {
#if PLATFORM_SDK_WINDOWS_VARJO || PLATFORM_SDK_LYNX_OPENXR || PLATFORM_SDK_META_OPENXR
                return true;
#elif PLATFORM_SDK_PICO_SDK
                return false;
                //Pico 4E, Pico 4, Pico 4 (?)
                return (SystemInfo.deviceModel == "Pico A8E50" || SystemInfo.deviceModel == "Pico A81E0" || SystemInfo.deviceModel == "Pico A8110");
#elif PLATFORM_SDK_QUALCOMM_SPACES
                if (SystemInfo.deviceModel.ToUpper().Contains("LENOVO VRX"))
                    return false;
                Qualcomm.Snapdragon.Spaces.BaseRuntimeFeature feature = OpenXRSettings.Instance.GetFeature<Qualcomm.Snapdragon.Spaces.BaseRuntimeFeature>();
                return (feature != null && feature.IsPassthroughSupported());
#elif PLATFORM_SDK_QUALCOMM_OPENXR
                Ultraleap.Tracking.OpenXR.XR2Feature feature = OpenXRSettings.Instance.GetFeature<Ultraleap.Tracking.OpenXR.XR2Feature>();
                return feature != null && (SystemInfo.deviceModel == "QUALCOMM Anorak for arm64"); 
#else
                return false;
#endif
            }
        }

        /* Is passthrough enabled? */
        public bool PassthroughEnabled => _enablePassthrough && PassthroughSupported;

        /* Toggle passthrough on/off, if possible on the platform */
        public void TogglePassthrough()
        {
            EnablePassthrough(!_enablePassthrough);
        }

        /* Enable or disable passthrough, if possible on the platform */
        public void EnablePassthrough(bool enabled)
        {
            if (!PassthroughSupported)
            {
                _enablePassthrough = false;
				OnPassthroughSettingChanged?.Invoke(_enablePassthrough);
                return;
            }
            _enablePassthrough = enabled;

            if (_enablePassthrough)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0, 0, 0, 0);
            }
            else
            {
                Camera.main.clearFlags = _cameraClearFlags;
                Camera.main.backgroundColor = _cameraColour;
            }

#if PLATFORM_SDK_WINDOWS_VARJO
            if (_enablePassthrough)
                Varjo.XR.VarjoMixedReality.StartRender();
            else
                Varjo.XR.VarjoMixedReality.StopRender();
#elif PLATFORM_SDK_PICO_SDK
            Unity.XR.PXR.PXR_Boundary.EnableSeeThroughManual(_enablePassthrough);
#elif PLATFORM_SDK_LYNX_OPENXR
            lynx.LynxAPI.SetAR(_enablePassthrough);
#elif PLATFORM_SDK_QUALCOMM_SPACES
            Qualcomm.Snapdragon.Spaces.BaseRuntimeFeature feature = OpenXRSettings.Instance.GetFeature<Qualcomm.Snapdragon.Spaces.BaseRuntimeFeature>();
            if (feature != null)
                feature.SetPassthroughEnabled(_enablePassthrough);
#elif PLATFORM_SDK_QUALCOMM_OPENXR
            Ultraleap.Tracking.OpenXR.XR2Feature feature = OpenXRSettings.Instance.GetFeature<Ultraleap.Tracking.OpenXR.XR2Feature>();
            if (feature != null)
                feature.EnvironmentBlendMode = _enablePassthrough ? UnityEngine.XR.OpenXR.NativeTypes.XrEnvironmentBlendMode.AlphaBlend : UnityEngine.XR.OpenXR.NativeTypes.XrEnvironmentBlendMode.Opaque;
#elif PLATFORM_SDK_META_OPENXR
            UnityEngine.XR.ARFoundation.ARCameraManager mgr = Camera.main.gameObject.GetComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();
            if (mgr == null)
                mgr = Camera.main.gameObject.AddComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();
            mgr.enabled = _enablePassthrough;
#endif

            OnPassthroughSettingChanged?.Invoke(_enablePassthrough);
        }

        /* Handle URP resolution scaling based on the build target */
        private void ResolutionScalingURP()
        {
            try
            {
                float resScale;
                switch (_platformSDK)
                {
                    case PlatformSDK.LYNX_OPENXR:
                        resScale = 0.6f;
                        break;
                    case PlatformSDK.VIVE_OPENXR:
                    case PlatformSDK.QUALCOMM_OPENXR:
                        resScale = 0.7f;
                        break;
                    case PlatformSDK.META_OPENXR:
                    case PlatformSDK.PICO_OPENXR:
                    case PlatformSDK.PICO_SDK:
                        resScale = 0.9f;
                        break;
                    default:
                        resScale = 1.0f;
                        break;
                }
                ScalableBufferManager.ResizeBuffers(resScale, resScale);
                ((UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset).renderScale = resScale;
            }
            catch { }
        }

        /* Build function for CI */
        static void PerformBuild()
        {
#if UNITY_EDITOR
            // Organise the command line arguments by getting the valid ones
            Dictionary<string, string> args = GetOrganisedArgs();

            // Make app name platform specific with extension
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    args["app_name"] = args["name"] + ".exe";
                    break;
                case BuildTarget.Android:
                    args["app_name"] = args["app_name"] + ".apk";
                    break;
            }

            // Set the apps name in the player settings
            PlayerSettings.productName = args["name"];

            // Gather the chosen scenes to be built
            List<string> scenePaths = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                if (EditorBuildSettings.scenes[i].enabled)
                    scenePaths.Add(EditorBuildSettings.scenes[i].path);

            // Try to build the application
            BuildReport result = BuildPipeline.BuildPlayer(scenePaths.ToArray(), "../Build/" + args["app_name"], EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);

            // If the build did not complete, we should exit
            if (!File.Exists("../Build/" + args["app_name"]))
                EditorApplication.Exit(1);

            //Post-build nice utils
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    // Add a useful install script for apks
                    if (!EditorUserBuildSettings.exportAsGoogleAndroidProject)
                        File.WriteAllText("../Build/install.bat", "adb uninstall " + Application.identifier + "\nadb install -r \"" + args["app_name"] + "\"\nadb shell monkey -p " + Application.identifier + " 1\nadb kill-server\npause");
                    break;
            }
#endif
        }

        static Dictionary<string, string> GetOrganisedArgs()
        {
            Dictionary<string, string> args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            {
                var arguments = Environment.GetCommandLineArgs();
                for (int i = 1; i < arguments.Length; i++)
                {
                    var match = Regex.Match(arguments[i], "-([^=]+)=(.*)");
                    if (!match.Success) continue;
                    var vName = match.Groups[1].Value;
                    var vValue = match.Groups[2].Value;
                    args[vName] = vValue;
                }
            }

            args["app_name"] = args["app_name"].Replace('[', '_').Replace(']', '_');

            return args;
        }

        public enum PlatformSDK
        {
            WINDOWS_OPENXR,   // for Windows
            PICO_SDK,         // for Pico Neo 3, Pico 4
            PICO_OPENXR,      // for Pico Neo 3, Pico 4
            VIVE_OPENXR,      // for Vive Focus 3, XR Elite
            LYNX_OPENXR,      // for Lynx R1
            STOCK_OPENXR,     // for devices that support the Khronos loader
            QUALCOMM_SPACES,  // for Lenovo VRX
            QUALCOMM_OPENXR,  // for SKU4, Morpheus
            META_OPENXR,      // for Quest 2, Quest Pro
            WINDOWS_VARJO,    // for Windows (Varjo SDK)

            EDITOR,
        }

        public enum HandSDK
        {
            ULTRALEAP_LEAPC,  // to enable XRHands via direct LeapC, on supported devices
            ULTRALEAP_OPENXR, // to enable XRHands via UL's OpenXR layer, on supported devices
            PICO_SDK,         // to enable XRHands via Pico SDK, on devices using PICO_SDK PLATFORM_SDK
            VIVE_OPENXR,      // to enable XRHands via Vive via OpenXR, on devices using VIVE_OPENXR PLATFORM_SDK
            QUALCOMM_SPACES,  // to enable XRHands via Qualcomm via Spaces OpenXR, on devices using QUALCOMM_SPACES PLATFORM_SDK
            META_OPENXR,      // to enable XRHands via Meta via OpenXR, on devices using META_OPENXR PLATFORM_SDK

            EDITOR,
        }
    }
}
