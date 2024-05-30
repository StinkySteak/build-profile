using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System;
using StinkySteak.PipelineBuilder.Data;
using System.Reflection;

namespace StinkySteak.PipelineBuilder
{
    public class BaseBuildConfigPipelineBuilder
    {
        public void BuildConfig(BaseBuildConfig config)
        {
            BuildTarget previousTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup previousTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            int subTarget = GetActiveSubtargetFor(previousTarget);

            BuildTargetGroup buildTarget = config.BuildTargetGroup;

            PlayerSettings.SetScriptingBackend(buildTarget, config.ScriptingImplementation);

            if (config.GetStandaloneSubTarget() == StandaloneBuildSubtarget.Server)
                PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Server, config.ScriptingImplementation);

            PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget, out string[] symbols);

            List<string> symbolList = symbols.ToList();

            PreProcessScriptingSymbols(config, symbolList);

            string[] scenes = config.GetScenePaths();

            string finalPath = Path.Combine(GetLocationPath(config), $"{PlayerSettings.productName}{GetFileExtension(config.BuildTarget)}");

            BuildPlayerOptions buildOptions = new()
            {
                scenes = scenes,
                locationPathName = finalPath, // Specify build output path
                target = config.BuildTarget,
                options = config.BuildOptions,

                //Not working on Unity 2021, fixed on Unity 2022
                extraScriptingDefines = symbolList.ToArray(),
                subtarget = (int)config.GetStandaloneSubTarget(),
                targetGroup = buildTarget,
            };

            string[] temp = symbols;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, symbolList.ToArray());
            PrintConfig(config);

            BuildPipeline.BuildPlayer(buildOptions);

            Debug.Log($"[{nameof(BaseBuildConfigPipelineBuilder)}]: Game build available in: {finalPath}");

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, temp);

            SwitchActiveBuildTargetAndSubtargetNoCheck(previousTargetGroup, previousTarget, subTarget);

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

            methodInfo.Invoke(null, new object[] { targetGroup, target , subtarget });
        }

        public virtual void PreProcessScriptingSymbols(BaseBuildConfig buildConfig, in List<string> symbols)
        {
            throw new NotImplementedException();
        }

        public virtual string GetLocationPath(BaseBuildConfig config)
        {
            throw new NotImplementedException();
        }

        public virtual void PrintConfig(BaseBuildConfig config)
        {
            StringBuilder stringBuilder = new();

            List<string> symbols = new();

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