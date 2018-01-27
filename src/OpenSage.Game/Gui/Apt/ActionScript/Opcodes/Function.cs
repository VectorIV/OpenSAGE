﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    public static class FunctionCommon
    {
        public static void ExecuteFunction(string funcName,List<Value> args, ObjectContext scope, ActionContext context)
        {
            if(scope==null)
            {
                Debug.WriteLine("[ERROR] cannot execute function \"" + funcName + "\" on null object");
                return;
            }

            if (scope.IsBuiltInFunction(funcName))
            {
                scope.CallBuiltInFunction(funcName, args, context);
            }
            else
            {
                var func = scope.GetMember(funcName).ToFunction();
                var vm = context.Apt.ActionScriptVM;
                vm.Execute(func, args, scope);
            }
        }
    }

    /// <summary>
    /// Declare a new named or anonymous function (depending on function name) that will either be
    /// pushed to stack or set as a variable. 
    /// </summary>
    public sealed class DefineFunction : InstructionBase
    {
        public override InstructionType Type => InstructionType.DefineFunction;
        public override uint Size => 20;

        public override void Execute(ActionContext context)
        {
            var name = Parameters[0].ToString();
            var nParams = Parameters[1].ToInteger();
            var size = Parameters[2 + nParams].ToInteger();

            //create a list of parameters
            var paramList = Parameters
                .Skip(2)
                .Take(nParams)
                .ToList();

            //get all the instructions
            var code = context.Stream.GetInstructions(size);

            var func = new Function() { Parameters = paramList, Instructions = code };
            var funcVal = Value.FromFunction(func);

            if (name.Length > 0)
                context.Scope.Variables[name] = funcVal;
            //anonymous function/lambda function
            else
                context.Stack.Push(funcVal);
        }
    }

    /// <summary>
    /// Return out of the current function back to the calling point
    /// </summary>
    public sealed class Return : InstructionBase
    {
        public override InstructionType Type => InstructionType.Return;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call an anonymous method that is on the stack. Function arguments are also popped from the stack
    /// </summary>
    public sealed class CallMethodPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallMethodPop;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call a named method that is in the current scope. Function arguments are popped from the stack
    /// </summary>
    public sealed class CallNamedMethodPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedMethodPop;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var id = Parameters[0].ToInteger();
            var funcName = context.Scope.Constants[id].ToString();        
            var obj = context.Stack.Pop().ResolveRegister(context).ToObject();
            var argCount = context.Stack.Pop().ToInteger();

            var args = new List<Value>();
            for (int i = 0; i < argCount; ++i)
            {
                args.Add(context.Stack.Pop());
            }

            FunctionCommon.ExecuteFunction(funcName, args, obj, context);
        }
    }

    /// <summary>
    /// Call a named method that is in the current scope. Has no arguments
    /// </summary>
    public sealed class CallNamedFunc : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedFunc;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var id = Parameters[0].ToInteger();
            var funcName = context.Scope.Constants[id].ToString();

            var args = new List<Value>();

            FunctionCommon.ExecuteFunction(funcName, args, context.Scope, context);
        }
    }

    /// <summary>
    /// Call a named method that is in the current scope. Function arguments are popped from the stack
    /// </summary>
    public sealed class CallNamedFuncPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedFuncPop;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var id = Parameters[0].ToInteger();
            var funcName = context.Scope.Constants[id].ToString();
            var argCount = context.Stack.Pop().ToInteger();

            var args = new List<Value>();
            for (int i = 0; i < argCount; ++i)
            {
                args.Add(context.Stack.Pop());
            }

            FunctionCommon.ExecuteFunction(funcName, args, context.Scope, context);
        }
    }

    /// <summary>
    /// Call a function that is defined in the current scope
    /// </summary>
    public sealed class CallFunction : InstructionBase
    {
        public override InstructionType Type => InstructionType.CallFunction;
        public override uint Size => 0;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call a function that is defined in the current scope
    /// </summary>
    public sealed class CallFunc : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallFunc;
        public override uint Size => 0;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
