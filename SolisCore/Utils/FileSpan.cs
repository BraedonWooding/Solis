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

        public override string ToString()
        {
            // TODO: fix this, this should ideally open the file and calculate line numbers
            //       for performance we should probably not scan the file each time and instead
            //       as part of lexing just track every X lines so that we can just scan over a small subset of the lines
            // we still have to scan because of multi-byte chars (maybe???)
            return $"{File} {ByteOffset}:{ByteOffset + ByteLength}";
        }
    }
}
