# Templ

[![openupm](https://img.shields.io/npm/v/com.willykc.templ?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.willykc.templ/)

Integrates [Scriban](https://github.com/scriban/scriban/) template engine with Unity editor.

* Template based asset generation, including scripts, prefabs, scriptable objects, or any text asset.
* Automatic generation when input assets or templates change.
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

Create new templates by right-clicking anywhere in the project hierarchy and selecting *Create/Scriban Template*.

Add and configure entries in **TemplSettings** by specifying input, template, directory and output filename (target output file extension must be included in filename). Templ includes by default two entry types: **ScriptableObjectEntry** and **AssemblyDefinitionEntry**.

**ScriptableObjectEntry** takes as input any scriptable object and must be referred to in templates as `scriptableObject`.

**AssemblyDefinitionEntry** takes as input any assembly definition and must be referred to in templates as `assembly`.

## Scriban

The Scriban [language](https://github.com/scriban/scriban/blob/master/doc/language.md) syntax is well documented.

It is recommended to use [Visual Studio Code](https://code.visualstudio.com/) and [Scriban syntax highlights](https://marketplace.visualstudio.com/items?itemName=xoofx.scriban) to write and edit templates.

By default, Templ will create new templates with *.sbn* extension. For script templates, change the extension to *.sbncs* for better syntax highlights.

The Scriban license can be found [here](https://github.com/scriban/scriban/blob/master/license.txt).

## Samples

The included samples showcase some of Templ's use cases.

The following output file extensions should be used when configuring samples:

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

The `IsInputChanged` method must determine if input asset has changed or is about to be deleted.

### Custom template functions

To add custom template functions, define a static class and apply `[TemplFunctions]` attribute to it. Every static method declared in this class will be exposed to Scriban when rendering templates.

Templ includes by default a number of [custom template functions](Editor/TemplFunctions.cs). Templ will log an error when custom template function names collide, and will not render any template until custom template function name duplicates are removed.

## Coding guidelines

This repository follows [these coding guidelines](https://github.com/raywenderlich/c-sharp-style-guide).

## License

[MIT](LICENSE)
