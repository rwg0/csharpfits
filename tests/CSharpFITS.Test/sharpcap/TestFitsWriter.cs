using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using nom.tam.fits;
using nom.tam.util;
using NUnit.Framework;

namespace CSharpFITS_v1._1.tests.sharpcap
{
    [TestFixture]
    public class TestFitsWriter
    {
        [Test]
        public void TestWrite8bpp()
        {
            ABitLikeFitsFileWriter ablffw = new ABitLikeFitsFileWriter(8, ABitLikeFitsFileWriter.ColourSpaceId.Mono, 1, new Size(1024, 1024), 1 );

            var cb = 1024*1024;
            var buffer = Marshal.AllocCoTaskMem(cb);
            ablffw.WriteFrameImpl(buffer, cb, new ABitLikeFitsFileWriter.RawFrameInfo() {Binning = 1, CameraName = "Test", ColourSpace = ABitLikeFitsFileWriter.ColourSpaceId.Mono, ExposureMs = 1.0, Width = 1024, Height = 1024});
        }


        [Test]
        public void TestWrite16bpp()
        {
            ABitLikeFitsFileWriter ablffw = new ABitLikeFitsFileWriter(16, ABitLikeFitsFileWriter.ColourSpaceId.Bayer_BGGR,2, new Size(1024, 1024), 1);

            var cb = 1024 * 1024 * 2;
            var buffer = Marshal.AllocCoTaskMem(cb);
            ablffw.WriteFrameImpl(buffer, cb, new ABitLikeFitsFileWriter.RawFrameInfo() { Binning = 1, CameraName = "Test", ColourSpace = ABitLikeFitsFileWriter.ColourSpaceId.Bayer_BGGR, ExposureMs = 1.0, Width = 1024, Height = 1024 });
        }

        [Test]
        public void TestWrite16bppBig()
        {
            var size = new Size(6000,4000);
            ABitLikeFitsFileWriter ablffw = new ABitLikeFitsFileWriter(16, ABitLikeFitsFileWriter.ColourSpaceId.Bayer_BGGR, 2, size, 1);

            var cb = size.Width * size.Height* 2;
            var buffer = Marshal.AllocCoTaskMem(cb);
            ablffw.WriteFrameImpl(buffer, cb, new ABitLikeFitsFileWriter.RawFrameInfo() { Binning = 1, CameraName = "Test", ColourSpace = ABitLikeFitsFileWriter.ColourSpaceId.Bayer_BGGR, ExposureMs = 1.0, Width = size.Width, Height = size.Height });
        }


        private class ABitLikeFitsFileWriter
        {
            private int _significantBitDepth;
            private ColourSpaceId _colourSpaceId;
            private short _bytesPerPixel;
            private Size _size;
            private int _colourPlanes;

            public ABitLikeFitsFileWriter(int significantBitDepth, ColourSpaceId colourSpaceId, short bytesPerPixel, Size size, int colourPlanes)
            {
                _significantBitDepth = significantBitDepth;
                _colourSpaceId = colourSpaceId;
                _bytesPerPixel = bytesPerPixel;
                _size = size;
                _colourPlanes = colourPlanes;
            }

            public enum ColourSpaceId
            {
                Mono = 0,
                RGB = 1,
                Bayer_RGGB = 2,
                Bayer_GRBG = 3,
                Bayer_GBRG = 4,
                Bayer_BGGR = 5,

                Bayer_CYYM,
                Bayer_YMCY,
                Bayer_MYYC,

                Bayer_Unknown,
            }


            public class RawFrameInfo
            {
                public int Width { get; set; }
                public int Height { get; set; }
                public ColourSpaceId ColourSpace { get; set; }
                public double? ExposureMs { get; set; }
                public int Binning { get; set; }
                public double? PixelSize { get; set; }
                public double? SensorTemp { get; set; }
                public string CameraName { get; set; }
            }

