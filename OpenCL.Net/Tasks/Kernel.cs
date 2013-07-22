using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;

using Microsoft.Build.Framework;
using System.IO;
using System.Text.RegularExpressions;

using OpenCL.Net.Extensions;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace OpenCL.Net.Tasks
{
    public sealed class Kernel : ITask
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        public ITaskItem[] InputFiles { get; set; }

        [Output] public ITaskItem[] OutputFiles { get; set; }

        public bool Execute()
        {
            for (int i = 0; i < InputFiles.Length; i++)
            {
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(string.Format("Generating kernel wrappers for {0}", Path.GetFileName(InputFiles[i].ItemSpec)), "Kernel", "OpenCL.Net", MessageImportance.High));
                File.WriteAllText(OutputFiles[i].ItemSpec,
                  ProcessKernelFile(InputFiles[i].ItemSpec, File.ReadAllText(InputFiles[i].ItemSpec)));
            }

            BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Done.", "Kernel", "OpenCL.Net", MessageImportance.High));
            return true;
        }

        private const string KernelName = "kernelName";
        private const string Qualifier = "qualifier";
        private const string Datatype = "datatype";
        private const string Pointer = "pointer";
        private const string Identifier = "identifier";
        private const string VectorWidth = "vectorWidth";

        private static readonly Regex _kernelParser = new Regex(@"(__)?kernel\s+void\s+(?<kernelName>[\w_]+)\s*\((\s*((__)?(?<qualifier>(global|local))\s+)?(?(qualifier)|(?<qualifier>))(?<datatype>(bool|char|unsigned char|uchar|short|unsigned short|ushort|float|int|unsigned int|uint|long|unsigned long|ulong|size_t))(?<vectorWidth>(16|2|3|4|8)?)\s*(?<pointer>\*?)\s+(?<identifier>[_\w]+)\s*,?\s*)+\)",
            RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private static string GenerateCSharpCode(CodeCompileUnit compileunit)
        {
            var provider = new CSharpCodeProvider();

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var tw = new IndentedTextWriter(sw, "    "))
                provider.GenerateCodeFromCompileUnit(compileunit, tw,
                    new CodeGeneratorOptions());

            return sb.ToString();
        }

        private static string TranslateType(string clType, int vectorWidth)
        {
            if (vectorWidth == 0)
                switch (clType)
                {
                    case "bool": return typeof(bool).FullName;

                    case "char": return typeof(char).FullName;

                    case "unsigned char":
                    case "uchar": return typeof(byte).FullName;

                    case "short": return typeof(short).FullName;

                    case "unsigned short":
                    case "ushort": return typeof(ushort).FullName;

                    case "int": return typeof(int).FullName;

                    case "unsigned int":
                    case "uint": return typeof(uint).FullName;

                    case "long": return typeof(long).FullName;

                    case "unsigned long":
                    case "ulong": return typeof(ulong).FullName;

                    case "float": return typeof(float).FullName;

                    case "size_t": return typeof(IntPtr).FullName;

                    default:
                        return "Unknown";
                }
            else
            {
                switch (clType)
                {
                    case "char":
                    case "uchar":
                    case "short":
                    case "ushort":
                    case "int":
                    case "uint":
                    case "long":
                    case "ulong":
                    case "float":
                    case "double":
                        return string.Format("{0}{1}", clType, vectorWidth);

                    default:
                        return "Unknown";
                }
            }
        }

        private static string ProcessKernelFile(string filename, string kernelFileContents)
        {
            var codeUnit = new CodeCompileUnit();
            var ns = new CodeNamespace(Path.GetFileNameWithoutExtension(filename));
            codeUnit.Namespaces.Add(ns);

            var lines = kernelFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var match = _kernelParser.Match(line);
                if (match.Success)
                {
                    var kernelName = match.Groups[KernelName].Value;
                    var kernel = new CodeTypeDeclaration(kernelName);
                    ns.Types.Add(kernel);
                    ns.Imports.Add(new CodeNamespaceImport("OpenCL.Net"));
                    ns.Imports.Add(new CodeNamespaceImport("OpenCL.Net.Extensions"));

                    kernel.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                    kernel.BaseTypes.Add(typeof(KernelWrapperBase));

                    var constructor = new CodeConstructor();
                    kernel.Members.Add(constructor);

                    var constructorParams = new CodeParameterDeclarationExpression(typeof(Cl.CommandQueue), "commandQueue");
                    constructor.Parameters.Add(constructorParams);
                    constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("commandQueue"));
                    constructor.Attributes = MemberAttributes.Public;

                    var executePrivateMethod = new CodeMemberMethod
                    {
                        Name = "run",
                        Attributes = MemberAttributes.Private | MemberAttributes.Final,
                        ReturnType = new CodeTypeReference(typeof(Cl.Event))
                    };
                    var execute1DMethod = new CodeMemberMethod
                    {
                        Name = "Run",
                        Attributes = MemberAttributes.Public | MemberAttributes.Final,
                        ReturnType = new CodeTypeReference(typeof(Cl.Event))
                    };
                    var execute2DMethod = new CodeMemberMethod
                    {
                        Name = "Run",
                        Attributes = MemberAttributes.Public | MemberAttributes.Final,
                        ReturnType = new CodeTypeReference(typeof(Cl.Event))
                    };
                    var execute3DMethod = new CodeMemberMethod
                    {
                        Name = "Run",
                        Attributes = MemberAttributes.Public | MemberAttributes.Final,
                        ReturnType = new CodeTypeReference(typeof(Cl.Event))
                    };

                    var callPrivateExecute1D = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "run"));
                    var callPrivateExecute2D = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "run"));
                    var callPrivateExecute3D = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "run"));

                    execute1DMethod.Statements.Add(new CodeMethodReturnStatement(callPrivateExecute1D));
                    execute2DMethod.Statements.Add(new CodeMethodReturnStatement(callPrivateExecute2D));
                    execute3DMethod.Statements.Add(new CodeMethodReturnStatement(callPrivateExecute3D));

                    kernel.Members.Add(executePrivateMethod);
                    kernel.Members.Add(execute1DMethod);
                    kernel.Members.Add(execute2DMethod);
                    kernel.Members.Add(execute3DMethod);

                    for (int i = 0; i < match.Groups[Identifier].Captures.Count; i++)
                    {
                        bool isPointer = !string.IsNullOrEmpty(match.Groups[Pointer].Captures[i].Value);
                        var rawDatatype = match.Groups[Datatype].Captures[i].Value;
                        var name = match.Groups[Identifier].Captures[i].Value;
                        var vectorWidth = match.Groups[VectorWidth].Captures[i].Value == string.Empty ? 0 : int.Parse(match.Groups[VectorWidth].Captures[i].Value);
                        var qualifier = match.Groups[Qualifier].Captures[i].Value;
                        var local = false;

                        CodeParameterDeclarationExpression parameter = null;
                        switch (qualifier)
                        {
                            case "global":
                                parameter = new CodeParameterDeclarationExpression(string.Format("OpenCL.Net.Cl.Mem<{0}>", TranslateType(rawDatatype, vectorWidth)), name);
                                break;
                            case "local":
                                local = true;
                                name = name + "_length";
                                parameter = new CodeParameterDeclarationExpression(typeof(int), name);
                                break;
                            case "":
                                parameter = new CodeParameterDeclarationExpression(TranslateType(rawDatatype, vectorWidth), name);
                                break;
                        }
                        if (parameter != null)
                        {
                            executePrivateMethod.Parameters.Add(parameter);
                            execute1DMethod.Parameters.Add(parameter);
                            execute2DMethod.Parameters.Add(parameter);
                            execute3DMethod.Parameters.Add(parameter);

                            callPrivateExecute1D.Parameters.Add(new CodeArgumentReferenceExpression(name));
                            callPrivateExecute2D.Parameters.Add(new CodeArgumentReferenceExpression(name));
                            callPrivateExecute3D.Parameters.Add(new CodeArgumentReferenceExpression(name));
                        }

                        var setArgument = local ?
                            new CodeMethodInvokeExpression(new CodeSnippetExpression("OpenCL.Net.Cl"), "SetKernelArg",
                                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Kernel"), new CodeSnippetExpression(i.ToString()),
                                new CodeCastExpression(new CodeTypeReference(typeof(IntPtr)), new CodeArgumentReferenceExpression(name)),
                                new CodeSnippetExpression("null")) :
                            new CodeMethodInvokeExpression(new CodeSnippetExpression("OpenCL.Net.Cl"), "SetKernelArg",
                                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Kernel"), new CodeSnippetExpression(i.ToString()),
                                new CodeArgumentReferenceExpression(name));
                        executePrivateMethod.Statements.Add(setArgument);
                    }

                    executePrivateMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "globalWorkSize0"));
                    executePrivateMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "globalWorkSize1 = 0"));
                    executePrivateMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "globalWorkSize2 = 0"));

                    executePrivateMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "localWorkSize0 = 0"));
                    executePrivateMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "localWorkSize1 = 0"));
                    executePrivateMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "localWorkSize2 = 0"));

                    var eventWaitListParam = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Cl.Event[])), "eventWaitList");
                    eventWaitListParam.CustomAttributes.Add(new CodeAttributeDeclaration("System.ParamArrayAttribute"));
                    executePrivateMethod.Parameters.Add(eventWaitListParam);
                    executePrivateMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(Cl.Event), "ev"));
                    executePrivateMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(Cl.ErrorCode), "err"));
                    executePrivateMethod.Statements.Add(new CodeAssignStatement(
                        new CodeVariableReferenceExpression("err"),
                        // = 
                        new CodeMethodInvokeExpression(new CodeSnippetExpression("OpenCL.Net.Cl"), "EnqueueNDRangeKernel",
                            new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CommandQueue"),
                            new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Kernel"),
                            new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "GetWorkDimension",
                                new CodeArgumentReferenceExpression("globalWorkSize0"),
                                new CodeArgumentReferenceExpression("globalWorkSize1"),
                                new CodeArgumentReferenceExpression("globalWorkSize2")),
                            new CodeSnippetExpression("null"),
                            new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "GetWorkSizes",
                                new CodeArgumentReferenceExpression("globalWorkSize0"),
                                new CodeArgumentReferenceExpression("globalWorkSize1"),
                                new CodeArgumentReferenceExpression("globalWorkSize2")),
                            new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "GetWorkSizes",
                                new CodeArgumentReferenceExpression("localWorkSize0"),
                                new CodeArgumentReferenceExpression("localWorkSize1"),
                                new CodeArgumentReferenceExpression("localWorkSize2")),

                            new CodeCastExpression(new CodeTypeReference(typeof(uint)),
                                new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("eventWaitList"), "Length")),
                            new CodeArgumentReferenceExpression("eventWaitList"),
                            new CodeVariableReferenceExpression("out ev"))));
                    executePrivateMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeSnippetExpression("OpenCL.Net.Cl"), "Check", new CodeVariableReferenceExpression("err")));
                    executePrivateMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("ev")));

                    execute1DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "globalWorkSize"));
                    execute1DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "localWorkSize"));
                    execute1DMethod.Parameters.Add(eventWaitListParam);
                    callPrivateExecute1D.Parameters.Add(new CodeArgumentReferenceExpression("globalWorkSize0: globalWorkSize"));
                    callPrivateExecute1D.Parameters.Add(new CodeArgumentReferenceExpression("localWorkSize0: localWorkSize"));
                    callPrivateExecute1D.Parameters.Add(new CodeArgumentReferenceExpression("eventWaitList: eventWaitList"));

                    execute2DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "globalWorkSize0"));
                    execute2DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "globalWorkSize1"));
                    execute2DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "localWorkSize0"));
                    execute2DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "localWorkSize1"));
                    execute2DMethod.Parameters.Add(eventWaitListParam);
                    callPrivateExecute2D.Parameters.Add(new CodeArgumentReferenceExpression("globalWorkSize0: globalWorkSize0"));
                    callPrivateExecute2D.Parameters.Add(new CodeArgumentReferenceExpression("globalWorkSize1: globalWorkSize1"));
                    callPrivateExecute2D.Parameters.Add(new CodeArgumentReferenceExpression("localWorkSize0: localWorkSize0"));
                    callPrivateExecute2D.Parameters.Add(new CodeArgumentReferenceExpression("localWorkSize1: localWorkSize1"));
                    callPrivateExecute2D.Parameters.Add(new CodeArgumentReferenceExpression("eventWaitList: eventWaitList"));

                    execute3DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "globalWorkSize0"));
                    execute3DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "globalWorkSize1"));
                    execute3DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "globalWorkSize2"));
                    execute3DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "localWorkSize0"));
                    execute3DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "localWorkSize1"));
                    execute3DMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(uint)), "localWorkSize2"));
                    execute3DMethod.Parameters.Add(eventWaitListParam);
                    callPrivateExecute3D.Parameters.Add(new CodeArgumentReferenceExpression("globalWorkSize0: globalWorkSize0"));
                    callPrivateExecute3D.Parameters.Add(new CodeArgumentReferenceExpression("globalWorkSize1: globalWorkSize1"));
                    callPrivateExecute3D.Parameters.Add(new CodeArgumentReferenceExpression("globalWorkSize2: globalWorkSize2"));
                    callPrivateExecute3D.Parameters.Add(new CodeArgumentReferenceExpression("localWorkSize0: localWorkSize0"));
                    callPrivateExecute3D.Parameters.Add(new CodeArgumentReferenceExpression("localWorkSize1: localWorkSize1"));
                    callPrivateExecute3D.Parameters.Add(new CodeArgumentReferenceExpression("localWorkSize2: localWorkSize2"));
                    callPrivateExecute3D.Parameters.Add(new CodeArgumentReferenceExpression("eventWaitList: eventWaitList"));
                }
            }

            return GenerateCSharpCode(codeUnit);
        }

        private static string ReadFile(string filename)
        {
            using (var reader = new StreamReader(filename))
                return reader.ReadToEnd();
        }
    }
}
