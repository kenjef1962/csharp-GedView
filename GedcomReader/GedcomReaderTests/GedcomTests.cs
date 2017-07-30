using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomTests
    {
        [TestMethod]
        public void ReadGedcomTest()
        {
            Test_NoGedcomFile();
            Test_NonExistantGedcomFile();
            Test_EmptyGedcomFile();
            Test_FullGedcomFile();
        }

        private void Test_NoGedcomFile()
        {
            var gedcom = new Gedcom(string.Empty);

            Assert.AreNotEqual(null, gedcom);
            Assert.IsTrue(string.IsNullOrEmpty(gedcom.Filename));
        }

        private void Test_NonExistantGedcomFile()
        {
            var gedcomFile = @"..\..\TestFiles\NonExistant.ged";
            var gedcom = new Gedcom(gedcomFile);

            Assert.AreNotEqual(null, gedcom);
            Assert.AreEqual(gedcom.Filename, gedcomFile);
        }

        private void Test_EmptyGedcomFile()
        {
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            var gedcom = new Gedcom(gedcomFile);

            Assert.AreNotEqual(null, gedcom);
            Assert.IsFalse(string.IsNullOrEmpty(gedcom.Filename));
            Assert.AreEqual(gedcomFile, gedcom.Filename);
            Assert.AreEqual(null, gedcom.Header);
            Assert.AreEqual(0, gedcom.Individuals.Count);
            Assert.AreEqual(0, gedcom.Families.Count);
            Assert.AreEqual(0, gedcom.Repositories.Count);
            Assert.AreEqual(0, gedcom.Sources.Count);
            Assert.AreEqual(0, gedcom.Media.Count);
            Assert.AreEqual(0, gedcom.Notes.Count);
        }

        private void Test_FullGedcomFile()
        {
            var gedcomFile = @"..\..\TestFiles\Gedcom\Jefferson_Active (Sync).ged";
            var gedcom = new Gedcom(gedcomFile, true);

            Assert.AreNotEqual(null, gedcom);
            Assert.IsFalse(string.IsNullOrEmpty(gedcom.Filename));
            Assert.AreEqual(gedcomFile, gedcom.Filename);
            Assert.AreNotEqual(null, gedcom.Header);

            Assert.AreNotEqual(null, gedcom.Individuals);
            Assert.AreEqual(330, gedcom.Individuals.Count);

            Assert.AreNotEqual(null, gedcom.Families);
            Assert.AreEqual(109, gedcom.Families.Count);

            Assert.AreNotEqual(null, gedcom.Repositories);
            Assert.AreEqual(13, gedcom.Repositories.Count);

            Assert.AreNotEqual(null, gedcom.Sources);
            Assert.AreEqual(67, gedcom.Sources.Count);

            Assert.AreNotEqual(null, gedcom.Media);
            Assert.AreEqual(301, gedcom.Media.Count);

            Assert.AreNotEqual(null, gedcom.Notes);
            Assert.AreEqual(78, gedcom.Notes.Count);
        }
    }
}