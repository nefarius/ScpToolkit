using System;
using System.Diagnostics;
using HidReport.Contract.Enums;
using HidReport.DsActors;

namespace ScpControl.HidParser
{
    internal static class HidParsers
    {
        public static void ParseDPad(uint val, HidReport.Core.HidReport inputReport)
        {
            switch (val)
            {
                case 0:
                    inputReport.Set(ButtonsEnum.Up);
                    break;
                case 1:
                    inputReport.Set(ButtonsEnum.Up);
                    inputReport.Set(ButtonsEnum.Right);
                    break;
                case 2:
                    inputReport.Set(ButtonsEnum.Right);
                    break;
                case 3:
                    inputReport.Set(ButtonsEnum.Right);
                    inputReport.Set(ButtonsEnum.Down);
                    break;
                case 4:
                    inputReport.Set(ButtonsEnum.Down);
                    break;
                case 5:
                    inputReport.Set(ButtonsEnum.Down);
                    inputReport.Set(ButtonsEnum.Left);
                    break;
                case 6:
                    inputReport.Set(ButtonsEnum.Left);
                    break;
                case 7:
                    inputReport.Set(ButtonsEnum.Left);
                    inputReport.Set(ButtonsEnum.Up);
                    break;
            }
        }


        internal static class Ds3Consts
        {
            public static void ParseDs3(byte[] report, HidReport.Core.HidReport inputReport)
            {
                inputReport.BatteryStatus = (DsBattery)report[38];

                inputReport.Set(ButtonsEnum.Select  , IsBitSet(report[10], 0));
                inputReport.Set(ButtonsEnum.L3      , IsBitSet(report[10], 1));
                inputReport.Set(ButtonsEnum.R3      , IsBitSet(report[10], 2));
                inputReport.Set(ButtonsEnum.Start   , IsBitSet(report[10], 3));
                inputReport.Set(ButtonsEnum.Up      , IsBitSet(report[10], 4));
                inputReport.Set(ButtonsEnum.Right   , IsBitSet(report[10], 5));
                inputReport.Set(ButtonsEnum.Down    , IsBitSet(report[10], 6));
                inputReport.Set(ButtonsEnum.Left    , IsBitSet(report[10], 7));
                inputReport.Set(ButtonsEnum.L2      , IsBitSet(report[11], 0));
                inputReport.Set(ButtonsEnum.R2      , IsBitSet(report[11], 1));
                inputReport.Set(ButtonsEnum.L1      , IsBitSet(report[11], 2));
                inputReport.Set(ButtonsEnum.R1      , IsBitSet(report[11], 3));
                inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[11], 4));
                inputReport.Set(ButtonsEnum.Circle  , IsBitSet(report[11], 5));
                inputReport.Set(ButtonsEnum.Cross   , IsBitSet(report[11], 6));
                inputReport.Set(ButtonsEnum.Square  , IsBitSet(report[11], 7));
                inputReport.Set(ButtonsEnum.Ps      , IsBitSet(report[12], 0));
                //13

                inputReport.Set(AxesEnum.Lx      , report[14]);
                inputReport.Set(AxesEnum.Ly      , report[15]);
                inputReport.Set(AxesEnum.Rx      , report[16]);
                inputReport.Set(AxesEnum.Ry      , report[17]);
                inputReport.Set(AxesEnum.Up      , report[22]);
                inputReport.Set(AxesEnum.Right   , report[23]);
                inputReport.Set(AxesEnum.Down    , report[24]);
                inputReport.Set(AxesEnum.Left    , report[25]);
                inputReport.Set(AxesEnum.L2      , report[26]);
                inputReport.Set(AxesEnum.R2      , report[27]);
                inputReport.Set(AxesEnum.L1      , report[28]);
                inputReport.Set(AxesEnum.R1      , report[29]);
                inputReport.Set(AxesEnum.Triangle, report[30]);
                inputReport.Set(AxesEnum.Circle  , report[31]);
                inputReport.Set(AxesEnum.Cross   , report[32]);
                inputReport.Set(AxesEnum.Square  , report[33]);

                int accelerometerX = (report[49] << 8) | (report[50] << 0); //0-1023
                Debug.Assert(accelerometerX >= 0);
                Debug.Assert(accelerometerX <= 1023);
                int accelerometerY = (report[51] << 8) | (report[52] << 0); //0-1023
                Debug.Assert(accelerometerY >= 0);
                Debug.Assert(accelerometerY <= 1023);
                int accelerometerZ = (report[53] << 8) | (report[54] << 0); //0-1023
                Debug.Assert(accelerometerZ >= 0);
                Debug.Assert(accelerometerZ <= 1023);
                int gyrometerX = (report[55] << 8) | (report[56] << 0); //0-1023
                Debug.Assert(gyrometerX >= 0);
                Debug.Assert(gyrometerX <= 1023);
                accelerometerX -= 512;
                accelerometerY -= 512;
                accelerometerZ -= 512;
                gyrometerX -= 498;

                const int g1Value = 115;
                int forceVectror = (int)
                    Math.Sqrt(accelerometerX * accelerometerX + accelerometerY * accelerometerY +
                              accelerometerZ * accelerometerZ);

