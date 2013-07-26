#region License and Copyright Notice
// Copyright (c) 2010 Ananth B.
// All rights reserved.
// 
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenCL.Net.Extensions
{
    public static class CLExtensions
    {
        public static Cl.Environment CreateCLEnvironment(this string platformWildCard, 
            Cl.DeviceType deviceType = Cl.DeviceType.Default, 
            Cl.CommandQueueProperties commandQueueProperties = Cl.CommandQueueProperties.None)
        {
            return new Cl.Environment(platformWildCard, deviceType, commandQueueProperties);
        }

        private static Cl.Kernel _CompileKernel(this Cl.Context context, string source, string kernelName, out string errors, string options = null)
        {
            errors = string.Empty;
            Cl.ErrorCode error;
            var devicesInfoBuffer = Cl.GetContextInfo(context, Cl.ContextInfo.Devices, out error);
            var devices = devicesInfoBuffer.CastToArray<Cl.Device>((devicesInfoBuffer.Size / Marshal.SizeOf(typeof(IntPtr))));
            var program = Cl.CreateProgramWithSource(context, 1, new[] { source }, new[] { (IntPtr)source.Length }, out error);
            error = Cl.BuildProgram(program, (uint)devices.Length, devices, options == null ? string.Empty : options, null, IntPtr.Zero);
            if (error != Cl.ErrorCode.Success)
            {
                errors = string.Join("\n", from device in devices
                                            select Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error).ToString());
                return new Cl.Kernel();
            }
            return Cl.CreateKernel(program, kernelName, out error);
        }

        public static Cl.Kernel CompileKernel(this Cl.Context context, string path, string kernelName, out string errors, string options = null)
        {
            errors = string.Empty;
            if (!Path.IsPathRooted(path))
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            return _CompileKernel(context, File.ReadAllText(path), kernelName, out errors, options);
        }
        public static Cl.Kernel CompileKernel(this Cl.Context context, string path, string kernelName, string options = null)
        {
            string errors;
            var result = CompileKernel(context, path, kernelName, out errors, options);
            if (!result.IsValid() || !string.IsNullOrEmpty(errors))
                throw new Cl.Exception(Cl.ErrorCode.InvalidKernel, errors);

            return result;
        }
        public static Cl.Kernel CompileKernelFromSource(this Cl.Context context, string source, string kernelName, out string errors, string options = null)
        {
            return _CompileKernel(context, source, kernelName, out errors, options);
        }
        public static Cl.Kernel CompileKernelFromSource(this Cl.Context context, string source, string kernelName, string options = null)
        {
            string errors;
            var result = CompileKernelFromSource(context, source, kernelName, out errors, options);
            if (!result.IsValid() || !string.IsNullOrEmpty(errors))
                throw new Cl.Exception(Cl.ErrorCode.InvalidKernel, errors);

            return result;
        }

        public struct KernelArgChain
        {
            internal Cl.Kernel Kernel;
            internal uint Count;
        }
        // Any value type
        public static KernelArgChain SetKernelArg<T>(this Cl.Kernel kernel, T value) where T : struct
        {
            Cl.SetKernelArg<T>(kernel, 0, value).Check();
            return new KernelArgChain { Kernel = kernel, Count = 0 };
        }
        public static KernelArgChain SetKernelArg<T>(this KernelArgChain argChain, T buffer) where T : struct
        {
            Cl.SetKernelArg<T>(argChain.Kernel, ++argChain.Count, buffer).Check();
            return argChain;
        }

        // IMem
        public static KernelArgChain SetKernelArg(this Cl.Kernel kernel, Cl.IMem buffer)
        {
            Cl.SetKernelArg(kernel, 0, Cl.TypeSize<IntPtr>.Size, buffer).Check();
            return new KernelArgChain { Kernel = kernel, Count = 0 };
        }
        public static KernelArgChain SetKernelArg(this KernelArgChain argChain, Cl.IMem buffer)
        {
            Cl.SetKernelArg(argChain.Kernel, ++argChain.Count, Cl.TypeSize<IntPtr>.Size, buffer).Check();
            return argChain;
        }

        // Local memory
        public static KernelArgChain SetKernelArg<T>(this Cl.Kernel kernel, int length) where T : struct
        {
            Cl.SetKernelArg<T>(kernel, 0, length).Check();
            return new KernelArgChain { Kernel = kernel, Count = 0 };
        }
        public static KernelArgChain SetKernelArg<T>(this KernelArgChain argChain, int length) where T : struct
        {
            var size = Cl.TypeSize<T>.SizeInt * length;
            Cl.SetKernelArg<T>(argChain.Kernel, ++argChain.Count, size);
            return argChain;
        }

        public static Cl.Event EnqueueKernel(this Cl.CommandQueue commandQueue, Cl.Kernel kernel, 
            uint globalWorkSize, 
            uint localWorkSize = 0, 
            params Cl.Event[] waitFor)
        {
            Cl.Event e;
            Cl.EnqueueNDRangeKernel(commandQueue, kernel, 1, null,
                new[] { (IntPtr)globalWorkSize },
                localWorkSize == 0 ? null : new[] { (IntPtr)localWorkSize },
                (uint)waitFor.Length, waitFor.Length == 0 ? null : waitFor, out e).Check();
            return e;
        }
        public static Cl.Event EnqueueKernel(this Cl.CommandQueue commandQueue, Cl.Kernel kernel, 
            uint globalWorkSize0, uint globalWorkSize1, 
            uint localWorkSize0 = 0, uint localWorkSize1 = 0, 
            params Cl.Event[] waitFor)
        {
            Cl.Event e;
            Cl.EnqueueNDRangeKernel(commandQueue, kernel, 2, null,
                new[] { (IntPtr)globalWorkSize0, (IntPtr)globalWorkSize1 },
                (localWorkSize0 == 0) && (localWorkSize1 == 0) ? 
                    null : 
                    new[] { (IntPtr)localWorkSize0, (IntPtr)localWorkSize1 },
                (uint)waitFor.Length, waitFor.Length == 0 ? null : waitFor, out e).Check();
            return e;
        }
        public static Cl.Event EnqueueKernel(this Cl.CommandQueue commandQueue, Cl.Kernel kernel,
            uint globalWorkSize0, uint globalWorkSize1, uint globalWorkSize2,
            uint localWorkSize0 = 0, uint localWorkSize1 = 0, uint localWorkSize2 = 0,
            params Cl.Event[] waitFor)
        {
            Cl.Event e;
            Cl.EnqueueNDRangeKernel(commandQueue, kernel, 1, null,
                new[] { (IntPtr)globalWorkSize0, (IntPtr)globalWorkSize1, (IntPtr)globalWorkSize2 },
                (localWorkSize0 == 0) && (localWorkSize1 == 0) && (localWorkSize2 == 0) ?
                    null :
                    new[] { (IntPtr)localWorkSize0, (IntPtr)localWorkSize1, (IntPtr)localWorkSize2 },
                (uint)waitFor.Length, waitFor.Length == 0 ? null : waitFor, out e).Check();
            return e;
        }

        public static void EnqueueWaitForEvents(this Cl.CommandQueue commandQueue, params Cl.Event[] waitFor)
        {
            Cl.EnqueueWaitForEvents(commandQueue, (uint)waitFor.Length, waitFor);
        }
        public static Cl.Event EnqueueWriteToBuffer<T>(this Cl.CommandQueue commandQueue, Cl.IMem<T> buffer, T[] data, int offset = 0, long length = -1, params Cl.Event[] waitFor)
            where T: struct
        {
            Cl.Event e;
            var elemSize = Cl.TypeSize<T>.SizeInt;
            Cl.EnqueueWriteBuffer(commandQueue, buffer, Cl.Bool.False, (IntPtr)(offset * elemSize), (IntPtr)((length == -1 ? data.Length : length) * elemSize), data, (uint)waitFor.Length, waitFor, out e)
                .Check();
            return e;
        }
        public static void WriteToBuffer<T>(this Cl.CommandQueue commandQueue, Cl.IMem<T> buffer, T[] data, int offset = 0, long length = -1, params Cl.Event[] waitFor)
            where T : struct
        {
            Cl.Event e;
            var elemSize = Cl.TypeSize<T>.SizeInt;
            Cl.EnqueueWriteBuffer(commandQueue, buffer, Cl.Bool.True, (IntPtr)(offset * elemSize), (IntPtr)((length == -1 ? data.Length : length) * elemSize), data, (uint)waitFor.Length, waitFor, out e)
                .Check();
            e.Dispose();
        }

        public static Cl.Event EnqueueReadFromBuffer<T>(this Cl.CommandQueue commandQueue, Cl.IMem<T> buffer, T[] array, int offset = 0, long length = -1, params Cl.Event[] waitFor)
            where T: struct
        {
            Cl.Event e;
            var elemSize = Cl.TypeSize<T>.SizeInt;
            Cl.EnqueueReadBuffer(commandQueue, buffer, Cl.Bool.False, (IntPtr)(offset * elemSize), (IntPtr)((length == -1 ? array.Length : length) * elemSize), array,
                (uint)waitFor.Length, waitFor, out e)
                .Check();

            return e;
        }
        public static void ReadFromBuffer<T>(this Cl.CommandQueue commandQueue, Cl.IMem<T> buffer, T[] array, int offset = 0, long length = -1, params Cl.Event[] waitFor)
            where T : struct
        {
            Cl.Event e;
            var elemSize = Cl.TypeSize<T>.SizeInt;
            Cl.EnqueueReadBuffer(commandQueue, buffer, Cl.Bool.True, (IntPtr)(offset * elemSize), (IntPtr)((length == -1 ? array.Length : length) * elemSize), array,
                (uint)waitFor.Length, waitFor, out e)
                .Check();

            e.Dispose();
        }

        public static Cl.ErrorCode Flush(this Cl.CommandQueue commandQueue)
        {
            return Cl.Flush(commandQueue);
        }
        public static Cl.ErrorCode Wait(this Cl.Event ev)
        {
            return Cl.WaitForEvents(1, new[] { ev });
        }
        public static Cl.ErrorCode Finish(this Cl.CommandQueue commandQueue)
        {
            return Cl.Finish(commandQueue);
        }

        public static Cl.IMem<T> CreateBuffer<T>(this Cl.Context context, int length, Cl.MemFlags flags = Cl.MemFlags.None, bool zero = false) where T: struct
        {
            var hostData = new T[length];
            Cl.ErrorCode err;
            
            var result = Cl.CreateBuffer<T>(context, flags | Cl.MemFlags.CopyHostPtr, hostData, out err);
            
            err.Check();
            hostData = null;
            
            return result;
        }
        public static Cl.IMem<T> CreateBuffer<T>(this Cl.Context context, T[] data, Cl.MemFlags flags = Cl.MemFlags.None) where T : struct
        {
            Cl.ErrorCode err;
            var result = Cl.CreateBuffer<T>(context, flags | Cl.MemFlags.CopyHostPtr, data, out err);
            err.Check();

            return result;
        }
    }
}
