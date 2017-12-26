﻿using System.IO;

namespace SourceLink
{
    public static class FileUtil
    {
        public static StreamWriter OpenWrite(string file) {
            return new StreamWriter(File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None));
        }
    }
}
