#region License and Copyright Notice

//OpenCL.Net: .NET bindings for OpenCL

//Copyright (c) 2010 Ananth B.
//All rights reserved.

//The contents of this file are made available under the terms of the
//Eclipse Public License v1.0 (the "License") which accompanies this
//distribution, and is available at the following URL:
//http://www.opensource.org/licenses/eclipse-1.0.php

//Software distributed under the License is distributed on an "AS IS" basis,
//WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
//the specific language governing rights and limitations under the License.

//By using this software in any fashion, you are agreeing to be bound by the
//terms of the License.

#endregion

using System;
using System.Runtime.InteropServices;

namespace OpenCL.Net
{
    #region Basic type aliases

    using cl_uint = UInt32;

    #endregion

    public static partial class Cl
    {

        public const string Library = "opencl.dll";
        
        #region Platform API

        [DllImport(Library)]
        private static extern ErrorCode clGetPlatformIDs(cl_uint numEntries,
                                                         [Out] [MarshalAs(UnmanagedType.LPArray)] PlatformId[] platforms,
                                                         out cl_uint numPlatforms);
        public static ErrorCode GetPlatformIDs(cl_uint numEntries,
                                               PlatformId[] platforms,
                                               out cl_uint numPlatforms)
        {
            return clGetPlatformIDs(numEntries, platforms, out numPlatforms);
        }

        [DllImport(Library)]
        private static extern ErrorCode clGetPlatformInfo(IntPtr platform,
                                                          PlatformInfo paramName,
                                                          IntPtr paramValueSize,
                                                          IntPtr paramValue,
                                                          out IntPtr paramValueSizeRet);
        public static ErrorCode GetPlatformInfo(PlatformId platformId,
                                                PlatformInfo paramName,
                                                IntPtr paramValueBufferSize,
                                                InfoBuffer paramValue,
                                                out IntPtr paramValueSize)
        {
            return clGetPlatformInfo((platformId as IHandle).Handle, paramName, paramValueBufferSize, paramValue.Address, out paramValueSize);
        }

        #endregion

        #region Device API

        [DllImport(Library)]
        private static extern ErrorCode clGetDeviceIDs(IntPtr platform,
                                                       DeviceType deviceType,
                                                       cl_uint numEntries,
                                                       [Out] [MarshalAs(UnmanagedType.LPArray)] DeviceId[] devices,
                                                       out cl_uint numDevices);
        public static ErrorCode GetDeviceIDs(PlatformId platform,
                                             DeviceType deviceType,
                                             cl_uint numEntries,
                                             DeviceId[] devices,
                                             out cl_uint numDevices)
        {
            return clGetDeviceIDs((platform as IHandle).Handle, deviceType, numEntries, devices, out numDevices);
        }

        [DllImport(Library)]
        private static extern ErrorCode clGetDeviceInfo(IntPtr device,
                                                        DeviceInfo paramName,
                                                        IntPtr paramValueSize,
                                                        IntPtr paramValue,
                                                        out IntPtr paramValueSizeRet);
        public static ErrorCode GetDeviceInfo(DeviceId device,
                                              DeviceInfo paramName,
                                              IntPtr paramValueSize,
                                              InfoBuffer paramValue,
                                              out IntPtr paramValueSizeRet)
        {
            return clGetDeviceInfo((device as IHandle).Handle, paramName, paramValueSize, paramValue.Address, out paramValueSizeRet);
        }

        #endregion

        #region Context API

        [DllImport(Library)]
        private static extern IntPtr clCreateContext([In] [MarshalAs(UnmanagedType.LPArray)] ContextProperty[] properties,
                                                     cl_uint numDevices,
                                                     [In] [MarshalAs(UnmanagedType.LPArray)] DeviceId[] devices,
                                                     ContextNotify pfnNotify,
                                                     IntPtr userData,
                                                     out ErrorCode errcodeRet);
        public static Context CreateContext(ContextProperty[] properties,
                                               cl_uint numDevices,
                                               DeviceId[] devices,
                                               ContextNotify pfnNotify,
                                               IntPtr userData,
                                               out ErrorCode errcodeRet)
        {
            return new Context(clCreateContext(properties, numDevices, devices, pfnNotify, userData, out errcodeRet));
        }

