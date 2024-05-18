using System.Collections.Generic;
using UnityEngine;

namespace StinkySteak.PipelineBuilder.Data
{
    public class BaseBuildConfigContainer : ScriptableObject
    {
        [SerializeField] protected BaseBuildConfig[] _configs;

        public IReadOnlyList<BaseBuildConfig> Configs => _configs;
    }
}