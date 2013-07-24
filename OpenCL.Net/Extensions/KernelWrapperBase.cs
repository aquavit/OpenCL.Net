using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace OpenCL.Net.Extensions
{
    public abstract class KernelWrapperBase
    {
        private Cl.Kernel _kernel;
        private readonly Cl.Context _context;
        
        protected KernelWrapperBase(Cl.Context context)
        {
            _context = context;
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

        protected Cl.ErrorCode Compile(string source, string kernelName, out string errors, string options = null)
        {
            errors = string.Empty;
            Cl.ErrorCode error;
            var devicesInfoBuffer = Cl.GetContextInfo(_context, Cl.ContextInfo.Devices, out error);
            var devices = devicesInfoBuffer.CastToArray<Cl.Device>((devicesInfoBuffer.Size / Marshal.SizeOf(typeof(Cl.Device))));
            var program = Cl.CreateProgramWithSource(_context, 1, new[] { source }, new[] { (IntPtr)source.Length }, out error);
            error = Cl.BuildProgram(program, (uint)devices.Length, devices, options == null ? string.Empty : options, null, IntPtr.Zero);
            if (error != Cl.ErrorCode.Success)
            {
                errors = string.Join("\n", from device in devices
                                            select Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error).ToString());
                throw new Cl.Exception(error, errors);
            }
            _kernel = Cl.CreateKernel(program, kernelName, out error);
            return error;
        }

        protected Cl.ErrorCode Compile(string kernelSource, string kernelName, string options = null)
        {
            string errors;
            var result = Compile(kernelSource, kernelName, out errors, options);
            if (result != Cl.ErrorCode.Success)
                throw new Cl.Exception(result, errors);

            return result;
        }

        public Cl.Context Context { get { return _context; } }
        public Cl.Kernel Kernel { get { return _kernel; } }
    }
}
