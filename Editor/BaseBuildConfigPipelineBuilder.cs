using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System;
using StinkySteak.PipelineBuilder.Data;

namespace StinkySteak.PipelineBuilder
{
    public class BaseBuildConfigPipelineBuilder
    {
        public void BuildConfig(BaseBuildConfig config)
        {
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
#if !UNITY_2022_2_OR_NEWER
            // Unity Pre 2022 Fix. Apply scripting symbols for this config
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, symbolList.ToArray());
#endif
            PrintConfig(config);

            BuildPipeline.BuildPlayer(buildOptions);

            Debug.Log($"[{nameof(BaseBuildConfigPipelineBuilder)}]: Game build available in: {finalPath}");

#if !UNITY_2022_2_OR_NEWER
            // Re-apply old scripting definition
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, temp);
#endif
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