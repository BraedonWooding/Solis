using System;
using System.Collections.Generic;
using System.Text;

namespace SolisCore.Utils
{
    public readonly struct FileSpan
    {
        public readonly string File;
        public readonly int ByteOffset;
        public readonly int ByteLength;

        public FileSpan(string file, int byteOffset, int byteLength)
        {
            File = file;
            ByteOffset = byteOffset;
            ByteLength = byteLength;
        }
    }
}
