using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace OpenCL.Net.Tests
{
    [TestFixture]
    public sealed class KernelWrapperGenerationTests
    {
        [Test]
        public void TestKernelGeneration()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, @"TestKernels\TestKernel1.cl");
            var wrapper = OpenCL.Net.Tasks.Kernel.ProcessKernelFile(filePath, File.ReadAllText(filePath));
        }
    }
}
