using System;
using NUnit.Framework;

namespace nom.tam.fits
{
    [TestFixture]
    public class TestDate
    {
        [Test]
        public void DateTest()
        {
            Assertion.AssertEquals("t1", true, TestArg("20/09/79"));
            Assertion.AssertEquals("t2", true, TestArg("1997-07-25"));
            Assertion.AssertEquals("t3", true, TestArg("1987-06-05T04:03:02.01"));
            Assertion.AssertEquals("t4", true, TestArg("1998-03-10T16:58:34"));
            Assertion.AssertEquals("t5", true, TestArg(null));
            Assertion.AssertEquals("t6", true, TestArg("        "));

            Assertion.AssertEquals("t7", false, TestArg("20/09/"));
            Assertion.AssertEquals("t8", false, TestArg("/09/79"));
            Assertion.AssertEquals("t9", false, TestArg("09//79"));
            Assertion.AssertEquals("t10", false, TestArg("20/09/79/"));

            Assertion.AssertEquals("t11", false, TestArg("1997-07"));
            Assertion.AssertEquals("t12", false, TestArg("-07-25"));
            Assertion.AssertEquals("t13", false, TestArg("1997--07-25"));
            Assertion.AssertEquals("t14", false, TestArg("1997-07-25-"));

            Assertion.AssertEquals("t15", false, TestArg("5-Aug-1992"));
            Assertion.AssertEquals("t16", false, TestArg("28/02/91 16:32:00"));
            Assertion.AssertEquals("t17", false, TestArg("18-Feb-1993"));
            Assertion.AssertEquals("t18", false, TestArg("nn/nn/nn"));
        }

        bool TestArg(String arg)
        {
            try
            {
                FitsDate fd = new FitsDate(arg);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