        [DllImport(Library)]
        private static extern IntPtr clCreateContextFromType([In] [MarshalAs(UnmanagedType.LPArray)] ContextProperty[] properties,
                                                             DeviceType deviceType,
                                                             ContextNotify pfnNotify,
                                                             IntPtr userData,
                                                             [Out] [MarshalAs(UnmanagedType.I4)] out ErrorCode errcodeRet);
        public static Context CreateContextFromType(ContextProperty[] properties,
                                                       DeviceType deviceType,
                                                       ContextNotify pfnNotify,
                                                       IntPtr userData,
                                                       out ErrorCode errcodeRet)
        {
            return new Context(clCreateContextFromType(properties, deviceType, pfnNotify, userData, out errcodeRet));
        }

        [DllImport(Library)]
        private static extern ErrorCode clRetainContext(IntPtr context);
        public static ErrorCode RetainContext(Context context)
        {
            return clRetainContext((context as IHandle).Handle);
        }

        [DllImport(Library)]
        private static extern ErrorCode clReleaseContext(IntPtr context);
        public static ErrorCode ReleaseContext(Context context)
        { 
            return clReleaseContext((context as IHandle).Handle);
        }

        [DllImport(Library)]
        private static extern ErrorCode clGetContextInfo(IntPtr context,
                                                         ContextInfo paramName,
                                                         IntPtr paramValueSize,
                                                         IntPtr paramValue,
                                                         out IntPtr paramValueSizeRet);
        public static ErrorCode GetContextInfo(Context context,
                                               ContextInfo paramName,
                                               IntPtr paramValueSize,
                                               InfoBuffer paramValue,
                                               out IntPtr paramValueSizeRet)
        {
            return clGetContextInfo((context as IHandle).Handle, paramName, paramValueSize, paramValue.Address, out paramValueSizeRet);
        }

        #endregion

        #region Memory Object API

        [DllImport(Library)]
        private static extern IntPtr clCreateBuffer(IntPtr context, MemFlags flags, IntPtr size, IntPtr hostPtr, out ErrorCode errcodeRet);
        public static Mem CreateBuffer(Context context, MemFlags flags, IntPtr size, object hostData, out ErrorCode errcodeRet)
        {
            using (var hostPtr = hostData.Pin())
                return new Mem(clCreateBuffer((context as IHandle).Handle, flags, size, hostPtr, out errcodeRet));
        }

        [DllImport(Library)]
        private static extern ErrorCode clRetainMemObject(IntPtr memObj);
        public static ErrorCode RetainMemObject(Mem memObj)
        {
            return clRetainMemObject((memObj as IHandle).Handle);
        }

        [DllImport(Library)]
        private static extern ErrorCode clReleaseMemObject(IntPtr memObj);
        public static ErrorCode ReleaseMemObject(Mem memObj)
        {
            return clReleaseMemObject((memObj as IHandle).Handle);
        }

        [DllImport(Library)]
        private static extern ErrorCode clGetSupportedImageFormats(IntPtr context,
                                                                   MemFlags flags,
                                                                   MemObjectType imageType,
                                                                   cl_uint numEntries,
                                                                   [Out] [MarshalAs(UnmanagedType.LPArray)] ImageFormat[] imageFormats,
                                                                   out cl_uint numImageFormats);
        public static ErrorCode GetSupportedImageFormats(Context context, 
                                                         MemFlags flags,
                                                         MemObjectType imageType,
                                                         cl_uint numEntries,
                                                         ImageFormat[] imageFormats,
                                                         out cl_uint numImageFormats)
        {
            return clGetSupportedImageFormats((context as IHandle).Handle, flags, imageType, numEntries, imageFormats, out numImageFormats);
        }

