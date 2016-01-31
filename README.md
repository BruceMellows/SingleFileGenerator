SingleFileGenerator
===================

This framework for a Visual Studio **_single file generator_** will be accompanied by an article (yet to be written). The intent is to provide a simple way to write a single file generator.

The basis for this framework can be veiwed at [VSIX Deployable Single File Generator Sample](http://blogs.msdn.com/b/vsx/archive/2013/11/27/building-a-vsix-deployable-single-file-generator.aspx)

This framework and steps to use it are designed **ONLY** for Visual Studio 2010 (because that is what I have)

## TLDR;
1. Ensure that the [SDK for Visual Studio 2010](https://www.microsoft.com/en-au/download/details.aspx?id=2680) is installed
2. Download the solution from github
3. Open the downloaded solution
4. Create your transformation provider assembly ...
  1. `Add->New project->Visual C#->Windows->Class Library`
  2. Reference the `SingleFileGeneratorInterface` assembly
  3. Create your transformation provider class ... `Add->Class ...` and inherit from `BadCompany.SingleFileGeneratorInterface.IProvider` and implement this interface
  4. Sign this assembly with a strong name key
5. Create your package assembly ... `Add->New project->Other Project Types->Extensibility->Visual Studio Package` (Navigate through the wizard)
  1. Reference `SimpleFileGenerator`, `SingleFileGeneratorInterface` and your transformation provider assembly
  2. Copy `C#\SimpleFileGenerator\CodeGeneratorRegistrationAttribute.cs` from [the code](http://code.msdn.microsoft.com/VSIX-Deployable-Single-ee39e3fe) to your package assembly project *(it's license makes me unwilling to supply it directly)*
  3. Create your transformation provider bridge class (**NOTE:** the name of this class is the name of your custom tool) ... `Add->Class ...` and replace it with the following body (adjusted for your namespace/class name/etc)
```
		namespace XxxMyPackage
		{
			using System.Runtime.InteropServices;
			using Microsoft.VisualStudio.Shell;
			using Microsoft.Demo.SimpleFileGenerator;
			using BadCompany.SingleFileGenerator;
			
			[ComVisible(true)]
			[Guid(GuidList.guidMySingeFileGeneratorPkgString)]
			[ProvideObject(typeof(XxxMyCustomTool))]
			[CodeGeneratorRegistration(typeof(XxxMyCustomTool), "XxxMyCustomTool", "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}", GeneratesDesignTimeSource = true)]
			public class XxxMyCustomTool : VsSingleFileGenerator<XxxMyProvider>
			{
			}
		}
```
6. Compile
7. Debug your package assembly (your transformation provider may still throw new NotImplementedException() if you have not done anything more that implement the interface in the steps above)

## Things worth mentioning here;

* I am **NOT** an "Extensibility" expert - probably not even a novice
* The GUID `"{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}"` means that your SFG is suitable for a C# project
  * You can locate other GUIDs in the `Microsoft Visual Studio 10.0/Common7/IDE/PublicAssemblies/VSLangProj80.dll` assembly
  * I did add the VisualBasic GUID, but it did not appear to run correctly
* You can quite easily move all the code into the package assembly, I prefer to split it up so that I can unit test the transformation provider
* When implementing `IGeneratorProvider.WriteOutput`, do not use the `inputFilePath` argument to read the content, but rather use `inputFileContent`, this allows for easier unit testing
* You might **strongly** consider following the steps at [VSIX Deployable Single File Generator Sample](http://blogs.msdn.com/b/vsx/archive/2013/11/27/building-a-vsix-deployable-single-file-generator.aspx) to make your SFG available to Express editions of Visual Studio
