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

namespace OpenCL.Net
{
    public static partial class Cl
    {
        #region Platform API

        public static PlatformId[] GetPlatformIDs(out ErrorCode error)
        {
            uint platformCount;

            error = GetPlatformIDs(0, null, out platformCount);
            if (error != ErrorCode.Success)
                return new PlatformId[0];

            var platformIds = new PlatformId[platformCount] ;
            error = GetPlatformIDs(platformCount, platformIds, out platformCount);
            if (error != ErrorCode.Success)
                return new PlatformId[0];
            
            return platformIds;
        }

        #endregion

        #region Device API

        public static DeviceId[] GetDeviceIDs(PlatformId platform, DeviceType deviceType, out ErrorCode error)
        {
            uint deviceCount;
            error = GetDeviceIDs(platform, deviceType, 0, null, out deviceCount);
            if (error != ErrorCode.Success)
                return new DeviceId[0];

            var deviceIds = new DeviceId[deviceCount];
            error = GetDeviceIDs(platform, deviceType, deviceCount, deviceIds, out deviceCount);
            if (error != ErrorCode.Success)
                return new DeviceId[0];
            
            return deviceIds;
        }

        public static InfoBuffer GetDeviceInfo(DeviceId device, DeviceInfo paramName, out ErrorCode error)
        {
            return GetInfo(GetDeviceInfo, device, paramName, out error);
        }

        #endregion

        #region Memory Object API

        public static ImageFormat[] GetSupportedImageFormats(Context context, MemFlags flags, MemObjectType imageType, out ErrorCode error)
        {
            uint imageFormatCount;
            error = GetSupportedImageFormats(context, flags, imageType, 0, null, out imageFormatCount);
            if (error != ErrorCode.Success)
                return new ImageFormat[0];

            var imageFormats = new ImageFormat[imageFormatCount];
            error = GetSupportedImageFormats(context, flags, imageType, imageFormatCount, imageFormats, out imageFormatCount);
            if (error != ErrorCode.Success)
                return new ImageFormat[0];
            
            return imageFormats;
        }

        #endregion
    }
}