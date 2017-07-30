using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomIndividualTests
    {
        [TestMethod]
        public void DoGedcomIndividualTests()
        {
            Test_NullStream();
            Test_EmptyFile();
			Test_NonIndividual();
			Test_IndividualNoTokens();
			Test_IndividualAllTokens();
			Test_IndividualAbruptEOF_MultipleTests();
		}

        private void Test_NullStream()
        {
            // Null File Stream
            GedcomLine line;
            var Individual = GedcomIndividual.Read(null, null, out line);

            Assert.AreEqual(null, Individual);
            Assert.AreEqual(null, line);
        }

        private void Test_EmptyFile()
        {
            // Empty File
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var Individual = GedcomIndividual.Read(stream, line, out line);

                Assert.AreEqual(null, Individual);
                Assert.AreEqual(null, line);
            }
        }

        private void Test_NonIndividual()
        {
			// Non Individual
			var gedcomFile = @"..\..\TestFiles\Individual\Individual_NonIndividual.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var individual = GedcomIndividual.Read(stream, line, out line);

				Assert.AreEqual(null, individual);
			}
        }

		private void Test_IndividualNoTokens()
		{
			// Non Individual
			var gedcomFile = @"..\..\TestFiles\Individual\Individual_NoTokens.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat first line
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var individual = GedcomIndividual.Read(stream, line, out line);

				Assert.AreNotEqual(null, individual);
				Assert.AreEqual("@I1@", individual.ID);

				Assert.AreNotEqual(null, individual.Names);
				Assert.AreEqual(0, individual.Names.Count);

				Assert.IsTrue(string.IsNullOrEmpty(individual.Sex));

				Assert.AreNotEqual(null, individual.Facts);
				Assert.AreEqual(0, individual.Facts.Count);

				Assert.AreNotEqual(null, individual.FamilyChildIDs);
				Assert.AreEqual(0, individual.FamilyChildIDs.Count);

				Assert.AreNotEqual(null, individual.FamilySpouseIDs);
				Assert.AreEqual(0, individual.FamilySpouseIDs.Count);

				Assert.AreNotEqual(null, individual.Citations);
				Assert.AreEqual(0, individual.Citations.Count);

				Assert.AreNotEqual(null, individual.Media);
				Assert.AreEqual(0, individual.Media.Count);

				Assert.AreNotEqual(null, individual.Notes);
				Assert.AreEqual(0, individual.Notes.Count);

				Assert.IsTrue(string.IsNullOrEmpty(individual.RFN));
				Assert.IsTrue(string.IsNullOrEmpty(individual.AFN));
				Assert.IsTrue(string.IsNullOrEmpty(individual.RIN));

				Assert.AreNotEqual(null, individual.ReferenceNumbers);
				Assert.AreEqual(0, individual.ReferenceNumbers.Count);

				Assert.AreEqual(null, individual.ChangeDate);
			}
		}

        private void Test_IndividualAllTokens()
        {
            // Full Individual
			var gedcomFile = @"..\..\TestFiles\Individual\Individual_AllTokens.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var individual = GedcomIndividual.Read(stream, line, out line);

				Assert.AreNotEqual(null, individual);
				Assert.AreEqual("@I1@", individual.ID);

				Assert.AreNotEqual(null, individual.Names);
				Assert.AreEqual(1, individual.Names.Count);
				Assert.AreEqual("Kendall Jay /Jefferson/", individual.Names[0].Fullname);

				Assert.AreEqual("M", individual.Sex);

				Assert.AreNotEqual(null, individual.Facts);
				Assert.AreEqual(3, individual.Facts.Count);
				Assert.AreEqual("BIRT", individual.Facts[0].Type);
				Assert.IsFalse(individual.Facts[0].IsShared);
				Assert.IsFalse(individual.Facts[0].IsLDS);
				Assert.IsFalse(individual.Facts[0].IsCustom);
				Assert.AreEqual("BAPL", individual.Facts[1].Type);
				Assert.IsFalse(individual.Facts[1].IsShared);
				Assert.IsTrue(individual.Facts[1].IsLDS);
				Assert.IsFalse(individual.Facts[1].IsCustom);
				Assert.AreEqual("CustomFact", individual.Facts[2].Type);
				Assert.IsFalse(individual.Facts[2].IsShared);
				Assert.IsFalse(individual.Facts[2].IsLDS);
				Assert.IsTrue(individual.Facts[2].IsCustom);

				Assert.AreNotEqual(null, individual.FamilyChildIDs);
				Assert.AreEqual(2, individual.FamilyChildIDs.Count);
				Assert.AreEqual("@F1@", individual.FamilyChildIDs[0]);
				Assert.AreEqual("@F2@", individual.FamilyChildIDs[1]);

				Assert.AreNotEqual(null, individual.FamilySpouseIDs);
				Assert.AreEqual(2, individual.FamilySpouseIDs.Count);
				Assert.AreEqual("@F3@", individual.FamilySpouseIDs[0]);
				Assert.AreEqual("@F4@", individual.FamilySpouseIDs[1]);

				Assert.AreNotEqual(null, individual.Citations);
				Assert.AreEqual(2, individual.Citations.Count);
				Assert.AreEqual("@S1@", individual.Citations[0].SourceID);
				Assert.AreEqual("@S2@", individual.Citations[1].SourceID);

				Assert.AreNotEqual(null, individual.Media);
				Assert.AreEqual(2, individual.Media.Count);
				Assert.AreEqual("@M1@", individual.Media[0].ID);
				Assert.AreEqual("@M2@", individual.Media[1].ID);

				Assert.AreNotEqual(null, individual.Notes);
				Assert.AreEqual(2, individual.Notes.Count);
				Assert.IsTrue(string.IsNullOrEmpty(individual.Notes[0].ID));
				Assert.AreEqual("@N1@", individual.Notes[1].ID);

				Assert.AreEqual("Rfn-1", individual.RFN);
				Assert.AreEqual("Afn-1", individual.AFN);
				Assert.AreEqual("Rin-1", individual.RIN);

				Assert.AreNotEqual(null, individual.ReferenceNumbers);
				Assert.AreEqual(2, individual.ReferenceNumbers.Count);
				Assert.AreEqual("Refn-1", individual.ReferenceNumbers[0]);
				Assert.AreEqual("Refn-2", individual.ReferenceNumbers[1]);

				Assert.AreNotEqual(null, individual.ChangeDate);
				Assert.AreEqual("01 Jan 2000", individual.ChangeDate.Date);
				Assert.AreEqual("12:34:56.789", individual.ChangeDate.Time);
				Assert.AreNotEqual(null, individual.ChangeDate.Notes);
				Assert.AreEqual(1, individual.ChangeDate.Notes.Count);
				Assert.AreEqual("Change note", individual.ChangeDate.Notes[0].Text);
			}
        }

		private void Test_IndividualAbruptEOF_MultipleTests()
		{
			var gedcomFile = @"..\..\TestFiles\Individual\Individual_AbruptEOF_MultipleTests.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat the test title line
				var line = GedcomLine.Read(stream);

				// Individual w/ ID
				line = GedcomLine.Read(stream);
				var individual = GedcomIndividual.Read(stream, line, out line);
				Assert.AreNotEqual(null, individual);
				Assert.IsTrue(individual.IsValid());
				Assert.AreEqual(null, line);

				// Individual w/o ID
				line = GedcomLine.Read(stream);
				individual = GedcomIndividual.Read(stream, line, out line);
				Assert.AreNotEqual(null, individual);
				Assert.IsFalse(individual.IsValid());
				Assert.AreEqual(null, line);

				// Individual FAMC
				line = GedcomLine.Read(stream);
				individual = GedcomIndividual.Read(stream, line, out line);
				Assert.AreNotEqual(null, individual);
				Assert.IsTrue(individual.IsValid());
				Assert.AreEqual(1, individual.FamilyChildIDs.Count);
				Assert.AreEqual("@F1@", individual.FamilyChildIDs[0]);
				Assert.AreEqual(null, line);

				// Individual FAMS
				line = GedcomLine.Read(stream);
				individual = GedcomIndividual.Read(stream, line, out line);
				Assert.AreNotEqual(null, individual);
				Assert.IsTrue(individual.IsValid());
				Assert.AreEqual(1, individual.FamilySpouseIDs.Count);
				Assert.AreEqual("@F1@", individual.FamilySpouseIDs[0]);
				Assert.AreEqual(null, line);
			}
		}
	}
}