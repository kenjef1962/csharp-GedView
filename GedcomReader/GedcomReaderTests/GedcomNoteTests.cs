using System;
using GedcomReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GedcomReaderTests
{
    [TestClass]
    public class GedcomNoteTests
    {
        [TestMethod]
        public void ReadGedcomNoteTest()
        {
            ReadGedcomNoteTest1();
            ReadGedcomNoteTest2();

            // Read simple text
            var gedcomFile = @"..\..\TestFiles\Note\NoteTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                ReadGedcomNoteTest3(stream);
                ReadGedcomNoteTest4(stream);
				ReadGedcomNoteTest5(stream);
				ReadGedcomNoteTest6(stream);
			}
        }

        private void ReadGedcomNoteTest1()
        {
            // Null File Stream
            GedcomLine line;
            var note = GedcomNote.Read(null, null, out line);

            Assert.AreEqual(null, note);
            Assert.AreEqual(null, line);

            var text = GedcomText.Read(null, null, out line);

            Assert.IsTrue(string.IsNullOrEmpty(text));
            Assert.AreEqual(null, line);
        }

        private void ReadGedcomNoteTest2()
        {
            // Empty File
            var gedcomFile = @"..\..\TestFiles\EmptyTest.ged";
            using (var stream = File.OpenText(gedcomFile))
            {
                var line = GedcomLine.Read(stream);
                var Note = GedcomNote.Read(stream, line, out line);

                Assert.AreEqual(null, Note);
                Assert.AreEqual(null, line);

                var text = GedcomText.Read(null, null, out line);

                Assert.IsTrue(string.IsNullOrEmpty(text));
                Assert.AreEqual(null, line);
            }
        }

        private void ReadGedcomNoteTest3(StreamReader stream)
        {
            // Non Note
            var line = GedcomLine.Read(stream);
            var note = GedcomNote.Read(stream, line, out line);

            Assert.AreEqual(null, note);
            Assert.AreEqual(null, line);
        }

		private void ReadGedcomNoteTest4(StreamReader stream)
		{
			// Note
			var line = GedcomLine.Read(stream);
			var note = GedcomNote.Read(stream, line, out line);

			Assert.AreNotEqual(null, note);
			Assert.AreEqual("@N1@", note.ID);
			Assert.AreEqual("Note line 1\r\nNote line 2", note.Text);

			Assert.AreEqual(null, line);
		}

		private void ReadGedcomNoteTest5(StreamReader stream)
		{
			// Note 
			var line = GedcomLine.Read(stream);
			var note = GedcomNote.Read(stream, line, out line);

			Assert.AreNotEqual(null, note);
			Assert.IsTrue(string.IsNullOrEmpty(note.ID));
			Assert.AreEqual("Note line", note.Text);

			Assert.AreEqual(null, line);
		}

        private void ReadGedcomNoteTest6(StreamReader stream)
        {
            // Regular text
            var line = GedcomLine.Read(stream);
            var text = GedcomText.Read(stream, line, out line);
            Assert.IsFalse(string.IsNullOrEmpty(text));
            Assert.AreEqual("Note line 1\r\nNote line 2", text);

            Assert.AreEqual(null, line);
        }
    }
}