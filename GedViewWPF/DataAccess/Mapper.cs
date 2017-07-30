using GedcomReader;
using GedViewWPF.Model;
using GedViewWPF.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace GedViewWPF.DataAccess
{
    public class Mapper
    {
        public static Person GedcomPersonToPerson(Gedcom gedcom, GedcomIndividual gedcomPerson)
        {
            var person = new Person();
            
            if (gedcomPerson != null)
            {
                var name = (gedcomPerson.Names == null) ? null : gedcomPerson.Names[0];

                // Person Basic Info
                person.ID = gedcomPerson.ID;
                person.Fullname = (name != null) ? name.Fullname : string.Empty;
                person.Surname = (name != null) ? name.Surname : string.Empty;
                person.Given = (name != null) ? name.Given : string.Empty;
                person.Sex = gedcomPerson.Sex;

                var gedcomFamilyID = ((gedcomPerson.FamilySpouseIDs != null) && (0 < gedcomPerson.FamilySpouseIDs.Count)) ? gedcomPerson.FamilySpouseIDs[0] : null;

                // Person Basic Facts
                foreach (var gedcomFact in gedcom.Facts.Values)
                {
                    if ((gedcomFact.PrimaryID == person.ID) || (gedcomFact.SecondaryID == person.ID))
                    {
                        if ((gedcomFact.Type == "BIRT") && (person.Birth == null))
                        {
                            person.Birth = Mapper.GedcomFactToFact(gedcom, gedcomFact);
                        }
                        else if ((gedcomFact.Type == "MARR") && (person.Marriage == null))
                        {
                            person.Marriage = Mapper.GedcomFactToFact(gedcom, gedcomFact);
                        }
                        else if ((gedcomFact.Type == "DEAT") && (person.Death == null))
                        {
                            person.Death = Mapper.GedcomFactToFact(gedcom, gedcomFact);
                        }
                    }
                }

                // Person Lifespan
                var lifespan = string.Empty;

                if ((person.Birth != null) && (person.Birth.Date != null))
                    lifespan += (person.Birth.Date.Year != null) ? person.Birth.Date.Year.ToString() : "????";
                else
                    lifespan += "????";

                lifespan += " - ";

                if ((person.Death != null) && (person.Death.Date != null))
                    lifespan += (person.Death.Date.Year != null) ? person.Death.Date.Year.ToString() : "????";
                else
                    lifespan += "????";

                person.Lifespan = lifespan;
            }

            person.CitationCount = (gedcomPerson.Citations != null) ? gedcomPerson.Citations.Count : 0;
            person.MediaCount = (gedcomPerson.Media != null) ? gedcomPerson.Media.Count : 0;
            person.NoteCount = (gedcomPerson.Notes != null) ? gedcomPerson.Notes.Count : 0;

            return person;
        }

        public static Family GedcomFamilyToFamily(Gedcom gedcom, GedcomFamily gedcomFamily)
        {
            var family = new Family();

            if (gedcomFamily != null)
            {
                family.ID = gedcomFamily.ID;

                if (!string.IsNullOrEmpty(gedcomFamily.HusbandID))
                {
                    var gedcomHusband = gedcom.Individuals[gedcomFamily.HusbandID];
                    family.Husband = Mapper.GedcomPersonToPerson(gedcom, gedcomHusband);
                }

                if (!string.IsNullOrEmpty(gedcomFamily.WifeID))
                {
                    var gedcomWife = gedcom.Individuals[gedcomFamily.WifeID];
                    family.Wife = Mapper.GedcomPersonToPerson(gedcom, gedcomWife);
                }

                foreach (var gedcomFact in gedcomFamily.Facts)
                {
                    if (gedcomFact.Type == "MARR")
                    {
                        family.Marriage = Mapper.GedcomFactToFact(gedcom, gedcomFact);
                        break;
                    }
                }

                family.Children = new List<Person>();

                foreach (var childID in gedcomFamily.ChildIDs)
                {
                    var gedcomChild = gedcom.Individuals[childID];
                    var child = Mapper.GedcomPersonToPerson(gedcom, gedcomChild);
                    family.Children.Add(child);
                }

                Utils.SortChildren(family);
            }

            return family;
        }

        public static Fact GedcomFactToFact(Gedcom gedcom, GedcomFact gedcomFact)
        {
            var fact = new Fact();

            if (gedcomFact != null)
            {
                fact.ID = gedcomFact.ID;
                fact.Type = gedcomFact.Type;
                fact.Date = Mapper.GedcomDateToDate(gedcom, gedcomFact.Date);
                fact.Place = (gedcomFact.Place != null) ? gedcomFact.Place : string.Empty;

                fact.CitationCount = (gedcomFact.Citations != null) ? gedcomFact.Citations.Count : 0;
                fact.MediaCount = (gedcomFact.Media != null) ? gedcomFact.Media.Count : 0;
                fact.NoteCount = (gedcomFact.Notes != null) ? gedcomFact.Notes.Count : 0;
            }

            return fact;
        }

        public static Date GedcomDateToDate(Gedcom gedcom, string gedcomDate)
        {
            var date = new Date();

            if (gedcomDate != null)
            {
                date.DateStr = gedcomDate;
                date.DateDT = null;

                DateTime dt;
                if (DateTime.TryParse(gedcomDate, out dt))
                    date.DateDT = dt;
            }

            return date;
        }

        public static Citation GedcomCitationToCitation(Gedcom gedcom, GedcomCitation gedcomCitation)
        {
            var citation = new Citation();

            if (gedcomCitation != null)
            {
                citation.ID = gedcomCitation.ID;
                citation.SourceID = gedcomCitation.SourceID;
                citation.Page = gedcomCitation.Page;
                citation.Quality = gedcomCitation.Quality;

                citation.MediaCount = (gedcomCitation.Media != null) ? gedcomCitation.Media.Count : 0;
                citation.NoteCount = (gedcomCitation.Notes != null) ? gedcomCitation.Notes.Count : 0;

                var gedcomSource = gedcom.Sources[gedcomCitation.SourceID];
                if (gedcomSource != null)
                {
                    citation.SourceTitle = gedcomSource.Title;
                }

                if (0 < citation.MediaCount)
                {
                    try
                    {
                        var mediaID = gedcomCitation.Media[0].ID;
                        if (!string.IsNullOrEmpty(mediaID))
                        {
                            var gedcomMedia = gedcom.Media[mediaID];
                            if ((gedcomMedia != null) && File.Exists(gedcomMedia.Filename))
                            {
                                citation.MediaFilename = gedcomMedia.Filename;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Just eat any exception that happens
                    }
                }
            }

            return citation;
        }

        public static Source GedcomSourceToSource(Gedcom gedcom, GedcomSource gedcomSource)
        {
            var source = new Source();

            if (gedcomSource != null)
            {
                source.ID = gedcomSource.ID;
                source.Title = gedcomSource.Title;
            }

            return source;
        }

        public static Media GedcomMediaToMedia(Gedcom gedcom, GedcomMedia gedcomMedia)
        {
            var media = new Media();

            if (gedcomMedia != null)
            {
                media.ID = gedcomMedia.ID;
                media.Title = gedcomMedia.Title;
                media.Filename = gedcomMedia.Filename;
                media.Format = gedcomMedia.Format;
            }

            return media;
        }
    }
}
