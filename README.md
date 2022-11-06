# Templ

[![openupm](https://img.shields.io/npm/v/com.willykc.templ?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.willykc.templ/)

Integrates [Scriban](https://github.com/scriban/scriban/) template engine with Unity editor.

* Template based asset generation, including scripts, prefabs, scriptable objects, or any text asset.
* Automatic generation when input assets or templates change.
* Generation of scaffold tree structures with directories and files.
* Extensible design allows custom inputs and template functions.

## Unity version support

Tested to work with Unity 2020.3 and 2021.3.

## How to install

Install the latest version of [OpenUPM CLI](https://openupm.com/docs/getting-started.html#installing-openupm-cli), browse to your project directory and run:

```shell
openupm add com.willykc.templ
```

## Getting started

After installation completes, a prompt should show up in the Unity editor. When clicking on Proceed, the **TemplSettings** asset will be created under the *Assets/Editor/TemplData* directory.

The **TemplSettings** asset can always be located by clicking on the *Windows/Templ/Settings* menu.

Create new templates by right-clicking anywhere in the project hierarchy and selecting *Create/Templ/Scriban Template*.

## Live Entries

Live entries allow automatic generation of assets whenever *input* assets change.

Add and configure live entries in **TemplSettings** by specifying input, template, directory and output filename (target output file extension must be included in filename). Templ includes by default two entry types: **ScriptableObjectEntry** and **AssemblyDefinitionEntry**.

**ScriptableObjectEntry** takes as input any scriptable object and must be referred to in templates as `scriptableObject`.

**AssemblyDefinitionEntry** takes as input any assembly definition and must be referred to in templates as `assembly`.

## Scaffolds

Scaffolds are tree structures representing hierarchies of directories and files that can be generated anywhere in the project hierarchy.

Create new scaffold definitions by selecting *Create/Templ/Scaffold Definition* or *Create/Templ/Dynamic Scaffold Definition*

Use the toolbar in the standard scaffold inspector to create, clone or delete directory and file nodes under the *Root* node.

Nodes can be moved by dragging and dropping them anywhere in the scaffold tree.

Double-click on any node to open it for edits. Node names support Scriban statements, and will be rendered during generation. Templates can be assigned to file nodes in edit mode.

Every scaffold change can be undone/redone as any other operation in the Unity editor.

### Generating a scaffold

Add any scaffold to the *Selectable Scaffolds* list in **TemplSettings** to enable it for generation.

Select *Generate Templ Scaffold* from the context menu after right-clicking any existing asset in the project hierarchy. The selected asset will be exposed to the scaffold as **Selection**, and can be accessed in templates and node names.

A selection menu will be shown with all valid scaffolds in the *Selectable Scaffolds* list.

Select the scaffold you wish to generate. If the selected scaffold is configured with a default input, a dialog will be shown where input values can be edited before generation. Press the button below to generate the scaffold. If the selected scaffold is not configured with a default input, then the scaffold will be generated immediately after selected.

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

## Reserved Keywords

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

The included samples showcase some of Templ's use cases.

The following output file extensions should be used when configuring live entries from samples:

* **ScriptableObject to XML**: *.xml*
* **ScriptableObject to Code**: *.cs*
* **ScriptableObject to ScriptableObject**: *.asset*
* **AssemblyDefinition to Code**: *.cs*
* **AssemblyDefinition to ScriptableObject**: *.asset*
* **AssemblyDefinition to Prefab**: *.prefab*
* **Extensions**: *.txt*

## Extensibility

The sample **Extensions** contains a custom entry type that takes as input a **TextAsset**, and custom template functions exposed to Scriban when rendering templates. It is recommended as a starting point for creating custom entries and/or custom template functions.

### Custom Entry

To add a custom entry, extend and implement the `TemplEntry` abstract class. Apply `[TemplEntryInfo]` attribute and specify `ChangeTypes` and `Deferred` property values. The `ChangeTypes` property controls which type of changes should the entry respond to: `Import`, `Move` and/or `Delete`. The `Deferred` property controls whether template is rendered before or after assembly reloads. The custom entry class must not be abstract. Containing assembly name must not start with `Unity`.

Apply `[TemplInput]` attribute to desired input field. Selected input field must be public and extend `UnityEngine.Object` type.

Override the `InputField` getter to provide a custom value to Scriban.

Override the `IsValidInputField` getter to customize how to determine if input field is valid.

Override the `IsInputChanged` method to customize how to determine if input asset has changed or is about to be deleted.

### Custom template functions

To add custom template functions, define a static class and apply `[TemplFunctions]` attribute to it. Every static method declared in this class will be exposed to Scriban when rendering templates.

Templ includes by default a number of [custom template functions](Editor/TemplFunctions.cs). Templ will log an error when custom template function names collide, and will not render any template until custom template function name duplicates are removed.

## Coding guidelines

This repository follows [these coding guidelines](https://github.com/raywenderlich/c-sharp-style-guide).

## License

[MIT](LICENSE)
