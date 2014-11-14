// derived from MIT licensed ReSharper.Scout.DebugSymbols
// https://code.google.com/p/scoutplugin/source/browse/trunk/src/DebugSymbols/SrcSrv.cs?r=23

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SourceLink.SymbolStore
{
    [
        ComImport,
        Guid("997DD0CC-A76F-4c82-8D79-EA87559D27AD"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        ComVisible(false)
    ]
    public interface ISymUnmanagedSourceServerModule
    {
        [PreserveSig]
        int GetSourceServerData(out uint dataByteCount, out IntPtr data);
    };

    public static class SrcSrv
    {
        public static void Init(IntPtr sessionCookie, string symbolCacheDir)
        {
            SrcSrvSetOptions(1);
            SrcSrvInit(sessionCookie, symbolCacheDir);
        }

        public static long LoadModule(IntPtr sessionCookie, string moduleFilePath, ISymUnmanagedSourceServerModule sourceServerModule)
        {
            if (moduleFilePath == null) throw new ArgumentNullException("moduleFilePath");
            if (sourceServerModule == null) throw new ArgumentNullException("sourceServerModule");

            long moduleCookie = ((long)moduleFilePath.ToLower().GetHashCode()) << 30;

            if (SrcSrvIsModuleLoaded(sessionCookie, moduleCookie))
            {
                // Already loaded.
                //
                return moduleCookie;
            }

            IntPtr data;
            uint dataByteCount;

            if (sourceServerModule.GetSourceServerData(out dataByteCount, out data) < 0)
            {
                // VS2005 fails on .pdb files produced by Phoenix compiler.
                // https://connect.microsoft.com/Phoenix/
                //
                return 0L;
            }

            try
            {
                return SrcSrvLoadModule(sessionCookie, Path.GetFileName(moduleFilePath),
                    moduleCookie, data, dataByteCount) ? moduleCookie : 0L;
            }
            finally
            {
                Marshal.FreeCoTaskMem(data);
            }
        }

        public static string GetFileUrl(IntPtr sessionCookie, long moduleCookie, string sourceFilePath)
        {
            if (sourceFilePath == null) throw new ArgumentNullException("sourceFilePath");

            StringBuilder url = new StringBuilder(2048);

            return SrcSrvGetFile(sessionCookie, moduleCookie, sourceFilePath,
                null, url, (uint)url.Capacity) ? url.ToString() : null;
        }

        private const string module = "srcsrv.dll";

        [DllImport(module, SetLastError = true)]
        public static extern uint SrcSrvSetOptions(uint options);

        [DllImport(module, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SrcSrvSetParentWindow(IntPtr wnd);

        [DllImport(module, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SrcSrvInit(IntPtr sessionCookie, string symbolCacheDir);

        public delegate bool SrcSrvCallbackProc(uint @event, long param1, long param2);

        [DllImport(module, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SrcSrvRegisterCallback(IntPtr sessionCookie, SrcSrvCallbackProc callback, long moduleCookie);

        [DllImport(module, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SrcSrvIsModuleLoaded(IntPtr sessionCookie, long moduleCookie);

        [DllImport(module, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SrcSrvLoadModule(IntPtr sessionCookie, string moduleFileName, long moduleCookie, IntPtr symbolClob, uint clobLen);

        [DllImport(module, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SrcSrvGetFile(IntPtr sessionCookie, long moduleCookie, string sourceFileLocalPath, string optParams, StringBuilder buffer, uint bufferlen);

        [DllImport(module, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SrcSrvGetToken(IntPtr sessionCookie, long moduleCookie, string sourceFileName, out IntPtr tokenClob, out uint clobLen);

        [DllImport(module, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SrcSrvExecToken(IntPtr sessionCookie, IntPtr tokenOut, string optParams, StringBuilder buffer, uint bufferlen);
    }
}