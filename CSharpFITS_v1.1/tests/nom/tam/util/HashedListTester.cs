using nom.tam.fits;

namespace nom.tam.util
{
    /* Copyright: Thomas McGlynn 1999.
    * This code may be used for any purpose, non-commercial
    * or commercial so long as this copyright notice is retained
    * in the source code or included in or referred to in any
    * derived software.
    */
    using System;
    using nom.tam.util;
    using System.Collections;
    using NUnit.Framework;

    /// <summary>This class tests and illustrates the use
    /// of the HashedList class.  Tests are in three
    /// parts.  
    /// <p>
    /// The first section tests the methods
    /// that are present in the Collection interface.
    /// All of the optional methods of that interface
    /// are supported.  This involves tests of the
    /// HashedClass interface directly.
    /// <p>
    /// The second set of tests uses the Cursor
    /// returned by the GetCursor() method and tests
    /// the standard Cursor methods to display
    /// and remove rows from the HashedList.
    /// <p>
    /// The third set of tests tests the extended
    /// capabilities of the HashedListCursor
    /// to add rows to the table, and to work
    /// as a cursor to move in a non-linear fashion
    /// through the list.
    /// <p>
    /// There is as yet no testing that the HashedList
    /// fails appropriately and gracefully.
    /// </p></p></p></p>
    /// </summary>
    [TestFixture]
    public class HashedListTester
    {

        [STAThread]

        [Test]
        public void Test()
        {
            HashedList h = new HashedList();
            h.Add("key1", 20);
            h.Add("key2", 21);

            Cursor c = h.GetCursor();
            Console.WriteLine("current: " + ((DictionaryEntry)c.Current).Value );
        }

