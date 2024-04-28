using Argon;
using SolisCore.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class CustomContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type type)
        {
            if (type.IsAssignableTo(typeof(IOperatorExpression)))
            {
                return CreateObjectContract(type);
            }
            else
            {
                return base.CreateContract(type);
            }
        }
    }

    public static class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Init()
        {
            VerifierSettings.DontIgnoreEmptyCollections();
            VerifierSettings.AddExtraSettings((settings) =>
            {
                settings.ContractResolver = new CustomContractResolver();
            });
        }
    }
}
