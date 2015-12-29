// http://www.codeproject.com/Questions/531409/fileplusisplususedplusbyplusanotherplusprocessplus

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Decchi.Utilities
{
    public static class DetectOpenedFile
    {
        private static readonly bool IsX64 = (IntPtr.Size == 8);
        private static readonly string[] exts = { ".mp3", ".ogg", ".wav", ".aiff", ".asf", ".flac", ".mpc", ".wav", ".tak" };

        public static string GetOpenedFile(IntPtr hwnd)
        {
            int pid;
            if (NativeMethods.GetWindowThreadProcessId(hwnd, out pid) == 0 && pid != 0) return null;

            var handles = GetOpenedFiles(pid);
            int i, j;

            string path;
            string ext;

            for (i = 0; i < handles.Length; ++i)
            {
                path = GetFilePath(handles[i], pid);
                if (string.IsNullOrEmpty(path)) continue;

                ext = Path.GetExtension(path);
                for (j = 0; j < exts.Length; ++j)
                    if (ext == exts[j])
                        return path;
            }

            return null;
        }

        private static NativeMethods.SYSTEM_HANDLE_INFORMATION[] GetOpenedFiles(int pid)
        {
            var nHandleInfoSize = 0x10000;
            var ipHandlePointer = IntPtr.Zero;

            var lstHandles = new List<NativeMethods.SYSTEM_HANDLE_INFORMATION>();

            try
            {
                ipHandlePointer = Marshal.AllocHGlobal(nHandleInfoSize);
                var nLength = 0;
                IntPtr ipHandle;

                // CNST_SYSTEM_HANDLE_INFORMATION = 16;
                while (NativeMethods.NtQuerySystemInformation(16, ipHandlePointer, nHandleInfoSize, ref nLength) == NativeMethods.STATUS_INFO_LENGTH_MISMATCH)
                {
                    nHandleInfoSize = nLength;
                    Marshal.FreeHGlobal(ipHandlePointer);
                    ipHandlePointer = Marshal.AllocHGlobal(nLength);
                }

                long lHandleCount;
                if (IsX64)
                {
                    lHandleCount = Marshal.ReadInt64(ipHandlePointer);
                    ipHandle = IntPtr.Add(ipHandlePointer, 8);
                }
                else
                {
                    lHandleCount = Marshal.ReadInt32(ipHandlePointer);
                    ipHandle = IntPtr.Add(ipHandlePointer, 4);
                }

                for (long lIndex = 0; lIndex < lHandleCount; lIndex++)
                {
                    var shHandle = new NativeMethods.SYSTEM_HANDLE_INFORMATION();
                    if (IsX64)
                    {
                        shHandle = (NativeMethods.SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ipHandle, typeof(NativeMethods.SYSTEM_HANDLE_INFORMATION));
                        ipHandle = IntPtr.Add(ipHandle, Marshal.SizeOf(shHandle) + 8);
                    }
                    else
                    {
                        ipHandle = IntPtr.Add(ipHandle, Marshal.SizeOf(shHandle) + 0);
                        shHandle = (NativeMethods.SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ipHandle, typeof(NativeMethods.SYSTEM_HANDLE_INFORMATION));
                    }
                    if (shHandle.ProcessID != pid) continue;
                    lstHandles.Add(shHandle);
                }

            }
            catch
            { }
            finally
            {
                if (ipHandlePointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(ipHandlePointer);
            }

            return lstHandles.ToArray();
        }

        private static string GetFilePath(NativeMethods.SYSTEM_HANDLE_INFORMATION systemHandleInformation, int pid)
        {
            var ipProcessHwnd = NativeMethods.OpenProcess(NativeMethods.ProcessAccessFlags.All, false, pid);
            var objBasic      = new NativeMethods.OBJECT_BASIC_INFORMATION();
            var objObjectType = new NativeMethods.OBJECT_TYPE_INFORMATION();
            var objObjectName = new NativeMethods.OBJECT_NAME_INFORMATION();
            var strObjectName = "";
            var nLength = 0;
            IntPtr ipTemp, ipHandle;

            if (!NativeMethods.DuplicateHandle(ipProcessHwnd, systemHandleInformation.Handle, NativeMethods.GetCurrentProcess(), out ipHandle, 0, false, NativeMethods.DUPLICATE_SAME_ACCESS))
                return null;
            
            var ipBasic = Marshal.AllocHGlobal(Marshal.SizeOf(objBasic));
            NativeMethods.NtQueryObject(ipHandle, NativeMethods.ObjectInformationClass.ObjectBasicInformation, ipBasic, Marshal.SizeOf(objBasic), ref nLength);
            objBasic = (NativeMethods.OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(ipBasic, objBasic.GetType());
            Marshal.FreeHGlobal(ipBasic);

            var ipObjectType = Marshal.AllocHGlobal(objBasic.TypeInformationLength);
            nLength = objBasic.TypeInformationLength;
            while (NativeMethods.NtQueryObject(ipHandle, NativeMethods.ObjectInformationClass.ObjectTypeInformation, ipObjectType, nLength, ref nLength) == NativeMethods.STATUS_INFO_LENGTH_MISMATCH)
            {
                Marshal.FreeHGlobal(ipObjectType);
                if (nLength == 0) return null;
                ipObjectType = Marshal.AllocHGlobal(nLength);
            }
            objObjectType = (NativeMethods.OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(ipObjectType, typeof(NativeMethods.OBJECT_TYPE_INFORMATION));

            ipTemp = IsX64 ? new IntPtr(objObjectType.Name.Buffer.ToInt64() >> 32) : objObjectType.Name.Buffer;

            var strObjectTypeName = Marshal.PtrToStringUni(ipTemp, objObjectType.Name.Length >> 1);
            Marshal.FreeHGlobal(ipObjectType);

            if (strObjectTypeName != "File")
                return null;

            nLength = objBasic.NameInformationLength;

            var ipObjectName = Marshal.AllocHGlobal(nLength);
            while (NativeMethods.NtQueryObject(ipHandle,NativeMethods.ObjectInformationClass.ObjectNameInformation, ipObjectName, nLength, ref nLength) == NativeMethods.STATUS_INFO_LENGTH_MISMATCH)
            {
                Marshal.FreeHGlobal(ipObjectName);
                if (nLength == 0) return null;
                ipObjectName = Marshal.AllocHGlobal(nLength);
            }
            objObjectName = (NativeMethods.OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(ipObjectName, typeof(NativeMethods.OBJECT_NAME_INFORMATION));

            ipTemp = IsX64 ? new IntPtr(objObjectName.Name.Buffer.ToInt64() >> 32) : objObjectName.Name.Buffer;
            if (ipTemp != IntPtr.Zero)
            {
                var baTemp = new byte[nLength];
                try
                {
                    Marshal.Copy(ipTemp, baTemp, 0, nLength);

                    strObjectName = Marshal.PtrToStringUni(ipTemp);
                }
                catch (AccessViolationException)
                {
                    return null;
                }
                finally
                {
                    NativeMethods.CloseHandle(ipHandle);
                    Marshal.FreeHGlobal(ipObjectName);
                }
            }
            else
            {
                NativeMethods.CloseHandle(ipHandle);
                Marshal.FreeHGlobal(ipObjectName);
            }

            try
            {
                return GetRegularFileNameFromDevice(strObjectName);
            }
            catch
            {
                return null;
            }
        }

        private static string GetRegularFileNameFromDevice(string strRawName)
        {
            string strFileName = strRawName;

            var drives = Environment.GetLogicalDrives();
            for (int i = 0; i < drives.Length; ++i)
            {
                var sbTargetPath = new StringBuilder(NativeMethods.MAX_PATH);
                if (NativeMethods.QueryDosDevice(drives[i].Substring(0, 2), sbTargetPath, NativeMethods.MAX_PATH) == 0)
                    return strRawName;

                string strTargetPath = sbTargetPath.ToString();
                if (strFileName.StartsWith(strTargetPath))
                {
                    strFileName = strFileName.Replace(strTargetPath, drives[i].Substring(0, 2));
                    break;
                }
            }

            return strFileName;
        }
    }
}
