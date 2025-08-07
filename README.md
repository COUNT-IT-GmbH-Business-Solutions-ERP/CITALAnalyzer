# CITALAnalyzer

If you are not happy with some rules or only feel like you need one rule of this analyzer, you can always control the rules with a [Custom.ruleset.json](CITALAnalyzer.ruleset.json) and disable all rules you don't need.


## Configuration

Some rules can be configured by adding a file named `CITALAnalyzer.json` in the root of your project.
**Important:** The file will only be read on startup of the linter, meaning if you make any changes you need to reload VS Code once.

### How to append new rules


1. In `CITALAnalyzer/CITALAnalyzer.ruleset.json`, copy an existing rule block and adjust it accordingly (id, Action, justification).

2. In `CITALAnalyzer/CITALAnalyzerAnalyzers.resx`, copy a rule block and update the contents (Title, Format, Description — this is the comment shown to the user).

3. In `CITALAnalyzer/DiagnosticDescriptors`, copy a rule block and modify the values (id, title, messageFormat, category, defaultSeverity, isEnabledByDefault, description, helpLinkUri).

4. In `CITALAnalyzer/Design/`, create a new `.cs` file where the logical part of the rule check will be implemented.


## Can I disable certain rules?

Since the linter integrates with the AL compiler directly, you can use the custom rule sets like you are used to from the other code cops.
https://docs.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/devenv-rule-set-syntax-for-code-analysis-tools

Of course you can also use pragmas for disabling a rule just for a certain place in code.
https://docs.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/directives/devenv-directive-pragma-warning

## Rules

|Id| Title|Default Severity|
|---|---|---|
|[CI0001](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0001-%E2%80%90-Variable-Name-Prefix)|Variable names must not contain a prefix unless to avoid naming conflicts.|Warning|
|[CI0002](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0002-%E2%80%90-Table-Field-Captions)|Captions must always be defined for table fields, even if the field name is the same as the caption.|Warning|
|[CI0003](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0003-%E2%80%90-DataClassification)|Omit DataClassification, except for AppSource or when it is explicitly required.|Warning|
|[CI0004](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0004-%E2%80%90-EventSub-Name)|The name of the event subscriber must match the name of the event it subscribes to.|Warning|
|[CI0005](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0005-%E2%80%90-EventSub-Parameter)|In an EventSubscriber, only the parameters that are actually needed should be passed.|Warning|
|[CI0006](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0006-%E2%80%90-Locked-Captions)|For empty captions, the Locked property must be set to true.|Warning|
|[CI0007](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0007-%E2%80%90-Variable-Naming)|Names of variables must always include the AL object and must not contain special characters.|Warning|
|[CI0008](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0008-%E2%80%90-EventSub-File-Name)|Event subscribers must always be placed in a codeunit that has the same name as the object to which the event belongs.|Warning|
|[CI0009](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0009-%E2%80%90-no-addbefore%22,-%22addafter%22,-%22movebefore%22-&-%22moveafter)|It is not allowed to use “addbefore”, “addafter”, “movebefore” or “moveafter” in PageExtensions|Warning|
|[CI0010](https://github.com/COUNT-IT-GmbH-Business-Solutions-ERP/CITALAnalyzer/wiki/CIG0010-%E2%80%90-call-by-reference,-only-when-modifying-variable)|Call by reference may only be used if the variable is modified within the procedure.|Warning|

## Codespace

### Project Setup and Compilation Guide

This part outlines the steps required to clone, build, and integrate the custom AL code analyzer into your development environment.

#### 1. Clone the Project

Start by cloning the repository containing the analyzer source code.

#### 2. Restore NuGet Packages

Restore all necessary NuGet packages for the project to resolve dependencies. This includes packages like `Newtonsoft.Json` and others used by the analyzer.

- Alternatively, if there are existing manual references, delete those and rely solely on NuGet package references, which will be automatically managed.

#### 3. Update Dependencies

Copy the following required DLL dependencies into the project folder:

- `Microsoft.Dynamics.Analyzer.Common.dll`
- `Microsoft.Dynamics.Nav.CodeAnalysis.dll`
- `Microsoft.Dynamics.Nav.CodeAnalysis.Workspace.dll`

#### 4. Build the Project

Build the project using the .NET CLI to generate the analyzer assembly:

```bash
dotnet clean
dotnet build
```

Upon successful build, the compiled DLL will be located in one of the following output directories:

- `bin/Debug/netstandard2.1/`
- `bin/Debug/net8.0/`

#### 5. Deploy the Analyzer DLL

Copy the generated DLL file (`CITALAnalyzer.dll`) into the AL analyzer folder, where other Microsoft analyzers are stored:

C:\Users\\\<YourUser>\\.vscode\extensions\ms-dynamics-smb.al-\<version>\bin\Analyzers

Replace `<YourUser>` with your Windows username and `<version>` with your installed AL extension version.

#### 6. Configure Visual Studio Code

Add the DLL to the AL Code Analyzer settings by modifying your AL settings JSON configuration. Add the following entry under the `"al.codeAnalyzers"` array:

```json
"${analyzerfolder}CITALAnalyzer.dll",
```

#### 7. Restart Visual Studio Code

Reload or restart Visual Studio Code to apply the changes. Your custom analyzer should now be active and running alongside the built-in analyzers.

### Common errors

### see for more detailed information

e.g. Debugging, pipeline integration, testing, other rules,...

https://github.com/StefanMaron/BusinessCentral.LinterCop


