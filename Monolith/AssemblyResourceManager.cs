using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private void CopyMethod(MethodDefinition templateMethod, TypeDefinition targetType)
        {
            var targetModule = targetType.Module;

            var newMethod = new MethodDefinition(templateMethod.Name, templateMethod.Attributes, targetModule.Import(templateMethod.ReturnType));
            targetType.Methods.Add(newMethod);

            foreach (var variableDefinition in templateMethod.Body.Variables)
            {
                newMethod.Body.Variables.Add(new VariableDefinition(targetModule.Import(variableDefinition.VariableType)));


            }
            foreach (var parameterDefinition in templateMethod.Parameters)
            {
                newMethod.Parameters.Add(new ParameterDefinition(targetModule.Import(parameterDefinition.ParameterType)));
            }
            foreach (var instruction in templateMethod.Body.Instructions)
            {
                var constructorInfo = typeof(Instruction).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, 
                    new[] { typeof(OpCode), typeof(object) }, null);
                var newInstruction = (Instruction)constructorInfo.Invoke(new[] { instruction.OpCode, instruction.Operand});
                var fieldDefinition = newInstruction.Operand as FieldDefinition;
                if (fieldDefinition != null)
                {
                    targetModule.Import(fieldDefinition.FieldType);
                    newInstruction.Operand = targetType.Fields.FirstOrDefault(x => x.Name == fieldDefinition.Name);
                    if(newInstruction.Operand == null)
                        continue;
                }

                if (newInstruction.Operand is MethodReference)
                {
                    //Try really hard to import type
                    var methodReference = targetModule.Import((MethodReference)newInstruction.Operand);
                    if ((methodReference.DeclaringType != null) && (methodReference.DeclaringType.DeclaringType != null))
                    {
                        methodReference.DeclaringType.DeclaringType = targetType;
                    }

                    newInstruction.Operand = methodReference;

                    if (!methodReference.MethodReturnType.ReturnType.IsGenericParameter)
                    {
                        targetModule.Import(methodReference.MethodReturnType.ReturnType);
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
                assembly.MainModule.Types.Add(typeDef.Resolve());


                foreach (var method in srcType.Methods)
                {
                    CopyMethod(method, typeDef);
                }
                assembly.Write(FileName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return false;
        }

    }
}