// http://www.codeproject.com/Questions/531409/fileplusisplususedplusbyplusanotherplusprocessplus

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Decchi.Utilities.InteropServices;

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

            var ipProcessHwnd = NativeMethods.OpenProcess(NativeMethods.ProcessAccessFlags.All, false, pid);

            var handles = GetOpenedFiles(pid);
            int i, j;

            string path;
            string ext;

            try
            {
                for (i = 0; i < handles.Length; ++i)
                {
                    path = GetFilePath(handles[i], ipProcessHwnd);
                    if (string.IsNullOrEmpty(path)) continue;

                    ext = Path.GetExtension(path);
                    for (j = 0; j < exts.Length; ++j)
                        if (ext == exts[j])
                            return path;
                }
            }
            catch
            {
            }
            finally
            {
                NativeMethods.CloseHandle(ipProcessHwnd);
            }

            return null;
        }

        private static NativeMethods.SYSTEM_HANDLE_INFORMATION[] GetOpenedFiles(int pid)
        {
            var nHandleInfoSize = 0x10000;
            
            try
            {
                using (var ipHandlePointer = new UnmanagedMemory(nHandleInfoSize))
                {
                    var nLength = 0;
                    IntPtr ipHandle;

                    // CNST_SYSTEM_HANDLE_INFORMATION = 16;
                    while (NativeMethods.NtQuerySystemInformation(16, ipHandlePointer, nHandleInfoSize, ref nLength) == NativeMethods.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        nHandleInfoSize = nLength;
                        ipHandlePointer.Reallocate(nLength);
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

                    var lstHandles = new List<NativeMethods.SYSTEM_HANDLE_INFORMATION>();
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

                    return lstHandles.ToArray();
                }
            }
            catch
            { }

            return new NativeMethods.SYSTEM_HANDLE_INFORMATION[0];
        }

        private static string GetFilePath(NativeMethods.SYSTEM_HANDLE_INFORMATION systemHandleInformation, IntPtr ipProcessHwnd)
        {
            var objBasic      = new NativeMethods.OBJECT_BASIC_INFORMATION();
            var objObjectType = new NativeMethods.OBJECT_TYPE_INFORMATION();
            var objObjectName = new NativeMethods.OBJECT_NAME_INFORMATION();
            var strObjectName = "";
            var nLength = 0;
            IntPtr ipTemp, ipHandle = IntPtr.Zero;

            try
            {
                if (!NativeMethods.DuplicateHandle(ipProcessHwnd, systemHandleInformation.Handle, NativeMethods.GetCurrentProcess(), out ipHandle, 0, false, NativeMethods.DUPLICATE_SAME_ACCESS))
                    return null;

                using (var ipBasic = new UnmanagedStruct<NativeMethods.OBJECT_BASIC_INFORMATION>())
                {
                    NativeMethods.NtQueryObject(ipHandle, NativeMethods.ObjectInformationClass.ObjectBasicInformation, ipBasic, ipBasic.Length, ref nLength);
                    objBasic = ipBasic.PtrToStructure();
                }

                //////////////////////////////////////////////////
                using (var ipObjectType = new UnmanagedMemory(objBasic.TypeInformationLength))
                {
                    nLength = objBasic.TypeInformationLength;
                    while (NativeMethods.NtQueryObject(ipHandle, NativeMethods.ObjectInformationClass.ObjectTypeInformation, ipObjectType, nLength, ref nLength) == NativeMethods.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        if (nLength == 0) return null;
                        ipObjectType.Reallocate(nLength);
                    }
                    objObjectType = (NativeMethods.OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(ipObjectType, typeof(NativeMethods.OBJECT_TYPE_INFORMATION));

                    ipTemp = IsX64 ? new IntPtr(objObjectType.Name.Buffer.ToInt64() >> 32) : objObjectType.Name.Buffer;

                    if (Marshal.PtrToStringUni(ipTemp, objObjectType.Name.Length >> 1) != "File") return null;
                }

                //////////////////////////////////////////////////

                nLength = objBasic.NameInformationLength;

                using (var ipObjectName = new UnmanagedMemory(nLength))
                {
                    while (NativeMethods.NtQueryObject(ipHandle, NativeMethods.ObjectInformationClass.ObjectNameInformation, ipObjectName, nLength, ref nLength) == NativeMethods.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        if (nLength == 0) return null;
                        ipObjectName.Reallocate(nLength);
                    }
                    objObjectName = (NativeMethods.OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(ipObjectName, typeof(NativeMethods.OBJECT_NAME_INFORMATION));

                    ipTemp = IsX64 ? new IntPtr(objObjectName.Name.Buffer.ToInt64() >> 32) : objObjectName.Name.Buffer;
                    if (ipTemp != IntPtr.Zero)
                    {
                        strObjectName = Marshal.PtrToStringUni(ipTemp);

                        return GetRegularFileNameFromDevice(strObjectName);
                    }
                }
            }
            catch
            { }
            finally
            {
                if (ipHandle != IntPtr.Zero)
                    NativeMethods.CloseHandle(ipHandle);
            }

            return null;
        }

        private static string GetRegularFileNameFromDevice(string strRawName)
        {
            var sbTargetPath = new StringBuilder(NativeMethods.MAX_PATH);
            string strFileName = strRawName;
            string strTargetPath;

            var drives = Environment.GetLogicalDrives();
            for (int i = 0; i < drives.Length; ++i)
            {
                sbTargetPath.Remove(0, sbTargetPath.Length - 1);
                if (NativeMethods.QueryDosDevice(drives[i].Substring(0, 2), sbTargetPath, NativeMethods.MAX_PATH) == 0)
                    return strRawName;

                strTargetPath = sbTargetPath.ToString();
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
