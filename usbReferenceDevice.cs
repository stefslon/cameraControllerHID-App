//-----------------------------------------------------------------------------
//
//  usbReferenceDevices.cs
//
//  WFF GenericHID Test Application (Version 4_0)
//  A test application for the WFF GenericHID Communication Library
//
//  Copyright (c) 2011 Simon Inns
//
//  Permission is hereby granted, free of charge, to any person obtaining a
//  copy of this software and associated documentation files (the "Software"),
//  to deal in the Software without restriction, including without limitation
//  the rights to use, copy, modify, merge, publish, distribute, sublicense,
//  and/or sell copies of the Software, and to permit persons to whom the
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//  DEALINGS IN THE SOFTWARE.
//
//  Web:    http://www.waitingforfriday.com
//  Email:  simon.inns@gmail.com
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The following namespace allows debugging output (when compiled in debug mode)
using System.Diagnostics;

namespace cameraControllerHID
{
    using WFF_GenericHID_Communication_Library;


    /// <summary>
    /// This class performs several different tests against the 
    /// reference hardware/firmware to confirm that the USB
    /// communication library is functioning correctly.
    /// 
    /// It also serves as a demonstration of how to use the class
    /// library to perform different types of read and write
    /// operations.
    /// </summary>
    class usbReferenceDevice : WFF_GenericHID_Communication_Library
    {
        /// <summary>
        /// Class constructor - place any initialisation here
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="pid"></param>
        public usbReferenceDevice(int vid, int pid, int page)
            : base(vid, pid, page)
        {
            cameraName = "";
        }

        public string cameraName;

        public bool transmit(usbCmd txCommand)
        {
            if (isDeviceAttached) { 
                // Perform the write command
                bool success;
                byte[] temp = txCommand.ToByteArray();
                success = writeSingleReportToDevice(temp);
                return success;
            }
            return false;
        }

        public bool receive(ref usbCmd rxCommand)
        {
            if (isDeviceAttached)
            {
                // Perform the read command
                Byte[] inputReportBuffer = new Byte[8];
                bool success;
                success = readSingleReportFromDevice(ref inputReportBuffer);
                rxCommand.FromByteArray(inputReportBuffer);
                return success;
            }
            return false;
        }

        public string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            return (sb.ToString());
        }

        public bool step(short panDirection, short tiltDirection)
        {
            usbCmd txBuffer = new usbCmd();

            txBuffer.cmd = (byte)usbCmd.cmds.Step;
            txBuffer.payload0 = (short)((panDirection == 0) ? 0 : (panDirection > 0 ? +1 : -1));
            txBuffer.payload1 = (short)((tiltDirection == 0) ? 0 : (tiltDirection > 0 ? +1 : -1));
            txBuffer.payload2 = 0;

            return transmit(txBuffer);
        }

        public bool run(short panSpeedDirection, short tiltSpeedDirection)
        {
            // Advance pan/tilt by a single step
            usbCmd txBuffer = new usbCmd();

            txBuffer.cmd = (byte)usbCmd.cmds.Run;
            txBuffer.payload0 = panSpeedDirection;
            txBuffer.payload1 = tiltSpeedDirection;
            txBuffer.payload2 = 0;

            return transmit(txBuffer);
        }

        public bool stop()
        {
            usbCmd txBuffer = new usbCmd();

            txBuffer.cmd = (byte)usbCmd.cmds.Stop;
            txBuffer.payload0 = 0;
            txBuffer.payload1 = 0;
            txBuffer.payload2 = 0;

            return transmit(txBuffer);
        }

        public bool setPreset(short presetNumber)
        {
            usbCmd txBuffer = new usbCmd();

            txBuffer.cmd = (byte)usbCmd.cmds.SetPS;
            txBuffer.payload0 = presetNumber;
            txBuffer.payload1 = 0;
            txBuffer.payload2 = 0;

            return transmit(txBuffer);
        }

        public bool goPreset(short presetNumber, ushort msecToComplete)
        {
            usbCmd txBuffer = new usbCmd();

            txBuffer.cmd = (byte)usbCmd.cmds.GoPS;
            txBuffer.payload0 = presetNumber;
            txBuffer.payload1 = (short)msecToComplete;
            txBuffer.payload2 = 0;

            return transmit(txBuffer);
        }

