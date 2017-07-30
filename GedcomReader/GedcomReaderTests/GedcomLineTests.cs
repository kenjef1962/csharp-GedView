using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomLineTests
    {
        [TestMethod]
        public void DoGedcomLineTests()
        {
            Test_NullFileStream();

            var gedcomFile = @"..\..\TestFiles\Line\Line_MultipleTests.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                Test_EmptyLine(stream);
                Test_InvalidLevel(stream);
                Test_ValidLevel(stream);
                Test_LevelTokenLower(stream);
                Test_LevelTokenUpper(stream);
                Test_LevelTokenData(stream);
                Test_LevelIDToken(stream);
                Test_LevelTokenID(stream);
                Test_EOF(stream);
            }
        }

        private void Test_NullFileStream()
        {
            // Null File Stream
            var line = GedcomLine.Read(null);
            Assert.AreEqual(null, line);
        }

        private void Test_EmptyLine(StreamReader stream)
        {
            // Empty Line
            var line = GedcomLine.Read(stream);
            Assert.AreEqual(null, line);
        }

        private void Test_InvalidLevel(StreamReader stream)
        {
            // A
            var line = GedcomLine.Read(stream);
            Assert.AreEqual(null, line);
        }

        private void Test_ValidLevel(StreamReader stream)
        {
            // 0
            var line = GedcomLine.Read(stream);
            Assert.AreNotEqual(null, line);
			Assert.IsFalse(line.IsValid());
			Assert.AreEqual(0, line.Level);
            Assert.IsTrue(string.IsNullOrEmpty(line.Token));
            Assert.IsTrue(string.IsNullOrEmpty(line.Data));
            Assert.IsFalse(line.IsXref);
        }

        private void Test_LevelTokenLower(StreamReader stream)
        {
            // 0 Token
            var line = GedcomLine.Read(stream);
			Assert.AreNotEqual(null, line);
			Assert.IsTrue(line.IsValid());
			Assert.AreEqual(0, line.Level);
            Assert.AreEqual("TOKEN", line.Token);
            Assert.IsTrue(string.IsNullOrEmpty(line.Data));
            Assert.IsFalse(line.IsXref);
        }

        private void Test_LevelTokenUpper(StreamReader stream)
        {
            // 0 TOKEN
            var line = GedcomLine.Read(stream);
			Assert.AreNotEqual(null, line);
			Assert.IsTrue(line.IsValid());
			Assert.AreEqual(0, line.Level);
            Assert.AreEqual("TOKEN", line.Token);
            Assert.IsTrue(string.IsNullOrEmpty(line.Data));
            Assert.IsFalse(line.IsXref);
        }

        private void Test_LevelTokenData(StreamReader stream)
        {
            // 0 TOKEN Data Data
            var line = GedcomLine.Read(stream);
            Assert.AreNotEqual(null, line);
			Assert.IsTrue(line.IsValid());
			Assert.AreEqual(0, line.Level);
            Assert.AreEqual("TOKEN", line.Token);
            Assert.AreEqual("Data Data", line.Data);
            Assert.IsFalse(line.IsXref);
        }

        private void Test_LevelIDToken(StreamReader stream)
        {
            // 0 @ID@ TOKEN
            var line = GedcomLine.Read(stream);
            Assert.AreNotEqual(null, line);
			Assert.IsTrue(line.IsValid());
			Assert.AreEqual(0, line.Level);
            Assert.AreEqual("TOKEN", line.Token);
            Assert.AreEqual("@ID@", line.Data);
            Assert.IsTrue(line.IsXref);
        }

        private void Test_LevelTokenID(StreamReader stream)
        {
            // 0 TOKEN @ID@
            var line = GedcomLine.Read(stream);
            Assert.AreNotEqual(null, line);
			Assert.IsTrue(line.IsValid());
			Assert.AreEqual(0, line.Level);
            Assert.AreEqual("TOKEN", line.Token);
            Assert.AreEqual("@ID@", line.Data);
            Assert.IsFalse(line.IsXref);
        }

        private void Test_EOF(StreamReader stream)
        {
            // End of file
            var line = GedcomLine.Read(stream);
            Assert.AreEqual(null, line);
        }
    }
}