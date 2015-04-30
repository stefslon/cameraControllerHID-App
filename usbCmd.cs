using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace cameraControllerHID
{
    class usbCmd
    {
        public enum cmds : byte
        {
            Run = 0x01,
            Step = 0x11,
            Stop = 0xff,
            GetName = 0x21,
            GetNameC = 0x22,
            SetName = 0x23,
            SetNameC = 0x24,
            GetPS = 0x31,
            GetPSC = 0x32,
            SetPS = 0x33,
            SetPos = 0x35,
            GoPS = 0x36,
            GetPos = 0x37,
            GetPosC = 0x38,
            ReCal = 0x50,
            Ping = 0x99
        };

        // Total packet size: 8 bytes
        public byte cmd;            // 1 byte
        public short payload0;      // 3 x 2 bytes
        public short payload1;
        public short payload2;

        // Convert an object to a byte array
        public byte[] ToByteArray()
        {
            byte[] byteArray = new byte[8];

            byteArray[0] = cmd;
            byteArray[1] = (byte)(payload0 & 255);
            byteArray[2] = (byte)(payload0 >> 8);
            byteArray[3] = (byte)(payload1 & 255);
            byteArray[4] = (byte)(payload1 >> 8);
            byteArray[5] = (byte)(payload2 & 255);
            byteArray[6] = (byte)(payload2 >> 8);
            byteArray[7] = 0;   // padding byte

            return byteArray;
        }

        // Convert a byte array to an Object
        public void FromByteArray(byte[] arrBytes)
        {
            cmd = arrBytes[0];
            payload0 = (short)((arrBytes[2] << 8) + arrBytes[1]);
            payload1 = (short)((arrBytes[4] << 8) + arrBytes[3]);
            payload2 = (short)((arrBytes[6] << 8) + arrBytes[5]);
        }
    }
}
