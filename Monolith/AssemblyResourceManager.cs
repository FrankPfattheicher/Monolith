using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Monolith
{
    public class AssemblyResourceManager
    {
        public string FileName { get; private set; }
        public string AssemblyPath { get; private set; }

        private readonly AssemblyDefinition assembly;

        public AssemblyResourceManager(string name)
        {
            FileName = name;
            AssemblyPath = Path.GetDirectoryName(name) ?? string.Empty;
            assembly = AssemblyDefinition.ReadAssembly(name);
        }

        public List<string> GetResourceAssemblies()
        {
            var resources = assembly.MainModule.Resources
                .Where(IsAssemblyResource)
                .Select(res => res.Name)
                .ToList();

            return resources;
        }

        private bool IsAssemblyResource(Resource resource)
        {
            var er = resource as EmbeddedResource;
            if (er == null)
                return false;

            try
            {
                var res = AssemblyDefinition.ReadAssembly(er.GetResourceStream());
                if (res == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public List<string> GetLocalAssemblies()
        {
            var resources = Directory.EnumerateFiles(AssemblyPath, "*.dll")
                .Where(IsAssemblyFile)
                .Select(Path.GetFileName)
                .ToList();

            return resources;
        }
        public bool IsAssemblyFile(string fileName)
        {
            try
            {
                var ass = AssemblyDefinition.ReadAssembly(fileName);
                if (ass == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool AddAssemblyResource(string resFileName, bool move)
        {
            try
            {
                var resourceName = assembly.Name.Name + "." + Path.GetFileName(resFileName);
                var resourceData = File.ReadAllBytes(resFileName);
                var resource = new EmbeddedResource(resourceName, ManifestResourceAttributes.Private, resourceData);
                assembly.MainModule.Resources.Add(resource);
                assembly.Write(FileName);

                if (move)
                {
                    File.Delete(resFileName);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool RemoveAssemblyResource(string resourceName, bool export)
        {
            try
            {
                var resource = assembly.MainModule.Resources.FirstOrDefault(r => r.Name == resourceName) as EmbeddedResource;
                if (resource == null)
                {
                    return false;
                }
                if (export)
                {
                    var resourceFile = resourceName.Replace(assembly.Name.Name + ".", string.Empty);
                    resourceFile = Path.Combine(Path.GetDirectoryName(FileName) ?? string.Empty, resourceFile);
                    File.WriteAllBytes(resourceFile, resource.GetResourceData());
                }

                assembly.MainModule.Resources.Remove(resource);
                assembly.Write(FileName);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static MethodDefinition CopyMethod(MethodDefinition templateMethod, TypeDefinition targetType, object optParam)
        {
            var targetModule = targetType.Module;

            var newMethod = new MethodDefinition(templateMethod.Name, templateMethod.Attributes, targetModule.Import(templateMethod.ReturnType));
            targetType.Methods.Add(newMethod);

            foreach (var parameterDefinition in templateMethod.Parameters)
            {
                newMethod.Parameters.Add(new ParameterDefinition(targetModule.Import(parameterDefinition.ParameterType)));
            }

            if (templateMethod.Body != null)
            {
                foreach (var variableDefinition in templateMethod.Body.Variables)
                {
                    newMethod.Body.Variables.Add(new VariableDefinition(targetModule.Import(variableDefinition.VariableType)));
                }
                foreach (var instruction in templateMethod.Body.Instructions)
                {
                    var constructorInfo = typeof(Instruction).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                        new[] { typeof(OpCode), typeof(object) }, null);
                    var newInstruction = (Instruction)constructorInfo.Invoke(new[] { instruction.OpCode, instruction.Operand });
                    var fieldDefinition = newInstruction.Operand as FieldDefinition;
                    if (fieldDefinition != null)
                    {
                        targetModule.Import(fieldDefinition.FieldType);
                        newInstruction.Operand = targetType.Fields.FirstOrDefault(x => x.Name == fieldDefinition.Name);
                        if (newInstruction.Operand == null)
                            continue;
                    }

                    if (newInstruction.Operand is MethodReference)
                    {
                        //Try really hard to import type
                        var methodRef = (MethodReference) newInstruction.Operand;
                        if (methodRef.DeclaringType == templateMethod.DeclaringType)
                        {
                            methodRef = (MethodReference)optParam;
                        }
                        var methodReference = targetModule.Import(methodRef);
                        if ((methodReference.DeclaringType != null) && (methodReference.DeclaringType.DeclaringType != null))
                        {
                            methodReference.DeclaringType.DeclaringType = targetType;
                        }

                        newInstruction.Operand = methodReference;

                        if (!methodReference.MethodReturnType.ReturnType.IsGenericParameter)
                        {
                            targetModule.Import(methodReference.MethodReturnType.ReturnType.Resolve());
                        }
                        targetModule.Import(methodReference.DeclaringType);

                    }
                    if (newInstruction.Operand is TypeReference)
                    {
                        newInstruction.Operand = targetModule.Import(newInstruction.Operand as TypeReference);
                    }
                    newMethod.Body.Instructions.Add(newInstruction);
                }
            }

            return newMethod;
        }

        public bool AddAssemblyResolver()
        {
            try
            {
                var srcName = Assembly.GetExecutingAssembly().Location;
                var srcAssembly = AssemblyDefinition.ReadAssembly(srcName);
                var srcType = srcAssembly.MainModule.Types.FirstOrDefault(t => t.Name == "MonolithAssemblyResolver");
                if (srcType == null)
                {
                    return false;
                }

                var typeDef = new TypeDefinition(assembly.Name.Name, srcType.Name, srcType.Attributes);
                assembly.MainModule.Types.Insert(0, typeDef.Resolve());

                var debugMethod = srcType.Methods.FirstOrDefault(m => m.Name == "OutputDebugString");
                var resolveMethod = srcType.Methods.FirstOrDefault(m => m.Name == "CurrentDomainOnAssemblyResolve");
                var initMethod = srcType.Methods.FirstOrDefault(m => m.Name == "Init");

                if ((initMethod == null) || (resolveMethod == null))
                {
                    return false;
                }

                MethodDefinition dm = null;
                if (debugMethod != null)
                {
                    dm = CopyMethod(debugMethod, typeDef, null);
                }
                var rm = CopyMethod(resolveMethod, typeDef, dm);
                var im = CopyMethod(initMethod, typeDef, rm);

                //var ilProcessor = assembly.MainModule.EntryPoint.Body.GetILProcessor();
                //assembly.MainModule.EntryPoint.Body.Instructions.Insert(0, ilProcessor.Create(OpCodes.Nop));
                //assembly.MainModule.EntryPoint.Body.Instructions.Insert(1, ilProcessor.Create(OpCodes.Call, im));

                assembly.MainModule.Import(typeDef.Resolve());

                var ilProcessor =  im.Body.GetILProcessor();
                var ret = im.Body.Instructions.First(op => op.OpCode == OpCodes.Ret);
                var ix = im.Body.Instructions.IndexOf(ret);

                im.Body.Instructions.Insert(ix++, ilProcessor.Create(OpCodes.Nop));
                im.Body.Instructions.Insert(ix, ilProcessor.Create(OpCodes.Call, assembly.EntryPoint));
                
                assembly.EntryPoint = im;

                assembly.Write(FileName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
            return false;
        }

    }
}