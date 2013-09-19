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
using System.Diagnostics;
using System.Linq;

using OpenCL.Net.Extensions;
using OpenCL.Net;

namespace Simple
{
    public struct Test
    {
    }

    [DebuggerTypeProxy(typeof(MyTypeProxy<>))]
    struct MyType<T>
    {
    }

    class MyTypeProxy<T>
    {
        MyType<T> type;
        public MyTypeProxy(MyType<T> type)
        {
            this.type = type;
        }
        public string Hash
        {
            get { return type.GetHashCode().ToString(); }
        }
    }

    class Program
    {
        private const int ArrayLength = 1024;

        static void Main(string[] args)
        {
            var env = "*Intel*".CreateCLEnvironment();

            var random = new Random();
            var a = env.Context.CreateBuffer((from i in Enumerable.Range(0, ArrayLength) select (float)random.NextDouble()).ToArray(), 
                MemFlags.ReadOnly);
            var b = env.Context.CreateBuffer((from i in Enumerable.Range(0, ArrayLength) select (float)random.NextDouble()).ToArray(), 
                MemFlags.WriteOnly);

            var kernel = new Kernel.doSomething(env.Context);
            kernel.Compile(string.Format("-cl-opt-disable -g -s \"{0}\"", Kernel.Kernel_Source.OriginalKernelPath));

            var kernelRun = kernel.Run(env.CommandQueues[0], a, b, 100, 10f, ArrayLength);
            var results = new float[ArrayLength];
            env.CommandQueues[0].ReadFromBuffer(b, results, waitFor: kernelRun);

            env.Dispose();
        }
    }
}
