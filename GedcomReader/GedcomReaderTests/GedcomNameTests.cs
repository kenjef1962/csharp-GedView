using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomNameTests
    {
        [TestMethod]
        public void DoGedcomNameTests()
        {
            Test_NullStream();
            Test_EmptyFile();
			Test_Name_MultipleTests();
        }

        private void Test_NullStream()
        {
            // Null File Stream
            GedcomLine line;
            var name = GedcomName.Read(null, null, out line);

            Assert.AreEqual(null, name);
            Assert.AreEqual(null, line);
        }

        private void Test_EmptyFile()
        {
            // Empty File
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var name = GedcomName.Read(stream, line, out line);

                Assert.AreEqual(null, name);
                Assert.AreEqual(null, line);
            }
        }

        private void Test_Name_MultipleTests()
        {
            var gedcomFile = @"..\..\TestFiles\Name\Name_MultipleTests.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
				// Non Name
				var line = GedcomLine.Read(stream);
				var name = GedcomName.Read(stream, line, out line);

				Assert.AreEqual(null, name);
				Assert.AreEqual(null, line);

				// Empty Name
				line = GedcomLine.Read(stream);
				name = GedcomName.Read(stream, line, out line);

				Assert.AreNotEqual(null, name);
				Assert.IsFalse(name.IsValid());
				Assert.IsTrue(string.IsNullOrEmpty(name.Fullname));
				Assert.IsTrue(string.IsNullOrEmpty(name.Prefix));
				Assert.IsTrue(string.IsNullOrEmpty(name.Given));
				Assert.IsTrue(string.IsNullOrEmpty(name.Nickname));
				Assert.IsTrue(string.IsNullOrEmpty(name.SurnamePrefix));
				Assert.IsTrue(string.IsNullOrEmpty(name.Surname));
				Assert.IsTrue(string.IsNullOrEmpty(name.Suffix));
				Assert.AreNotEqual(null, name.Citations);
				Assert.AreEqual(0, name.Citations.Count);
				Assert.AreNotEqual(null, name.Notes);
				Assert.AreEqual(0, name.Notes.Count);
				Assert.AreEqual(null, line);

				// Basic Name - Given /Surname/
				line = GedcomLine.Read(stream);
				name = GedcomName.Read(stream, line, out line);

				Assert.AreNotEqual(null, name);
				Assert.IsTrue(name.IsValid());
				Assert.AreEqual("Kendall Jay /Jefferson/", name.Fullname);
				Assert.AreEqual("Kendall Jay", name.Given);
				Assert.IsTrue(string.IsNullOrEmpty(name.Prefix));
				Assert.IsTrue(string.IsNullOrEmpty(name.Nickname));
				Assert.IsTrue(string.IsNullOrEmpty(name.SurnamePrefix));
				Assert.AreEqual("Jefferson", name.Surname);
				Assert.IsTrue(string.IsNullOrEmpty(name.Suffix));
				Assert.AreNotEqual(null, name.Citations);
				Assert.AreEqual(0, name.Citations.Count);
				Assert.AreNotEqual(null, name.Notes);
				Assert.AreEqual(0, name.Notes.Count);
				Assert.AreEqual(null, line);

				// Basic Name - /Surname/ Given
				line = GedcomLine.Read(stream);
				name = GedcomName.Read(stream, line, out line);

				Assert.AreNotEqual(null, name);
				Assert.IsTrue(name.IsValid());
				Assert.AreEqual("/Jefferson/ Kendall Jay", name.Fullname);
				Assert.AreEqual("Kendall Jay", name.Given);
				Assert.IsTrue(string.IsNullOrEmpty(name.Prefix));
				Assert.IsTrue(string.IsNullOrEmpty(name.Nickname));
				Assert.IsTrue(string.IsNullOrEmpty(name.SurnamePrefix));
				Assert.AreEqual("Jefferson", name.Surname);
				Assert.IsTrue(string.IsNullOrEmpty(name.Suffix));
				Assert.AreNotEqual(null, name.Citations);
				Assert.AreEqual(0, name.Citations.Count);
				Assert.AreNotEqual(null, name.Notes);
				Assert.AreEqual(0, name.Notes.Count);
				Assert.AreEqual(null, line);

				// Basic Name - Given /Surname/ Given2
				line = GedcomLine.Read(stream);
				name = GedcomName.Read(stream, line, out line);

				Assert.AreNotEqual(null, name);
				Assert.IsTrue(name.IsValid());
				Assert.AreEqual("Kendall /Jefferson/ Jay", name.Fullname);
				Assert.AreEqual("Kendall Jay", name.Given);
				Assert.IsTrue(string.IsNullOrEmpty(name.Prefix));
				Assert.IsTrue(string.IsNullOrEmpty(name.Nickname));
				Assert.IsTrue(string.IsNullOrEmpty(name.SurnamePrefix));
				Assert.AreEqual("Jefferson", name.Surname);
				Assert.IsTrue(string.IsNullOrEmpty(name.Suffix));
				Assert.AreNotEqual(null, name.Citations);
				Assert.AreEqual(0, name.Citations.Count);
				Assert.AreNotEqual(null, name.Notes);
				Assert.AreEqual(0, name.Notes.Count);
				Assert.AreEqual(null, line);

				// Basic Name - Given Surname
				line = GedcomLine.Read(stream);
				name = GedcomName.Read(stream, line, out line);

				Assert.AreNotEqual(null, name);
				Assert.IsTrue(name.IsValid());
				Assert.AreEqual("Kendall Jay Jefferson", name.Fullname);
				Assert.AreEqual("Kendall Jay", name.Given);
				Assert.IsTrue(string.IsNullOrEmpty(name.Prefix));
				Assert.IsTrue(string.IsNullOrEmpty(name.Nickname));
				Assert.IsTrue(string.IsNullOrEmpty(name.SurnamePrefix));
				Assert.AreEqual("Jefferson", name.Surname);
				Assert.IsTrue(string.IsNullOrEmpty(name.Suffix));
				Assert.AreNotEqual(null, name.Citations);
				Assert.AreEqual(0, name.Citations.Count);
				Assert.AreNotEqual(null, name.Notes);
				Assert.AreEqual(0, name.Notes.Count);
				Assert.AreEqual(null, line);

				// Full Name
				line = GedcomLine.Read(stream);
				name = GedcomName.Read(stream, line, out line);

				Assert.AreNotEqual(null, name);
				Assert.IsTrue(name.IsValid());
				Assert.AreEqual("Kendall Jay /Jefferson/", name.Fullname);
				Assert.AreEqual("Mr.", name.Prefix);
				Assert.AreEqual("Kendall Jay", name.Given);
				Assert.AreEqual("KenJef", name.Nickname);
				Assert.AreEqual("<none>", name.SurnamePrefix);
				Assert.AreEqual("Jefferson", name.Surname);
				Assert.AreEqual("<none>", name.Suffix);
				Assert.AreEqual(2, name.Citations.Count);
				Assert.AreEqual("Citation 1 details", name.Citations[0].Page);
				Assert.AreEqual("Citation 2 details", name.Citations[1].Page);
				Assert.AreEqual(2, name.Notes.Count);
				Assert.AreEqual("Note 1 details", name.Notes[0].Text);
				Assert.AreEqual("Note 2 details", name.Notes[1].Text);

				Assert.AreEqual(null, line);
			}
        }
    }
}