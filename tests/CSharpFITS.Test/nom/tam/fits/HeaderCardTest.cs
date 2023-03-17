using System;
using NUnit.Framework;

namespace nom.tam.fits
{
    [TestFixture]
    public class HeaderCardTest
    {
        [Test]
        public void Test1()
        {
            var p = new HeaderCard("SIMPLE  =                     T");

            Assertion.AssertEquals("t1", "SIMPLE", p.Key);
            Assertion.AssertEquals("t2", "T", p.Value);
            Assertion.AssertNull("t3", p.Comment);

            p = new HeaderCard("VALUE   =                   123");
            Assertion.AssertEquals("t4", "VALUE", p.Key);
            Assertion.AssertEquals("t5", "123", p.Value);
            Assertion.AssertNull("t3", p.Comment);

            p = new HeaderCard("VALUE   =    1.23698789798798E23 / Comment ");
            Assertion.AssertEquals("t6", "VALUE", p.Key);
            Assertion.AssertEquals("t7", "1.23698789798798E23", p.Value);
            Assertion.AssertEquals("t8", "Comment", p.Comment);

            String lng = "111111111111111111111111111111111111111111111111111111111111111111111111";
            p = new HeaderCard($"COMMENT {lng}");
            Assertion.AssertEquals("t9", "COMMENT", p.Key);
            Assertion.AssertNull("t10", p.Value);
            Assertion.AssertEquals("t11", lng, p.Comment);

            bool thrown = false;
            try
            {
                //
                p = new HeaderCard("VALUE   = '   ");
            }
            catch (Exception)
            {
                thrown = true;
            }
            Assertion.AssertEquals("t12", true, thrown);


            p = new HeaderCard($"COMMENT {lng}{lng}");
            Assertion.AssertEquals("t13", lng, p.Comment);

        }

        [Test]
        public void Test3()
        {

            HeaderCard p = new HeaderCard("KEY", "VALUE", "COMMENT");
            Assertion.AssertEquals("x1",
                "KEY     = 'VALUE'              / COMMENT                                        ",
                    p.ToString());

            p = new HeaderCard("KEY", 123, "COMMENT");
            Assertion.AssertEquals("x2",
                "KEY     =                  123 / COMMENT                                        ",
                    p.ToString());

            p = new HeaderCard("KEY", 1.23, "COMMENT");
            Assertion.AssertEquals("x3",
                "KEY     =                 1.23 / COMMENT                                        ",
                    p.ToString());

            p = new HeaderCard("KEY", true, "COMMENT");
            Assertion.AssertEquals("x4",
                "KEY     =                    T / COMMENT                                        ",
                    p.ToString());

            bool thrown = false;
            try
            {
                p = new HeaderCard("LONGKEYWORD", 123, "COMMENT");
            }
            catch (Exception)
            {
                thrown = true;
            }
            Assertion.AssertEquals("x5", true, thrown);

            thrown = false;
            String lng = "00000000001111111111222222222233333333334444444444555555555566666666667777777777";
            try
            {
                p = new HeaderCard("KEY", lng, "COMMENT");
            }
            catch (Exception)
            {
                thrown = true;
            }
            Assertion.AssertEquals("x6", true, thrown);

        }

        [Test]
        public void TestHierarch()
        {

            HeaderCard hc;
            String key = "HIERARCH.TEST1.TEST2.INT";
            bool thrown = false;
            try
            {
                hc = new HeaderCard(key, 123, "Comment");
            }
            catch (Exception)
            {
                thrown = true;
            }
            Assertion.AssertEquals("h1", true, thrown);

            String card = "HIERARCH TEST1 TEST2 INT=           123 / Comment                               ";
            hc = new HeaderCard(card);
            Assertion.AssertEquals("h2", "HIERARCH", hc.Key);
            Assertion.AssertNull("h3", hc.Value);
            Assertion.AssertEquals("h4", "TEST1 TEST2 INT=           123 / Comment", hc.Comment);

            FitsFactory.UseHierarch = true;


            hc = new HeaderCard(key, 123, "Comment");

            Assertion.AssertEquals("h5", key, hc.Key);
            Assertion.AssertEquals("h6", "123", hc.Value);
            Assertion.AssertEquals("h7", "Comment", hc.Comment);

            hc = new HeaderCard(card);
            Assertion.AssertEquals("h8", key, hc.Key);
            Assertion.AssertEquals("h9", "123", hc.Value);
            Assertion.AssertEquals("h10", "Comment", hc.Comment);
        }

        [Test]
        public void TestLongDoubles()
        {
            // Check to see if we make long double values fit in the recommended space.
            HeaderCard hc = new HeaderCard("TEST", -1.234567890123456789E-123, "dummy");
            String val = hc.Value;

            Assertion.AssertEquals("tld1", val.Length, 20);
        }
    }
}