        public bool gotoPosition(short panPosition, short tiltPosition, ushort msecToComplete)
        {
            usbCmd txBuffer = new usbCmd();

            txBuffer.cmd = (byte)usbCmd.cmds.SetPos;
            txBuffer.payload0 = panPosition;
            txBuffer.payload1 = tiltPosition;
            txBuffer.payload2 = (short)msecToComplete;

            return transmit(txBuffer);
        }

        public bool getPosition(ref short panPosition, ref short tiltPosition)
        {
            usbCmd txBuffer = new usbCmd();
            usbCmd rxBuffer = new usbCmd();
            bool success;

            txBuffer.cmd = (byte)usbCmd.cmds.GetPos;
            txBuffer.payload0 = 0;
            txBuffer.payload1 = 0;
            txBuffer.payload2 = 0;

            success = transmit(txBuffer);

            if (success)
            {
                success = receive(ref rxBuffer);
                if (rxBuffer.cmd == (byte)usbCmd.cmds.GetPosC)
                {
                    panPosition = rxBuffer.payload0;
                    tiltPosition = rxBuffer.payload1;
                    return true;
                }
            }

            return false;
        }

        public bool reCalibrate()
        {
            usbCmd txBuffer = new usbCmd();

            txBuffer.cmd = (byte)usbCmd.cmds.ReCal;
            txBuffer.payload0 = 0;
            txBuffer.payload1 = 0;
            txBuffer.payload2 = 0;

            return transmit(txBuffer);
        }

        public string getName()
        {
            string deviceName = "";
            StringBuilder dnStr = new StringBuilder();

            bool success;
            usbCmd txBuffer = new usbCmd();
            usbCmd rxBuffer = new usbCmd();
            int numChars;
            int numPacks;

            txBuffer.cmd = (byte)usbCmd.cmds.GetName;
            txBuffer.payload0 = 0;
            txBuffer.payload1 = 0;
            txBuffer.payload2 = 0;

            success = transmit(txBuffer);

            if (success)
            {
                success = receive(ref rxBuffer);
                if (rxBuffer.cmd == (byte)usbCmd.cmds.GetNameC)
                {
                    numChars = rxBuffer.payload0;
                    if (numChars>0)
                    {
                        numPacks = (int)Math.Ceiling((float)numChars / 3.0);
                        for (int ic = 0; ic < numPacks; ic++)
                        {
                            receive(ref rxBuffer);
                            if (ic * 3 < numChars) dnStr.Append((char)rxBuffer.payload0);
                            if (ic * 3 + 1 < numChars) dnStr.Append((char)rxBuffer.payload1);
                            if (ic * 3 + 2 < numChars) dnStr.Append((char)rxBuffer.payload2);
                        }
                    }
                }
            }

            deviceName = dnStr.ToString();
            return deviceName;
        }

        public bool setName(string deviceName)
        {
            bool success;

            usbCmd txBuffer = new usbCmd();
            usbCmd rxBuffer = new usbCmd();
            int numChars;
            int numPacks;

            numChars = Math.Max(Math.Min(deviceName.Length, 30), 0);
            numPacks = (int)Math.Ceiling((float)numChars / 3.0);

            txBuffer.cmd = (byte)usbCmd.cmds.SetName;
            txBuffer.payload0 = (short)numChars;
            txBuffer.payload1 = (short)numPacks;
            txBuffer.payload2 = 0;

            success = transmit(txBuffer);

            if (success)
            {
                success = receive(ref rxBuffer);
                if (rxBuffer.cmd == (byte)usbCmd.cmds.SetNameC)
                {
                    txBuffer.cmd = (byte)usbCmd.cmds.SetNameC;
                    for (int ic = 0; ic < numPacks; ic++)
                    {
                        if (ic * 3 < numChars) txBuffer.payload0 = (byte)deviceName[ic * 3];
                        if (ic * 3 + 1 < numChars) txBuffer.payload1 = (byte)deviceName[ic * 3 + 1];
                        if (ic * 3 + 2 < numChars) txBuffer.payload2 = (byte)deviceName[ic * 3 + 2];

                        success = transmit(txBuffer);
                    }

                }
            }

            return true;
        }
    }
}
