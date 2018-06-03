using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;

using NUnit.Framework;

using nom.tam.fits;
using nom.tam.image;
using nom.tam.util;

namespace nom.tam.fits
{
    [TestFixture]
    public class AsciiTableTest
    {
        Object[] GetSampleCols()
        {
            float[] realCol = new float[50];
            for (int i = 0; i < realCol.Length; i += 1)
            {
                realCol[i] = 10000F * (i) * (i) * (i) + 1;
            }
            int[] intCol = Array.ConvertAll<float, int>(realCol, Convert.ToInt32);
            long[] longCol = Array.ConvertAll<float, Int64>(realCol, Convert.ToInt64);
            double[] doubleCol = Array.ConvertAll<float, double>(realCol, Convert.ToDouble);
            String[] strCol = new String[realCol.Length];

            for (int i = 0; i < realCol.Length; i += 1)
            {
                strCol[i] = "ABC" + (realCol[i]) + "CDE";
            }

            System.Console.Out.WriteLine("**** Create a table from the data kernel****");

            return new Object[] { realCol, intCol, longCol, doubleCol, strCol };
        }

        Fits MakeAsciiTable()
        {
            Object[] obj = GetSampleCols();

            // Create new AsciiTable
            Fits f = new Fits();
            f.AddHDU(Fits.MakeHDU(obj));
            return f;
        }

