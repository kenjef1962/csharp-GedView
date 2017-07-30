using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomUnknownTokenTests
    {
        [TestMethod]
        public void ReadGedcomUnknownTokenTest()
        {
            var gedcomFile = @"..\..\TestFiles\UnknownToken\UnknownTokenTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                ReadGedcomUnknownTokenTest1(stream);
                ReadGedcomUnknownTokenTest2(stream);
            }
        }

        private void ReadGedcomUnknownTokenTest1(StreamReader stream)
        {
            var line = GedcomLine.Read(stream);
            Assert.AreEqual(0, line.Level);
            Assert.AreEqual("HEAD", line.Token);

			line = ReadToken.ReadUnsupportedToken(stream, line);
            Assert.AreEqual(0, line.Level);
            Assert.AreEqual("TRLR", line.Token);

            // Eat null divider line
            GedcomLine.Read(stream);
        }

        private void ReadGedcomUnknownTokenTest2(StreamReader stream)
        {
            var line = GedcomLine.Read(stream);
            Assert.AreEqual(1, line.Level);
            Assert.AreEqual("BIRT", line.Token);

            line = ReadToken.ReadUnsupportedToken(stream, line);
            Assert.AreEqual(1, line.Level);
            Assert.AreEqual("DEAT", line.Token);
        }
    }
}