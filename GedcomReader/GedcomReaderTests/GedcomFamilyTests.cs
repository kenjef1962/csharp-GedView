using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomFamilyTests
    {
        [TestMethod]
        public void DoGedcomFamilyTest()
        {
            Test_NullStream();
            Test_EmptyFile();
            Test_NonFamily();
			Test_FamilyWithNoTokens();
			Test_FamilyWithAllTokens();
			Test_FamilyAbruptEOF_MultipleTests();
		}

        private static void Test_NullStream()
        {
            GedcomLine line;
            var family = GedcomFamily.Read(null, null, out line);
            Assert.AreEqual(null, family);
        }

        private static string Test_EmptyFile()
        {
            // Empty Header
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var family = GedcomFamily.Read(stream, line, out line);
                Assert.AreEqual(null, family);
            }
            return gedcomFile;
        }

        private static void Test_NonFamily()
        {
            var gedcomFile = @"..\..\TestFiles\Family\Family_NonFamily.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
				// Eat first line
			    var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var family = GedcomFamily.Read(stream, line, out line);
				Assert.AreEqual(null, family);
			}
        }

		private static void Test_FamilyWithNoTokens()
		{
			var gedcomFile = @"..\..\TestFiles\Family\Family_NoTokens.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat first line
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var family = GedcomFamily.Read(stream, line, out line);
				Assert.AreNotEqual(null, family);
				Assert.AreEqual("@F1@", family.ID);
				Assert.AreEqual(null, family.HusbandID);
				Assert.AreEqual(null, family.WifeID);

				Assert.AreNotEqual(null, family.ChildIDs);
				Assert.AreEqual(0, family.ChildIDs.Count);

				Assert.AreNotEqual(null, family.Facts);
				Assert.AreEqual(0, family.Facts.Count);

				Assert.AreNotEqual(null, family.Citations);
				Assert.AreEqual(0, family.Citations.Count);

				Assert.AreNotEqual(null, family.Media);
				Assert.AreEqual(0, family.Media.Count);

				Assert.AreNotEqual(null, family.Notes);
				Assert.AreEqual(0, family.Notes.Count);
			}
		}

        private static void Test_FamilyWithAllTokens()
        {
            var gedcomFile = @"..\..\TestFiles\Family\Family_AllTokens.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
				// Eat first line
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var family = GedcomFamily.Read(stream, line, out line);
				Assert.AreNotEqual(null, family);
				Assert.AreEqual("@F1@", family.ID);
				Assert.AreEqual("@I1@", family.HusbandID);
				Assert.AreEqual("@I2@", family.WifeID);

				Assert.AreNotEqual(null, family.ChildIDs);
				Assert.AreEqual(2, family.ChildIDs.Count);
				Assert.AreEqual("@I3@", family.ChildIDs[0]);
				Assert.AreEqual("@I4@", family.ChildIDs[1]);

				Assert.AreEqual("2", family.NoChildren);

				Assert.AreNotEqual(null, family.SubmitterIDs);
				Assert.AreEqual(2, family.SubmitterIDs.Count);
				Assert.AreEqual("@SUBM1@", family.SubmitterIDs[0]);
				Assert.AreEqual("@SUBM2@", family.SubmitterIDs[1]);

				Assert.AreNotEqual(null, family.Facts);
				Assert.AreEqual(3, family.Facts.Count);
				Assert.AreEqual("MARR", family.Facts[0].Type);
				Assert.IsFalse(family.Facts[0].IsCustom);
				Assert.IsTrue(family.Facts[0].IsShared);
				Assert.IsFalse(family.Facts[0].IsLDS);

				Assert.AreEqual("SLGS", family.Facts[1].Type);
				Assert.IsFalse(family.Facts[1].IsCustom);
				Assert.IsTrue(family.Facts[1].IsShared);
				Assert.IsTrue(family.Facts[1].IsLDS);

				Assert.AreEqual("Custom", family.Facts[2].Type);
				Assert.IsTrue(family.Facts[2].IsCustom);
				Assert.IsTrue(family.Facts[2].IsShared);
				Assert.IsFalse(family.Facts[2].IsLDS);

				Assert.AreNotEqual(null, family.Citations);
				Assert.AreEqual(2, family.Citations.Count);
				Assert.AreEqual("@S1@", family.Citations[0].SourceID);
				Assert.AreEqual("Citation 1 detail", family.Citations[0].Page);
				Assert.AreEqual("@S2@", family.Citations[1].SourceID);
				Assert.AreEqual("Citation 2 detail", family.Citations[1].Page);

				Assert.AreNotEqual(null, family.Media);
				Assert.AreEqual(2, family.Media.Count);
				Assert.AreEqual("@M1@", family.Media[0].ID);
				Assert.AreEqual("@M2@", family.Media[1].ID);

				Assert.AreNotEqual(null, family.Notes);
				Assert.AreEqual(2, family.Notes.Count);
				Assert.IsTrue(string.IsNullOrEmpty(family.Notes[0].ID));
				Assert.AreEqual("@N1@", family.Notes[1].ID);

				Assert.AreNotEqual(null, family.ReferenceNumbers);
				Assert.AreEqual(2, family.ReferenceNumbers.Count);
				Assert.AreEqual("Refn-1", family.ReferenceNumbers[0]);
				Assert.AreEqual("Refn-2", family.ReferenceNumbers[1]);
				Assert.AreEqual("Rin-1", family.RIN);

				Assert.AreNotEqual(null, family.ChangeDate);
				Assert.AreEqual("01 Jan 2000", family.ChangeDate.Date);
				Assert.AreEqual("12:34:56.789", family.ChangeDate.Time);
				Assert.AreNotEqual(null, family.ChangeDate.Notes);
				Assert.AreEqual(1, family.ChangeDate.Notes.Count);
				Assert.AreEqual("Change note", family.ChangeDate.Notes[0].Text);
			}
        }

		private void Test_FamilyAbruptEOF_MultipleTests()
		{
			var gedcomFile = @"..\..\TestFiles\Family\Family_AbruptEOF_MultipleTests.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat the test title line
				var line = GedcomLine.Read(stream);

				// Family w/ ID
				line = GedcomLine.Read(stream);
				var family = GedcomFamily.Read(stream, line, out line);
				Assert.AreNotEqual(null, family);
				Assert.IsTrue(family.IsValid());
				Assert.AreEqual(null, line);

				// Family w/o ID
				line = GedcomLine.Read(stream);
				family = GedcomFamily.Read(stream, line, out line);
				Assert.AreNotEqual(null, family);
				Assert.IsFalse(family.IsValid());
				Assert.AreEqual(null, line);
			}
		}
	}
}