        [DllImport(Library)]
        private static extern IntPtr clCreateImage2D(IntPtr context,
                                                     MemFlags flags,
                                                     IntPtr imageFormat,
                                                     IntPtr imageWidth,
                                                     IntPtr imageHeight,
                                                     IntPtr imageRowPitch,
                                                     IntPtr hostPtr,
                                                     out ErrorCode errcodeRet);
        public static Mem CreateImage2D(Context context,
                                           MemFlags flags,
                                           ImageFormat imageFormat,
                                           IntPtr imageWidth,
                                           IntPtr imageHeight,
                                           IntPtr imageRowPitch,
                                           object hostData,
                                           out ErrorCode errorcodeRet)
        {
            using (var hostPtr = hostData.Pin())
            using (var imageFormatPtr = imageFormat.Pin())
                return new Mem(clCreateImage2D((context as IHandle).Handle, flags, imageFormatPtr, imageWidth, imageHeight, imageRowPitch, hostPtr, out errorcodeRet));
        }

        [DllImport(Library)]
        private static extern IntPtr clCreateImage3D(IntPtr context,
                                                     MemFlags flags,
                                                     IntPtr imageFormat,
                                                     IntPtr imageWidth,
                                                     IntPtr imageHeight,
                                                     IntPtr imageDepth,
                                                     IntPtr imageRowPitch,
                                                     IntPtr imageSlicePitch,
                                                     IntPtr hostPtr,
                                                     out ErrorCode errcodeRet);
        public static Mem CreateImage3D(Context context,
                                           MemFlags flags,
                                           ImageFormat imageFormat,
                                           IntPtr imageWidth,
                                           IntPtr imageHeight,
                                           IntPtr imageDepth,
                                           IntPtr imageRowPitch,
                                           IntPtr imageSlicePitch,
                                           object hostData,
                                           out ErrorCode errcodeRet)
        {
            using (var hostPtr = hostData.Pin())
            using (var imageFormatPtr = imageFormat.Pin())
                return new Mem(clCreateImage3D((context as IHandle).Handle, flags, imageFormatPtr, imageWidth, imageHeight, imageDepth, imageRowPitch, imageSlicePitch, hostPtr, out errcodeRet));
        }


        [DllImport(Library)]
        private static extern ErrorCode clGetMemObjectInfo(IntPtr memObj, 
                                                           MemInfo paramName, 
                                                           IntPtr paramValueSize, 
                                                           IntPtr paramValue, 
                                                           out IntPtr paramValueSizeRet);
        public static ErrorCode GetMemObjectInfo(Mem memObj,
                                                 MemInfo paramName,
                                                 IntPtr paramValueSize,
                                                 InfoBuffer paramValue,
                                                 out IntPtr paramValueSizeRet)
        {
            return clGetMemObjectInfo((memObj as IHandle).Handle, paramName, paramValueSize, paramValue.Address, out paramValueSizeRet);
        }

        [DllImport(Library)]
        private static extern ErrorCode clGetImageInfo(IntPtr image, 
                                                       ImageInfo paramName, 
                                                       IntPtr paramValueSize, 
                                                       IntPtr paramValue, 
                                                       out IntPtr paramValueSizeRet);
        public static ErrorCode GetImageInfo(Mem image,
                                             ImageInfo paramName,
                                             IntPtr paramValueSize,
                                             InfoBuffer paramValue,
                                             out IntPtr paramValueSizeRet)
        {
            return clGetImageInfo((image as IHandle).Handle, paramName, paramValueSize, paramValue.Address, out paramValueSizeRet);
        }

        #endregion

        #region Program Object API

        [DllImport(Library)]
        private static extern ErrorCode clUnloadCompiler();
        public static ErrorCode UnloadCompiler()
        {
            return clUnloadCompiler();
        }

