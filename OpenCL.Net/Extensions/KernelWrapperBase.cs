using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCL.Net.Extensions
{
    public abstract class KernelWrapperBase
    {
        private readonly Cl.CommandQueue _commandQueue;
        private readonly Cl.Kernel _kernel;
        
        protected KernelWrapperBase(Cl.CommandQueue commandQueue)
        {
            _commandQueue = commandQueue;
        }

        protected uint GetWorkDimension(uint x, uint y, uint z)
        {
            return (uint)((x > 0 ? 1 : 0) + (y > 0 ? 1 : 0) + (z > 0 ? 1 : 0));
        }

        protected IntPtr[] GetWorkSizes(uint x, uint y, uint z)
        {
            var sum = GetWorkDimension(x, y, z);
            switch (sum)
            {
                case 1:
                    return new[] { (IntPtr)x };

                case 2:
                    return new[] { (IntPtr)x, (IntPtr)y };

                case 3:
                    return new[] { (IntPtr)x, (IntPtr)y, (IntPtr)z };

                default:
                    throw new Cl.Exception(Cl.ErrorCode.InvalidWorkDimension);
            }
        }

        public Cl.CommandQueue CommandQueue { get { return _commandQueue; } }
        public Cl.Kernel Kernel { get { return _kernel; } }
    }
}
