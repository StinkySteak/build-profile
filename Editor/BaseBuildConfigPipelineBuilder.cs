using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System;
using StinkySteak.PipelineBuilder.Data;
using System.Reflection;
using UnityEditor.Build;

namespace StinkySteak.PipelineBuilder
{
    public class BaseBuildConfigPipelineBuilder
    {
        internal struct PlatformProperty
        {
            public BuildTarget BuildTarget;
            public BuildTargetGroup BuildTargetGroup;
            public StandaloneBuildSubtarget StandaloneBuildSubtarget;
            public NamedBuildTarget NamedBuildTarget;
        }

        public void BuildConfig(BaseBuildConfig config)
        {
            PlatformProperty previousPlatform = GetCurrentPlatformProperty();
            PlatformProperty targetedPlatform = GetTargetedPlatformProperty(config);


            PlayerSettings.SetScriptingBackend(targetedPlatform.NamedBuildTarget, config.ScriptingImplementation);
            PlayerSettings.GetScriptingDefineSymbols(targetedPlatform.NamedBuildTarget, out string[] symbols);

            List<string> symbolList = symbols.ToList();
            string[] previousSymbols = symbols;

            PreProcessScriptingSymbols(config, symbolList);

            string[] scenes = config.GetScenePaths();

            string finalPath = Path.Combine(GetLocationPath(config), $"{PlayerSettings.productName}{GetFileExtension(config.BuildTarget)}");

            BuildPlayerOptions buildOptions = new()
            {
                scenes = scenes,
                locationPathName = finalPath, // Specify build output path
                target = config.BuildTarget,
                options = config.BuildOptions,
                subtarget = (int)config.GetStandaloneSubTarget(),
                targetGroup = targetedPlatform.BuildTargetGroup,
            };

            PlayerSettings.SetScriptingDefineSymbols(targetedPlatform.NamedBuildTarget, symbolList.ToArray());
            PrintSymbols(config, symbolList);

            BuildPipeline.BuildPlayer(buildOptions);

            Debug.Log($"[{nameof(BaseBuildConfigPipelineBuilder)}]: Game build available in: {finalPath}");

            PlayerSettings.SetScriptingDefineSymbols(targetedPlatform.NamedBuildTarget, previousSymbols);

            SwitchActiveBuildTargetAndSubtargetNoCheck(previousPlatform.BuildTargetGroup, previousPlatform.BuildTarget, (int)previousPlatform.StandaloneBuildSubtarget);
        }

        private PlatformProperty GetTargetedPlatformProperty(BaseBuildConfig config)
        {
            BuildTarget target = config.BuildTarget;
            BuildTargetGroup buildTargetGroup = config.BuildTargetGroup;
            StandaloneBuildSubtarget subTarget = config.GetStandaloneSubTarget();
            NamedBuildTarget namedBuildTarget = GetNamedBuildTarget(config.BuildTargetGroup, config.GetStandaloneSubTarget());

            PlatformProperty prop;
            prop.BuildTarget = config.BuildTarget;
            prop.BuildTargetGroup = buildTargetGroup;
            prop.NamedBuildTarget = namedBuildTarget;
            prop.StandaloneBuildSubtarget = config.GetStandaloneSubTarget();

            return prop;
        }
        private PlatformProperty GetCurrentPlatformProperty()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            StandaloneBuildSubtarget subTarget = (StandaloneBuildSubtarget)GetActiveSubtargetFor(target);
            NamedBuildTarget namedBuildTarget = GetNamedBuildTarget(buildTargetGroup, subTarget);

            PlatformProperty prop;
            prop.BuildTarget = target;
            prop.BuildTargetGroup = buildTargetGroup;
            prop.NamedBuildTarget = namedBuildTarget;
            prop.StandaloneBuildSubtarget = subTarget;

            return prop;
        }

        private static int GetActiveSubtargetFor(BuildTarget target)
        {
            Type type = typeof(EditorUserBuildSettings);

            MethodInfo methodInfo = type.GetMethod("GetActiveSubtargetFor", BindingFlags.Static | BindingFlags.NonPublic);

            int subTarget = (int)methodInfo.Invoke(null, new object[] { target });

            return subTarget;
        }

        private static void SwitchActiveBuildTargetAndSubtargetNoCheck(BuildTargetGroup targetGroup, BuildTarget target, int subtarget)
        {
            Type type = typeof(EditorUserBuildSettings);

            MethodInfo methodInfo = type.GetMethod("SwitchActiveBuildTargetAndSubtargetNoCheck", BindingFlags.Static | BindingFlags.NonPublic);

            methodInfo.Invoke(null, new object[] { targetGroup, target, subtarget });
        }

        public virtual void PreProcessScriptingSymbols(BaseBuildConfig buildConfig, in List<string> symbols)
        {
            throw new NotImplementedException();
        }

        public virtual string GetLocationPath(BaseBuildConfig config)
        {
            throw new NotImplementedException();
        }

        private NamedBuildTarget GetNamedBuildTarget(BuildTargetGroup buildTargetGroup, StandaloneBuildSubtarget subtarget)
        {
            bool isStandalone = buildTargetGroup == BuildTargetGroup.Standalone;
            bool isServer = subtarget == StandaloneBuildSubtarget.Server;

            if (isStandalone && isServer)
            {
                return NamedBuildTarget.Server;
            }

            return NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
        }

        public virtual void PrintSymbols(BaseBuildConfig config, List<string> symbols)
        {
            StringBuilder stringBuilder = new();

            foreach (string item in symbols)
                stringBuilder.Append($"{item};");

            if (symbols.Count > 0)
                Debug.Log($"[{nameof(BaseBuildConfigPipelineBuilder)}]: Building config ({config.FolderName}) with: {stringBuilder}");
            else
                Debug.Log($"[{nameof(BaseBuildConfigPipelineBuilder)}]: Building config ({config.FolderName}) with no symbols");
        }

        protected virtual string GetFileExtension(BuildTarget buildTarget)
        {
            if (buildTarget == BuildTarget.StandaloneWindows || buildTarget == BuildTarget.StandaloneWindows64)
                return ".exe";
            else if (buildTarget == BuildTarget.WebGL)
                return string.Empty; //Directory
            else if (buildTarget == BuildTarget.StandaloneLinux64 || buildTarget == BuildTarget.LinuxHeadlessSimulation)
                return ".x86_64";
            Debug.LogError($"[{nameof(BaseBuildConfigPipelineBuilder)}]: No file extension supported for: {buildTarget}");

            return string.Empty;
        }
    }
}