        [Test]
        public void TestHashedList()
        {
            HashedList h1 = new HashedList();
            HashedList h2 = new HashedList();

            Cursor i = h1.GetCursor(-1);
            Cursor j;


            // Add a few unkeyed rows.
            h1.Add("Row 1");
            h1.Add("Row 2");
            h1.Add("Row 3");

            System.Console.Out.WriteLine("***** Collection methods *****\n");
            show("Three unkeyed elements", h1);
            h1.RemoveUnkeyedObject("Row 2");
            show("Did we remove Row 2?", h1);

            h1.Clear();
            show("Cleared", h1);


            // Insert Rows with Keys.
            h1.Add("key 1", "Row 1");
            h1.Add("key 2", "Row 2");
            h1.Add("key 3", "Row 3");

            show("Three keyed elements", h1);
            h1.Remove("key 2");
            show("Did we remove Row 2 using a key?", h1);
            h1.Clear();
            show("Cleared", h1);


            // Again insert Rows with Keys.
            h1.Add("key 1", "Row 1");
            h1.Add("key 2", "Row 2");
            h1.Add("key 3", "Row 3");
            show("Three elements again!", h1);
            System.Console.Out.WriteLine("Check contains (true):" + h1.ContainsValue("Row 2"));


            // Inserting Rows in h2.
            h2.Add("key 4", "Row 4");
            h2.Add("key 5", "Row 5");
            System.Console.Out.WriteLine("Check containsAll (false):" + h1.ContainsAll(h2));

            h1.AddAll(h2);
            show("Should have 5 elements now", h1);
            System.Console.Out.WriteLine("Check containsAll (true):" + h1.ContainsAll(h2));
            System.Console.Out.WriteLine("Check contains (true):" + h1.ContainsKey("key 4"));

            h1.RemoveValue("Row 4");
            show("Dropped Row 4:", h1);
            System.Console.Out.WriteLine("Check containsAll (false):" + h1.ContainsAll(h2));
            System.Console.Out.WriteLine("Check contains (false):" + h1.ContainsKey("Row 4"));

            System.Console.Out.WriteLine("Check isEmpty (false):" + h1.Empty);
            h1.RemoveValue("Row 1");
            h1.RemoveValue("Row 2");
            h1.RemoveValue("Row 3");
            h1.RemoveValue("Row 5");
            show("Removed all elements", h1);
            System.Console.Out.WriteLine("Check isEmpty (true):" + h1.Empty);


            h1.Add("Row 1");
            h1.Add("Row 2");
            h1.Add("Row 3");
            h1.AddAll(h2);
            show("Back to 5", h1);
            h1.RemoveAll(h2);
            show("Testing removeAll back to 3?", h1);
            h1.AddAll(h2);
            h1.RetainAll(h2);
            show("Testing retainAll now just 2?", h1);


            System.Console.Out.WriteLine("\n\n**** Test Cursor **** \n");

            j = h1.GetCursor();
            while (j.MoveNext())
            {
                System.Console.Out.WriteLine("Cursor got: [" + ((DictionaryEntry)j.Current).Key + "] \"" +
                                                                ((DictionaryEntry)j.Current).Value + "\"");
            }

            h1.Clear();
            h1.Add("key 1", "Row 1");
            h1.Add("key 2", "Row 2");
            h1.Add("Row 3");
            h1.Add("key 4", "Row 4");
            h1.Add("Row 5");
            j = h1.GetCursor();
            j.MoveNext();
            j.MoveNext();
            j.Remove(); // Should get rid of second row
            show("Removed second row with cursor", h1);
            System.Console.Out.WriteLine("Cursor should still be OK:" + j.MoveNext() + " [" +
                                                                ((DictionaryEntry)j.Current).Key + "] \"" +
                                                                ((DictionaryEntry)j.Current).Value + "\"");
            System.Console.Out.WriteLine("Cursor should still be OK:" + j.MoveNext() + " [" +
                                                                ((DictionaryEntry)j.Current).Key + "] \"" +
                                                                ((DictionaryEntry)j.Current).Value + "\"");
            System.Console.Out.WriteLine("Cursor should be done:" + j.MoveNext());

            System.Console.Out.WriteLine("\n\n**** HashedListCursor ****\n");
            i = h1.GetCursor(-1);
            System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " [" +
                                                                ((DictionaryEntry)i.Current).Key + "] \"" +
                                                                ((DictionaryEntry)i.Current).Value + "\"");
            System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " [" +
                                                                ((DictionaryEntry)i.Current).Key + "] \"" +
                                                                ((DictionaryEntry)i.Current).Value + "\"");
            System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " [" +
                                                                ((DictionaryEntry)i.Current).Key + "] \"" +
                                                                ((DictionaryEntry)i.Current).Value + "\"");
            System.Console.Out.WriteLine("Cursor should still be OK:" + i.MoveNext() + " [" +
                                                                ((DictionaryEntry)i.Current).Key + "] \"" +
                                                                ((DictionaryEntry)i.Current).Value + "\"");
            System.Console.Out.WriteLine("Cursor should be done:" + i.MoveNext());

            i.Key = "key 1";
            i.MoveNext();
            i.Add("key 2", "Row 2");
            System.Console.Out.WriteLine("Cursor should still be OK:" + " [" +
                                                                ((DictionaryEntry)i.Current).Key + "] \"" +
                                                                ((DictionaryEntry)i.Current).Value + "\"");
            i.MoveNext();
            System.Console.Out.WriteLine("Cursor should still be OK:" + " [" +
                                                                ((DictionaryEntry)i.Current).Key + "] \"" +
                                                                ((DictionaryEntry)i.Current).Value + "\"");
            i.MoveNext();
            System.Console.Out.WriteLine("Cursor should still be OK:" + " [" +
                                                                ((DictionaryEntry)i.Current).Key + "] \"" +
                                                                ((DictionaryEntry)i.Current).Value + "\"");
            System.Console.Out.WriteLine("Cursor should be done:" + i.MoveNext());

            i.Key = "key 4";
            System.Console.Out.WriteLine("Cursor should still be OK:" + " [" +
                                                                ((DictionaryEntry)i.Current).Key + "] \"" +
                                                                ((DictionaryEntry)i.Current).Value + "\"");
            i.MoveNext();
            System.Console.Out.WriteLine("Cursor should still be OK:" + " [" +
                                                                ((DictionaryEntry)i.Current).Key + "] \"" +
                                                                ((DictionaryEntry)i.Current).Value + "\"");
            System.Console.Out.WriteLine("Cursor should be done:" + i.MoveNext());

            i.Key = "key 2";
            i.MoveNext();
            i.MoveNext();
            i.Add("Row 3.5");
            i.Add("Row 3.6");
            show("Added some rows... should be 7", h1);

            i = h1.GetCursor("key 2");
            i.Add("Row 1.5");
            i.Add("key 1.7", "Row 1.7");
            i.Add("Row 1.9");
            System.Console.Out.WriteLine("Cursor should point to 2:" + ((System.Collections.DictionaryEntry)i.Current).Key);
            i.Key = "key 1.7";
            System.Console.Out.WriteLine("Cursor should point to 1.7:" + ((System.Collections.DictionaryEntry)i.Current).Key);
        }

        public static void show(System.String descrip, HashedList h)
        {
            System.Console.Out.WriteLine(descrip + " : [" + h.Count + "]");
            Object[] o = h.toArray();
            for (int i = 0; i < o.Length; i += 1)
            {
                System.Console.Out.WriteLine("  " + o[i]);
            }
        }


        // Test methods with Assertions

        [Test]
        public void TestCollection()
        {

	        HashedList h1 = new HashedList();
	        HashedList h2 = new HashedList();
        	
	        Cursor i = h1.GetCursor(-1);
	       
	        // Add a few unkeyed rows.
	        h1.Add("Row 1");
	        h1.Add("Row 2");
	        h1.Add("Row 3");
        	
	        Assertion.AssertEquals("Adding unkeyed rows", 3, h1.Count); 
        	
	        Assertion.AssertEquals("Has row 1", true, h1.ContainsValue("Row 1"));
	        Assertion.AssertEquals("Has row 2", true, h1.ContainsValue("Row 2"));
	        h1.RemoveValue("Row 2");
	        Assertion.AssertEquals("Has row 1", true, h1.ContainsValue("Row 1"));
	        Assertion.AssertEquals("Has row 2", false, h1.ContainsValue("Row 2"));
        	
	        Assertion.AssertEquals("Delete unkeyed rows", 2, h1.Count);
	        h1.Clear();
	        Assertion.AssertEquals("Cleared unkeyed rows", 0, h1.Count);
        	

            // Add few Keyed rows.
	        h1.Add("key 1", "Row 1");
	        h1.Add("key 2", "Row 2");
	        h1.Add("key 3", "Row 3");
        	
	        Assertion.AssertEquals("Adding keyed rows", 3, h1.Count);

            Assertion.AssertEquals("Has Row 1", true, h1.ContainsValue("Row 1"));
	        Assertion.AssertEquals("Has key 1", true, h1.ContainsKey("key 1"));
            Assertion.AssertEquals("Has Row 2", true, h1.ContainsValue("Row 2"));
	        Assertion.AssertEquals("Has key 2", true, h1.ContainsKey("key 2"));
            Assertion.AssertEquals("Has Row 3", true, h1.ContainsValue("Row 3"));
	        Assertion.AssertEquals("Has key 3", true, h1.ContainsKey("key 3"));
        	
	        h1.RemoveKey("key 2");
	        Assertion.AssertEquals("Delete keyed row", 2, h1.Count);
            Assertion.AssertEquals("Has Row 1", true, h1.ContainsValue("Row 1"));
	        Assertion.AssertEquals("Has key 1", true, h1.ContainsKey("key 1"));
            Assertion.AssertEquals("Has Row 2", false, h1.ContainsValue("Row 2"));
	        Assertion.AssertEquals("Has key 2", false, h1.ContainsKey("key 2"));
            Assertion.AssertEquals("Has Row 3", true, h1.ContainsValue("Row 3"));
	        Assertion.AssertEquals("Has key 3", true, h1.ContainsKey("key 3"));
        	
	        h1.Clear();
	        Assertion.AssertEquals("Clear keyed rows", 0, h1.Count);
        	
	        h1.Add("key 1", "Row 1");
	        h1.Add("key 2", "Row 2");
	        h1.Add("key 3", "Row 3");
	        Assertion.AssertEquals("Re-Adding keyed rows", 3, h1.Count);
            Assertion.AssertEquals("Has Row 2", true, h1.ContainsValue("Row 2"));
            Assertion.AssertEquals("Has key 2", true, h1.ContainsKey("key 2"));
        	
	        h2.Add("key 4", "Row 4");
	        h2.Add("key 5", "Row 5");
        	
	        Assertion.AssertEquals("containsAll(beforeAdd)", false, h1.ContainsAll(h2));
        	
	        h1.AddAll(h2);
        	
	        Assertion.AssertEquals("AddAll()", 5, h1.Count);
	        Assertion.AssertEquals("ContainsAll(afterAdd)", true, h1.ContainsAll(h2));
	        Assertion.AssertEquals("has row 4", true, h1.ContainsValue("Row 4"));
	        h1.RemoveValue("Row 4");
	        Assertion.AssertEquals("dropped row 4", false, h1.ContainsValue("Row 4"));
	        Assertion.AssertEquals("ContainsAll(afterDrop)", false, h1.ContainsAll(h2));
        	
	        Assertion.AssertEquals("Empty(false)", false, h1.Empty);
	        h1.RemoveValue("Row 1");
	        h1.RemoveValue("Row 2");
	        h1.RemoveValue("Row 3");
	        h1.RemoveValue("Row 5");
	        Assertion.AssertEquals("isEmpty(true)", true, h1.Empty);
	        h1.Add("Row 1");
	        h1.Add("Row 2");
	        h1.Add("Row 3");
	        h1.AddAll(h2);
	        Assertion.AssertEquals("Adding back", 5, h1.Count);
	        h1.RemoveAll(h2);
        	
	        Assertion.AssertEquals("removeAll()", 3, h1.Count);
	        h1.AddAll(h2);
        	
	        Assertion.AssertEquals("Adding back again", 5, h1.Count);
	        h1.RetainAll(h2);
	        Assertion.AssertEquals("retainAll()", 2, h1.Count);
        	
        }
        
        [Test]
        public void TestIterator()
        {
        	
	        HashedList h1 = new HashedList();
        	
	        h1.Add("key 4", "Row 4");
	        h1.Add("key 5", "Row 5");
        	
        	
	        Cursor j = h1.GetCursor();
	        Assertion.AssertEquals("next1", true, j.MoveNext());
	        Assertion.AssertEquals("TestIter1", "Row 4", (String) ((DictionaryEntry)j.Current).Value);
	        Assertion.AssertEquals("next2", true, j.MoveNext());
            Assertion.AssertEquals("TestIter2", "Row 5", (String)((DictionaryEntry)j.Current).Value);
	        Assertion.AssertEquals("next3", false, j.MoveNext());
        	
	        h1.Clear();
        	
	        h1.Add("key 1", "Row 1");
	        h1.Add("key 2", "Row 2");
	        h1.Add("Row 3");
	        h1.Add("key 4", "Row 4");
	        h1.Add("Row 5");

            Assertion.AssertEquals("Before remove", true, h1.ContainsValue("Row 2"));
	        j = h1.GetCursor();
	        j.MoveNext();
	        j.MoveNext();
	        j.Remove();  // Should get rid of second row
	        Assertion.AssertEquals("After remove", false, h1.ContainsValue("Row 2"));
            Assertion.AssertEquals("n3", true, j.MoveNext());
            Assertion.AssertEquals("n3v", "Row 3", (String)((DictionaryEntry)j.Current).Value);
	        Assertion.AssertEquals("n4", true, j.MoveNext());
            Assertion.AssertEquals("n4v", "Row 4", (String)((DictionaryEntry)j.Current).Value);
	        Assertion.AssertEquals("n5", true, j.MoveNext());
            Assertion.AssertEquals("n5v", "Row 5", (String)((DictionaryEntry)j.Current).Value);
	        Assertion.AssertEquals("n6", false, j.MoveNext());
        }
        
        [Test]
        public void TestCursor()
        {
        	
	        HashedList h1 = new HashedList();
        	
	        h1.Add("key 1", "Row 1");
	        h1.Add("Row 3");
	        h1.Add("key 4", "Row 4");
	        h1.Add("Row 5");
        	
	        Cursor j = (Cursor) h1.GetCursor(0);
            Assertion.AssertEquals("n1xv", "Row 1", (String) ((DictionaryEntry)j.Current).Value);
            j.MoveNext();
            Assertion.AssertEquals("n1xv", "Row 3", (String) ((DictionaryEntry)j.Current).Value);
        	
	        Assertion.AssertEquals("No Row 2", false, h1.ContainsKey("key 2"));
	        Assertion.AssertEquals("No Row 2", false, h1.ContainsValue("Row 2"));
	        j.Key = "key 1";
            Assertion.AssertEquals("setKey()", "Row 1", (String) ((DictionaryEntry)j.Current).Value);
            j.MoveNext();
	        j.Add("key 2", "Row 2");
	        Assertion.AssertEquals("has Row 2", true, h1.ContainsValue("Row 2"));
	        Assertion.AssertEquals("after add", "Row 3", (String) ((DictionaryEntry)j.Current).Value);
        	

	        j.Key = "key 4";
            Assertion.AssertEquals("setKey(1)", "Row 4", (String) ((DictionaryEntry)j.Current).Value);
            j.MoveNext();
            Assertion.AssertEquals("setKey(2)", "Row 5", (String) ((DictionaryEntry)j.Current).Value);
	        Assertion.AssertEquals("setKey(3)", false, j.MoveNext());
        	
        	
	        j.Key = "key 2";
            Assertion.AssertEquals("setKey(4)", "Row 2", (String) ((DictionaryEntry)j.Current).Value);
            j.MoveNext();
            Assertion.AssertEquals("setKey(5)", "Row 3", (String) ((DictionaryEntry)j.Current).Value);
	        j.Add("Row 3.5");
	        j.Add("Row 3.6");
	        Assertion.AssertEquals("After add", 7, h1.Count);
        	
	        j = h1.GetCursor("key 2");
	        j.Add("Row 1.5");
	        j.Add("key 1.7", "Row 1.7");
	        j.Add("Row 1.9");
            Assertion.AssertEquals("next() after adds", "Row 2", (String) ((DictionaryEntry)j.Current).Value);
	        j.Key = "key 1.7";
            Assertion.AssertEquals("next() after adds", "Row 1.7", (String) ((DictionaryEntry)j.Current).Value);
            j.MoveNext();
            j.MovePrevious();
            Assertion.AssertEquals("prev(1)", "Row 1.7", (String) ((DictionaryEntry)j.Current).Value);
            j.MovePrevious();
            Assertion.AssertEquals("prev(2)", "Row 1.5", (String) ((DictionaryEntry)j.Current).Value);
	        Assertion.AssertEquals("prev(3)", true, j.MovePrevious());
            Assertion.AssertEquals("prev(4)", "Row 1", (String) ((DictionaryEntry)j.Current).Value);
	        Assertion.AssertEquals("prev(5)", false, j.MovePrevious());
        }

    }
}