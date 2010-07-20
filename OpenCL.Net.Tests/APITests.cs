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

using NUnit.Framework;

namespace OpenCL.Net.Tests
{
    [TestFixture]
    public sealed class StatelessAPITests
    {
        [Test]
        public void PlatformQueries()
        {
            uint platformCount;
            Cl.ErrorCode result = Cl.GetPlatformIDs(0, null, out platformCount);
            Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get platform count");
            Console.WriteLine("{0} platforms found", platformCount);

            var platformIds = new Cl.PlatformId[platformCount];
            result = Cl.GetPlatformIDs(platformCount, platformIds, out platformCount);
            Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get platform ids");

            foreach (Cl.PlatformId platformId in platformIds)
            {
                IntPtr paramSize;
                result = Cl.GetPlatformInfo(platformId, Cl.PlatformInfo.Name, IntPtr.Zero, Cl.InfoBuffer.Empty, out paramSize);
                Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get platform name size");

                using (var buffer = new Cl.InfoBuffer(paramSize))
                {
                    result = Cl.GetPlatformInfo(platformIds[0], Cl.PlatformInfo.Name, paramSize, buffer, out paramSize);
                    Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get platform name string");

                    Console.WriteLine("Platform: {0}", buffer);
                }
            }
        }

        [Test]
        public void PlatformQueries2()
        {
            Cl.ErrorCode error;
            foreach (Cl.PlatformId platform in Cl.GetPlatformIDs(out error))
                Console.WriteLine("Platform Name: {0}, version {1}\nPlatform Vendor: {2}",
                                  Cl.GetInfo(Cl.GetPlatformInfo, platform, Cl.PlatformInfo.Name, out error),
                                  Cl.GetInfo(Cl.GetPlatformInfo, platform, Cl.PlatformInfo.Version, out error),
                                  Cl.GetInfo(Cl.GetPlatformInfo, platform, Cl.PlatformInfo.Vendor, out error));
        }

        [Test]
        public void DeviceQueries()
        {
            uint platformCount;
            Cl.ErrorCode result = Cl.GetPlatformIDs(0, null, out platformCount);
            Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get platform count");
            Console.WriteLine("{0} platforms found", platformCount);

            var platformIds = new Cl.PlatformId[platformCount];
            result = Cl.GetPlatformIDs(platformCount, platformIds, out platformCount);
            Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get platform ids");

            foreach (Cl.PlatformId platformId in platformIds)
            {
                IntPtr paramSize;
                result = Cl.GetPlatformInfo(platformId, Cl.PlatformInfo.Name, IntPtr.Zero, Cl.InfoBuffer.Empty, out paramSize);
                Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get platform name size");

                using (var buffer = new Cl.InfoBuffer(paramSize))
                {
                    result = Cl.GetPlatformInfo(platformIds[0], Cl.PlatformInfo.Name, paramSize, buffer, out paramSize);
                    Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get platform name string");
                }

                uint deviceCount;
                result = Cl.GetDeviceIDs(platformIds[0], Cl.DeviceType.All, 0, null, out deviceCount);
                Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get device count");

                var deviceIds = new Cl.DeviceId[deviceCount];
                result = Cl.GetDeviceIDs(platformIds[0], Cl.DeviceType.All, deviceCount, deviceIds, out deviceCount);
                Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get device ids");

                result = Cl.GetDeviceInfo(deviceIds[0], Cl.DeviceInfo.Vendor, IntPtr.Zero, Cl.InfoBuffer.Empty, out paramSize);
                Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get device vendor name size");
                using (var buf = new Cl.InfoBuffer(paramSize))
                {
                    result = Cl.GetDeviceInfo(deviceIds[0], Cl.DeviceInfo.Vendor, paramSize, buf, out paramSize);
                    Assert.AreEqual(result, Cl.ErrorCode.Success, "Could not get device vendor name string");
                    var deviceVendor = buf.ToString();
                }
            }
        }

        [Test]
        public void DeviceQueries2()
        {
            Cl.ErrorCode error;
            foreach (Cl.PlatformId platform in Cl.GetPlatformIDs(out error))
                foreach (Cl.DeviceId device in Cl.GetDeviceIDs(platform, Cl.DeviceType.All, out error))
                    Console.WriteLine("Device name: {0}", Cl.GetInfo(Cl.GetDeviceInfo, device, Cl.DeviceInfo.Name, out error));
        }

        [Test]
        public void ContextCreation()
        {
            Cl.ErrorCode error;
            
            // Select the device we want
            var device = (from dev in Cl.GetDeviceIDs(
                            (from platform in Cl.GetPlatformIDs(out error)
                            where Cl.GetInfo(Cl.GetPlatformInfo, platform, Cl.PlatformInfo.Name, out error).ToString() == "NVIDIA CUDA"
                            select platform).First(), Cl.DeviceType.Gpu, out error)
                          select dev).First();

            using (Cl.Context context = Cl.CreateContext(null, 1, new[] { device }, null, IntPtr.Zero, out error))
            {
                var refCount = Cl.GetInfo(Cl.GetContextInfo, context, Cl.ContextInfo.ReferenceCount, out error).CastTo<uint>();
            }
        }
    }

    [TestFixture]
    public sealed class APITests
    {
        private Cl.Context _context;
        private Cl.DeviceId _device;