                // http://www.instructables.com/id/Accelerometer-Gyro-Tutorial/
                //TODO: use Kalman filter
                double yaw = 0;
                double pitch = (Math.Atan2(accelerometerZ, accelerometerY) + Math.PI/2)/Math.PI*180;
                double roll  = (Math.Atan2(accelerometerZ, accelerometerX) + Math.PI/2)/Math.PI*180;
                accelerometerX -= (accelerometerX * g1Value) / forceVectror;
                accelerometerY -= (accelerometerY * g1Value) / forceVectror;
                accelerometerZ -= (accelerometerZ * g1Value) / forceVectror;

                //Debug.Print($"Ax {accelerometerX:+0000;-0000} Ay {accelerometerY:+0000;-0000} Az {accelerometerZ:+0000;-0000} Pitch {pitch} Roll {roll} Gx {gyrometerX:+0000;-0000}");

                inputReport.MotionMutable = new DsAccelerometer()
                {
                    X = (short)(accelerometerX),
                    Y = (short)(accelerometerY),
                    Z = (short)(accelerometerZ),
                };
                inputReport.OrientationMutable = new DsGyroscope()
                {
                    Yaw = (short) (yaw ),
                    Pitch = (short) (pitch ),
                    Roll = (short) (roll)
                };
            }
        }

        internal static class Ds4Consts
        {
            public static DsBattery MapBattery(byte value)
            {
                switch (value)
                {
                    case 0x10:
                    case 0x11:
                    case 0x12:
                    case 0x13:
                    case 0x14:
                    case 0x15:
                    case 0x16:
                    case 0x17:
                    case 0x18:
                    case 0x19:
                    case 0x1A:
                        return DsBattery.Charging;
                    case 0x1B:
                        return DsBattery.Charged;
                }

                return DsBattery.None;
            }

            //input

            public static void ParseDs4(byte[] report, HidReport.Core.HidReport inputReport)
            {
                inputReport.BatteryStatus = (DsBattery)((byte)((report[38] + 2) / 2));

                var val = (uint)report[13] & 0xF;
                ParseDPad(val, inputReport);

                inputReport.Set(ButtonsEnum.Square  , IsBitSet(report[13], 4 ));
                inputReport.Set(ButtonsEnum.Cross   , IsBitSet(report[13], 5 ));
                inputReport.Set(ButtonsEnum.Circle  , IsBitSet(report[13], 6 ));
                inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[13], 7 ));
                inputReport.Set(ButtonsEnum.L1      , IsBitSet(report[14], 0));
                inputReport.Set(ButtonsEnum.R1      , IsBitSet(report[14], 1));
                inputReport.Set(ButtonsEnum.L2      , IsBitSet(report[14], 2));
                inputReport.Set(ButtonsEnum.R2      , IsBitSet(report[14], 3));
                inputReport.Set(ButtonsEnum.Share   , IsBitSet(report[14], 4));
                inputReport.Set(ButtonsEnum.Options , IsBitSet(report[14], 5));
                inputReport.Set(ButtonsEnum.L3      , IsBitSet(report[14], 6));
                inputReport.Set(ButtonsEnum.R3      , IsBitSet(report[14], 7));
                inputReport.Set(ButtonsEnum.Ps      , IsBitSet(report[15], 0));
                inputReport.Set(ButtonsEnum.Touchpad, IsBitSet(report[15], 1));

                inputReport.Set(AxesEnum.Lx, report[9]);
                inputReport.Set(AxesEnum.Ly, report[10]);
                inputReport.Set(AxesEnum.Rx, report[11]);
                inputReport.Set(AxesEnum.Ry, report[12]);
                inputReport.Set(AxesEnum.L2, report[16]);
                inputReport.Set(AxesEnum.R2, report[17]);

                inputReport.MotionMutable = new DsAccelerometer()
                {
                    Y = (short)((report[22] << 8) | report[21]),
                    X = (short)-((report[24] << 8) | report[23]),
                    Z = (short)-((report[26] << 8) | report[25])
                };

                inputReport.OrientationMutable = new DsGyroscope()
                {
                    Roll = (short) -((report[28] << 8) | report[27]),
                    Yaw = (short) ((report[30] << 8) | report[29]),
                    Pitch = (short)((report[32] << 8) | report[31])
                };
                inputReport.TrackPadTouch0Mutable = new DsTrackPadTouch()
                {
                    Id = report[43] & 0x7f,
                    IsActive = report[43] >> 7 == 0,
                    X = ((report[45] & 0x0f) << 8) | report[44],
                    Y = report[46] << 4 | ((report[45] & 0xf0) >> 4)
                };
                inputReport.TrackPadTouch1Mutable = new DsTrackPadTouch()
                {
                    Id = report[47] & 0x7f,
                    IsActive = report[47] >> 7 == 0,
                    X = ((report[49] & 0x0f) << 8) | report[48],
                    Y = report[50] << 4 | ((report[49] & 0xf0) >> 4)

                };
            }
        }
        public static bool IsBitSet(byte value, int offset)
        {
            return ((value >> offset) & 1) == 0x01;
        }

        public static bool IsQuickDisconnect(this HidReport.Core.HidReport inputReport)
        {
            // detect Quick Disconnect combo (L1, R1 and PS buttons pressed at the same time)
            if (inputReport[ButtonsEnum.L1].IsPressed
                && inputReport[ButtonsEnum.R1].IsPressed
                && inputReport[ButtonsEnum.Ps].IsPressed)
            {
                // unset PS button
                inputReport.Unset(ButtonsEnum.Ps);
                return true;
            }
            return false;
        }
    }
}
