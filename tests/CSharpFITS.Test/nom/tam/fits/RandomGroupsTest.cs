using System;
using System.IO;

using NUnit.Framework;
using nom.tam.util;

namespace nom.tam.fits
{
    [TestFixture]
    public class RandomGroupsTest
    {
        [Test]
        public void TestRandomGroup()
        {
            //float[,] fa = new float[20,20];
            float[][] fa = new float[20][];
            for (int i = 0; i < fa.Length; i++)
                fa[i] = new float[20];
            float[] pa = new float[3];

            BufferedFile bf = new BufferedFile("rg1.fits", FileAccess.ReadWrite, FileShare.ReadWrite);

            Object[][] data = new Object[1][];
            data[0] = new Object[2];
            data[0][0] = pa;
            data[0][1] = fa;

            Console.Out.WriteLine("***** Write header ******");
            BasicHDU hdu = Fits.MakeHDU(data);
            Header hdr = hdu.Header;

            // Change the number of groups
            hdr.AddValue("GCOUNT", 20, "Number of groups");
            hdr.Write(bf);

            Console.Out.WriteLine("***** Write data group by group ******");
            for (int i = 0; i < 20; i += 1)
            {

                for (int j = 0; j < pa.Length; j += 1)
                {
                    pa[j] = i + j;
                }
                for (int j = 0; j < fa.GetLength(0); j += 1)
                {
                    try
                    {
                        //  fa[j, j] = i * j;
                        fa[j][j] = i * j;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message); ;
                        Console.WriteLine($"i ,j value:{i}   {j}");
                    }
                }
                // Write a group
                bf.WriteArray(data);
            }

            byte[] padding = new byte[FitsUtil.Padding(20 * ArrayFuncs.ComputeSize(data))];
            Console.Out.WriteLine("****** Write padding ******");
            bf.Write(padding);

            bf.Flush();
            bf.Close();

            Console.Out.WriteLine("****** Read data back in ******");
            Fits f = new Fits("rg1.fits");
            BasicHDU[] hdus = f.Read();

            data = (Object[][])hdus[0].Kernel;
            // data = hdus[0].Kernel;
            Console.Out.WriteLine("**** Check parameter and data info *****");
            for (int i = 0; i < data.Length; i += 1)
            {

                pa = (float[])data[i][0];
                // fa = (float[,]) data[i,1];
                Array[] tfa = (Array[])data[i][1];
                for (int j = 0; j < pa.Length; j += 1)
                {
                    Assertion.AssertEquals($"paramTest:{i} {j}", (float)(i + j), pa[j]);
                }
                for (int j = 0; j < fa.Length; j += 1)
                {
                    // Assertion.AssertEquals("dataTest:" + i + " " + j, (float)(i * j), fa[j,j]);
                    Assertion.AssertEquals($"dataTest:{i} {j}", (float)(i * j), ((Array)tfa.GetValue(j)).GetValue(j));
                }
            }

            f = new Fits();

            Console.Out.WriteLine("**** Create HDU from kernel *****");

            // Generate a FITS HDU from the kernel.
            f.AddHDU(Fits.MakeHDU(data));
            bf = new BufferedFile("rg2.fits", FileAccess.ReadWrite, FileShare.ReadWrite);
            Console.Out.WriteLine("**** Write new file *****");
            f.Write(bf);

            bf.Flush();
            bf.Close();

            Console.Out.WriteLine("**** Read and check *****");
            f = new Fits("rg2.fits");
            data = (Object[][])f.Read()[0].Kernel;
            for (int i = 0; i < data.Length; i += 1)
            {

                pa = (float[])data[i][0];
                //   fa = (float[,]) data[i,1];
                Array[] tfa = (Array[])data[i][1];
                for (int j = 0; j < pa.Length; j += 1)
                {
                    Assertion.AssertEquals($"paramTest:{i} {j}", (float)(i + j), pa[j]);
                }
                for (int j = 0; j < fa.Length; j += 1)
                {
                    //Assertion.AssertEquals("dataTest:" + i + " " + j, (float)(i * j), fa[j,j]);
                    Assertion.AssertEquals($"dataTest:{i} {j}", (float)(i * j), ((Array)tfa.GetValue(j)).GetValue(j));
                }
            }

        }
    }
}

