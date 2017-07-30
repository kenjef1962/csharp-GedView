using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomCitationTests
    {
        [TestMethod]
        public void ReadGedcomCitationTest()
        {
            ReadGedcomCitationTest1();
            ReadGedcomCitationTest2();

            // Read simple text
            var gedcomFile = @"..\..\TestFiles\Citation\CitationTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                ReadGedcomCitationTest3(stream);
                ReadGedcomCitationTest4(stream);
            }
        }

        private void ReadGedcomCitationTest1()
        {
            // Null File Stream
            GedcomLine line;
            var citation = GedcomCitation.Read(null, null, out line);

            Assert.AreEqual(null, citation);
            Assert.AreEqual(null, line);
        }

        private void ReadGedcomCitationTest2()
        {
            // Empty File
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var citation = GedcomCitation.Read(stream, line, out line);

                Assert.AreEqual(null, citation);
                Assert.AreEqual(null, line);
            }
        }

        private void ReadGedcomCitationTest3(StreamReader stream)
        {
            // Non Citation
            var line = GedcomLine.Read(stream);
            var citation = GedcomCitation.Read(stream, line, out line);

            Assert.AreEqual(null, citation);
            Assert.AreEqual(null, line);
        }

        private void ReadGedcomCitationTest4(StreamReader stream)
        {
            // Citation 
            var line = GedcomLine.Read(stream);
            var citation = GedcomCitation.Read(stream, line, out line);

            Assert.AreNotEqual(null, citation);
            Assert.AreEqual("@S1@", citation.SourceID);
            Assert.AreEqual("Citation 1 Detail", citation.Page);
            Assert.AreEqual("Citation 1 Text", citation.Data);
            Assert.AreEqual("Citation 1 Quality", citation.Quality);
            Assert.AreNotEqual(null, citation.Media);
            Assert.AreEqual(2, citation.Media.Count);
            Assert.AreNotEqual(null, citation.Notes);
            Assert.AreEqual(2, citation.Notes.Count);

            Assert.AreEqual(null, line);
        }
    }
}