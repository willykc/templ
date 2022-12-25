# Templ

[![openupm](https://img.shields.io/npm/v/com.willykc.templ?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.willykc.templ/)

Integrates [Scriban](https://github.com/scriban/scriban/) template engine with Unity editor.

* Template based asset generation, including scripts, prefabs, scriptable objects, or any text asset.
* Automatic generation when input assets or templates change.
* Generation of scaffold tree structures with directories and files.
* Extensible design allows custom inputs and template functions.
* Public API allows full control of asset generation.

## Unity version support

Tested to work with Unity 2020.3 and 2021.3.

## How to install

Install the latest version of [OpenUPM CLI](https://openupm.com/docs/getting-started.html#installing-openupm-cli), browse to your project directory and run:

```shell
openupm add com.willykc.templ
```

## Getting started

After installation completes, a prompt will show up in the Unity editor. When clicking on Proceed, the **TemplSettings** asset will be created under the *Assets/Editor/TemplData* directory.

The **TemplSettings** asset can always be located by clicking on the *Windows/Templ/Settings* menu. In case **TemplSettings** is removed, clicking on the same menu will create a fresh copy at the default location shown above.

Create new templates by right-clicking anywhere in the project hierarchy and selecting *Create/Templ/Scriban Template*.

## Live Entries

Live entries allow automatic generation of assets whenever *input* assets change.

Add and configure live entries in the **TemplSettings** by specifying input, template, directory and output filename (target output file extension must be included in filename). The default entry types are **ScriptableObjectEntry** and **AssemblyDefinitionEntry**.

**ScriptableObjectEntry** takes as input any scriptable object. The scriptable object is exposed to templates as `scriptableObject`.

**AssemblyDefinitionEntry** takes as input any assembly definition. The assembly itself (not the assembly definition asset) is exposed to templates as `assembly`.

## Scaffolds

Scaffolds are tree structures representing hierarchies of directories and files that can be generated anywhere in the project hierarchy.

Create new scaffold definitions by selecting *Create/Templ/Scaffold Definition* or *Create/Templ/Dynamic Scaffold Definition*

Use the toolbar in the standard scaffold inspector to create, clone or delete directory and file nodes under the *Root* node.

Nodes can be moved by dragging and dropping them anywhere in the scaffold tree.

Double-click on any node to open it for edits. Node names support Scriban statements, and will be rendered during generation. Templates can be assigned to file nodes in edit mode.

Every scaffold change can be undone/redone as any other operation in the Unity editor.

### Generating a scaffold

Add any scaffold to the *Selectable Scaffolds* list in **TemplSettings** to enable it for generation.

Right-click on any existing asset in the project hierarchy and select *Generate Templ Scaffold* from the context menu. A selection menu will be shown with all valid scaffolds in the *Selectable Scaffolds* list.

Select the scaffold you wish to generate. If the selected scaffold is configured with a default input, a dialog will be shown where input values can be edited before generation. Press the button below to generate the scaffold. If the selected scaffold is not configured with a default input, then the scaffold will be generated immediately after selected.

The selected asset in the project hierarchy will be exposed to the scaffold as **Selection**, and can be accessed in templates and node names.

Scaffolds can be regenerated in the same directory. Just before regeneration, a dialog will be shown to allow selecting which assets should be overwritten or skipped.

### Default Input

Create any scriptable object to define a default input for a scaffold.

Any value contained in the serialized scriptable object instance will be shown as a default value when generating the scaffold.

Custom inspectors for default input scriptable objects will be shown when generating scaffolds.

### Dynamic Scaffolds

Dynamic scaffolds enable more flexibility by allowing the tree structure be adjusted dynamically based on input values.

Instead of an editable tree structure in the inspector, dynamic scaffold only require to be configured with a tree template whose output should be a YAML representation of the scaffold tree.

The following is a sample of a scaffold tree represented in YAML format:

```yaml
- DirectoryName:
    - file.txt: Path/To/Template.sbn
    - SubdirectoryName:
        - anotherFile.txt: Path/To/AnotherTemplate.sbn
          variable: value
- rootLevelFile.txt: Path/To/RootLevelTemplate.sbn
```

When a dynamic scaffold is generated, the tree template must output a YAML tree compliant with the data structure shown in the sample above.

Any key/value pairs defined under templates in the YAML tree, such as `variable: value` in the sample, will be exposed to the template as is.

Templates can be referenced in YAML by asset paths or GUIDs.

Any standard scaffold can serve as the basis for a dynamic scaffold by opening the context menu in the scaffold inspector and selecting *Copy YAML Tree* or *Copy YAML Tree with GUIDs*.

YAML is parsed using SharpYaml. The license can be found [here](https://github.com/xoofx/SharpYaml/blob/master/LICENSE.txt).

## Reserved Template Keywords

* **OutputAssetPath**: Exposed to both live entry and scaffold templates, it is the asset path of the generated asset.
* **Input**: Exposed to scaffold templates only, it is the scaffold generation input object.
* **Selection**: Exposed to scaffold templates only, it is the selected asset when generating the scaffold.
* **Seed**: Exposed to scaffold templates only, it is a unique GUID for the scaffold generation.
* **RootPath**: Exposed to scaffold templates only, is is the root path of the scaffold generation.

## Scriban

The Scriban [language](https://github.com/scriban/scriban/blob/master/doc/language.md) syntax is well documented.

It is recommended to use [Visual Studio Code](https://code.visualstudio.com/) and [Scriban syntax highlights](https://marketplace.visualstudio.com/items?itemName=xoofx.scriban) to write and edit templates.

By default, Templ will create new templates with *.sbn* extension. For script templates, change the extension to *.sbncs* for better syntax highlights.

The Scriban license can be found [here](https://github.com/scriban/scriban/blob/master/license.txt).

## Samples

The included samples showcase some use cases.

The following output file extensions should be used when configuring live entries from samples:

* **ScriptableObject to XML**: *.xml*
* **ScriptableObject to Code**: *.cs*
* **ScriptableObject to ScriptableObject**: *.asset*
* **AssemblyDefinition to Code**: *.cs*
* **AssemblyDefinition to ScriptableObject**: *.asset*
* **AssemblyDefinition to Prefab**: *.prefab*
* **Extensions**: *.txt*

## Extensibility

The sample **Extensions** contains a custom entry type that takes as input a **TextAsset**. It also contains custom template functions exposed to Scriban when rendering templates. It is recommended as a starting point for creating custom entries and/or custom template functions.

### Custom Entries

To add a custom entry, extend and implement the `TemplEntry` abstract class. Apply the `[TemplEntryInfo]` attribute and specify `changeTypes`, `Deferred` and `DisplayName` parameters. The `changeTypes` parameter determines which type of changes should the entry respond to: `Import`, `Move` and/or `Delete`. The `Deferred` property determines whether the template should be rendered before or after assembly reloads and defaults to `false`. The `DisplayName` property determines how the custom entry should be displayed in the dropdown menu when adding it in the **TemplSettings**. If no value is specified for `DisplayName`, the dropdown menu will display the custom entry class name. The custom entry class must not be abstract, must provide a public default constructor, and the containing assembly name must not start with `Unity`.

Apply the `[TemplInput]` attribute to the desired input field. By default, the input field value is exposed to templates as the field name itself. To define exactly how to expose input values to templates, use the `ExposedAs` property. Using special or whitespace characters in the `ExposedAs` property will require [the special variable 'this'](https://github.com/scriban/scriban/blob/master/doc/language.md#41-the-special-variable-this) in templates. The chosen input field must be public and extend `UnityEngine.Object` type.

Override the `InputField` getter to provide a custom value to Scriban.

Override the `IsValidInputField` getter to customize how to determine if input field is valid.

Override the `IsInputChanged` method to customize how to determine if input asset has changed or is about to be deleted.

### Custom template functions

To add custom template functions, define a static class and apply the `[TemplFunctions]` attribute to it. Every static method declared in this class will be exposed to Scriban when rendering templates.

Several [custom template functions](Editor/TemplFunctions.cs) are included by default. An error will be logged when custom template function names collide. Templates will not render until custom template function name duplicates are removed.

`Assert` is one of the default custom template functions. It allows to assert any boolean condition in templates and show a specific error message when the condition is not met.

## Public API

The following methods and types are exposed as a public API to allow more control over asset generation. Live entries can be managed and rendered on demand. Scaffolds can be generated and enabled or disabled for selection.

```c#
TemplManagers.EntryManager.GetEntries()
```

Gets all configured entries in settings.

### Returns

| Type | Description |
| :--- | :--- |
| `Willykc.Templ.Editor.Entry.TemplEntry[]` | The array of configured entries. |

### Exceptions

**InvalidOperationException**:

**TemplSettings** does not exist.

```c#
TemplManagers.EntryManager.AddEntry<T>(UnityEngine.Object,Willykc.Templ.Editor.ScribanAsset,System.String)
```

Adds a new entry in settings. Added entries will not render automatically.

| Type Parameter | Description |
| :--- | :--- |
| `T` | **Required**. The type of entry. |

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `inputAsset` | `UnityEngine.Object` | **Required**. The input asset to monitor for changes. |
| `template` | `Willykc.Templ.Editor.ScribanAsset` | **Required**. The template to render. |
| `outputAssetPath` | `System.String` | **Required**. The output asset path. |

### Returns

| Type | Description |
| :--- | :--- |
| `System.String` | The entry ID. |

### Exceptions

**InvalidOperationException**:

**TemplSettings** does not exist.

-or-

T is not a valid entry Type.

-or-

Existing entry already uses `outputAssetPath`.

**ArgumentNullException**:

`inputAsset` is null.

-or-

`template` is null.

**ArgumentException**:

`outputAssetPath` is null.

-or-

`outputAssetPath` is empty.

-or-

`outputAssetPath` is not a valid path.

-or-

`outputAssetPath` contains invalid file name characters.

-or-

`inputAsset` equals **TemplSettings** instance.

-or-

`inputAsset` is of Type `Willykc.Templ.Editor.ScribanAsset`.

-or-

`inputAsset` does not match Type of `[TemplInput]` field.

-or-

`template` contains syntax errors.

**DirectoryNotFoundException**:

`outputAssetPath`'s directory does not exist.

```c#
TemplManagers.EntryManager.UpdateEntry(System.String,UnityEngine.Object,Willykc.Templ.Editor.ScribanAsset,System.String)
```

Updates an existing entry in settings. Updated entries will not render automatically.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `id` | `System.String` | **Required**. The ID of the entry. |
| `inputAsset` | `UnityEngine.Object` | **Optional**. The input asset to monitor for changes. |
| `template` | `Willykc.Templ.Editor.ScribanAsset` | **Optional**. The template to render. |
| `outputAssetPath` | `System.String` | **Optional**. The output asset path. |

### Exceptions

**InvalidOperationException**:

**TemplSettings** does not exist.

-or-

`id` does not match any existing entry.

-or-

Existing entry already uses `outputAssetPath`.

**ArgumentNullException**:

`id` is null.

**ArgumentException**:

`outputAssetPath` is not a valid path.

-or-

`outputAssetPath` contains invalid file name characters.

-or-

`inputAsset` equals **TemplSettings** instance.

-or-

`inputAsset` is of Type `Willykc.Templ.Editor.ScribanAsset`.

-or-

`inputAsset` does not match Type of `[TemplInput]` field.

-or-

`template` contains syntax errors.

**DirectoryNotFoundException**:

`outputAssetPath`'s directory does not exist.

```c#
TemplManagers.EntryManager.RemoveEntry(System.String)
```

Removes an existing entry from settings.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `id` | `System.String` | **Required**. The entry ID. |

### Exceptions

**InvalidOperationException**:

**TemplSettings** does not exist.

-or-

`id` does not match any existing entry.

**ArgumentNullException**:

`id` is null.

```c#
TemplManagers.EntryManager.EntryExists(System.String)
```

Determines if an entry exist with the given outputAssetPath.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `outputAssetPath` | `System.String` | **Required**. The output asset path. |

### Returns

| Type | Description |
| :--- | :--- |
| `System.Boolean` | True or false depending on existence of entry. |

### Exceptions

**InvalidOperationException**:

**TemplSettings** does not exist.

**ArgumentNullException**:

`outputAssetPath` is null.

```c#
TemplManagers.EntryManager.ForceRenderEntry(System.String)
```

Forces to render a specific entry in settings. In case entry ID matches an invalid entry, it will not be rendered.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `id` | `System.String` | **Required**. The entry ID. |

### Exceptions

**InvalidOperationException**:

**TemplSettings** does not exist.

-or-

`id` does not match any existing entry.

-or-

Matching entry is invalid.

**ArgumentNullException**:

`id` is null.

```c#
TemplManagers.EntryManager.ForceRenderAllValidEntries()
```

Forces to render all valid entries in settings.

### Exceptions

**InvalidOperationException**:

**TemplSettings** does not exist.

```c#
TemplManagers.ScaffoldManager.GenerateScaffoldAsync(Willykc.Templ.Editor.Scaffold.TemplScaffold,System.String,System.Object,UnityEngine.Object,Willykc.Templ.Editor.Scaffold.OverwriteOptions,System.Threading.CancellationToken)
```

Generates scaffold at target path. Asset database must be refreshed afterwards for the editor to show the generated assets.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `scaffold` | `Willykc.Templ.Editor.Scaffold.TemplScaffold` | **Required**. The scaffold to generate. |
| `targetPath` | `System.String` | **Required**. The asset path where to generate the scaffold. |
| `input` | `System.Object` | **Optional**. The input value to use during generation. |
| `selection` | `UnityEngine.Object` | **Optional**. The selection value to use during generation. |
| `overwriteOption` | `Willykc.Templ.Editor.Scaffold.OverwriteOptions` | **Optional**. The options to control asset overwrite behaviour. |
| `cancellationToken` | `System.Threading.CancellationToken` | **Optional**. The cancellation token. It can only cancel UI prompts, once generation starts it must fail or conclude. |

### Returns

| Type | Description |
| :--- | :--- |
| `System.String[]` | The array of generated asset paths. Null is returned in case user cancels UI prompts or generation errors are found. |

### Exceptions

**ArgumentNullException**:

`scaffold` is null.

**ArgumentException**:

`targetPath` is null.

-or-

`targetPath` is empty.

-or-

`scaffold` is invalid.

**DirectoryNotFoundException**:

`targetPath`'s directory does not exist.

```c#
TemplManagers.ScaffoldManager.EnableScaffoldForSelection(Willykc.Templ.Editor.Scaffold.TemplScaffold)
```

Enables scaffold in settings for selection from the context menu.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `scaffold` | `Willykc.Templ.Editor.Scaffold.TemplScaffold` | **Required**. The scaffold to enable for selection. |

### Exceptions

**InvalidOperationException**:

**TemplSettings** does not exist.

**ArgumentNullException**:

`scaffold` is null.

**ArgumentException**:

`scaffold` is invalid.

```c#
TemplManagers.ScaffoldManager.DisableScaffoldForSelection(Willykc.Templ.Editor.Scaffold.TemplScaffold)
```

Disables scaffold in settings for selection from the context menu.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `scaffold` | `Willykc.Templ.Editor.Scaffold.TemplScaffold` | **Required**. The scaffold to disable for selection. |

### Exceptions

**InvalidOperationException**:

**TemplSettings** does not exist.

**ArgumentNullException**:

`scaffold` is null.

```c#
class ScribanAsset
```

Namespace: Willykc.Templ.Editor

Extends: `UnityEngine.ScriptableObject`

| Property | Type | Access | Description |
| :--- | :--- | :--- | :--- |
| `Text` | `System.String` | `get;` | Template text. |
| `HasErrors` | `System.Boolean` | `get;` | Template validity. |
| `ParsingErrors` | `System.String[]` | `get;` | List of syntax errors. |

```c#
class TemplEntry
```

Namespace: Willykc.Templ.Editor.Entry

Extends: `System.Object`

| Property | Type | Access | Description |
| :--- | :--- | :--- | :--- |
| `Id` | `System.String` | `get;` | Unique entry id. |
| `Template` | `ScribanAsset` | `get;` | Entry template. |
| `InputAsset` | `UnityEngine.Object` | `get;` | Monitored asset. |
| `OutputPath` | `System.String` | `get;` | Path to the output asset. |
| `IsValid` | `System.Boolean` | `get;` | Entry validity. |

```c#
class ScriptableObjectEntry
```

| Field | Type | Description |
| :--- | :--- | :--- |
| `scriptableObject` | `UnityEngine.ScriptableObject` | The input scriptable object. |

Namespace: Willykc.Templ.Editor.Entry

Extends: ` Willykc.Templ.Editor.Entry.TemplEntry`

```c#
class AssemblyDefinitionEntry
```

| Field | Type | Description |
| :--- | :--- | :--- |
| `assembly` | `UnityEditorInternal.AssemblyDefinitionAsset` | The input assembly definition. |

Namespace: Willykc.Templ.Editor.Entry

Extends: ` Willykc.Templ.Editor.Entry.TemplEntry`

```c#
class TemplScaffold
```

Namespace: Willykc.Templ.Editor.Scaffold

Extends: `UnityEngine.ScriptableObject`

| Property | Type | Access | Description |
| :--- | :--- | :--- | :--- |
| `DefaultInput` | `UnityEngine.ScriptableObject` | `get;` | Default input instance. |

```c#
class TemplDynamicScaffold
```

Namespace: Willykc.Templ.Editor.Scaffold

Extends: `Willykc.Templ.Editor.Scaffold.TemplScaffold`

| Property | Type | Access | Description |
| :--- | :--- | :--- | :--- |
| `TreeTemplate` | `Willykc.Templ.Editor.ScribanAsset` | `get;` | Scaffold YAML tree template. |

```c#
enum OverwriteOptions
```

Namespace: Willykc.Templ.Editor.Scaffold

| Name | Value | Description |
| :--- | :--- | :--- 
| `None` | 0 | Show prompt for overwrites. |
| `ShowPrompt` | 1 | Show prompt for overwrites. |
| `OverwriteAll` | 2 | Overwrite all existing files. |
| `SkipAll` | 3 | Leave all existing files. |

## Coding guidelines

This repository follows [these coding guidelines](https://github.com/raywenderlich/c-sharp-style-guide).

## License

[MIT](LICENSE)
