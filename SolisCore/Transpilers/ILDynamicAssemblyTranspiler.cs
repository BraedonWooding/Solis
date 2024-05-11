using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SolisCore.Transpilers
{
    /// <summary>
    /// This outputs a dynamic assembly that can be loaded.
    /// 
    /// In .netcore (<9) you can't save this assembly, but in .netframework
    /// or .net9 you can save this to disk!  Though it's much slower than the equivalent ILAssemblyTranspiler.
    /// </summary>
    public class ILDynamicAssemblyTranspiler : Transpiler
    {
        public ILDynamicAssemblyTranspiler(string translationUnitName)
        {
            // https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-reflection-emit-assemblybuilder
            AssemblyBuilder myAsmBuilder = AssemblyBuilder.DefineDynamicAssembly(
                           new AssemblyName(translationUnitName),
                           AssemblyBuilderAccess.RunAndCollect);

            ModuleBuilder pointModule = myAsmBuilder.DefineDynamicModule("Main");

            TypeBuilder pointTypeBld = pointModule.DefineType("Point",
                                       TypeAttributes.Public);
        }
    }
}
