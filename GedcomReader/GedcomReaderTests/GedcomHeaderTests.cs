using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomHeaderTests
    {
        [TestMethod]
        public void DoGedcomHeaderTests()
        {
            Test_NullFileSteam();    
            Test_EmpyFile();
            Test_NonHeader();
            Test_NoTokens();
			Test_AllTokens();
			Test_UnknownTokens();
			Test_Validity();
			Test_AbruptEOF();
			Test_CodeCoverage();
        }

		private void Test_NullFileSteam()
        {
            GedcomLine line;
            var header = GedcomHeader.Read(null, null, out line);

            Assert.AreEqual(null, line);
            Assert.AreEqual(null, header);
        }

		private void Test_EmpyFile()
        {
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var header = GedcomHeader.Read(stream, line, out line);

                Assert.AreEqual(null, line);
                Assert.AreEqual(null, header);
            }
        }

        private void Test_NonHeader()
        {
			var gedcomFile = @"..\..\TestFiles\Header\Header_NonHeader.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat the test title line
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var header = GedcomHeader.Read(stream, line, out line);

				Assert.AreEqual(null, line);
				Assert.AreEqual(null, header);
			}
        }

        private void Test_NoTokens()
        {
			var gedcomFile = @"..\..\TestFiles\Header\Header_NoTokens.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat the test title line
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var header = GedcomHeader.Read(stream, line, out line);

				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.IsTrue(string.IsNullOrEmpty(header.Source));
				Assert.IsTrue(string.IsNullOrEmpty(header.SourceVersion));
				Assert.IsTrue(string.IsNullOrEmpty(header.SourceProduct));
				Assert.IsTrue(string.IsNullOrEmpty(header.SourceCompany));
				Assert.AreEqual(null, header.SourceAddress);
				Assert.IsTrue(string.IsNullOrEmpty(header.Desintation));
				Assert.IsTrue(string.IsNullOrEmpty(header.TransmissionDate));
				Assert.IsTrue(string.IsNullOrEmpty(header.SubmitterID));
				Assert.IsTrue(string.IsNullOrEmpty(header.SubmissionID));
				Assert.IsTrue(string.IsNullOrEmpty(header.Filename));
				Assert.IsTrue(string.IsNullOrEmpty(header.GedcomVersion));
				Assert.IsTrue(string.IsNullOrEmpty(header.GedcomForm));
				Assert.IsTrue(string.IsNullOrEmpty(header.Charset));
				Assert.IsTrue(string.IsNullOrEmpty(header.CharsetVersion));
				Assert.IsTrue(string.IsNullOrEmpty(header.Language));
				Assert.IsTrue(string.IsNullOrEmpty(header.Note));
			}
        }

        private void Test_AllTokens()
        {
			var gedcomFile = @"..\..\TestFiles\Header\Header_AllTokens.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat the test title line
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var header = GedcomHeader.Read(stream, line, out line);

				Assert.AreNotEqual(null, header);
				Assert.IsTrue(header.IsValid());
				Assert.AreEqual("FTM", header.Source);
				Assert.AreEqual("Family Tree Maker (22.0.0.239)", header.SourceVersion);
				Assert.AreEqual("Family Tree Maker for Windows", header.SourceProduct);
				Assert.IsFalse(string.IsNullOrEmpty(header.SourceCompany));
				Assert.AreEqual("Ancestry.com", header.SourceCompany);
				Assert.AreNotEqual(null, header.SourceAddress);
				Assert.AreEqual("360 W 4800 N", header.SourceAddress.Address);
				Assert.IsTrue(string.IsNullOrEmpty(header.SourceAddress.Address1));
				Assert.IsTrue(string.IsNullOrEmpty(header.SourceAddress.Address2));
				Assert.AreEqual("Provo", header.SourceAddress.City);
				Assert.AreEqual("UT", header.SourceAddress.State);
				Assert.AreEqual("84604", header.SourceAddress.PostalCode);
				Assert.AreEqual("USA", header.SourceAddress.Country);
				Assert.AreEqual("GED55", header.Desintation);
				Assert.AreEqual("19 Aug 2013", header.TransmissionDate);
				Assert.AreEqual("@SUBM@", header.SubmitterID);
				Assert.AreEqual("@SUBN@", header.SubmissionID);
				Assert.AreEqual("ReadHeaderTest.ged", header.Filename);
				Assert.AreEqual("(c) 2013 Ancestry.com", header.Copyright);
				Assert.AreEqual("5.5", header.GedcomVersion);
				Assert.AreEqual("LINEAGE-LINKED", header.GedcomForm);
				Assert.AreEqual("UTF-8", header.Charset);
				Assert.AreEqual("1.0", header.CharsetVersion);
				Assert.AreEqual("English", header.Language);
				Assert.AreEqual("Note line 1\r\nNote line 2", header.Note);
				Assert.AreEqual(null, line);
			}
        }

		private void Test_UnknownTokens()
		{
			var gedcomFile = @"..\..\TestFiles\Header\Header_UnknownTokens.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat the test title line
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				var header = GedcomHeader.Read(stream, line, out line);

				Assert.AreNotEqual(null, header);
				Assert.AreEqual(null, line);
			}
		}

		private void Test_Validity()
		{
			var gedcomFile = @"..\..\TestFiles\Header\Header_Validity_MultipleTests.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat the test title line
				var line = GedcomLine.Read(stream);

				// Missing Source
				line = GedcomLine.Read(stream);
				var header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);

				// Missing Submitter ID
				line = GedcomLine.Read(stream);
				header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);

				// Missing Gedcom Version
				line = GedcomLine.Read(stream);
				header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);

				// Missing Gedcom Form
				line = GedcomLine.Read(stream);
				header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);

				// Missing Characterset
				line = GedcomLine.Read(stream);
				header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);

				// All required elements present
				line = GedcomLine.Read(stream);
				header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsTrue(header.IsValid());
				Assert.AreEqual(null, line);
			}
		}

		private void Test_AbruptEOF()
		{
			var gedcomFile = @"..\..\TestFiles\Header\Header_AbruptEOF_MultipleTests.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat the test title line
				var line = GedcomLine.Read(stream);

				// Header
				line = GedcomLine.Read(stream);
				var header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);

				// Header Source
				line = GedcomLine.Read(stream);
				header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);

				// Header Source Company
				line = GedcomLine.Read(stream);
				header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);

				// Header Gedcom
				line = GedcomLine.Read(stream);
				header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);

				// Header Date
				line = GedcomLine.Read(stream);
				header = GedcomHeader.Read(stream, line, out line);
				Assert.AreNotEqual(null, header);
				Assert.IsFalse(header.IsValid());
				Assert.AreEqual(null, line);
			}
		}

		private void Test_CodeCoverage()
		{
			var gedcomFile = @"..\..\TestFiles\Header\Header_CodeCoverage.ged";
			using (var stream = File.OpenText(gedcomFile))
			{
				// Eat the test title line
				var line = GedcomLine.Read(stream);

				line = GedcomLine.Read(stream);
				line = GedcomHeader.ReadToken_CHAR(stream, line, null);
				line = GedcomHeader.ReadToken_DATE(stream, line, null);
				line = GedcomHeader.ReadToken_GEDC(stream, line, null);
				line = GedcomHeader.ReadToken_SOUR(stream, line, null);
				line = GedcomHeader.ReadToken_SOUR_CORP(stream, line, null);
			}
		}
	}
}