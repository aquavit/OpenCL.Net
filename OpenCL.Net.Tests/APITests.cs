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
using System.Linq;

using OpenCL.Net.Extensions;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace OpenCL.Net.Tests
{
    [TestFixture]
    public sealed class StatelessAPITests
    {
        [Test]
        public void PlatformQueries()
        {
            uint platformCount;
            ErrorCode result = Cl.GetPlatformIDs(0, null, out platformCount);
            Assert.AreEqual(result, ErrorCode.Success, "Could not get platform count");
            Console.WriteLine("{0} platforms found", platformCount);

            var platformIds = new Platform[platformCount];
            result = Cl.GetPlatformIDs(platformCount, platformIds, out platformCount);
            Assert.AreEqual(result, ErrorCode.Success, "Could not get platform ids");

            foreach (Platform platformId in platformIds)
            {
                IntPtr paramSize;
                result = Cl.GetPlatformInfo(platformId, PlatformInfo.Name, IntPtr.Zero, InfoBuffer.Empty, out paramSize);
                Assert.AreEqual(result, ErrorCode.Success, "Could not get platform name size");

                using (var buffer = new InfoBuffer(paramSize))
                {
                    result = Cl.GetPlatformInfo(platformIds[0], PlatformInfo.Name, paramSize, buffer, out paramSize);
                    Assert.AreEqual(result, ErrorCode.Success, "Could not get platform name string");

                    Console.WriteLine("Platform: {0}", buffer);
                }
            }
        }

        [Test]
        public void PlatformQueries2()
        {
            ErrorCode error;
            foreach (Platform platform in Cl.GetPlatformIDs(out error))
            {
                if (!platform.IsValid())
                    Console.WriteLine("Invalid handle");
                Console.WriteLine("Platform Name: {0}, version {1}\nPlatform Vendor: {2}",
                                  Cl.GetPlatformInfo(platform, PlatformInfo.Name, out error),
                                  Cl.GetPlatformInfo(platform, PlatformInfo.Version, out error),
                                  Cl.GetPlatformInfo(platform, PlatformInfo.Vendor, out error));
            }
        }

        [Test]
        public void DeviceQueries()
        {
            uint platformCount;
            ErrorCode result = Cl.GetPlatformIDs(0, null, out platformCount);
            Assert.AreEqual(result, ErrorCode.Success, "Could not get platform count");
            Console.WriteLine("{0} platforms found", platformCount);

            var platformIds = new Platform[platformCount];
            result = Cl.GetPlatformIDs(platformCount, platformIds, out platformCount);
            Assert.AreEqual(result, ErrorCode.Success, "Could not get platform ids");

            foreach (Platform platformId in platformIds)
            {
                IntPtr paramSize;
                result = Cl.GetPlatformInfo(platformId, PlatformInfo.Name, IntPtr.Zero, InfoBuffer.Empty, out paramSize);
                Assert.AreEqual(result, ErrorCode.Success, "Could not get platform name size");

                using (var buffer = new InfoBuffer(paramSize))
                {
                    result = Cl.GetPlatformInfo(platformIds[0], PlatformInfo.Name, paramSize, buffer, out paramSize);
                    Assert.AreEqual(result, ErrorCode.Success, "Could not get platform name string");
                }

                uint deviceCount;
                result = Cl.GetDeviceIDs(platformIds[0], DeviceType.All, 0, null, out deviceCount);
                Assert.AreEqual(result, ErrorCode.Success, "Could not get device count");

                var deviceIds = new Device[deviceCount];
                result = Cl.GetDeviceIDs(platformIds[0], DeviceType.All, deviceCount, deviceIds, out deviceCount);
                Assert.AreEqual(result, ErrorCode.Success, "Could not get device ids");

                result = Cl.GetDeviceInfo(deviceIds[0], DeviceInfo.Vendor, IntPtr.Zero, InfoBuffer.Empty, out paramSize);
                Assert.AreEqual(result, ErrorCode.Success, "Could not get device vendor name size");
                using (var buf = new InfoBuffer(paramSize))
                {
                    result = Cl.GetDeviceInfo(deviceIds[0], DeviceInfo.Vendor, paramSize, buf, out paramSize);
                    Assert.AreEqual(result, ErrorCode.Success, "Could not get device vendor name string");
                    var deviceVendor = buf.ToString();
                }
            }
        }

        [Test]
        public void DeviceQueries2()
        {
            ErrorCode error;
            foreach (Platform platform in Cl.GetPlatformIDs(out error))
                foreach (Device device in Cl.GetDeviceIDs(platform, DeviceType.All, out error))
                    Console.WriteLine("Device name: {0}", Cl.GetDeviceInfo(device, DeviceInfo.Name, out error));
        }

        [Test]
        public void ContextCreation()
        {
            ErrorCode error;
            
            // Select the device we want
            var device = (from dev in Cl.GetDeviceIDs(
                            (from platform in Cl.GetPlatformIDs(out error)
                            select platform).First(), DeviceType.Gpu, out error)
                          select dev).First();

            uint refCount;
            using (Context context = Cl.CreateContext(null, 1, new[] { device }, null, IntPtr.Zero, out error))
                refCount = Cl.GetContextInfo(context, ContextInfo.ReferenceCount, out error).CastTo<uint>();
        }
    }

    [TestFixture]
    public sealed class APITests
    {
        private Context _context;
        private Device _device;

        [TestFixtureSetUp]
        public void Setup()
        {
            ErrorCode error;

            _device = (from device in
                           Cl.GetDeviceIDs(
                               (from platform in Cl.GetPlatformIDs(out error)
                                where Cl.GetPlatformInfo(platform, PlatformInfo.Name, out error).ToString() == "NVIDIA CUDA"
                                select platform).First(), DeviceType.Gpu, out error)
                       select device).First();

            _context = Cl.CreateContext(null, 1, new[] { _device }, null, IntPtr.Zero, out error);
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        [Test]
        public void SupportedImageFormats()
        {
            ErrorCode error;

            Console.WriteLine(MemObjectType.Image2D);
            foreach (ImageFormat imageFormat in Cl.GetSupportedImageFormats(_context, MemFlags.ReadOnly, MemObjectType.Image2D, out error))
                Console.WriteLine("{0} {1}", imageFormat.ChannelOrder, imageFormat.ChannelType);
            
            Console.WriteLine(MemObjectType.Image3D);
            foreach (ImageFormat imageFormat in Cl.GetSupportedImageFormats(_context, MemFlags.ReadOnly, MemObjectType.Image2D, out error))
                Console.WriteLine("{0} {1}", imageFormat.ChannelOrder, imageFormat.ChannelType);
        }

        [Test]
        public void MemBufferTests()
        {
            const int bufferSize = 100;
            
            ErrorCode error;
            Random random = new Random();

            float[] values = (from value in Enumerable.Range(0, bufferSize) select (float)random.NextDouble()).ToArray();
            IMem buffer = Cl.CreateBuffer(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, (IntPtr)(sizeof (float) * bufferSize), values, out error);
            Assert.AreEqual(error, ErrorCode.Success);

            Assert.AreEqual(Cl.GetMemObjectInfo(buffer, MemInfo.Type, out error).CastTo<MemObjectType>(), MemObjectType.Buffer);
            Assert.AreEqual(Cl.GetMemObjectInfo(buffer, MemInfo.Size, out error).CastTo<uint>(), values.Length * sizeof (float));

            // TODO: Verify values
            //int index = 0;
            //foreach (float value in Cl.GetMemObjectInfo(buffer, Cl.MemInfo.HostPtr, out error).CastToEnumerable<float>(Enumerable.Range(0, 100)))
            //{
            //    Assert.AreEqual(values[index], value);
            //    index++;
            //}

            buffer.Dispose();
        }

        [Test]
        public void CreateImageTests()
        {
            ErrorCode error;

            if (Cl.GetDeviceInfo(_device, DeviceInfo.ImageSupport, out error).CastTo<Bool>() == Bool.False)
            {
                Console.WriteLine("No image support");
                return;
            }

            {
                var image2DData = new float[200 * 200 * sizeof(float)];
                IMem image2D = Cl.CreateImage2D(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, new ImageFormat(ChannelOrder.RGBA, ChannelType.Float),
                                                  (IntPtr)200, (IntPtr)200, (IntPtr)0, image2DData, out error);
                Assert.AreEqual(error, ErrorCode.Success);

                Assert.AreEqual(Cl.GetImageInfo(image2D, ImageInfo.Width, out error).CastTo<uint>(), 200);
                Assert.AreEqual(Cl.GetImageInfo(image2D, ImageInfo.Height, out error).CastTo<uint>(), 200);

                image2D.Dispose();
            }

            {
                var image3DData = new float[200 * 200 * 200 * sizeof(float)];
                IMem image3D = Cl.CreateImage3D(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, new ImageFormat(ChannelOrder.RGBA, ChannelType.Float),
                                                  (IntPtr)200, (IntPtr)200, (IntPtr)200, IntPtr.Zero, IntPtr.Zero, image3DData, out error);
                Assert.AreEqual(error, ErrorCode.Success);

                Assert.AreEqual(Cl.GetImageInfo(image3D, ImageInfo.Width, out error).CastTo<uint>(), 200);
                Assert.AreEqual(Cl.GetImageInfo(image3D, ImageInfo.Height, out error).CastTo<uint>(), 200);
                Assert.AreEqual(Cl.GetImageInfo(image3D, ImageInfo.Depth, out error).CastTo<uint>(), 200);

                image3D.Dispose();
            }
        }

        [Test]
        public void CommandQueueAPI()
        {
            ErrorCode error;
            using (CommandQueue commandQueue = Cl.CreateCommandQueue(_context, _device, CommandQueueProperties.OutOfOrderExecModeEnable, out error))
            {
                Assert.AreEqual(ErrorCode.Success, error);

                Assert.AreEqual(1, Cl.GetCommandQueueInfo(commandQueue, CommandQueueInfo.ReferenceCount, out error).CastTo<uint>());

                Cl.RetainCommandQueue(commandQueue);
                Assert.AreEqual(2, Cl.GetCommandQueueInfo(commandQueue, CommandQueueInfo.ReferenceCount, out error).CastTo<uint>());

                Cl.ReleaseCommandQueue(commandQueue);
                Assert.AreEqual(1, Cl.GetCommandQueueInfo(commandQueue, CommandQueueInfo.ReferenceCount, out error).CastTo<uint>());

                Assert.AreEqual(Cl.GetCommandQueueInfo(commandQueue, CommandQueueInfo.Context, out error).CastTo<Context>(), _context);
                Assert.AreEqual(Cl.GetCommandQueueInfo(commandQueue, CommandQueueInfo.Device, out error).CastTo<Device>(), _device);
                Assert.AreEqual(Cl.GetCommandQueueInfo(commandQueue, CommandQueueInfo.Properties, out error).CastTo<CommandQueueProperties>(),
                    CommandQueueProperties.OutOfOrderExecModeEnable);
            }
        }

        [Test]
        public void ProgramAndKernelTests()
        {
            const string correctSource = @"
                // Simple test; c[i] = a[i] + b[i]

                __kernel void add_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] + b[xid];
                }
                
                __kernel void sub_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] - b[xid];
                }

                ";
            const string sourceWithErrors = @"
                // Erroneous kernel

                __kernel void add_array(__global float *a, __global float *b, __global float *c)
                {
                    foo(); // <-- Error right here!
                    int xid = get_global_id(0);
                    c[xid] = a[xid] + b[xid];
                }";

            ErrorCode error;


            using (Program program = Cl.CreateProgramWithSource(_context, 1, new[] { sourceWithErrors }, null, out error))
            {
                Assert.AreEqual(error, ErrorCode.Success);

                error = Cl.BuildProgram(program, 1, new[] { _device }, string.Empty, null, IntPtr.Zero);
                Assert.AreNotEqual(ErrorCode.Success, error);

                Assert.AreEqual(Cl.GetProgramBuildInfo(program, _device, ProgramBuildInfo.Status, out error).CastTo<BuildStatus>(), BuildStatus.Error);

                Console.WriteLine("There were error(s) compiling the provided kernel");
                Console.WriteLine(Cl.GetProgramBuildInfo(program, _device, ProgramBuildInfo.Log, out error));
            }

            using (Program program = Cl.CreateProgramWithSource(_context, 1, new[] { correctSource }, null, out error))
            {
                Assert.AreEqual(error, ErrorCode.Success);

                error = Cl.BuildProgram(program, 1, new[] { _device }, string.Empty, null, IntPtr.Zero);
                Assert.AreEqual(ErrorCode.Success, error);

                Assert.AreEqual(Cl.GetProgramBuildInfo(program, _device, ProgramBuildInfo.Status, out error).CastTo<BuildStatus>(), BuildStatus.Success);

                // Try to get information from the program
                Assert.AreEqual(Cl.GetProgramInfo(program, ProgramInfo.Context, out error).CastTo<Context>(), _context);
                Assert.AreEqual(Cl.GetProgramInfo(program, ProgramInfo.NumDevices, out error).CastTo<int>(), 1);
                Assert.AreEqual(Cl.GetProgramInfo(program, ProgramInfo.Devices, out error).CastTo<Device>(0), _device);

                Console.WriteLine("Program source was:");
                Console.WriteLine(Cl.GetProgramInfo(program, ProgramInfo.Source, out error));

                Kernel kernel = Cl.CreateKernel(program, "add_array", out error);
                Assert.AreEqual(error, ErrorCode.Success);

                kernel.Dispose();

                Kernel[] kernels = Cl.CreateKernelsInProgram(program, out error);
                Assert.AreEqual(error, ErrorCode.Success);
                Assert.AreEqual(kernels.Length, 2);
                Assert.AreEqual("add_array", Cl.GetKernelInfo(kernels[0], KernelInfo.FunctionName, out error).ToString());
                Assert.AreEqual("sub_array", Cl.GetKernelInfo(kernels[1], KernelInfo.FunctionName, out error).ToString());
            }
        }

        [Test]
        // Partially from OpenTK demo - Submitted by "mfagerlund"
        public void AddArrayAddsCorrectly()
        {
            const string correctSource = @"
                // Simple test; c[i] = a[i] + b[i]

                __kernel void add_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] + b[xid];
                }
                
                __kernel void sub_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] - b[xid];
                }

                ";

            ErrorCode error;

            using (Program program = Cl.CreateProgramWithSource(_context, 1, new[] { correctSource }, null, out error))
            {
                Assert.AreEqual(error, ErrorCode.Success);
                error = Cl.BuildProgram(program, 1, new[] { _device }, string.Empty, null, IntPtr.Zero);
                Assert.AreEqual(ErrorCode.Success, error);
                Assert.AreEqual(Cl.GetProgramBuildInfo(program, _device, ProgramBuildInfo.Status, out error).CastTo<BuildStatus>(), BuildStatus.Success);

                Kernel[] kernels = Cl.CreateKernelsInProgram(program, out error);
                Kernel kernel = kernels[0];

                const int cnBlockSize = 4;
                const int cnBlocks = 3;
                IntPtr cnDimension = new IntPtr(cnBlocks * cnBlockSize);

                // allocate host  vectors
                float[] A = new float[cnDimension.ToInt32()];
                float[] B = new float[cnDimension.ToInt32()];
                float[] C = new float[cnDimension.ToInt32()];

                // initialize host memory
                Random rand = new Random();
                for (int i = 0; i < A.Length; i++)
                {
                    A[i] = rand.Next() % 256;
                    B[i] = rand.Next() % 256;
                }

                //Cl.IMem hDeviceMemA = Cl.CreateBuffer(_context, Cl.MemFlags.CopyHostPtr | Cl.MemFlags.ReadOnly, (IntPtr)(sizeof(float) * cnDimension.ToInt32()), A, out error);
                //Assert.AreEqual(Cl.ErrorCode.Success, error);
                
                IMem<float> hDeviceMemA = Cl.CreateBuffer(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, A, out error);
                Assert.AreEqual(ErrorCode.Success, error);

                IMem hDeviceMemB = Cl.CreateBuffer(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, (IntPtr)(sizeof(float) * cnDimension.ToInt32()), B, out error);
                Assert.AreEqual(ErrorCode.Success, error);
                IMem hDeviceMemC = Cl.CreateBuffer(_context, MemFlags.WriteOnly, (IntPtr)(sizeof(float) * cnDimension.ToInt32()), IntPtr.Zero, out error);
                Assert.AreEqual(ErrorCode.Success, error);

                CommandQueue cmdQueue = Cl.CreateCommandQueue(_context, _device, (CommandQueueProperties)0, out error);

                Event clevent;

                int intPtrSize = 0;
                intPtrSize = Marshal.SizeOf(typeof(IntPtr));

                // setup parameter values
                error = Cl.SetKernelArg(kernel, 0, new IntPtr(intPtrSize), hDeviceMemA);
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 1, new IntPtr(intPtrSize), hDeviceMemB);
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.SetKernelArg(kernel, 2, new IntPtr(intPtrSize), hDeviceMemC);
                Assert.AreEqual(ErrorCode.Success, error);

                // write data from host to device
                error = Cl.EnqueueWriteBuffer(cmdQueue, hDeviceMemA, Bool.True, IntPtr.Zero,
                    new IntPtr(cnDimension.ToInt32() * sizeof(float)),
                    A, 0, null, out clevent);
                Assert.AreEqual(ErrorCode.Success, error);
                error = Cl.EnqueueWriteBuffer(cmdQueue, hDeviceMemB, Bool.True, IntPtr.Zero,
                    new IntPtr(cnDimension.ToInt32() * sizeof(float)),
                    B, 0, null, out clevent);
                Assert.AreEqual(ErrorCode.Success, error);

                // execute kernel
                error = Cl.EnqueueNDRangeKernel(cmdQueue, kernel, 1, null, new IntPtr[] { cnDimension }, null, 0, null, out clevent);
                Assert.AreEqual(ErrorCode.Success, error, error.ToString());

                // copy results from device back to host
                IntPtr event_handle = IntPtr.Zero;

                error = Cl.EnqueueReadBuffer(cmdQueue, hDeviceMemC, Bool.True, 0, C.Length, C, 0, null, out clevent);
                Assert.AreEqual(ErrorCode.Success, error, error.ToString());

                for (int i = 0; i < A.Length; i++)
                {
                    Assert.That(A[i] + B[i], Is.EqualTo(C[i]));
                }

                Cl.Finish(cmdQueue);

                Cl.ReleaseMemObject(hDeviceMemA);
                Cl.ReleaseMemObject(hDeviceMemB);
                Cl.ReleaseMemObject(hDeviceMemC);
            }
        }
    }

    [TestFixture]
    public sealed class VisualizerTest
    {
        [Test]
        public void Foo()
        {
            using (var env = "*Intel*".CreateCLEnvironment())
            {
                var data = Enumerable.Range(0, 10000).ToArray();
                var buffer = env.Context.CreateBuffer(data);
            }
        }
    }
}