        [TestFixtureSetUp]
        public void Setup()
        {
            Cl.ErrorCode error;

            _device = (from device in
                           Cl.GetDeviceIDs(
                               (from platform in Cl.GetPlatformIDs(out error)
                                where Cl.GetInfo(Cl.GetPlatformInfo, platform, Cl.PlatformInfo.Name, out error).ToString() == "NVIDIA CUDA"
                                select platform).First(), Cl.DeviceType.Gpu, out error)
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
            Cl.ErrorCode error;

            Console.WriteLine(Cl.MemObjectType.Image2D);
            foreach (Cl.ImageFormat imageFormat in Cl.GetSupportedImageFormats(_context, Cl.MemFlags.ReadOnly, Cl.MemObjectType.Image2D, out error))
                Console.WriteLine("{0} {1}", imageFormat.ChannelOrder, imageFormat.ChannelType);
            
            Console.WriteLine(Cl.MemObjectType.Image3D);
            foreach (Cl.ImageFormat imageFormat in Cl.GetSupportedImageFormats(_context, Cl.MemFlags.ReadOnly, Cl.MemObjectType.Image2D, out error))
                Console.WriteLine("{0} {1}", imageFormat.ChannelOrder, imageFormat.ChannelType);
        }

        [Test]
        public void MemBufferTests()
        {
            Cl.ErrorCode error;

            var values = new float[100];
            Cl.Mem buffer = Cl.CreateBuffer(_context, Cl.MemFlags.CopyHostPtr | Cl.MemFlags.ReadOnly, (IntPtr)(sizeof (float) * 100), values, out error);
            Assert.AreEqual(error, Cl.ErrorCode.Success);

            Assert.AreEqual(Cl.GetInfo(Cl.GetMemObjectInfo, buffer, Cl.MemInfo.Type, out error).CastTo<Cl.MemObjectType>(), Cl.MemObjectType.Buffer);
            Assert.AreEqual(Cl.GetInfo(Cl.GetMemObjectInfo, buffer, Cl.MemInfo.Size, out error).CastTo<uint>(), values.Length * sizeof (float));

            buffer.Dispose();
        }

        [Test]
        public void CreateImageTests()
        {
            Cl.ErrorCode error;

            if (Cl.GetDeviceInfo(_device, Cl.DeviceInfo.ImageSupport, out error).CastTo<Cl.Bool>() == Cl.Bool.False)
            {
                Console.WriteLine("No image support");
                return;
            }

            var image2DData = new float[200 * 200 * 4];
            Cl.Mem image2D = Cl.CreateImage2D(_context, Cl.MemFlags.CopyHostPtr | Cl.MemFlags.ReadOnly, new Cl.ImageFormat(Cl.ChannelOrder.RGBA, Cl.ChannelType.Float),
                                              (IntPtr)200, (IntPtr)200, (IntPtr)0, image2DData, out error);
            Assert.AreEqual(error, Cl.ErrorCode.Success);

            Assert.AreEqual(Cl.GetInfo(Cl.GetImageInfo, image2D, Cl.ImageInfo.Width, out error).CastTo<uint>(), 200);
            Assert.AreEqual(Cl.GetInfo(Cl.GetImageInfo, image2D, Cl.ImageInfo.Height, out error).CastTo<uint>(), 200);

            image2D.Dispose();

            var image3DData = new float[200 * 200 * 200 * 4];
            Cl.Mem image3D = Cl.CreateImage3D(_context, Cl.MemFlags.CopyHostPtr | Cl.MemFlags.ReadOnly, new Cl.ImageFormat(Cl.ChannelOrder.RGBA, Cl.ChannelType.Float),
                                              (IntPtr)200, (IntPtr)200, (IntPtr)200, IntPtr.Zero, IntPtr.Zero, image3DData, out error);
            Assert.AreEqual(error, Cl.ErrorCode.Success);

            Assert.AreEqual(Cl.GetInfo(Cl.GetImageInfo, image3D, Cl.ImageInfo.Width, out error).CastTo<uint>(), 200);
            Assert.AreEqual(Cl.GetInfo(Cl.GetImageInfo, image3D, Cl.ImageInfo.Height, out error).CastTo<uint>(), 200);
            Assert.AreEqual(Cl.GetInfo(Cl.GetImageInfo, image3D, Cl.ImageInfo.Depth, out error).CastTo<uint>(), 200);

            image3D.Dispose();
        }

        [Test]
        public void ProgramObjectTests()
        {
            const string correctKernel = @"
                // simple test of adding a[i] to b[i] to get c[i]
                __kernel void add_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] + b[xid];
                }";
            const string kernelWithErrors = @"
                // simple test of adding a[i] to b[i] to get c[i]
                __kernel void add_array(__global float *a, __global float *b, __global float *c)
                {
                    foo(); // <-- Error right here!
                    int xid = get_global_id(0);
                    c[xid] = a[xid] + b[xid];
                }";

            Cl.ErrorCode error;

            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { correctKernel }, null, out error))
            {
                Assert.AreEqual(error, Cl.ErrorCode.Success);

                error = Cl.BuildProgram(program, 1, new[] { _device }, string.Empty, null, IntPtr.Zero);
                Assert.AreEqual(Cl.ErrorCode.Success, error);

                // Continue here ...
                // var sizes = Cl.GetInfo(Cl.GetProgramInfo, program, Cl.ProgramInfo.BinarySizes, out error).CastTo<IntPtr[]>();
            }
            
            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { kernelWithErrors }, null, out error))
            {
                Assert.AreEqual(error, Cl.ErrorCode.Success);

                error = Cl.BuildProgram(program, 1, new[] { _device }, string.Empty, null, IntPtr.Zero);
                Assert.AreNotEqual(Cl.ErrorCode.Success, error);

                Console.WriteLine(Cl.GetInfo(Cl.GetProgramBuildInfo, program, _device, Cl.ProgramBuildInfo.Log, out error));
            }
        }
    }
}