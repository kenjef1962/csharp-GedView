using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomSourceTests
    {
        [TestMethod]
        public void ReadGedcomSourceTests()
        {
            ReadGedcomSourceTest1();
            ReadGedcomSourceTest2();

            // Read simple text
            var gedcomFile = @"..\..\TestFiles\Source\SourceTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                ReadGedcomSourceTest3(stream);
                ReadGedcomSourceTest4(stream);
            }
        }

        private void ReadGedcomSourceTest1()
        {
            // Null File Stream
            GedcomLine line;
            var Source = GedcomSource.Read(null, null, out line);

            Assert.AreEqual(null, Source);
            Assert.AreEqual(null, line);
        }

        private void ReadGedcomSourceTest2()
        {
            // Empty File
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var Source = GedcomSource.Read(stream, line, out line);

                Assert.AreEqual(null, Source);
                Assert.AreEqual(null, line);
            }
        }

        private void ReadGedcomSourceTest3(StreamReader stream)
        {
            // Non Source
            var line = GedcomLine.Read(stream);
            var Source = GedcomSource.Read(stream, line, out line);

            Assert.AreEqual(null, Source);
            Assert.AreEqual(null, line);
        }

        private void ReadGedcomSourceTest4(StreamReader stream)
        {
            // Full Source
            // Source ID w/ Name
            var line = GedcomLine.Read(stream);
            var Source = GedcomSource.Read(stream, line, out line);

            Assert.AreNotEqual(null, Source);
            Assert.AreEqual("@S1@", Source.ID);
            Assert.AreEqual("Author detail", Source.Author);
            Assert.AreEqual("Title detail", Source.Title);
            Assert.AreEqual("Publisher detail", Source.Publisher);
            Assert.AreEqual("@R1@", Source.Repository.ID);
            Assert.AreEqual("@M1@", Source.Media[0].ID);
            Assert.AreEqual("@M2@", Source.Media[1].ID);
            Assert.AreEqual("Note 1 details", Source.Notes[0].Text);
            Assert.AreEqual("Note 2 details", Source.Notes[1].Text);

            Assert.AreEqual(null, line);
        }
    }
}