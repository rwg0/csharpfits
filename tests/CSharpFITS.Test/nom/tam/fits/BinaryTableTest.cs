
using System;
using System.Collections.Generic;
using System.Text;
using nom.tam.fits;
using nom.tam.util;
using System.IO;
using NUnit.Framework;

namespace nom.tam.fits
{
    [TestFixture]
    public class BinaryTableTester
    {

        byte[] bytes = new byte[50];
        byte[][] bits = new byte[50][];
        bool[] bools = new bool[50];

        short[][] shorts = new short[50][];

        int[] ints = new int[50];
        float[][][] floats = new float[50][][];

        double[] doubles = new double[50];
        long[] longs = new long[50];
        String[] strings = new String[50];

        float[][] vf = new float[50][];
        short[][] vs = new short[50][];
        double[][] vd = new double[50][];
        bool[][] vbool = new bool[50][];
        
        [OneTimeSetUp]
        public void Initialize()
        {

            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = new byte[2];
            }

            for (int i = 0; i < shorts.Length; i++)
            {
                shorts[i] = new short[3];
            }

            for (int i = 0; i < floats.Length; i++)
            {
                floats[i] = new float[4][];
            }
            for (int i = 0; i < floats.Length; i++)
            {
                for (int j = 0; j < floats[i].Length; j++)
                {
                    floats[i][j] = new float[4];
                }
            }

            for (int i = 0; i < bytes.Length; i += 1)
            {
                bytes[i] = (byte)(2 * i);
                bits[i][0] = bytes[i];
                bits[i][1] = (byte)(~bytes[i]);
                bools[i] = (bytes[i] % 8) == 0 ? true : false;

                shorts[i][0] = (short)(2 * i);
                shorts[i][1] = (short)(3 * i);
                shorts[i][2] = (short)(4 * i);

                ints[i] = i * i;
                for (int j = 0; j < 4; j += 1)
                {
                    for (int k = 0; k < 4; k += 1)
                    {
                        floats[i][j][k] = (float)(i + j * Math.Exp(k));
                    }
                }
                doubles[i] = 3 * Math.Sin(i);
                longs[i] = i * i * i * i;
                strings[i] = "abcdefghijklmnopqrstuvwxzy".Substring(0, i % 20);

                vf[i] = new float[i + 1];
                vf[i][i / 2] = i * 3;
                vs[i] = new short[i / 10 + 1];
                vs[i][i / 10] = (short)-i;
                vd[i] = new double[i % 2 == 0 ? 1 : 2];
                vd[i][0] = 99.99;
                vbool[i] = new bool[i / 10];
                if (i >= 10)
                {
                    vbool[i][0] = i % 2 == 1;
                }
            }
        }

