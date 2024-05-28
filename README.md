# Unity Build Pipeline
This plugin allow you to build multiple platforms configurations in one click. Recommended for building cross-platform application.

## Unity Editor Version Compatiblity
- Unity 6 has a similar feature called [Build Profile](https://docs.unity3d.com/6000.0/Documentation/Manual/build-profiles.html)
- Some unity editor has [Known Issues](https://github.com/StinkySteak/builder-pipeline/edit/master/README.md#known-issues)

## How to Use?
1. Create a class Inherit `BaseBuildConfig`
  - Override `GetDefaultPath()`
2. Create a class Inherit `BaseBuildConfigContainer`
3. Create a class Inherit `BaseBuildConfigPipelineBuilder`
  - Override `GetLocationPath()`
  - Override `PreProcessScriptingSymbols()` (Leave empty if you don't have any symbol definition)
4. Create a class Inherit `BasePipelineBuilderWindow`
  - Override `GetBuilder()` and use the build from the builder you just created

## Example Usage
```cs
public class BuildConfig : BaseBuildConfig
{
    [SerializeField] protected BuildEnvironment _environment;
    public BuildEnvironment Environment => _environment;

    public override string GetDefaultPath()
    {
        return new DirectoryInfo(Application.dataPath).Parent.Parent.Parent.FullName;
    }
}
```

```cs
public class BuildConfigContainer : BaseBuildConfigContainer
{
    [SerializeField] protected BaseBuildConfig[] _activeConfigs;
    public IReadOnlyList<BaseBuildConfig> ActiveConfigs => _activeConfigs;

    [Button]
    private void SetAllActive()
    {
        _activeConfigs = _configs.ToArray();
    }
}
```

```cs
public class BuildConfigPipelineBuilder : BaseBuildConfigPipelineBuilder
{
    private const string BUILD_FOLDER_NAME = "Build";
    private const string PROJECT_NAME = "project-name";

    public override string GetLocationPath(BaseBuildConfig config)
    {
        string defaultPath = config.GetDefaultPath();
        return Path.Combine(defaultPath, BUILD_FOLDER_NAME, PROJECT_NAME, config.FolderName);
    }

    public override void PreProcessScriptingSymbols(BaseBuildConfig config, in List<string> symbolList)
    {
        BuildConfig buildConfig = config as BuildConfig;

        // Remove or add symbolList based on your needs
    }
}
```

```cs
public class PipelineBuilderWindow : BasePipelineBuilderWindow
{
    [MenuItem("Tools/BuildConfig")]
    public static void SetGameBuild()
     => GetWindow<PipelineBuilderWindow>("Build Config");

    protected override BaseBuildConfigPipelineBuilder GetBuilder()
    {
        return new BuildConfigPipelineBuilder2();
    }
}
```

 
## API Reference
| Name | Description     | 
| :-------- | :------- | 
| BaseBuildConfig | Build Configuration | 
| BaseBuildConfigContainer | Hold list of build configs | 
| BaseBuildConfigPipelineBuilder | Core logic for the build pipeline | 
| BasePipelineBuilderWindow | The user interface to build the configs | 

## Known Issues
#### Location Path Name is not working
- This issue is persist in 2021.3.21 - 23. 
- **No Workaround Available**
- [Unity Issue Tracker](https://issuetracker.unity3d.com/issues/buildpipeline-dot-buildplayer-ignores-buildplayeroptions-dot-locationpathname-and-attempts-to-build-to-the-cached-folder)
