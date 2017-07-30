using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NameDateParsing
{
    [Flags]
    public enum NameParseErrorType
    {
        None = 0x00,
        Unknown = 0x01,
        NameEmpty = 0x02,
        TitleFound = 0x04,
        NickNameFound = 0x08,
        FamilyNameMissing = 0x10,
        GivenNameMissing = 0x20
    }

    public class NameParseError
    {

        public readonly NameParseErrorType Reason;
        public readonly IPersonName ParsedName;
        public readonly string Title;
        public readonly string Nickname;
        public readonly string OriginalNameParsed;

        internal NameParseError(NameParseErrorType errorType)
        {
            Reason = errorType;
        }

        internal NameParseError(NameParseErrorType errorType, NameParser parser)
        {
            Reason = errorType;
            ParsedName = new PersonName(parser.Given, parser.Family, parser.Suffix);
            Title = parser.Title;
            Nickname = parser.NickName;
            OriginalNameParsed = parser.OriginalName;
        }

    }

    public class NameParseException : Exception
    {
        private NameParseError _error;

        internal NameParseException(NameParseError error)
            : base ()
        {
            _error = error;
        }

        internal NameParseException(NameParseError error, string msg)
            : base(msg)
        {
            _error = error;
        }

        internal NameParseException(NameParseError error, string msg, Exception innerException)
            : base(msg, innerException)
        {
            _error = error;
        }

        public NameParseError Error
        {
            get
            {
                return _error;
            }
        }
    }

    public class NameParser
    {
        private static Dictionary<string, Regex> s_parsers;
        private static Regex[] s_titles;

        string title;
        string fullName;
        string given;
        string family;
        string nickname;
        string suffix;
        string[] knownSuffixes = new string[] { "Jr.?", "Sr.?", "PhD.?", "M.?D.?", "Esq.?", "CD", "CPA.?", "JD.?", "DC.?", "DDS.?", "DMD.?", "DVM.?", "OD.?", "PE.?", "MBA.?", "RN.?", "TTEE.?" }; // DO NOT TRANSLATE
        string[] knownFamilyNames = new string[] { };// "Van Valkenburgh", "Van Wagenen", "St. Clair", "Von Rosen", "Van Dam", "Van Der Dorf" }; // DO NOT TRANSLATE
        // I found some last name parsing info at http://www.library.yale.edu/cataloging/music/entryele.htm
        string[] knownFamilyNameInitializers = new string[] { "o'", "d'", "van", "von", "ver", "st.", "saint", "der", "de", "la", "del", "las", "te", "ten", "ter", "le", "du", "des", "da", "dos", "da", "le", "detto", "di", "dit", "du", "in't", "lo", "'t", "te", "tan", "uit", "den", "uyt", "vulgo" }; // DO NOT TRANSLATE
        string[] knownLastPartMultipleName = new string[] { "filho", "neto", "netto", "sobrinho" }; // DO NOT TRANSLATE
        Regex knownSuffixesRegex;
        public string FullName { get { return ((string)(given + " /" + family + "/ " + suffix)).Trim(); } } // DO NOT TRANSLATE
        public string OriginalName { get { return fullName; } }
        public string NickName { get { return nickname; } }
        public string Given { get { return given; } }
        public string Family { get { return family; } }
        public string Suffix { get { return suffix; } }
        public string Title { get { return title; } }

        static NameParser()
        {
            s_parsers = new Dictionary<string, Regex>();
            s_parsers.Add("standard", new Regex("(([A-Za-z]+ )+)/([A-Za-z]+)/", RegexOptions.Compiled));
            s_parsers.Add("nickname", new Regex("\"([^\"]+)\"", RegexOptions.Compiled));

            s_titles = new Regex[]
            {
                new Regex("\\A(?i)Hr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)A[\\. ]+V[\\. ]+M(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Admiraal(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Air[\\. ]+Cdre(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Air[\\. ]+Commodore(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Air[\\. ]+Marshal(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Air[\\. ]+Vice[\\. ]+Marshal(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Alderman(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Alhaji(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Amb(assador)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Baron(ess)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Brig(adier)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Brig(adier)?[\\. ]+Gen(eral)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Bro(ther)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Canon(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Cardinal(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Chief(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Cik(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Col[\\. ]+Dr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Commandant(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Commissioner(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Commodore(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Comte(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Comtessa(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Congressman(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Conseiller(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Consul(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Conte(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Contessa(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Corporal(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Councillor(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Count(ess)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Crown[\\. ]+Prince(ss)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dame(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Datin(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dato(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Datuk(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Datuk[\\. ]+Seri(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Deacon(ess)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dean(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dhr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dipl[\\. ]+Ing(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Doctor(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dott[\\. ]+sa(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dott(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dr[\\. ]+Ing(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Dra(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Drs(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Embajador(a)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)En(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Encik(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Eng(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Eur[\\. ]+Ing(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Exma[\\. ]+Sra(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Exmo[\\. ]+Sr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)F[\\. ]+O(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Father(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)First[\\. ]+Lieut(enant)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)First[\\. ]+Officer(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Flt[\\. ]+Lieut(enant)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Flying[\\. ]+Officer(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Fr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Frau(lein)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Fru(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Gov(ernor)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Graaf(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Gravin(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)(Grp|Group)[\\. ]+(Capt|Captain)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)H[\\. ]+E[\\. ]+Dr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)H[\\. ]+H(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)H[\\. ]+M(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)H[\\. ]+R[\\. ]+H(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Hajah(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Hajim?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Her[\\. ]+(Highness|Majesty)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Herr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)High[\\. ]+Chief(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)His[\\. ]+(Highness|Holiness|Majesty)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Hon(orable)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Hr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Hra(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Ing(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Ir(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Jonkheer(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Judge(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Justice(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Khun[\\. ]+Ying(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Kolonel(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Lady(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Lcda(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Lic(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Lieut(enant)?[\\. ]+(Cdr|Commander)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Lieut(enant)?[\\. ]+Col(onel)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Lieut(enant)?[\\. ]+Gen(eral)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Lord(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)M[\\. ]+(L|R)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Madam(e|oiselle)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Maj(or)?[\\. ]+Gen(eral)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Maj(or)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Master(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Mevrouw(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Miss(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Mlle(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Mme(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Monsieur(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Monsignor(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Mrs?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Ms(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Mstr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Nti(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Pastor(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Pres(ident)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Prince(ss|ssa)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Prinses(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Prof(essor)?[\\. ]+Dr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Prof(essor)?[\\. ]+Sir(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Prof(essor)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Puan[\\. ]+(Sri)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Rabbi(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Rear[\\. ]+Admiral(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Rev(erend)?[\\. ]+(Canon|Dr|Doctor|Mother)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Rva(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sen(ator)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)(Sgt|Sergeant)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sheikh(a)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sig[\\. ]+(na|ra)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sig(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sister(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)(Sqn|Squadron)[\\. ]+(Ldr|Leader)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sr[\\. ]+D(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sr(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sra(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Srta(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sultan(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Tan[\\. ]+Sri([\\. ]+Dato)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Tengku(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Teuku(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Than[\\. ]+Puying(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)The[\\. ]+Hon(orable)?[\\. ]+(Dr|Justice|Miss|Mr|Mrs|Ms|Sir)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)The[\\. ]+Very[\\. ]+Rev(erend)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Toh[\\. ]+Puan(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Tun(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Vice[\\. ]+Admiral(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Vice[\\. ]+(Pres|President)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Viscount(ess)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Adm(iral)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Brig(adier)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Lieut(enant)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Generaal(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Gen(eral)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Rev(erend)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)(Cmdr|Commander)(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Capt(ain)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Col(onel)?(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Sir(\\.|\\b)", RegexOptions.Compiled), // DO NOT TRANSLATE
                new Regex("\\A(?i)Wg[\\. ]+Cdr", RegexOptions.Compiled) // DO NOT TRANSLATE
            };

        }

        public NameParser()
        {
            fullName = "";
            given = family = suffix = title = nickname = String.Empty;
        }


        public void ParseName(string _fullName)
        {

            given = family = suffix = title = nickname = String.Empty;

            Match match = s_parsers["standard"].Match(_fullName); // DO NOT TRANSLATE
            if (match.Success && match.Value.Equals(_fullName))
            {
                given = match.Groups[1].Value.Trim();
                family = match.Groups[3].Value;
                fullName = _fullName;
            }
            else
            {
                match = s_parsers["nickname"].Match(_fullName);
                if (match.Success)
                {
                    nickname = match.Groups[1].Value;
                }
                fullName = _fullName;
                FindTitles(_fullName);
                if (!MatchSlashes("(.*)/([^/]*)/(.*)")) // DO NOT TRANSLATE
                    if (!MatchSlashes(@"(.*)\\([^\\]*)\\(.*)")) // DO NOT TRANSLATE
                        if (!MatchSlashes(@"(.*)\s(\([^/]*\))(.*)")) // DO NOT TRANSLATE .. Added to allow for surnames with parenthesis
                        MatchName();
            }
        }

        protected void FindTitles(string _fullName)
        {
            foreach (Regex parser in s_titles)
            {
                Match match = parser.Match(_fullName);
                if (match.Groups.Count > 1)
                {
                    title = match.Groups[0].Value.Trim();
                    break;
                }
            }
        }

        protected bool MatchSlashes(string expression)
        {
            bool retVal = false;
            Match match = Regex.Match(fullName, expression);
            string first = match.Groups[1].Value.Replace(",", "").Replace("/", "").Replace(@"\", "").Trim(); // DO NOT TRANSLATE
            string fam = match.Groups[2].Value.Trim();
            string last = match.Groups[3].Value.Replace(",", "").Trim(); // DO NOT TRANSLATE
            if (((expression.IndexOf("/") > -1 && fullName.IndexOf("//") > -1) || // DO NOT TRANSLATE
                (expression.IndexOf(@"\") > -1 && fullName.IndexOf(@"\\") > -1))) // DO NOT TRANSLATE
            {
                fam = expression.IndexOf("/") > -1 ? "//" : @"\\"; // DO NOT TRANSLATE
            }


            if (expression.IndexOf(@"\(") > -1)
            {
                // When a parenthesis is in an expression

                if (last.Length > 0 && last.IndexOf("(") > -1)
                {
                    // If last has the parenthesis, we set it as the fam
                    given = (first + " " + fam).Trim();
                    fam = last;
                }
                else if (last.Length > 0 && fam.Length > 0 && fam.IndexOf("(") > -1)
                {
                    // If the fam has the parenthesis, check if last is a valid suffix
                    string suff = string.Empty;
                    if (last.IndexOf(" ") > -1)
                    {
                        // If it's a two part last, we can use the method
                        last = ParseSuffixes(last, true);
                        suff = suffix;

                        if (string.IsNullOrEmpty(suff) && string.IsNullOrEmpty(first))
                            return false;
                    }
                    else
                    {
                        // Check if the last itself is a "stand-alone suffix"

                        List<string> suffixes = GetKnownSuffixes();

                        suff = suffixes.Find(
                                delegate(string s)
                                {
                                    return (string.Compare(last, s, true) == 0);
                                }
                        );

                        if (string.IsNullOrEmpty(suff))
                        {
                            for (int i=0; i<RomanNumerals.Length; i++)
                            {
                                if (string.Compare(last, RomanNumerals[i], true) == 0)
                                {
                                    suff = RomanNumerals[i];
                                    break;
                                }
                            }
                        }

                        if (! string.IsNullOrEmpty(suff))
                        {
                            last = fam; // set the last as the name in parenthesis
                            fam = string.Empty;
                        }
                    }

                    if (string.IsNullOrEmpty(suff))
                    {
                        // If last isn't a suffix, we set fam to last and last to empty
                        
                        if (fam.Length > 0 && string.IsNullOrEmpty(nickname))
                            nickname = fam.Substring(1, fam.Length - 2);

                        first = (first + " " + fam).Trim(); // DO NOT TRANSLATE
                        if (last.IndexOf(" ") > -1)
                        {
                            if (!IsFamilyNamePart(last.Substring(0, last.IndexOf(" ")).ToLower().Trim('(')))// || IsFamilyName(family))
                            {
                                first = (first + " " + last.Substring(0, last.IndexOf(" "))).Trim(); ;
                                fam = last.Substring(last.IndexOf(" "), last.Length - last.IndexOf(" ")).Trim(); ;
                            }
                            else
                            {
                                fam = last;
                            }

                        }
                        else
                        {
                            fam = last;
                        }
                        last = string.Empty;
                    }
                    else
                    {
                        // If last was a suffix, we shift the parts around;
                        // We also treat the value between the parenthesis as a nickname

                        if (fam.Length > 0 && string.IsNullOrEmpty(nickname))
                            nickname = fam.Substring(1, fam.Length - 2);
                        
                        first = (first + " " + fam).Trim(); // DO NOT TRANSLATE
                        if (last.IndexOf(" ") > -1)
                        {
                            if (!IsFamilyNamePart(last.Substring(0, last.IndexOf(" ")).ToLower().Trim('(')))// || IsFamilyName(family))
                            {
                                first = (first + " " + last.Substring(0, last.IndexOf(" "))).Trim(); ;
                                fam = last.Substring(last.IndexOf(" "), last.Length - last.IndexOf(" ")).Trim(); ;
                            }
                            else
                            {
                                fam = last;
                            }
                        }
                        else
                        {
                            fam = last;
                        }
                        last = suff;
                    }
                }
            }
            
            if (fam.Length > 0)
            {
                retVal = true;
                family = (fam == "//" || fam == @"\\") ? string.Empty : fam; // DO NOT TRANSLATE

                if (first.Length > 0 && last.Length > 0)
                {
                    given = first;
                    suffix = last;
                }
                else if (first.Length == 0 && last.Length > 0)
                {
                    given = last;
                }
                else
                {
                    given = first;
                }
                given = ParseSuffixes(given, false);
                if (suffix.Length == 0)
                    fam = ParseSuffixes(fam, true);
                
            }
            return retVal;
        }

        protected bool IsInArray(string word, string[] array)
        {
            bool retVal = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (word.Equals(array[i]))
                {
                    retVal = true;
                    break;
                }
            }
            return retVal;
        }

        protected List<string> GetKnownSuffixes()
        {
            List<string> suffixes = new List<string>();

            foreach (string knownSuffix in knownSuffixes)
            {
                string pattern = "\\s+(" + knownSuffix.Replace(".", @"\.") + ")(\\s+|\\W+|$)";
                Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

                if (r.IsMatch(fullName))
                {
                    Match m = r.Match(fullName);
                    suffixes.Add(m.Groups[1].ToString());
                    //fullName = r.Replace(fullName, " ");
                }
            }

            //fullName = fullName.TrimEnd(new char[] { ',', ' ' });

            return suffixes;
        }

        //protected List<string> GetKnownSuffixes()
        //{
        //    MatchCollection matches = KnownSuffixesRegex.Matches(fullName + " "); // DO NOT TRANSLATE
        //    List<string> retVal = new List<string>();
        //    foreach (Match match in matches)
        //    {
        //        if (match.Groups[1].Value.Length > 0)
        //            retVal.Add(match.Groups[1].Value);
        //    }
        //    return retVal;
        //}

        protected Regex KnownSuffixesRegex
        {
            get
            {
                if (knownSuffixesRegex == null)
                {
                    StringBuilder matchStr = new StringBuilder(@"\b(?i)("); // starts with a word boundary  // DO NOT TRANSLATE
                    bool first = true;
                    foreach (string suffix in knownSuffixes)
                    {
                        if (!first)
                            matchStr.Append("|"); // DO NOT TRANSLATE
                        first = false;
                        matchStr.Append("("); // DO NOT TRANSLATE
                        matchStr.Append(suffix.Replace(".", @"\.")); // DO NOT TRANSLATE
                        matchStr.Append(@")"); // ends with a word boundary  // DO NOT TRANSLATE
                    }
                    matchStr.Append(@")[^A-Za-z]"); // DO NOT TRANSLATE
                    knownSuffixesRegex = new Regex(matchStr.ToString());
                }
                return knownSuffixesRegex;
            }
        }

        protected bool IsFamilyNamePart(string namePart)
        {
            return IsInArray(namePart, knownFamilyNameInitializers);
        }

        protected bool IsSingleWordName()
        {
            return fullName.IndexOf(" ") == -1 && fullName.IndexOf(",") == -1; // DO NOT TRANSLATE
        }

        protected void SetSuffix(List<string> suffixes)
        {
            foreach (string suff in suffixes)
            {
                if (suffix.IndexOf(suff) == -1)
                {
                    if (suffix.Length == 0)
                        suffix = suff;
                    else
                        suffix = suffix + " " + suff; // DO NOT TRANSLATE
                }

            }
        }

        protected string concatenateLastStrings(string[] words, int count)
        {
            StringBuilder retVal = new StringBuilder();
            for (int i = words.Length - count; i < words.Length; i++)
            {
                retVal.Append(" "); // DO NOT TRANSLATE
                retVal.Append(words[i]);
            }
            return retVal.ToString().Trim();
        }

        protected string ParseSuffixes(string name, bool stripPunctuation)
        {
            List<string> suffixes = GetKnownSuffixes();
            
            string retVal = name;
            if (suffixes.Count > 0)
            {
                string removeRegex = KnownSuffixesRegex.ToString().Substring(0, KnownSuffixesRegex.ToString().IndexOf("[")) + @"(\.|\b)"; // DO NOT TRANSLATE
                retVal = Regex.Replace(name + " ", removeRegex, "").Trim(); // DO NOT TRANSLATE
            }
            retVal = ParseRomanNumeralSuffixes(stripPunctuation ? retVal.Trim().Trim('.') : retVal.Trim(), suffixes);
            SetSuffix(suffixes);
            return retVal;
        }

        private static string[] RomanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV" }; // DO NOT TRANSLATE
        protected string ParseRomanNumeralSuffixes(string name, List<string> suffixes)
        {
            if (name.Length > 2 && name[name.Length - 1].Equals(',')) // DO NOT TRANSLATE
                name = name.Substring(0, name.Length - 1).Trim();
            if (name.Length > 1)
            {
                if (name.Substring(name.Length - 1).Equals(",")) // DO NOT TRANSLATE
                    name = name.Substring(0, name.Length - 1);
                foreach (string romanNumeral in RomanNumerals)
                {
                    if (name.Length > romanNumeral.Length + 1)
                    {
                        string end = name.Substring(name.Length - (romanNumeral.Length + 1)).ToUpper();
                        if (end.Equals(" " + romanNumeral) || end.Equals("," + romanNumeral)) // DO NOT TRANSLATE
                        {
                            suffixes.Insert(0, romanNumeral);
                            name = name.Substring(0, name.Length - (romanNumeral.Length + 1)).Trim();
                            if (name[name.Length - 1].Equals(',')) // DO NOT TRANSLATE
                                name = name.Substring(0, name.Length - 1).Trim();
                            break;
                        }
                    }
                }
            }
            return name;
        }

        protected void MatchName()
        {
            if (IsSingleWordName())
            {
                family = fullName;
            }
            else
            {
                string full = ParseSuffixes(fullName, true);
                if (full[full.Length - 1] == ',') // DO NOT TRANSLATE
                {
                    full = full.Substring(0, full.Length - 1);
                }
                if (full.IndexOf(",") > -1) // DO NOT TRANSLATE
                {
                    full = full.Substring(full.IndexOf(",") + 1).Trim() + " " + full.Substring(0, full.IndexOf(",")).Trim(); // DO NOT TRANSLATE
                }
                string[] nameParts = full.Replace(",", " ").Trim().Split(' '); // DO NOT TRANSLATE

                int numFamilyNames = NumberOfFamilyNames(nameParts);
                family = concatenateLastStrings(nameParts, numFamilyNames);

                if (numFamilyNames == 1)
                {
                    family = nameParts[nameParts.Length - 1];
                }
                for (int n = 0; n < nameParts.Length-numFamilyNames; n++)
                {
                    given = given + " " + nameParts[n]; // DO NOT TRANSLATE
                }
                suffix = suffix.Trim();
                family = family.Trim();
                given = given.Trim();
            }
        }

        int NumberOfFamilyNames(string[] nameParts)
        {
            int i = 0;
            for (i = 2; i <= nameParts.Length; i++)
            {
                //family = concatenateLastStrings(nameParts, i);
                if (!IsFamilyNamePart(nameParts[nameParts.Length-i].ToLower()))// || IsFamilyName(family))
                {
                    i -= 1;
                    break;
                }
            }
            if (i == 1 && nameParts.Length > 1)
            {
                if (IsInArray(nameParts[nameParts.Length - 1].ToLower(), knownLastPartMultipleName))
                    i = 2;

            }
            return i > nameParts.Length ? nameParts.Length : i;
        }
    }
}
