using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;

using nom.tam.fits;
using nom.tam.image;
using nom.tam.util;

namespace nom.tam.fits
{
    [TestFixture]
    public class FitsCopyTester
    {
        [Test]
        public void TestFitsCopy( /*String[] args*/ )
        {

            String file = "..\\..\\testdocs\\ht1.fits" /*args[0]*/;

            Fits f = new Fits(file);
            int i = 0;
            BasicHDU h;

            do
            {
                h = f.ReadHDU();
                if (h != null)
                {
                    if (i == 0)
                    {
                        System.Console.Out.WriteLine("\n\nPrimary header:\n");
                    }
                    else
                    {
                        System.Console.Out.WriteLine("\n\nExtension " + i + ":\n");
                    }
                    i += 1;
                    h.Info();
                }
            } while (h != null);

            BufferedFile bf = new BufferedFile("gbfits3.fits" /*args[1]*/, FileAccess.ReadWrite, FileShare.ReadWrite);
            f.Write(bf);
            bf.Close();

        }
    }
}