            public void WriteFrameImpl(IntPtr intPtr, int p, RawFrameInfo info)
            {

                object data = MarshalToCLR(intPtr, p);
                Fits fits = new Fits();
                BasicHDU hdu = FitsFactory.HDUFactory(data);
                if ((data is short[][] || data is short[][][]) && _significantBitDepth > 8)
                {
                    hdu.AddValue("BZERO", 32768.0, "");
                }

                AddMetadataToFrame(info, hdu);

                fits.AddHDU(hdu);
                using (FileStream fs = File.Create(Path.GetTempFileName()))
                {
                    using (BufferedDataStream f = new BufferedDataStream(fs))
                    {
                        fits.Write(f);
                    }
                }

            }

            private void AddMetadataToFrame(RawFrameInfo info, BasicHDU hdu)
            {
                if (info == null)
                    info = new RawFrameInfo() {ColourSpace = _colourSpaceId};

                if (info.ExposureMs.HasValue)
                    hdu.AddValue("EXPTIME", info.ExposureMs.Value/1000, "");

                if (info.PixelSize.HasValue)
                {
                    hdu.AddValue("XPIXSZ", info.PixelSize.Value, "");
                    hdu.AddValue("YPIXSZ", info.PixelSize.Value, "");
                }

                if (info.Binning > 0)
                {
                    hdu.AddValue("XBINNING", info.Binning, "");
                    hdu.AddValue("YBINNING", info.Binning, "");
                }

                if (info.SensorTemp.HasValue)
                {
                    hdu.AddValue("CCD-TEMP", info.SensorTemp.Value, "");
                }

                switch (info.ColourSpace)
                {
                    case ColourSpaceId.Mono:
                        break;
                    case ColourSpaceId.RGB:
                        hdu.AddValue("COLORTYP", "RGB", "");
                        break;
                    case ColourSpaceId.Bayer_RGGB:
                        hdu.AddValue("COLORTYP", "RGGB", "");
                        break;
                    case ColourSpaceId.Bayer_GRBG:
                        hdu.AddValue("COLORTYP", "GRBG", "");
                        break;
                    case ColourSpaceId.Bayer_GBRG:
                        hdu.AddValue("COLORTYP", "GBRG", "");
                        break;
                    case ColourSpaceId.Bayer_BGGR:
                        hdu.AddValue("COLORTYP", "BGGR", "");
                        break;
                    case ColourSpaceId.Bayer_CYYM:
                        hdu.AddValue("COLORTYP", "CYYM", "");
                        break;
                    case ColourSpaceId.Bayer_YMCY:
                        hdu.AddValue("COLORTYP", "YMCY", "");
                        break;
                    case ColourSpaceId.Bayer_MYYC:
                        hdu.AddValue("COLORTYP", "MYYC", "");
                        break;
                    case ColourSpaceId.Bayer_Unknown:
                        break;
                }

                hdu.AddValue("SWCREATE", "SharpCap", "");
                hdu.AddValue("DATE-OBS", DateTime.UtcNow.ToString("s"), "");

                if (!string.IsNullOrEmpty(info.CameraName))
                    hdu.AddValue("INSTRUME", info.CameraName, "");
            }


            private object MarshalToCLR(IntPtr intPtr, int iLen)
            {
                return BufferToArray(intPtr, iLen);
            }


            private unsafe object BufferToArray(IntPtr ipUnpacked, int iLen)
            {
                switch (_bytesPerPixel)
                {
                    case 1:
                    {
                        byte[] rawData = new byte[iLen];
                        Marshal.Copy(ipUnpacked, rawData, 0, iLen);
                        return ReshapeArray(rawData);
                    }
                    case 2:
                    {
                        ushort[] rawData = new ushort[iLen/2];
                        fixed (ushort* dest = rawData)
                        {
                            NativeMethods.RtlMoveMemory(new IntPtr(dest), ipUnpacked, (uint) iLen);
                        }
                        return ReshapeArray(rawData);
                    }
                    case 4:
                    {
                        uint[] rawData = new uint[iLen/4];
                        fixed (uint* dest = rawData)
                        {
                            NativeMethods.RtlMoveMemory(new IntPtr(dest), ipUnpacked, (uint) iLen);
                        }
                        return ReshapeArray(rawData);
                    }
                    case 6:
                    {
                        ushort[] rawData = new ushort[iLen/2];
                        fixed (ushort* dest = rawData)
                        {
                            NativeMethods.RtlMoveMemory(new IntPtr(dest), ipUnpacked, (uint) iLen);
                        }
                        return ReshapeArray(rawData);
                    }
                    default:
                        throw new NotImplementedException(String.Format("No conversion from {0} bytes per pixel to FITS",_bytesPerPixel));
                }
            }

