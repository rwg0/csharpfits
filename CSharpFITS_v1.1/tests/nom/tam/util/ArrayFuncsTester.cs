using System;
using nom.tam.fits;
using ArrayFuncs = nom.tam.util.ArrayFuncs;
using NUnit.Framework;

namespace nom.tam.util
{
   
    [TestFixture]
    public class ArrayFuncsTester
    {

        /// <summary>Test and demonstrate the ArrayFuncs methods.</summary>
        [Test]
        public void TestArrayFuncs()
        {

            // Check GetBaseClass(), GetBaseLength() and ComputeSize() methods
            int[, ,] test1 = new int[10, 9, 8];
            bool[][] test2 = new bool[4][];
            test2[0] = new bool[5];
            test2[1] = new bool[4];
            test2[2] = new bool[3];
            test2[3] = new bool[2];

            double[,] test3 = new double[10, 20];
            System.Text.StringBuilder[,] test4 = new System.Text.StringBuilder[3, 2];

            Assertion.AssertEquals("GetBaseClass()", typeof(int), ArrayFuncs.GetBaseClass(test1));
            Assertion.AssertEquals("GetBaseLength()", 4, ArrayFuncs.GetBaseLength(test1));
            Assertion.AssertEquals("ComputeSize()", 4 * 8 * 9 * 10, ArrayFuncs.ComputeSize(test1));

            Assertion.AssertEquals("GetBaseClass(boolean)", typeof(bool), ArrayFuncs.GetBaseClass(test2));
            Assertion.AssertEquals("GetBaseLength(boolean)", 1, ArrayFuncs.GetBaseLength(test2));
            Assertion.AssertEquals("ComputeSize() not rect", 1 * (2 + 3 + 4 + 5), ArrayFuncs.ComputeSize(test2));

            Assertion.AssertEquals("getBaseClass(double)", typeof(double), ArrayFuncs.GetBaseClass(test3));
            Assertion.AssertEquals("getBaseLength(double)", 8, ArrayFuncs.GetBaseLength(test3));
            Assertion.AssertEquals("computeSize(double)", 8 * 10 * 20, ArrayFuncs.ComputeSize(test3));

            Assertion.AssertEquals("getBaseClass(StrBuf)", typeof(System.Text.StringBuilder), ArrayFuncs.GetBaseClass(test4));
            Assertion.AssertEquals("getBaseLength(StrBuf)", -1, ArrayFuncs.GetBaseLength(test4));
            Assertion.AssertEquals("computeSize(StrBuf)", 0, ArrayFuncs.ComputeSize(test4));


            Object[] agg = new Object[4];
            agg[0] = test1;
            agg[1] = test2;
            agg[2] = test3;
            agg[3] = test4;

            Assertion.AssertEquals("getBaseClass(Object[])", typeof(Object), ArrayFuncs.GetBaseClass(agg));
            Assertion.AssertEquals("getBaseLength(Object[])", -1, ArrayFuncs.GetBaseLength(agg));


            // Add up all the primitive arrays and ignore the objects.
            Assertion.AssertEquals("computeSize(Object[])", 2880 + 14 + 1600 + 0, ArrayFuncs.ComputeSize(agg));

            for (int i = 0; i < ((Array)test1).GetLength(0); i += 1)
            {
                for (int j = 0; j < ((Array)test1).GetLength(1); j += 1)
                {
                    for (int k = 0; k < ((Array)test1).GetLength(2); k += 1)
                    {
                        test1[i, j, k] = i + j + k;
                    }
                }
            }

            /*
            // Check DeepClone() method: Does not work for multi-dimension Array.
            int[,,] test5 = (int[,,]) ArrayFuncs.DeepClone(test1);
        	
	        Assertion.AssertEquals("deepClone()", true, ArrayFuncs.ArrayEquals(test1, test5));
	        test5[1,1,1] = -3;
	        Assertion.AssertEquals("arrayEquals()", false, ArrayFuncs.ArrayEquals(test1, test5));
            */

            // Check Flatten() method
            int[] dimsOrig = ArrayFuncs.GetDimensions(test1);
            int[] test6 = (int[])ArrayFuncs.Flatten(test1);

            int[] dims = ArrayFuncs.GetDimensions(test6);

            Assertion.AssertEquals("getDimensions()", 3, dimsOrig.Length);
            Assertion.AssertEquals("getDimensions()", 10, dimsOrig[0]);
            Assertion.AssertEquals("getDimensions()", 9, dimsOrig[1]);
            Assertion.AssertEquals("getDimensions()", 8, dimsOrig[2]);
            Assertion.AssertEquals("flatten()", 1, dims.Length);


            // Check Curl method
            int[] newdims = { 8, 9, 10 };
            Array[] test7 = (Array[])ArrayFuncs.Curl(test6, newdims);
            int[] dimsAfter = ArrayFuncs.GetDimensions(test7);

            Assertion.AssertEquals("curl()", 3, dimsAfter.Length);
            Assertion.AssertEquals("getDimensions()", 8, dimsAfter[0]);
            Assertion.AssertEquals("getDimensions()", 9, dimsAfter[1]);
            Assertion.AssertEquals("getDimensions()", 10, dimsAfter[2]);


            /*
            // Check Convert Array method: Implemented in Java Package
            byte[,,] xtest1 = (byte[,,]) ArrayFuncs.convertArray(test1, typeof(byte));
        	
            Assertion.AssertEquals("convertArray(toByte)", typeof(byte), ArrayFuncs.GetBaseClass(xtest1));
            Assertion.AssertEquals("convertArray(tobyte)", test1[3,3,3], (int)xtest1[3,3,3]);

            double[,,] xtest2 = (double[,,]) ArrayFuncs.convertArray(test1, typeof(double));
            Assertion.AssertEquals("convertArray(toByte)", typeof(double), ArrayFuncs.GetBaseClass(xtest2));
            Assertion.AssertEquals("convertArray(tobyte)", test1[3,3,3], (int)xtest2[3,3,3]);
            */

            // Check NewInstance method
            int[] xtest3 = (int[])ArrayFuncs.NewInstance(typeof(int), 20);
            int[] xtd = ArrayFuncs.GetDimensions(xtest3);
            Assertion.AssertEquals("newInstance(vector)", 1, xtd.Length);
            Assertion.AssertEquals("newInstance(vector)", 20, xtd[0]);
            Array[] xtest4 = (Array[])ArrayFuncs.NewInstance(typeof(double), new int[] { 5, 4, 3, 2 });

            xtd = ArrayFuncs.GetDimensions(xtest4);
            Assertion.AssertEquals("newInstance(tensor)", 4, xtd.Length);
            Assertion.AssertEquals("newInstance(tensor)", 5, xtd[0]);
            Assertion.AssertEquals("newInstance(tensor)", 4, xtd[1]);
            Assertion.AssertEquals("newInstance(tensor)", 3, xtd[2]);
            Assertion.AssertEquals("newInstance(tensor)", 2, xtd[3]);
            Assertion.AssertEquals("nElements()", 120, ArrayFuncs.CountElements(xtest4));

            /*
            // Check TestPattern method: Implemented in Java package, not in C#.
            ArrayFuncs.TestPattern(xtest4, (byte)-1);
        	
            Assertion.AssertEquals("testPattern()", (double) -1,  xtest4[0,0,0,0]);
            Assertion.AssertEquals("testPattern()", (double) 118, xtest4[4,3,2,1]);
            double[] xtest4x = (double[])ArrayFuncs.GetBaseArray(xtest4);
        	
            Assertion.AssertEquals("getBaseArray()", 2, xtest4x.Length);
            */


            // Check ArrayEquals method
            double[] x = { 1, 2, 3, 4, 5 };
            double[] y = new double[x.Length];
            for (int i = 0; i < y.Length; i += 1)
            {
                y[i] = x[i] + 1E-10;
            }

            Assertion.AssertEquals("eqTest", false, ArrayFuncs.ArrayEquals(x, y));
            Assertion.AssertEquals("eqTest2", true, ArrayFuncs.ArrayEquals(x, y, 0d, 1e-9));
            Assertion.AssertEquals("eqTest3", true, ArrayFuncs.ArrayEquals(x, y, 1E-5, 1e-9));
            Assertion.AssertEquals("eqTest4", false, ArrayFuncs.ArrayEquals(x, y, 0d, 1e-11));
            Assertion.AssertEquals("eqTest5", false, ArrayFuncs.ArrayEquals(x, y, 1E-5, 0d));

            float[] fx = { 1, 2, 3, 4, 5 };
            float[] fy = new float[fx.Length];
            for (int i = 0; i < fy.Length; i += 1)
            {
                fy[i] = fx[i] + 1E-5F;
            }

            Assertion.AssertEquals("eqTest6", false, ArrayFuncs.ArrayEquals(fx, fy));
            Assertion.AssertEquals("eqTest7", true, ArrayFuncs.ArrayEquals(fx, fy, 1E-4, 0d));
            Assertion.AssertEquals("eqTest8", false, ArrayFuncs.ArrayEquals(fx, fy, 1E-6, 0d));
            Assertion.AssertEquals("eqTest9", false, ArrayFuncs.ArrayEquals(fx, fy, 0d, 0d));
            Assertion.AssertEquals("eqTest10", false, ArrayFuncs.ArrayEquals(fx, fy, 0d, 1E-4));

        }
    }
}