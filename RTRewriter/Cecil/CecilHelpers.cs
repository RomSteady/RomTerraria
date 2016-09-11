using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.Diagnostics;

namespace RTRewriter.Cecil
{
    class CecilHelpers
    {
        public static TypeDefinition FindTypeInAssembly(AssemblyDefinition assembly, string typeName)
        {
            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.GetTypes())
                {
                    if (type.FullName == typeName)
                    {
                        return type;
                    }
                }
            }

            throw new KeyNotFoundException(String.Format("Type '{0}' not found.", typeName));
        }

        public static MethodDefinition FindMethodInAssembly(AssemblyDefinition assembly, string methodName)
        {
            Debug.WriteLine(String.Format("Looking for {0}", methodName), "CecilHelpers");
            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        Debug.WriteLine(method.FullName);
                        if (method.FullName == methodName)
                        {
                            return method;
                        }
                    }
                }
            }
            throw new KeyNotFoundException(String.Format("Method '{0}' not found.", methodName));
        }

        public static FieldDefinition FindFieldInType(TypeDefinition type, string fieldName)
        {
            foreach (var field in type.Fields)
            {
                if (field.Name == fieldName)
                {
                    return field;
                }
            }
            throw new KeyNotFoundException(String.Format("Field '{0}' not found.", fieldName));
        }

        public static void ChangeDefaultInt32Value(MethodDefinition method, string fieldName, int newValue)
        {
            Debug.WriteLine(String.Format("Changing the default value for {0} to {1}", fieldName, newValue), "CecilHelpers");
            var il = method.Body.GetILProcessor();
            foreach (var instruction in il.Body.Instructions)
            {
                if (instruction.OpCode == Mono.Cecil.Cil.OpCodes.Stsfld)
                {
                    var field = (FieldDefinition)instruction.Operand;
                    Debug.WriteLine(String.Format("Looking at store to {0}", field.FullName), "CecilHelpers");
                    if (field.FullName == fieldName)
                    {
                        var previnst = instruction.Previous;
                        if (previnst.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4)
                        {
                            previnst.Operand = newValue;
                            return;
                        }
                    }
                }
            }
            throw new KeyNotFoundException(String.Format("Default value not found for '{0}'.", fieldName));
        }

        public static void ChangeDefaultBooleanValue(MethodDefinition method, string fieldName, bool newValue)
        {
            var il = method.Body.GetILProcessor();
            foreach (var instruction in il.Body.Instructions)
            {
                if (instruction.OpCode == Mono.Cecil.Cil.OpCodes.Stsfld ||
                    instruction.OpCode == Mono.Cecil.Cil.OpCodes.Stfld)
                {
                    var field = (FieldDefinition)instruction.Operand;
                    if (field.FullName == fieldName)
                    {
                        var previnst = instruction.Previous;
                        if (previnst.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_1 ||
                            previnst.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_0)
                        {
                            previnst.OpCode = newValue ? Mono.Cecil.Cil.OpCodes.Ldc_I4_1 : Mono.Cecil.Cil.OpCodes.Ldc_I4_0;
                            return;
                        }
                    }
                }
            }
            throw new KeyNotFoundException(String.Format("Default value not found for '{0}'.", fieldName));
        }

        public static void TurnMethodToNoOp(AssemblyDefinition assembly, string methodName)
        {
            var method = CecilHelpers.FindMethodInAssembly(assembly, methodName);
            var processor = method.Body.GetILProcessor();
            var firstInstruction = method.Body.Instructions[0];
            var newInstruction = processor.Create(Mono.Cecil.Cil.OpCodes.Ret);
            processor.InsertBefore(firstInstruction, newInstruction);
        }

        public static void ReplaceStringInMethod(MethodDefinition method, string oldString, string newString)
        {
            for (int i = 0; i < method.Body.Instructions.Count; i++)
            {
                if (method.Body.Instructions[i].OpCode == Mono.Cecil.Cil.OpCodes.Ldstr &&
                    method.Body.Instructions[i].Operand.ToString() == oldString)
                {
                    method.Body.Instructions[i].Operand = newString;
                }
            }
        }
    }
}
