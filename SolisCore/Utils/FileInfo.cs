using System.Linq;

namespace SolisCore.Utils
{
    public class FileInfo
    {
        public string Name { get; }
        
        /// <summary>
        /// TODO: We should probably "clear this out" when we finish lexing
        ///       since we don't need it anymore, and can re-fetch from file using scanning
        ///       keeping it to be simple.
        /// </summary>
        private string Contents { get; }
        
        public FileInfo(string name, string contents)
        {
            Name = name;
            Contents = contents;
        }

        public (int line, int column) GetLineNumber(int byteOffset)
        {
            // TODO: I'm lazy and this is horribly inefficient
            //       a cheap way would be to cache line numbers at certain byte offsets
            //       so we can binary search our byte offset across to find the closest line number
            //       then scan from there
            var line = Contents[..byteOffset].Count((c) => c == '\n');
            var lineOffset = Contents[..byteOffset].LastIndexOf('\n');
            var column = lineOffset >= 0 ? byteOffset - lineOffset : byteOffset;
            // we start line & columns at 1 not 0
            return (line + 1, column + 1);
        }
    }
}
