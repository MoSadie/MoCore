# MoCore

A common plugin used across my other BepInEx plugins.

## Setup

### Add the dll

Download the latest version from the [release page](https://github.com/mosadie/mocore/releases/latest) and place it into a lib folder with your code. Add the following to your .csProj file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    ...

    <ItemGroup>
        <Reference Include="MoCore">
            <HintPath>lib/com.mosadie.mocore.dll</HintPath>
        </Reference>
    </ItemGroup>

</Project>
```

### In your main Plugin class

Have the plugin class implement MoPlugin and call MoCore.RegisterPlugin and pass itself. The plugin should also make use of the [BepInDependency](https://docs.bepinex.dev/api/BepInEx.BepInDependency.html) attribute to make sure it loads after MoCore.

Example of the attribute:
```C#
[BepInDependency("com.mosadie.mocore", BepInDependency.DependencyFlags.HardDependency)]
```

Example of version checking in Awake:
```C#
if (!MoCore.MoCore.RegisterPlugin(this))
{
    Log.LogError("Failed to register plugin with MoCore. Please check the logs for more information.");
    return;
}
```

### Make sure to mark as dependencies on Thunderstore as well

If using the tcli here is how it looks in thunderstore.yml

```yml
[package.dependencies]
BepInEx-BepInExPack= "5.4.2100"
MoSadie-MoCore= "1.0.0"
```

## Features

### Game Version Checking

Return a url to a json file from `GetVersionCheckUrl` and you can use the return value of `RegisterPlugin` to determine if the game versinon supports your plugin. Pass null for the `GetVersionCheckUrl` to disable version checking. In addition there is a configuration option in MoCore to skip version checking.

Here is a example of what the version json file looks like:
```json
{
    "1.2.3": [ // Plugin version
        "4.1579" // Application.version value
    ]
}
```

# TODO LIST FOR v2

- [ ] Add HTTP Server to allow plugins to handle HTTP requests
    - [ ] Add Server Thread
	- [ ] Add handler interface
	- [ ] Add handler registration
	- [ ] Make everything work
	- [ ] Documentation for how to use
- [ ] Move/Copy Variable system from SlipChat to MoCore
- [ ] TEST EVERYTHING