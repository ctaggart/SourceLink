// derived from MIT licensed ReSharper.Scout.DebugSymbols
// https://code.google.com/p/scoutplugin/source/browse/trunk/src/DebugSymbols/SymSrv.cs?r=31

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SourceLink.SymbolStore
{
    public static class SymSrv
    {
        public static string DownloadFile(string url, string fileStorePath)
        {
            Uri uri = new Uri(url);

            // Hardcoded in vsdebug.dll, hardcoded here.
            fileStorePath = Path.Combine(fileStorePath, "src");

            // Convert /foo/bar/baz => foo\bar\baz
            string cacheFileName = Path.Combine(fileStorePath,
                uri.PathAndQuery.Substring(1).Replace('/', Path.DirectorySeparatorChar));

            // The file was already loaded.
            if (File.Exists(cacheFileName))
                return cacheFileName;

            // Mandatory for EULA dialog.
            //SymbolServerSetOptions(SSRVOPT_PARENTWIN, (long)ReSharper.VsShell.MainWindow.Handle);

            //#if DEBUG
            //            SymbolServerSetOptions(SSRVOPT_TRACE, 1);
            //#endif

            IntPtr fileHandle = IntPtr.Zero;
            try
            {
                IntPtr siteHandle;
                if (httpOpenFileHandle(uri.Scheme + "://" + uri.Host, uri.PathAndQuery, 0, out siteHandle, out fileHandle))
                {
                    // Force folder creation
                    //
                    string folderPath = Path.GetDirectoryName(cacheFileName);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    //bool  succeeded = ReSharper.ExecuteTask(
                    //    Path.GetFileName(cacheFileName), true,
                    //    delegate (IProgressIndicator progress)
                    //    {
                    //        progress.Start(1);
                    //        ulong totalRead = 0;

                    //        using (Stream fileStream = File.OpenWrite(cacheFileName))
                    //        {
                    //            byte[] buffer = new byte[8192];
                    //            uint read;
                    //            while (!progress.IsCanceled && httpReadFile(fileHandle, buffer, (uint)buffer.Length, out read) && read > 0)
                    //            {
                    //                totalRead += read;
                    //                progress.CurrentItemText = string.Format(Properties.Resources.SymSrv_DownloadProgress, totalRead);
                    //                fileStream.Write(buffer, 0, (int)read);
                    //            }
                    //        }

                    //        if (!progress.IsCanceled)
                    //            File.SetAttributes(cacheFileName, FileAttributes.ReadOnly);
                    //    });
                    //return succeeded? cacheFileName: null;

                    using (Stream fileStream = File.OpenWrite(cacheFileName))
                    {
                        byte[] buffer = new byte[8192];
                        uint read;
                        while (httpReadFile(fileHandle, buffer, (uint)buffer.Length, out read) && read > 0)
                        {
                            fileStream.Write(buffer, 0, (int)read);
                        }
                    }

                    return cacheFileName;
                }
                else
                {
                    //Logger.LogError("Failed to download url {0}: error {1}",
                    //    url, Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (fileHandle != IntPtr.Zero)
                    httpCloseHandle(fileHandle);
            }

            // Failed to download.
            //
            return null;
        }


        private const uint SSRVOPT_PARENTWIN = 0x000080;
        private const uint SSRVOPT_TRACE = 0x000400;

        private const string module = "symsrv.dll";

        public delegate bool SymSrvCallbackProc(uint @event, long param1, long param2);

        [DllImport(module, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymbolServerSetOptions(uint flag, long param);

        [DllImport(module, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool httpOpenFileHandle(string site, string file, int unused, out IntPtr siteHandle, out IntPtr fileHandle);

        [DllImport(module, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool httpReadFile(IntPtr hFile, byte[] buffer, uint dwNumberOfBytesToRead, out uint lpdwNumberOfBytesRead);

        [DllImport(module, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool httpCloseHandle(IntPtr handle);
    }
}