        void WriteFile(Fits f, String name)
        {
            BufferedFile bf = new BufferedFile(name, FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Flush();
            bf.Close();
           
        }

        [Test]
        public void TestAsciiTable()
        {
            CreateByColumn();
            CreateByRow();
            ReadByRow();
            ReadByColumn();
            ReadByElement();
            ModifyTable();
            Delete();
        }

        public void CreateByColumn()
        {
            Fits f = MakeAsciiTable();
            WriteFile(f, "at1.fits");
           
            System.Console.Out.WriteLine("\n\n**** Create a table column by Column ****");

            f = new Fits("at1.fits", FileAccess.Read);
            AsciiTableHDU hdu = (AsciiTableHDU)f.GetHDU(1);

            Object[] inputs = GetSampleCols();
            Object[] outputs = (Object[])hdu.Kernel;

            for (int i = 0; i < 50; i += 1)
            {
                ((String[])outputs[4])[i] = ((String[])outputs[4])[i].Trim();
            }

            for (int j = 0; j < 5; j += 1)
            {
                Assertion.AssertEquals("ByCol:" + j,
                                        true,
                                         ArrayFuncs.ArrayEquals(inputs[j], outputs[j], Math.Pow(10, -6), Math.Pow(10, -14)));
            }
          
        }

        Object[] GetRow(int i)
        {
            return new Object[] { new int[] { i }, new float[] { i }, new String[] { "Str" + i } };
        }

        Object[] GetRowBlock(int max)
        {
            Object[] o = new Object[] { new int[max], new float[max], new String[max] };
            for (int i = 0; i < max; i += 1)
            {
                ((int[])o[0])[i] = i;
                ((float[])o[1])[i] = i;
                ((String[])o[2])[i] = "Str" + i;
            }
            return o;
        }

        public void CreateByRow()
        {
            Fits f = new Fits();
            AsciiTable data = new AsciiTable();
            Object[] row = new Object[4];

            System.Console.Out.WriteLine("\n\n**** Create a table column by Row ****");

            for (int i = 0; i < 50; i += 1)
            {
                data.AddRow(GetRow(i));
            }

            f.AddHDU(Fits.MakeHDU(data));

            WriteFile(f, "at2.fits");
          
            // Read it back.
            f = new Fits("at2.fits", FileAccess.Read);

            Object[] output = (Object[])f.GetHDU(1).Kernel;
            Object[] input = GetRowBlock(50);

            for (int i = 0; i < 50; i += 1)
            {
                String[] str = (String[])output[2];
                String[] istr = (String[])input[2];
                int len1 = str[1].Length;
                str[i] = str[i].Trim();
                // The first row would have set the length for all the
                // remaining rows...
                if (istr[i].Length > len1)
                {
                    istr[i] = istr[i].Substring(0, len1);
                }
            }

            for (int j = 0; j < 3; j += 1)
            {
                Assertion.AssertEquals("ByRow:" + j, true, ArrayFuncs.ArrayEquals(input[j], output[j], Math.Pow(10, -6), Math.Pow(10, -14)));
            }
            
        }

        public void ReadByRow()
        {
            Fits f = new Fits("at1.fits", FileAccess.Read);
            Object[] cols = GetSampleCols();

            AsciiTableHDU hdu = (AsciiTableHDU)f.GetHDU(1);
            AsciiTable data = (AsciiTable)hdu.GetData();

            Console.WriteLine("Reading Table by Row");
            for (int i = 0; i < data.NRows; i += 1)
            {
                Assertion.AssertEquals("Rows:" + i, 50, data.NRows);
                Object[] row = (Object[])data.GetRow(i);
                Assertion.AssertEquals("Ascii Rows: float" + i, 1f, ((float[])cols[0])[i] / ((float[])row[0])[0], Math.Pow(10, -6));
                Assertion.AssertEquals("Ascii Rows: int" + i, ((int[])cols[1])[i], ((int[])row[1])[0]);
                Assertion.AssertEquals("Ascii Rows: long" + i, ((long[])cols[2])[i], ((long[])row[2])[0]);
                Assertion.AssertEquals("Ascii Rows: double" + i, 1f, ((double[])cols[3])[i] / ((double[])row[3])[0], Math.Pow(10, -14));
                String[] st = (String[])row[4];
                st[0] = st[0].Trim();
                Assertion.AssertEquals("Ascii Rows: Str" + i, ((String[])cols[4])[i], ((String[])row[4])[0]);
            }
        }

        public void ReadByColumn()
        {
            Fits f = new Fits("at1.fits", FileAccess.Read);
            AsciiTableHDU hdu = (AsciiTableHDU)f.GetHDU(1);
            AsciiTable data = (AsciiTable)hdu.GetData();
            Object[] cols = GetSampleCols();

            Assertion.AssertEquals("Number of rows", data.NRows, 50);
            Assertion.AssertEquals("Number of columns", data.NCols, 5);

            for (int j = 0; j < data.NCols; j += 1)
            {
                Object col = data.GetColumn(j);
                if (j == 4)
                {
                    String[] st = (String[])col;
                    for (int i = 0; i < st.Length; i += 1)
                    {
                        st[i] = st[i].Trim();
                    }
                }
                Assertion.AssertEquals("Ascii Columns:" + j, true, ArrayFuncs.ArrayEquals(cols[j], col, Math.Pow(10, -6), Math.Pow(10, -14)));
            }
        
        }

        public void ReadByElement()
        {
            Fits f = new Fits("at2.fits", FileAccess.Read);
            AsciiTableHDU hdu = (AsciiTableHDU)f.GetHDU(1);
            AsciiTable data = (AsciiTable)hdu.GetData();


            for (int i = 0; i < data.NRows; i += 1)
            {
                Object[] row = (Object[])data.GetRow(i);
                for (int j = 0; j < data.NCols; j += 1)
                {
                    Object val = data.GetElement(i, j);
                    Assertion.AssertEquals("Ascii readElement", true, ArrayFuncs.ArrayEquals(val, row[j]));
                }
            }
         
        }

        public void ModifyTable()
        {
            Fits f = new Fits("at1.fits", FileAccess.ReadWrite);

            Object[] samp = GetSampleCols();

            AsciiTableHDU hdu = (AsciiTableHDU)f.GetHDU(1);
            AsciiTable data = (AsciiTable)hdu.GetData();
            float[] f1 = (float[])data.GetColumn(0);
            float[] f2 = (float[])f1.Clone();
            for (int i = 0; i < f2.Length; i += 1)
            {
                f2[i] = 2 * f2[i];
            }

            data.SetColumn(0, f2);
            f1 = new float[] { 3.14159f };
            data.SetElement(3, 0, f1);

            hdu.SetNullString(0, "**INVALID**");
            data.SetNull(5, 0, true);
            data.SetNull(6, 0, true);

            Object[] row = new Object[5];
            row[0] = new float[] { 6.28f };
            row[1] = new int[] { 22 };
            row[2] = new long[] { 0 };
            row[3] = new double[] { -3 };
            row[4] = new String[] { "A string" };

            data.SetRow(5, row);

            data.SetElement(4, 2, new long[] { 54321 });

            BufferedFile bf = new BufferedFile("at1x.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Flush();
            bf.Close();
            // f.Write("at1x.fits");
            f = new Fits("at1x.fits", FileAccess.Read);
            AsciiTable tab = (AsciiTable)f.GetHDU(1).Data;
            Object[] kern = (Object[])tab.Kernel;

            float[] fx = (float[])kern[0];
            int[] ix = (int[])kern[1];
            long[] lx = (long[])kern[2];
            double[] dx = (double[])kern[3];
            String[] sx = (String[])kern[4];

            float[] fy = (float[])samp[0];
            int[] iy = (int[])samp[1];
            long[] ly = (long[])samp[2];
            double[] dy = (double[])samp[3];
            String[] sy = (String[])samp[4];

            Assertion.AssertEquals("Null", true, tab.IsNull(6, 0));
            Assertion.AssertEquals("Null2", false, tab.IsNull(5, 0));

            for (int i = 0; i < data.NRows; i += 1)
            {
                if (i != 5)
                {
                    if (i != 6)
                    {
                        // Null
                        Assertion.AssertEquals("f" + i, 1f, f2[i] / fx[i], Math.Pow(10, -6));
                    }
                    Assertion.AssertEquals("i" + i, iy[i], ix[i]);

                    if (i == 4)
                    {
                        Assertion.AssertEquals("l4", 54321L, lx[i]);
                    }
                    else
                    {
                        Assertion.AssertEquals("l" + i, ly[i], lx[i]);
                    }

                    Assertion.AssertEquals("d" + i, 1f, dy[i] / dx[i], Math.Pow(10, -14));
                    Assertion.AssertEquals("s" + i, sy[i], sx[i].Trim());
                }
            }
            Object[] r5 = (Object[])data.GetRow(5);
            String[] st = (String[])r5[4];
            st[0] = st[0].Trim();
            for (int i = 0; i < r5.Length; i++)
            {
                Assertion.AssertEquals("row5", true, ArrayFuncs.ArrayEquals(row[i], r5[i], Math.Pow(10, -6), Math.Pow(10, -14)));

            }
       
            //Assertion.AssertEquals("row5", true, ArrayFuncs.ArrayEquals(row, r5, Math.Pow(10,-6), Math.Pow(10,-14)));
        }

        public void Delete()
        {

            Fits f = new Fits("at1.fits", FileAccess.ReadWrite);

            TableHDU th = (TableHDU)f.GetHDU(1);
            Assertion.AssertEquals("delrBef", 50, th.NRows);

            th.DeleteRows(2, 2);
            Assertion.AssertEquals("delrAft", 48, th.NRows);

            BufferedFile bf = new BufferedFile("at1y.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Close();
            f.Close();

            f = new Fits("at1y.fits");
            th = (TableHDU)f.GetHDU(1);
            Assertion.AssertEquals("delrAft2", 48, th.NRows);

            Assertion.AssertEquals("delcBef", 5, th.NCols);
            th.DeleteColumnsIndexZero(3, 2);
            Assertion.AssertEquals("delcAft1", 3, th.NCols);

            th.DeleteColumnsIndexZero(0, 2);
            Assertion.AssertEquals("delcAft2", 1, th.NCols);
            bf = new BufferedFile("at1z.fits", FileAccess.ReadWrite, FileShare.ReadWrite);

            f.Write(bf);
            bf.Close();
            f.Close();

            f = new Fits("at1z.fits");
            th = (TableHDU)f.GetHDU(1);
            Assertion.AssertEquals("delcAft3", 1, th.NCols);
            f.Close();
        }

    }

    public class Assertion
    {
        public static void AssertEquals(string message, int expected, int actual)
        {
            Assert.AreEqual(expected, actual, message);
        }

        public static void AssertEquals(string message, Type expected, Type actual)
        {
            Assert.AreEqual(expected, actual, message);
        }

        public static void AssertEquals(string message, object expected, object actual)
        {
            Assert.AreEqual(expected, actual, message);
        }

        internal static void AssertEquals(string message, long expected, long actual)
        {
            Assert.AreEqual(expected, actual, message);
        }

        internal static void AssertEquals(string message, string expected, string actual)
        {
            Assert.AreEqual(expected, actual, message);
        }

        internal static void AssertEquals(string message, bool expected, bool actual)
        {
            Assert.AreEqual(expected, actual, message);
        }

        internal static void AssertEquals(string message, double expected, double actual, double delta = 0)
        {
            Assert.AreEqual(expected, actual, delta, message );
        }

        internal static void AssertEquals(string message, float expected, float actual, double delta = 0)
        {
            Assert.AreEqual(expected, actual, delta, message);
        }

        public static void AssertNull(string message, object o)
        {
            Assert.IsNull(o, message);
        }
    }
}
