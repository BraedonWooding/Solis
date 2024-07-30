using System;
using System.Collections.Generic;
using System.Text;

namespace SolisCore.Models
{
    public class SolisTypeParameter
    {
        public SolisTypeParameter(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        // TODO: Constraints??
    }

    public class SolisTypeDefinition
    {
        public SolisFlags Flags { get; set; }
        public string Name { get; set; }
        public List<SolisTypeParameter> TypeParameters { get; set; } = new();
    }

    public class SolisOpaqueTypeDefinition : SolisTypeDefinition
    {
        public SolisTypeDefinition BaseType { get; set; }
    }

    /// <summary>
    /// I hate the conflation/distinction of "function" and "method".
    /// 
    /// Typically in OOP method refers to a class "function".  There is very little
    /// meaningfull difference so having just one word would simplify a lot of confusion.
    /// 
    /// For the sake of consistency we will match it but preserving my objection.
    /// </summary>
    public class SolisMethodDefinition
    {
        public string Name { get; set; }
        public List<SolisFunctionParameter> Params { get; set; }
    }

    public class SolisFunctionParameter
    {
        public string Name { get; set; }

        /// <summary>
        /// If set this means that you need to refer to this parameter through this name externally.
        /// 
        /// For example GetColor(forPerson: "Bob") defined as GetColor(forPerson person: string)
        /// 
        /// Some parameter types (like bool) require the parameter name to be specified.
        /// </summary>
        public string? ExposedAlias { get; set; }

        public SolisTypeDefinition TypeDefinition { get; set; }
    }

    [Flags]
    public enum SolisFlags
    {
        None = 0,
        
        /// <summary>
        /// External means it's defined in a separate translation unit
        /// such as C#/C++/Rust/... bindings
        /// </summary>
        External = 1 << 0,

        /// <summary>
        /// A special case of external that means it is part of the standard lib.
        /// </summary>
        SystemLib = 1 << 1,

        /// <summary>
        /// Not linked to an instance
        /// </summary>
        Static = 1 << 2,

        /// <summary>
        /// Requires an instance
        /// </summary>
        Instance = 1 << 3,

        /// <summary>
        /// Internal type, has extra optimizations
        /// </summary>
        Primitive = 1 << 32,

        /// <summary>
        /// Copied by value
        /// </summary>
        Value = 1 << 33,

        /// <summary>
        /// Copied by reference
        /// </summary>
        Reference = 1 << 34,

        /// <summary>
        /// Is a copy of another type and means that it is a SolisOpaqueTypeDefinition.
        /// </summary>
        Opaque = 1 << 35,
    }
}
