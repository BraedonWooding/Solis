using System;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using SolisCore.Parser;
using System.Reflection.Metadata;
using SolisCore.Typechecking;
using System.Collections.Generic;

namespace SolisCore.Transpilers
{
    public readonly struct ILAssemblyState
    {
        private readonly PooledBlobBuilder _methodBuilder;
        internal readonly InstructionEncoder _il;
        internal readonly PooledBlobBuilder _currentMethodSignature;

        internal ILAssemblyState(PooledBlobBuilder methodBuilder, ControlFlowBuilder flowBuilder, PooledBlobBuilder currentMethodSignature)
        {
            _methodBuilder = methodBuilder;
            _il = new(methodBuilder, flowBuilder);
            _currentMethodSignature = currentMethodSignature;
        }

        public void Free()
        {
            _methodBuilder.Free();
        }

        internal static ILAssemblyState Create()
        {
            // Pooling this isn't probably useful, because List.Clear() is likely pretty expensive
            // it's O(N) since it needs to null out all the elements of the list.
            // it's likely cheaper to get the GC to just handle the unreachability.
            // So not going to that effort for now, if this becomes a GC/perf issue we can deal with it then
            ControlFlowBuilder cfb = new();
            return new ILAssemblyState(PooledBlobBuilder.GetInstance(), cfb, PooledBlobBuilder.GetInstance());
        }
    }

    public 

    /// <summary>
    /// This uses <see cref="MetadataBuilder"/> this is much much faster than <see cref="ILDynamicAssemblyTranspiler"/>
    /// </summary>
    public class ILAssemblyTranspiler : Transpiler<ILAssemblyState>
    {
        private readonly MetadataBuilder _metadataBuilder;
        private Dictionary<>

        /// <summary>
        /// Stores the entire translation unit.
        /// </summary>
        /// <remarks>
        /// Doesn't use a pooled instance since it contains the entire translation unit
        /// so it's not going to be recycled until we produce a dll/image.
        /// 
        /// This means that it's likely to use up the entire pool and negatively effect
        /// the application.
        /// 
        /// What we *might* do is have a pooled instance for a translation level
        /// so that we can produce multiple dlls re-using memory.
        /// 
        /// This might be useful for use-cases like mods where you might want
        /// each mod to live in it's own translation unit.
        /// </remarks>
        private readonly BlobBuilder _translationUnit;
        private readonly MethodBodyStreamEncoder _methodBodyStream;

        public ILAssemblyTranspiler(string translationUnitName)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.reflection.metadata.ecma335.metadatabuilder?view=net-8.0
            _metadataBuilder = new MetadataBuilder();
            _translationUnit = new BlobBuilder();
            _methodBodyStream = new MethodBodyStreamEncoder(_translationUnit);
        }

        public void ProcessType(SignatureTypeEncoder encoder, TypeAst type)
        {
            // TODO: We probably want to have a better way of handling this
            switch (type.Identifier.SourceValue)
            {
                case "int":
                    encoder.Int32();
                    break;
                case "float":
                    encoder.Double();
                    break;
                case "string":
                    encoder.String();
                    break;
                case "char":
                    encoder.Char();
                    break;
                case "bool":
                    encoder.Boolean();
                    break;
                default:
                    throw new NotImplementedException(type.Identifier.SourceValue);
            }
        }

        protected override void HandleFunction(FunctionDeclaration decl)
        {
            var state = ILAssemblyState.Create();

            new BlobEncoder(state._currentMethodSignature)
                .MethodSignature()
                .Parameters(decl.Args.Count, returnType =>
                {
                    if (decl.ReturnType == null || decl.ReturnType.IsVoid())
                    {
                        returnType.Void();
                    }
                    else
                    {
                        ProcessType(returnType.Type(), decl.ReturnType);
                    }
                }, parameters =>
                {
                    foreach (var arg in decl.Args)
                    {
                        // TODO: What happens if TypeAnnotation is null?
                        ProcessType(parameters.AddParameter().Type(), arg.TypeAnnotation);
                    }
                });

            base.Transpile(state, decl);

            _metadataBuilder.AddMemberReference()

            int mainBodyOffset = _methodBodyStream.AddMethodBody(state._il);

            MethodDefinitionHandle mainMethodDef = _metadataBuilder.AddMethodDefinition(
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                MethodImplAttributes.IL,
                _metadataBuilder.GetOrAddString(decl.Identifier.Value.SourceValue),
                _metadataBuilder.GetOrAddBlob(state._currentMethodSignature),
                mainBodyOffset,
                parameterList: default);

            state.Free();
        }
    }
}
