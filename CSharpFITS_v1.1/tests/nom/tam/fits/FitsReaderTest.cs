using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace nom.tam.fits
{
    using System;
    using nom.tam.fits;
    using nom.tam.util;
    using nom.tam.image;
    [TestFixture]
    public class FitsReaderTester
    {

        [Test]
        public void TestFits()
        {
            Fits f = new Fits(new FileStream("..\\..\\testdocs\\ht1.fits",FileMode.Open),false);
            // Fits f = new Fits(new FileStream("E:\\CSharpFITSIO\\AISCHV3_228_13637_0001_sv09-fd-int.fits.gz",FileMode.Open),true);
             //Fits f = new Fits(new FileStream("E:\\CSharpFITSIO\\LAB-2.0kms.fits",FileMode.Open),false);
            // Fits f = new Fits("http://skyview.gsfc.nasa.gov/cgi-bin/images?position=180.0%2C8.0&survey=NEAT&pixels=300%2C300&sampler=Clip&size=0.3%2C0.3&projection=Tan&coordinates=J2000.0&return=FITS");
            // Fits f = new Fits(new FileStream("D:\\VOIndia\\Sample FITS files\\Previous\\swift_events.fits",FileMode.Open),false);

          //  String fits = "E:\\CSharpFITSIO\\AISCHV3_228_13637_0001_sv09-fd-int.fits.gz";
          //  Fits f = new Fits(fits);
            Console.Out.WriteLine("FitsReader called.");

            int i = 0;
            BasicHDU h;

            do
            {
                h = f.ReadHDU();
                if (h != null)
                {
                    if (i == 0)
                    {
                        Console.Out.WriteLine("\n\nPrimary header:\n");
                    }
                    else
                    {
                        Console.Out.WriteLine("\n\nExtension " + i + ":\n");
                    }
                    i += 1;
                    h.Info();
                }
            } while (h != null);

        }
        [Test]
        public void TestReadBuffered()
        {
            BufferedFile bf = new BufferedFile("..\\..\\testdocs\\ht1.fits", FileAccess.Read, FileShare.None);
            
            Header       h        = Header.ReadHeader(bf);
            long n = h.DataSize;
            int          naxes    = h.GetIntValue("NAXIS");
            int          lastAxis = h.GetIntValue("NAXIS"+naxes);
           HeaderCard hnew= new HeaderCard("NAXIS", naxes - 1,"this is header card with naxes");
           h.AddCard(hnew);
            float[] line = new float[h.DataSize];
            for (int i = 0; i < lastAxis; i += 1)
            {
                Console.Out.WriteLine("read");
                bf.Read(line);
           }
      
         
            

        }
    }
}