        [Test]
        public void TestSimpleIO()
        {
            FitsFactory.UseAsciiTables = false;

            Fits f = new Fits();
            Object[] data = new Object[]{bytes, bits, bools, shorts, ints,
	    floats, doubles, longs, strings};
            f.AddHDU(Fits.MakeHDU(data));

            BinaryTableHDU bhdu = (BinaryTableHDU)f.GetHDU(1);
            bhdu.SetColumnName(0, "bytes", null);
            bhdu.SetColumnName(1, "bits", "bits later on");
            bhdu.SetColumnName(6, "doubles", null);
            bhdu.SetColumnName(5, "floats", "4 x 4 array");

            BufferedFile bf = new BufferedFile("bt1.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Flush();
            bf.Close();
         
            f = new Fits("bt1.fits");
            f.Read();

            Assertion.AssertEquals("NHDU", 2, f.NumberOfHDUs);


            BinaryTableHDU thdu = (BinaryTableHDU)f.GetHDU(1);
            Header hdr = thdu.Header;

            Assertion.AssertEquals("HDR1", 9, hdr.GetIntValue("TFIELDS"));
            Assertion.AssertEquals("HDR2", 2, hdr.GetIntValue("NAXIS"));
            Assertion.AssertEquals("HDR3", 8, hdr.GetIntValue("BITPIX"));
            Assertion.AssertEquals("HDR4", "BINTABLE", hdr.GetStringValue("XTENSION"));
            Assertion.AssertEquals("HDR5", "bytes", hdr.GetStringValue("TTYPE1"));
            Assertion.AssertEquals("HDR6", "doubles", hdr.GetStringValue("TTYPE7"));

            for (int i = 0; i < data.Length; i += 1)
            {
                Object col = thdu.GetColumn(i);
                if (i == 8)
                {
                    String[] st = (String[])col;

                    for (int j = 0; j < st.Length; j += 1)
                    {
                        st[j] = st[j].Trim();
                    }
                }
                Assertion.AssertEquals("Data" + i, true, ArrayFuncs.ArrayEquals(data[i], col));
            }
            f.Close();
        }

        [Test]
        public void TestRowDelete()
        {
            Fits f = new Fits("bt1.fits");
            f.Read();

            BinaryTableHDU thdu = (BinaryTableHDU)f.GetHDU(1);

            Assertion.AssertEquals("Del1", 50, thdu.NRows);
            thdu.DeleteRows(10, 20);
            Assertion.AssertEquals("Del2", 30, thdu.NRows);

            double[] dbl = (double[])thdu.GetColumn(6);
            Assertion.AssertEquals("del3", dbl[9], doubles[9]);
            Assertion.AssertEquals("del4", dbl[10], doubles[30]);

            BufferedFile bf = new BufferedFile("bt1x.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Close();
            f.Close();

            f = new Fits("bt1x.fits");
            f.Read();
            thdu = (BinaryTableHDU)f.GetHDU(1);
            dbl = (double[])thdu.GetColumn(6);
            Assertion.AssertEquals("del5", 30, thdu.NRows);
            Assertion.AssertEquals("del6", 9, thdu.NCols);
            Assertion.AssertEquals("del7", dbl[9], doubles[9]);
            Assertion.AssertEquals("del8", dbl[10], doubles[30]);

            thdu.DeleteRows(20);
            Assertion.AssertEquals("del9", 20, thdu.NRows);
            dbl = (double[])thdu.GetColumn(6);
            Assertion.AssertEquals("del10", 20, dbl.Length);
            Assertion.AssertEquals("del11", dbl[0], doubles[0]);
            Assertion.AssertEquals("del12", dbl[19], doubles[39]);
            f.Close();
        }

        [Test]
        public void TestVar()
        {
            Object[] data = new Object[] { floats, vf, vs, vd, shorts, vbool };
            Fits f = new Fits();
            f.AddHDU(Fits.MakeHDU(data));

            //BufferedDataStream bdos = new BufferedDataStream(new FileStream("bt2.fits", FileMode.Open, FileAccess.ReadWrite));
            BufferedFile bdos = new BufferedFile("bt2.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bdos);
            bdos.Close();

            f = new Fits("bt2.fits", FileAccess.Read);
            f.Read();
            BinaryTableHDU bhdu = (BinaryTableHDU)f.GetHDU(1);
            Header hdr = bhdu.Header;

            Assertion.AssertEquals("var1", true, hdr.GetIntValue("PCOUNT") > 0);
            Assertion.AssertEquals("var2", 6, hdr.GetIntValue("TFIELDS"));

            for (int i = 0; i < data.Length; i += 1)
            {
                Assertion.AssertEquals("vardata(" + i + ")", true, ArrayFuncs.ArrayEquals(data[i], bhdu.GetColumn(i)));
            }
           
        }
        [Test]
        public void TestSet()
        {
            Fits f = new Fits("bt2.fits", FileAccess.Read);
            f.Read();
            BinaryTableHDU bhdu = (BinaryTableHDU)f.GetHDU(1);
            Header hdr = bhdu.Header;

            // Check the various set methods on variable length data.
            float[] dta = (float[])bhdu.GetElement(4, 1);
            dta = new float[] { 22, 21, 20 };
            bhdu.SetElement(4, 1, dta);

            // BufferedDataStream bdos = new BufferedDataStream(new FileStream("bt2a.fits",FileMode.Open));
            BufferedFile bdos = new BufferedFile("bt2a.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bdos);
            bdos.Close();
           
            f = new Fits("bt2a.fits");
            bhdu = (BinaryTableHDU)f.GetHDU(1);
            float[] xdta = (float[])bhdu.GetElement(4, 1);

            Assertion.AssertEquals("ts1", true, ArrayFuncs.ArrayEquals(dta, xdta));
            Assertion.AssertEquals("ts2", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(3, 1), vf[3]));
            Assertion.AssertEquals("ts5", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(5, 1), vf[5]));

            Assertion.AssertEquals("ts4", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(4, 1), dta));

            float[] tvf = new float[] { 101, 102, 103, 104 };
            vf[4] = tvf;

            bhdu.SetColumn(1, vf);
            Assertion.AssertEquals("ts6", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(3, 1), vf[3]));
            Assertion.AssertEquals("ts7", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(4, 1), vf[4]));
            Assertion.AssertEquals("ts8", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(5, 1), vf[5]));

            // bdos = new BufferedDataStream(new FileStream("bt2b.fits",FileMode.Open));
            bdos = new BufferedFile("bt2b.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bdos);
            bdos.Close();
            f.Close();

            f = new Fits("bt2b.fits");
            bhdu = (BinaryTableHDU)f.GetHDU(1);
            Assertion.AssertEquals("ts9", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(3, 1), vf[3]));
            Assertion.AssertEquals("ts10", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(4, 1), vf[4]));
            Assertion.AssertEquals("ts11", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(5, 1), vf[5]));

            Object[] rw = (Object[])bhdu.GetRow(4);

            float[] trw = new float[] { -1, -2, -3, -4, -5, -6 };
            rw[1] = trw;

            bhdu.SetRow(4, rw);
            Assertion.AssertEquals("ts12", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(3, 1), vf[3]));
            Assertion.AssertEquals("ts13", false, ArrayFuncs.ArrayEquals(bhdu.GetElement(4, 1), vf[4]));
            Assertion.AssertEquals("ts14", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(4, 1), trw));
            Assertion.AssertEquals("ts15", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(5, 1), vf[5]));

            // bdos = new BufferedDataStream(new FileStream("bt2c.fits",FileMode.Open));
            bdos = new BufferedFile("bt2c.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bdos);
            bdos.Close();
            f.Close();

            f = new Fits("bt2c.fits");
            bhdu = (BinaryTableHDU)f.GetHDU(1);
            Assertion.AssertEquals("ts16", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(3, 1), vf[3]));
            Assertion.AssertEquals("ts17", false, ArrayFuncs.ArrayEquals(bhdu.GetElement(4, 1), vf[4]));
            Assertion.AssertEquals("ts18", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(4, 1), trw));
            Assertion.AssertEquals("ts19", true, ArrayFuncs.ArrayEquals(bhdu.GetElement(5, 1), vf[5]));
            f.Close();
        }

        [Test]
        public void BuildByColumn()
        {
            BinaryTable btab = new BinaryTable();

            btab.AddColumn(floats);
            btab.AddColumn(vf);
            btab.AddColumn(strings);
            btab.AddColumn(vbool);
            btab.AddColumn(ints);

            Fits f = new Fits();
            f.AddHDU(Fits.MakeHDU(btab));

            // BufferedDataStream bdos = new BufferedDataStream(new FileStream("bt3.fits",FileMode.Open));
            BufferedFile bdos = new BufferedFile("bt3.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bdos);
            bdos.Close();
           
            f = new Fits("bt3.fits");
            BinaryTableHDU bhdu = (BinaryTableHDU)f.GetHDU(1);
            btab = (BinaryTable)bhdu.Data;

            Assertion.AssertEquals("col1", true, ArrayFuncs.ArrayEquals(floats, bhdu.GetColumn(0)));
            Assertion.AssertEquals("col2", true, ArrayFuncs.ArrayEquals(vf, bhdu.GetColumn(1))); // problem is here only

            String[] col = (String[])bhdu.GetColumn(2);
            for (int i = 0; i < col.Length; i += 1)
            {
                col[i] = col[i].Trim();
            }
            Assertion.AssertEquals("coi3", true, ArrayFuncs.ArrayEquals(strings, col));

            Assertion.AssertEquals("col4", true, ArrayFuncs.ArrayEquals(vbool, bhdu.GetColumn(3)));
            Assertion.AssertEquals("col5", true, ArrayFuncs.ArrayEquals(ints, bhdu.GetColumn(4)));
            f.Close();
        }

       [Test]
        [Ignore("Fails with 'row6' i=54 on code from v1.1")]
        public void BuildByRow()
        {
            Fits f = new Fits("bt2.fits", FileAccess.Read);
            f.Read();
            BinaryTableHDU bhdu = (BinaryTableHDU)f.GetHDU(1);
            Header hdr = bhdu.Header;
            BinaryTable btab = (BinaryTable)bhdu.Data;
            for (int i = 0; i < 50; i += 1)
            {
                Object[] row = (Object[])btab.GetRow(i);
                float[] qx = (float[])row[1];
                Array[] p = (Array[])row[0];
                float[] pt = (float[])p.GetValue(0);
                pt[0] = (float)(i * Math.Sin(i));
                btab.AddRow(row);
            }
          
            f = new Fits();
            f.AddHDU(Fits.MakeHDU(btab));
            BufferedFile bf = new BufferedFile("bt4.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Flush();
            bf.Close();

           f = new Fits("bt4.fits", FileAccess.Read);

            btab = (BinaryTable)f.GetHDU(1).Data;
            Assertion.AssertEquals("row1", 100, btab.nRow);


            // Try getting data before we Read in the table.

            Array[] xf = (Array[])btab.GetColumn(0);
            Array[] xft = (Array[])xf.GetValue(50);
            float[] xftt = (float[])xft.GetValue(0);
            Assertion.AssertEquals("row2", (float)0, (float)xftt[0]);
            xft = (Array[])xf.GetValue(99);
            xftt = (float[])xft.GetValue(0);
            Assertion.AssertEquals("row3", (float)(49 * Math.Sin(49)), (float)xftt[0]);

            for (int i = 0; i < xf.Length; i += 3)
            {
                bool[] ba = (bool[])btab.GetElement(i, 5);
                float[] fx = (float[])btab.GetElement(i, 1);

                int trow = i % 50;

                Assertion.AssertEquals("row4", true, ArrayFuncs.ArrayEquals(ba, vbool[trow])); // prob 1
                Assertion.AssertEquals("row6", true, ArrayFuncs.ArrayEquals(fx, vf[trow]));
            }
            // Fill the table.
            Data data = f.GetHDU(1).Data;

            xf = (Array[])btab.GetColumn(0);
            xft = (Array[])xf.GetValue(50);
            xftt = (float[])xft.GetValue(0);
            Assertion.AssertEquals("row7", 0F, (float)xftt[0]);
            xft = (Array[])xf.GetValue(99);
            xftt = (float[])xft.GetValue(0);
            Assertion.AssertEquals("row8", (float)(49 * Math.Sin(49)), (float)xftt[0]);

            for (int i = 0; i < xf.Length; i += 3)
            {
                bool[] ba = (bool[])btab.GetElement(i, 5);
                float[] fx = (float[])btab.GetElement(i, 1);

                int trow = i % 50;

                Assertion.AssertEquals("row9", true, ArrayFuncs.ArrayEquals(ba, vbool[trow])); // prob 2
                Assertion.AssertEquals("row11", true, ArrayFuncs.ArrayEquals(fx, vf[trow]));
            }
            f.Close();
        }

        [Test]
        public void TestObj()
        {
            FitsFactory.UseAsciiTables = false;

            /*** Create a binary table from an Object[][] array */
            Object[][] x = new Object[5][];
            for (int i = 0; i < 5; i += 1)
            {
                x[i] = new Object[3];

                x[i][0] = new float[] { i };

                string temp = string.Concat("AString", i);
                x[i][1] = new string[] { temp };

                int[][] t = new int[2][];
                for (int j = 0; j < 2; j++)
                {
                    t[j] = new int[2];
                    t[j][0] = j * i;
                    t[j][1] = (j + 2) * i;
                }
                x[i][2] = t;
            }

            Fits f = new Fits();
            BasicHDU hdu = Fits.MakeHDU(x);
            f.AddHDU(hdu);
            BufferedFile bf = new BufferedFile("bt5.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Close();
            

            /* Now get rid of some columns */
            BinaryTableHDU xhdu = (BinaryTableHDU)hdu;

            // First column
            Assertion.AssertEquals("delcol1", 3, xhdu.NCols);
            xhdu.DeleteColumnsIndexOne(1, 1);
            Assertion.AssertEquals("delcol2", 2, xhdu.NCols);

            xhdu.DeleteColumnsIndexZero(1, 1);
            Assertion.AssertEquals("delcol3", 1, xhdu.NCols);

            bf = new BufferedFile("bt6.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Close();
          
            f = new Fits("bt6.fits");

            xhdu = (BinaryTableHDU)f.GetHDU(1);
            Assertion.AssertEquals("delcol4", 1, xhdu.NCols);
            f.Close();
        }

        [Test]
        public void TestDegenerate()
        {
            String[] sa = new String[10];
            int[,] ia = new int[10, 0];
            Fits f = new Fits();

            for (int i = 0; i < sa.Length; i += 1)
            {
                sa[i] = "";
            }

            Object[] data = new Object[] { sa, ia };
            BinaryTableHDU bhdu = (BinaryTableHDU)Fits.MakeHDU(data);
            Header hdr = bhdu.Header;
            f.AddHDU(bhdu);
            BufferedFile bf = new BufferedFile("bt7.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Close();

            Assertion.AssertEquals("degen1", 2, hdr.GetIntValue("TFIELDS"));
            Assertion.AssertEquals("degen2", 10, hdr.GetIntValue("NAXIS2"));
            Assertion.AssertEquals("degen3", 0, hdr.GetIntValue("NAXIS1"));

            f = new Fits("bt7.fits");
            bhdu = (BinaryTableHDU)f.GetHDU(1);

            hdr = bhdu.Header;
            Assertion.AssertEquals("degen4", 2, hdr.GetIntValue("TFIELDS"));
            Assertion.AssertEquals("degen5", 10, hdr.GetIntValue("NAXIS2"));
            Assertion.AssertEquals("degen6", 0, hdr.GetIntValue("NAXIS1"));
            f.Close();
        }

        [Test]
        public void TestDegen2()
        {
            FitsFactory.UseAsciiTables = false;

            Object[] data = new Object[]{
	      new String[]{"a", "b", "c", "d", "e", "f"},
	      new int[]   {1,2,3,4,5,6},
	      new float[] {1f,2f, 3f, 4f, 5f,6f},
	      new String[]{"", "", "" , "" , "", ""},
	      new String[]{"a", "", "c", "", "e", "f"},
	      new String[]{"", "b", "c", "d", "e", "f"},
	      new String[]{"a", "b", "c", "d", "e", ""},
	      new String[]{null, null, null, null, null, null},
	      new String[]{"a", null, "c", null, "e", "f"},
	      new String[]{null, "b", "c", "d", "e", "f"},
	      new String[]{"a", "b", "c", "d", "e", null}
	    };

            Fits f = new Fits();
            f.AddHDU(Fits.MakeHDU(data));
            BufferedFile ff = new BufferedFile("bt8.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(ff);
            ff.Flush();
            ff.Close();
           
            f = new Fits("bt8.fits");
            BinaryTableHDU bhdu = (BinaryTableHDU)f.GetHDU(1);

            Assertion.AssertEquals("deg21", "e", bhdu.GetElement(4, data.Length - 1));
            Assertion.AssertEquals("deg22", "", bhdu.GetElement(5, data.Length - 1));

            String[] col = (String[])bhdu.GetColumn(0);
            Assertion.AssertEquals("deg23", "a", col[0]);
            Assertion.AssertEquals("deg24", "f", col[5]);

            col = (String[])bhdu.GetColumn(3);
            Assertion.AssertEquals("deg25", "", col[0]);
            Assertion.AssertEquals("deg26", "", col[5]);

            col = (String[])bhdu.GetColumn(7);  // All nulls
            Assertion.AssertEquals("deg27", "", col[0]);
            Assertion.AssertEquals("deg28", "", col[5]);

            col = (String[])bhdu.GetColumn(8);

            Assertion.AssertEquals("deg29", "a", col[0]);
            Assertion.AssertEquals("deg210", "", col[1]);
            f.Close();
        }

    }
}
