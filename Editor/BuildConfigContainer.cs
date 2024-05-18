using System.Collections.Generic;
using UnityEngine;

namespace StinkySteak.PipelineBuilder.Data
{
    public class BuildConfigContainer : ScriptableObject
    {
        [SerializeField] protected BuildConfig[] _configs;

        public IReadOnlyList<BuildConfig> Configs => _configs;
    }
}