using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomFactTests
    {
        [TestMethod]
        public void DoGedcomFactTest()
        {
            Test_NullStream();
            Test_EmptyFile();
			Test_Facts_MultipleTests();
        }

        private void Test_NullStream()
        {
            GedcomLine line;
            var fact = GedcomFact.Read(null, null, out line);

            Assert.AreEqual(null, line);
            Assert.AreEqual(null, fact);
        }

        private void Test_EmptyFile()
        {
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var fact = GedcomFact.Read(stream, line, out line);

                Assert.AreEqual(null, fact);
                Assert.AreEqual(null, line);
            }
        }

		private void Test_Facts_MultipleTests()
        {
			var gedcomFile = @"..\..\TestFiles\Fact\Fact_MultipleTests.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Non Fact
				var line = GedcomLine.Read(stream);
				var fact = GedcomFact.Read(stream, line, out line);

				Assert.AreEqual(null, fact);
				Assert.AreEqual(null, line);

				// Fact 
				line = GedcomLine.Read(stream);
				fact = GedcomFact.Read(stream, line, out line);

				Assert.AreNotEqual(null, fact);
				Assert.IsTrue(fact.IsValid());
				Assert.AreEqual("@E1@", fact.ID);
				Assert.AreEqual("BIRT", fact.Type);
				Assert.AreEqual("BIRT", fact.Type);
				Assert.AreEqual("22 SEP 1962", fact.Date);
				Assert.AreEqual("Denver, Denver, Colorado, USA", fact.Place);
				Assert.IsFalse(fact.IsCustom);
				Assert.IsFalse(fact.IsShared);
				Assert.IsFalse(fact.IsLDS);
				Assert.AreNotEqual(null, fact.Citations);
				Assert.AreEqual(2, fact.Citations.Count);
				Assert.AreNotEqual(null, fact.Media);
				Assert.AreEqual(2, fact.Media.Count);
				Assert.AreNotEqual(null, fact.Notes);
				Assert.AreEqual(2, fact.Notes.Count);

				Assert.AreEqual(null, line);

				// Fact w/ CustomType
				line = GedcomLine.Read(stream);
				fact = GedcomFact.Read(stream, line, out line);

				Assert.AreNotEqual(null, fact);
				Assert.AreEqual("Custom", fact.Type);
				Assert.AreEqual("01 JAN 2000", fact.Date);
				Assert.AreEqual("Orem, Utah, Utah", fact.Place);
				Assert.IsTrue(fact.IsCustom);
				Assert.IsFalse(fact.IsShared);
				Assert.IsFalse(fact.IsLDS);

				Assert.AreEqual(null, line);

				// Shared Fact, Non-LDS
				line = GedcomLine.Read(stream);
				fact = GedcomFact.Read(stream, line, out line);

				Assert.AreNotEqual(null, fact);
				Assert.AreEqual("MARR", fact.Type);
				Assert.IsFalse(fact.IsCustom);
				Assert.IsTrue(fact.IsShared);
				Assert.IsFalse(fact.IsLDS);

				Assert.AreEqual(null, line);

				// Individual Fact, LDS
				line = GedcomLine.Read(stream);
				fact = GedcomFact.Read(stream, line, out line);

				Assert.AreNotEqual(null, fact);
				Assert.AreEqual("SLGC", fact.Type);
				Assert.IsFalse(fact.IsCustom);
				Assert.IsFalse(fact.IsShared);
				Assert.IsTrue(fact.IsLDS);

				Assert.AreEqual(null, line);

				// Shared Fact, LDS
				line = GedcomLine.Read(stream);
				fact = GedcomFact.Read(stream, line, out line);

				Assert.AreNotEqual(null, fact);
				Assert.AreEqual("SLGS", fact.Type);
				Assert.AreEqual("COMPLETED", fact.Status);
				Assert.AreEqual("JRIVE", fact.Temple);
				Assert.IsFalse(fact.IsCustom);
				Assert.IsTrue(fact.IsShared);
				Assert.IsTrue(fact.IsLDS);

				Assert.AreEqual(null, line);

				// Get a basic valid fact
				line = GedcomLine.Read(stream);
				fact = GedcomFact.Read(stream, line, out line);

				Assert.AreNotEqual(null, fact);
				Assert.IsTrue(fact.IsValid());

				// Force fact to be invalid
				fact.Type = string.Empty;
				Assert.IsFalse(fact.IsValid());

				// Force fact to be invalid
				fact.ID = string.Empty;
				Assert.IsFalse(fact.IsValid());
			}
        }
    }
}