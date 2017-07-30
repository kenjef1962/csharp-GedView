using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomRepositoryTests
    {
        [TestMethod]
        public void ReadGedcomRepositoryTest()
        {
            ReadGedcomRepositoryTest1();
            ReadGedcomRepositoryTest2();

            // Read simple text
            var gedcomFile = @"..\..\TestFiles\Repository\RepositoryTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                ReadGedcomRepositoryTest3(stream);
                ReadGedcomRepositoryTest4(stream);
            }
        }

        private void ReadGedcomRepositoryTest1()
        {
            // Null File Stream
            GedcomLine line;
            var repository = GedcomRepository.Read(null, null, out line);

            Assert.AreEqual(null, repository);
            Assert.AreEqual(null, line);
        }

        private void ReadGedcomRepositoryTest2()
        {
            // Empty File
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var repository = GedcomRepository.Read(stream, line, out line);

                Assert.AreEqual(null, repository);
                Assert.AreEqual(null, line);
            }
        }

        private void ReadGedcomRepositoryTest3(StreamReader stream)
        {
            // Non Repository
            var line = GedcomLine.Read(stream);
            var repository = GedcomRepository.Read(stream, line, out line);

            Assert.AreEqual(null, repository);
            Assert.AreEqual(null, line);
        }

        private void ReadGedcomRepositoryTest4(StreamReader stream)
        {
            // Full Repository
            // Repository ID w/ Name
            var line = GedcomLine.Read(stream);
            var repository = GedcomRepository.Read(stream, line, out line);

            Assert.AreNotEqual(null, repository);
            Assert.AreEqual("@R1@", repository.ID);
            Assert.AreEqual("Repository", repository.Name);
            Assert.AreNotEqual(null, repository.Address);
			Assert.AreEqual("360 W 4800 N", repository.Address.Address);
			Assert.AreEqual("Provo", repository.Address.City);
			Assert.AreEqual("Utah", repository.Address.State);
			Assert.AreEqual("84604", repository.Address.PostalCode);
			Assert.AreEqual("Repository Phone", repository.Phone);
            Assert.AreEqual("Repository Email", repository.Email);

            Assert.AreEqual(null, line);
        }
    }
}