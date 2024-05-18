using StinkySteak.PipelineBuilder.Data;
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace StinkySteak.PipelineBuilder.Window
{
    public class PipelineBuilderWindow : EditorWindow
    {
        [SerializeField] protected BuildConfigContainer _configContainer;
        [SerializeField] protected BuildConfig _singleConfig;
        private BuildConfigPipelineBuilder _builder;
        private const string FILE_EXPLORER = "explorer.exe";

        public PipelineBuilderWindow()
        {
            _builder = new BuildConfigPipelineBuilder();
        }

        protected virtual BuildConfigPipelineBuilder GetBuilder()
        {
            throw new NotImplementedException();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Build All", GetHeadingStyle());
            GUILayout.Label("Config Container");

            _configContainer = (BuildConfigContainer)EditorGUILayout.ObjectField(_configContainer, typeof(BuildConfigContainer), false);

            if (GUILayout.Button("Build All Configs"))
            {
                Debug.Log($"[{nameof(PipelineBuilderWindow)}]: Detected {_configContainer.Configs.Count} Configs. Building...");

                foreach (BuildConfig config in _configContainer.Configs)
                    _builder.BuildConfig(config);
            }

            GUILayout.Space(50);

            GUILayout.Label("Build Single", GetHeadingStyle());
            GUILayout.Label("Selected Config");

            _singleConfig = (BuildConfig)EditorGUILayout.ObjectField(_singleConfig, typeof(BuildConfig), false);

            if (GUILayout.Button("Build Single Config Only"))
            {
                Debug.Log($"[{nameof(PipelineBuilderWindow)}]: Building a single config: ({_singleConfig.name}). Building...");

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