        [DllImport(Library)]
        private static extern IntPtr clCreateProgramWithSource(Context context,
                                                               cl_uint count,
                                                               [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 1)] string[] strings,
                                                               [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysUInt, SizeParamIndex = 1)] IntPtr[] lengths,
                                                               out ErrorCode errcodeRet);
        public static Program CreateProgramWithSource(Context context,
                                                         cl_uint count,
                                                         string[] strings,
                                                         IntPtr[] lengths,
                                                         out ErrorCode errcodeRet)
        {
            return new Program(clCreateProgramWithSource(context, count, strings, lengths, out errcodeRet));
        }

        [DllImport(Library)]
        private static extern IntPtr clCreateProgramWithBinary(IntPtr context,
                                                               cl_uint numDevices,
                                                               [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysUInt)] DeviceId[] deviceList,
                                                               [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysUInt)] IntPtr[] lengths,
                                                               [MarshalAs(UnmanagedType.LPArray)] IntPtr binaries,
                                                               [Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ErrorCode[] binaryStatus,
                                                               out ErrorCode errcodeRet);
        public static Program CreateProgramWithBinary(Context context,
                                                         cl_uint numDevices,
                                                         DeviceId[] deviceList,
                                                         IntPtr[] lengths,
                                                         byte[][] binaries,
                                                         ErrorCode[] binaryStatus,
                                                         out ErrorCode errcodeRet)
        {
            using (var binariesPtr = binaries.Pin())
                return new Program(clCreateProgramWithBinary((context as IHandle).Handle, numDevices, deviceList, lengths, binariesPtr, binaryStatus, out errcodeRet));
        }

        [DllImport(Library)]
        private static extern ErrorCode clRetainProgram(IntPtr program);
        public static ErrorCode RetainProgram(Program program)
        {
            return clRetainProgram((program as IHandle).Handle);
        }


        [DllImport(Library)]
        private static extern ErrorCode clReleaseProgram(IntPtr program);
        public static ErrorCode ReleaseProgram(Program program)
        {
            return clReleaseProgram((program as IHandle).Handle);
        }

        [DllImport(Library)]
        private static extern ErrorCode clBuildProgram(IntPtr program,
                                                       cl_uint numDevices,
                                                       [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysUInt, SizeParamIndex = 1)] DeviceId[] deviceList,
                                                       [In] [MarshalAs(UnmanagedType.LPStr)] string options,
                                                       ProgramNotify pfnNotify,
                                                       IntPtr userData);
        public static ErrorCode BuildProgram(Program program,
                                             cl_uint numDevices,
                                             DeviceId[] deviceList,
                                             string options,
                                             ProgramNotify pfnNotify,
                                             IntPtr userData)
        {
            return clBuildProgram((program as IHandle).Handle, numDevices, deviceList, options, pfnNotify, userData);
        }

        [DllImport(Library)]
        private static extern ErrorCode clGetProgramInfo(IntPtr program,
                                                         ProgramInfo paramName,
                                                         IntPtr paramValueSize,
                                                         IntPtr paramValue,
                                                         out IntPtr paramValueSizeRet);
        public static ErrorCode GetProgramInfo(Program program,
                                               ProgramInfo paramName,
                                               IntPtr paramValueSize,
                                               InfoBuffer paramValue,
                                               out IntPtr paramValueSizeRet)
        {
            return clGetProgramInfo((program as IHandle).Handle, paramName, paramValueSize, paramValue.Address, out paramValueSizeRet);
        }

        [DllImport(Library)]
        private static extern ErrorCode clGetProgramBuildInfo(IntPtr program,
                                                              IntPtr device,
                                                              ProgramBuildInfo paramName,
                                                              IntPtr paramValueSize,
                                                              IntPtr paramValue,
                                                              out IntPtr paramValueSizeRet);
        public static ErrorCode GetProgramBuildInfo(Program program,
                                                    DeviceId device,
                                                    ProgramBuildInfo paramName,
                                                    IntPtr paramValueSize,
                                                    InfoBuffer paramValue,
                                                    out IntPtr paramValueSizeRet)
        {
            return clGetProgramBuildInfo((program as IHandle).Handle, (device as IHandle).Handle, paramName, paramValueSize, paramValue.Address, out paramValueSizeRet);
        }

        #endregion
    }
}