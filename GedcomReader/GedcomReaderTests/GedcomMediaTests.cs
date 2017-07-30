using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomMediaTests
    {
        [TestMethod]
        public void ReadGedcomMediaTests()
        {
            ReadGedcomMediaTest1();
            ReadGedcomMediaTest2();

            // Read simple text
            var gedcomFile = @"..\..\TestFiles\Media\MediaTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                ReadGedcomMediaTest3(stream);
                ReadGedcomMediaTest4(stream);
            }
        }

        private void ReadGedcomMediaTest1()
        {
            // Null File Stream
            GedcomLine line;
            var Media = GedcomMedia.Read(null, null, out line);

            Assert.AreEqual(null, Media);
            Assert.AreEqual(null, line);
        }

        private void ReadGedcomMediaTest2()
        {
            // Empty File
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var Media = GedcomMedia.Read(stream, line, out line);

                Assert.AreEqual(null, Media);
                Assert.AreEqual(null, line);
            }
        }

        private void ReadGedcomMediaTest3(StreamReader stream)
        {
            // Non Media
            var line = GedcomLine.Read(stream);
            var Media = GedcomMedia.Read(stream, line, out line);

            Assert.AreEqual(null, Media);
            Assert.AreEqual(null, line);
        }

        private void ReadGedcomMediaTest4(StreamReader stream)
        {
            // Full Media
            // Media ID w/ Name
            var line = GedcomLine.Read(stream);
            var Media = GedcomMedia.Read(stream, line, out line);

            Assert.AreNotEqual(null, Media);
            Assert.AreEqual("@M1@", Media.ID);
            Assert.AreEqual("Format", Media.Format);
            Assert.AreEqual("Title detail", Media.Title);
            Assert.AreEqual("c:\\path\\filename.ext", Media.Filename);
            Assert.AreEqual("Note 1 details", Media.Notes[0].Text);
            Assert.AreEqual("Note 2 details", Media.Notes[1].Text);

            Assert.AreEqual(null, line);
        }
    }
}