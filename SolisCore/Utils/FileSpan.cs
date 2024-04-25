using System;
using System.Collections.Generic;
using System.Text;

namespace SolisCore.Utils
{
    public readonly struct FileSpan
    {
        private readonly FileInfo File;
        public readonly int ByteOffset;
        public readonly int ByteLength;
        public string Serialized => ToString();

        public FileSpan(FileInfo file, int byteOffset, int byteLength)
        {
            File = file;
            ByteOffset = byteOffset;
            ByteLength = byteLength;
        }

        public override string ToString()
        {
            // TODO: multi-byte chars (maybe???)
            var (line, column) = File.GetLineNumber(ByteOffset);
            return $"{File.Name}({ByteOffset}:{ByteOffset + ByteLength}) at Line {line}:{column}";
        }
    }
}
