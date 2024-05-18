using StinkySteak.PipelineBuilder.Data;
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace StinkySteak.PipelineBuilder.Window
{
    public class BasePipelineBuilderWindow : EditorWindow
    {
        [SerializeField] protected BaseBuildConfigContainer _configContainer;
        [SerializeField] protected BaseBuildConfig _singleConfig;
        private BaseBuildConfigPipelineBuilder _builder;
        private const string FILE_EXPLORER = "explorer.exe";

        public BasePipelineBuilderWindow()
        {
            _builder = new BaseBuildConfigPipelineBuilder();
        }

        protected virtual BaseBuildConfigPipelineBuilder GetBuilder()
        {
            throw new NotImplementedException();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Build All", GetHeadingStyle());
            GUILayout.Label("Config Container");

            _configContainer = (BaseBuildConfigContainer)EditorGUILayout.ObjectField(_configContainer, typeof(BaseBuildConfigContainer), false);

            if (GUILayout.Button("Build All Configs"))
            {
                Debug.Log($"[{nameof(BasePipelineBuilderWindow)}]: Detected {_configContainer.Configs.Count} Configs. Building...");

                foreach (BaseBuildConfig config in _configContainer.Configs)
                    _builder.BuildConfig(config);
            }

            GUILayout.Space(50);

            GUILayout.Label("Build Single", GetHeadingStyle());
            GUILayout.Label("Selected Config");

            _singleConfig = (BaseBuildConfig)EditorGUILayout.ObjectField(_singleConfig, typeof(BaseBuildConfig), false);

            if (GUILayout.Button("Build Single Config Only"))
            {
                Debug.Log($"[{nameof(BasePipelineBuilderWindow)}]: Building a single config: ({_singleConfig.name}). Building...");

                _builder.BuildConfig(_singleConfig);
            }

            if (GUILayout.Button("Open build path location"))
                OpenSingleConfigLocation();
        }

        private void OpenSingleConfigLocation()
        {
            string path = _builder.GetLocationPath(_singleConfig);

            if (Directory.Exists(path))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = path,
                    FileName = FILE_EXPLORER
                };
            }

            Process.Start(path);
        }

        private GUIStyle GetHeadingStyle()
        {
            return new GUIStyle { fontSize = 18, normal = { textColor = Color.white } };
        }
    }
}