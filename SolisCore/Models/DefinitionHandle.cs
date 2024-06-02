namespace SolisCore.Models
{
    /// <summary>
    /// A very common structure in compilers.  This is basically just an "int"
    /// that refers to a definition in a table.  This is used when we build our 
    /// 
    /// By default the limit of 4 million objects should be plenty.
    /// 
    /// We use a type for:
    /// 1. We can't mix handles, you can't use a method handle in a parameter one.
    /// 2. 
    /// </summary>
    public struct DefinitionHandle<T>
    {
        public int Value { get; set; }

        public DefinitionHandle(int value)
        {
            Value = value;
        }
    }
}