            private object ReshapeArray(uint[] rawData)
            {
                int leftShift = 0;
                const int rightShift = 0;

                if (_significantBitDepth < 31)
                    leftShift = 31 - _significantBitDepth;

                if (_colourSpaceId == ColourSpaceId.RGB)
                {
                    int[][][] result = new int[3][][];
                    for (int plane = 0; plane < 3; plane++)
                    {
                        int index = 2 - plane;
                        result[plane] = new int[_size.Height][];
                        for (int j = 0; j < _size.Height; j++)
                        {
                            var width = _size.Width;
                            result[plane][j] = new int[width];
                            for (int i = 0; i < width; i++)
                            {
                                result[plane][j][i] = (int) ((rawData[index] << leftShift) >> rightShift);
                                index += _colourPlanes;
                            }
                        }
                    }

                    return result;
                }
                else
                {
                    int[][] result = new int[_size.Height][];
                    int index = 0;
                    for (int j = 0; j < _size.Height; j++)
                    {
                        var width = _size.Width;
                        result[j] = new int[width];
                        for (int i = 0; i < width; i++)
                        {
                            result[j][i] = (int) ((rawData[index] << leftShift) >> rightShift);
                            index++;
                        }
                    }

                    return result;
                }
            }

            private object ReshapeArray(byte[] rawData)
            {
                int leftShift = 15 - _significantBitDepth;

                if (_colourSpaceId == ColourSpaceId.RGB) // assume RGB48
                {
                    short[][][] result = new short[3][][];
                    for (int plane = 0; plane < 3; plane++)
                    {
                        int index = 2 - plane;
                        result[plane] = new short[_size.Height][];
                        for (int j = 0; j < _size.Height; j++)
                        {
                            var width = _size.Width;
                            result[plane][j] = new short[width];
                            for (int i = 0; i < width; i++)
                            {
                                result[plane][j][i] = (short) ((rawData[index] << leftShift));
                                index += _colourPlanes;
                            }
                        }
                    }

                    return result;
                }
                else
                {
                    short[][] result = new short[_size.Height][];
                    int index = 0;
                    for (int j = 0; j < _size.Height; j++)
                    {
                        var width = _size.Width;
                        result[j] = new short[width];
                        for (int i = 0; i < width; i++)
                        {
                            result[j][i] = (short) ((rawData[index] << leftShift));
                            index++;
                        }
                    }

                    return result;
                }
            }


            private object ReshapeArray(ushort[] rawData)
            {

                const int rightShift = 0;


                int leftShift = 16 - _significantBitDepth;

                if (_colourSpaceId == ColourSpaceId.RGB) // assume RGB48
                {
                    short[][][] result = new short[3][][];
                    for (int plane = 0; plane < 3; plane++)
                    {
                        int index = 2 - plane;
                        result[plane] = new short[_size.Height][];
                        for (int j = 0; j < _size.Height; j++)
                        {
                            var width = _size.Width;
                            result[plane][j] = new short[width];
                            for (int i = 0; i < width; i++)
                            {
                                result[plane][j][i] = (short) (((rawData[index] << leftShift) >> rightShift) - 32768);
                                index += _colourPlanes;
                            }
                        }
                    }

                    return result;
                }
                else
                {
                    short[][] result = new short[_size.Height][];
                    int index = 0;
                    for (int j = 0; j < _size.Height; j++)
                    {
                        var width = _size.Width;
                        result[j] = new short[width];
                        for (int i = 0; i < width; i++)
                        {
                            result[j][i] = (short) (((rawData[index] << leftShift) >> rightShift) - 32768);
                            index++;
                        }
                    }

                    return result;
                }
            }

            private class NativeMethods
            {
                [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), DllImport("kernel32.dll")]
                public static extern void RtlMoveMemory(IntPtr dest, IntPtr src, uint len);

            }
        }
    }
}
