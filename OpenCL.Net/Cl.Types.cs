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
    public static partial class Cl
    {
        #region Handles and Types

        internal interface IHandle
        {
            IntPtr Handle
            {
                get;
            }

            IntPtr Zero
            {
                get;
            }
        }

        internal interface IRefCountedHandle : IHandle, IDisposable
        {
            void Retain();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PlatformId : IHandle
        {
            private readonly IntPtr _handle;

            internal PlatformId(IntPtr handle)
            {
                _handle = handle;
            }

            #region IHandle Members

            IntPtr IHandle.Handle
            {
                get
                {
                    return _handle;
                }
            }

            public IntPtr Zero
            {
                get
                {
                    return IntPtr.Zero;
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceId : IHandle
        {
            private readonly IntPtr _handle;

            internal DeviceId(IntPtr handle)
            {
                _handle = handle;
            }

            #region IHandle Members

            IntPtr IHandle.Handle
            {
                get
                {
                    return _handle;
                }
            }

            public IntPtr Zero
            {
                get
                {
                    return IntPtr.Zero;
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ImageFormat
        {
            [MarshalAs(UnmanagedType.U4)]
            private ChannelOrder _channelOrder;
            [MarshalAs(UnmanagedType.U4)]
            private ChannelType _channelType;

            public ImageFormat(ChannelOrder channelOrder, ChannelType channelType)
            {
                _channelOrder = channelOrder;
                _channelType = channelType;
            }

            public ChannelOrder ChannelOrder
            {
                get
                {
                    return _channelOrder;
                }
                set
                {
                    _channelOrder = value;
                }
            }

            public ChannelType ChannelType
            {
                get
                {
                    return _channelType;
                }
                set
                {
                    _channelType = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ContextProperty
        {
            private static readonly ContextProperty _zero = new ContextProperty(0);

            private readonly uint _propertyName;
            private readonly IntPtr _propertyValue;

            public ContextProperty(ContextProperties property, IntPtr value)
            {
                _propertyName = (uint)property;
                _propertyValue = value;
            }

            public ContextProperty(ContextProperties property)
            {
                _propertyName = (uint)property;
                _propertyValue = IntPtr.Zero;
            }

            public static ContextProperty Zero
            {
                get
                {
                    return _zero;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Context : IRefCountedHandle
        {
            private readonly IntPtr _handle;

            internal Context(IntPtr handle)
            {
                _handle = handle;
            }

            #region IHandle Members

            IntPtr IHandle.Handle
            {
                get
                {
                    return _handle;
                }
            }

            public IntPtr Zero
            {
                get
                {
                    return IntPtr.Zero;
                }
            }

            #endregion

            #region IRefCountedHandle Members

            public void Retain()
            {
                RetainContext(this);
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                ReleaseContext(this);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Mem : IRefCountedHandle
        {
            private readonly IntPtr _handle;

            internal Mem(IntPtr handle)
            {
                _handle = handle;
            }

            #region IRefCountedHandle Members

            public void Retain()
            {
                RetainMemObject(this);
            }

            #endregion

            #region IHandle Members

            IntPtr IHandle.Handle
            {
                get
                {
                    return _handle;
                }
            }

            public IntPtr Zero
            {
                get
                {
                    return IntPtr.Zero;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                ReleaseMemObject(this);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Program : IRefCountedHandle
        {
            private readonly IntPtr _handle;

            internal Program(IntPtr handle)
            {
                _handle = handle;
            }

            #region IRefCountedHandle Members

            public void Retain()
            {
                RetainProgram(this);
            }

            #endregion

            #region IHandle Members

            IntPtr IHandle.Handle
            {
                get
                {
                    return _handle;
                }
            }

            public IntPtr Zero
            {
                get
                {
                    return IntPtr.Zero;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                ReleaseProgram(this);
            }

            #endregion
        }

        #endregion
    }
}
