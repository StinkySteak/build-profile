using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace StinkySteak.PipelineBuilder.Data
{
    public class BaseBuildConfig : ScriptableObject
    {
        [SerializeField] protected string _folderName;
        [SerializeField] protected BuildTargetGroup _buildTargetGroup;
        [SerializeField] protected BuildTarget _buildTarget;
        [SerializeField] protected StandaloneBuildSubtarget _standaloneBuildTarget;

        [Space]
        [SerializeField] protected SceneAsset[] _scenes;

        [SerializeField] protected ScriptingImplementation _scriptingImplementation;
        [SerializeField] protected BuildOptions _buildOptions;

        protected bool IsServerCompatible()
        {
            return
                _buildTarget == BuildTarget.StandaloneWindows ||
                _buildTarget == BuildTarget.StandaloneWindows64 ||
                _buildTarget == BuildTarget.StandaloneLinux64;
        }

        public StandaloneBuildSubtarget GetStandaloneSubTarget()
        {
            if (IsServerCompatible())
                return _standaloneBuildTarget;

            return StandaloneBuildSubtarget.Player;
        }

        public string FolderName => _folderName;
        public BuildTargetGroup BuildTargetGroup => _buildTargetGroup;
        public BuildTarget BuildTarget => _buildTarget;
        public ScriptingImplementation ScriptingImplementation => _scriptingImplementation;
        public BuildOptions BuildOptions => _buildOptions;

        public virtual string GetDefaultPath()
        {
            return new DirectoryInfo(Application.dataPath).Parent.Parent.Parent.FullName;
        }

        public string[] GetScenePaths()
        {
            List<string> paths = new();

            foreach (SceneAsset scene in _scenes)
            {
                string path = AssetDatabase.GetAssetPath(scene);
                paths.Add(path);
            }

            return paths.ToArray();
        }

        protected void PrintScenePaths()
        {
            string[] paths = GetScenePaths();
            SceneAsset[] scenes = _scenes;

            StringBuilder sb = new();

            sb.AppendLine($"[{nameof(BaseBuildConfig)}]: Registered scenes:");
            for (int i = 0; i < scenes.Length; i++)
            {
                sb.AppendLine($"Scene: ({scenes[i].name}) Path: {paths[i]}");
            }

            Debug.Log(sb);
        }
    }
}