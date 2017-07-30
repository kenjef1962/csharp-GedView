
namespace NameDateParsing
{
    public static class SerialDayNumber
    {
        static readonly int GREG_SDN_OFFSET = 32045;
        static readonly int GREG_DAYS_PER_5_MONTHS = 153;
        static readonly int GREG_DAYS_PER_4_YEARS = 1461;
        static readonly int GREG_DAYS_PER_400_YEARS = 146097;

        public static readonly string[] WeekDays = 
		{
			Properties.Resources.WeekDaysSunday, 
			Properties.Resources.WeekDaysMonday, 
			Properties.Resources.WeekDaysTuesday, 
			Properties.Resources.WeekDaysWednesday,
            Properties.Resources.WeekDaysThursday, 
			Properties.Resources.WeekDaysFriday, 
			Properties.Resources.WeekDaysSaturday}; 

        public static int DayOfWeek(int sdn)
        {
            return (sdn+1) % 7;
        }

        public static void SdnToGregorian(int sdn, out int year, out int month, out int day)
        {
            int       century;
            int       temp;
            int       dayOfYear;

            year = 0;
            month = 0;
            day = 0;

            if (sdn <= 0)
            {
                return;
            }

            temp = (sdn + GREG_SDN_OFFSET) * 4 - 1;

            /* Calculate the century (year/100). */
            century = temp / GREG_DAYS_PER_400_YEARS;

            /* Calculate the year and day of year (1 <= dayOfYear <= 366). */
            temp = ((temp % GREG_DAYS_PER_400_YEARS) / 4) * 4 + 3;
            year = (century * 100) + (temp / GREG_DAYS_PER_4_YEARS);
            dayOfYear = (temp % GREG_DAYS_PER_4_YEARS) / 4 + 1;

            /* Calculate the month and day of month. */
            temp = dayOfYear * 5 - 3;
            month = temp / GREG_DAYS_PER_5_MONTHS;
            day = (temp % GREG_DAYS_PER_5_MONTHS) / 5 + 1;

            /* Convert to the normal beginning of the year. */
            if (month < 10) {
                month += 3;
            } else {
                year += 1;
                month -= 9;
            }

            /* Adjust to the B.C./A.D. type numbering. */
            year -= 4800;
            if (year <= 0) year--;
        }

        public static int
        GregorianToSdn(
            int inputYear,
            int inputMonth,
            int inputDay)
        {
            int year;
            int month;

            /* check for invalid dates */
            if (inputYear == 0 || inputYear < -4714 ||
                inputMonth <= 0 || inputMonth > 12 ||
                inputDay <= 0 || inputDay > 31)
            {
                return(0);
            }

            /* check for dates before SDN 1 (Nov 25, 4714 B.C.) */
            if (inputYear == -4714) {
                if (inputMonth < 11) {
                    return(0);
                }
                if (inputMonth == 11 && inputDay < 25) {
                    return(0);
                }
            }

            /* Make year always a positive number. */
            if (inputYear < 0) {
                year = inputYear + 4801;
            } else {
                year = inputYear + 4800;
            }

            /* Adjust the start of the year. */
            if (inputMonth > 2) {
                month = inputMonth - 3;
            } else {
                month = inputMonth + 9;
                year--;
            }

            return( ((year / 100) * GREG_DAYS_PER_400_YEARS) / 4
                    + ((year % 100) * GREG_DAYS_PER_4_YEARS) / 4
                    + (month * GREG_DAYS_PER_5_MONTHS + 2) / 5
                    + inputDay
                    - GREG_SDN_OFFSET );
        }

        static readonly int JUL_SDN_OFFSET        = 32083;
        static readonly int JUL_DAYS_PER_5_MONTHS = 153;
        static readonly int JUL_DAYS_PER_4_YEARS = 1461;

        public static void
        SdnToJulian(
            int  sdn,
            out int      year,
            out int      month,
            out int      day)
        {
            int       temp;
            int       dayOfYear;

            year = 0;
            month = 0;
            day = 0;
            if (sdn <= 0) {
                return;
            }

            temp = (sdn + JUL_SDN_OFFSET) * 4 - 1;

            /* Calculate the year and day of year (1 <= dayOfYear <= 366). */
            year = temp / JUL_DAYS_PER_4_YEARS;
            dayOfYear = (temp % JUL_DAYS_PER_4_YEARS) / 4 + 1;

            /* Calculate the month and day of month. */
            temp = dayOfYear * 5 - 3;
            month = temp / JUL_DAYS_PER_5_MONTHS;
            day = (temp % JUL_DAYS_PER_5_MONTHS) / 5 + 1;

            /* Convert to the normal beginning of the year. */
            if (month < 10) {
                month += 3;
            } else {
                year += 1;
                month -= 9;
            }

            /* Adjust to the B.C./A.D. type numbering. */
            year -= 4800;
            if (year <= 0) year--;

        }

        public static int 
        JulianToSdn(
            int inputYear,
            int inputMonth,
            int inputDay)
        {
            int year;
            int month;

            /* check for invalid dates */
            if (inputYear == 0 || inputYear < -4713 ||
                inputMonth <= 0 || inputMonth > 12 ||
                inputDay <= 0 || inputDay > 31)
            {
                return(0);
            }

            /* check for dates before SDN 1 (Jan 2, 4713 B.C.) */
            if (inputYear == -4713) {
                if (inputMonth == 1 && inputDay == 1) {
                    return(0);
                }
            }

            /* Make year always a positive number. */
            if (inputYear < 0) {
                year = inputYear + 4801;
            } else {
                year = inputYear + 4800;
            }

            /* Adjust the start of the year. */
            if (inputMonth > 2) {
                month = inputMonth - 3;
            } else {
                month = inputMonth + 9;
                year--;
            }

            return( (year * JUL_DAYS_PER_4_YEARS) / 4
                    + (month * JUL_DAYS_PER_5_MONTHS + 2) / 5
                    + inputDay
                    - JUL_SDN_OFFSET );
        }

        static readonly int  HALAKIM_PER_HOUR = 1080;
        static readonly int  HALAKIM_PER_DAY = 25920;
        static readonly int  HALAKIM_PER_LUNAR_CYCLE = ((29 * HALAKIM_PER_DAY) + 13753);
        static readonly int  HALAKIM_PER_METONIC_CYCLE = (HALAKIM_PER_LUNAR_CYCLE * (12 * 19 + 7));

        static readonly int  SDN_OFFSET = 347997;
        static readonly int  NEW_MOON_OF_CREATION = 31524;

        static readonly int  SUNDAY =    0;
        static readonly int  MONDAY =    1;
        static readonly int  TUESDAY =   2;
        static readonly int  WEDNESDAY = 3;
        //static readonly int  THURSDAY =  4;
        static readonly int  FRIDAY =    5;
        //static readonly int  SATURDAY =  6;

        static readonly int  NOON = (18 * HALAKIM_PER_HOUR);
        static readonly int  AM3_11_20 = ((9 * HALAKIM_PER_HOUR) + 204);
        static readonly int  AM9_32_43 = ((15 * HALAKIM_PER_HOUR) + 589);

        static int []monthsPerYear = {
            12, 12, 13, 12, 12, 13, 12, 13, 12, 12, 13, 12, 12, 13, 12, 12, 13, 12, 13
        };

        static int []yearOffset = {
            0, 12, 24, 37, 49, 61, 74, 86, 99, 111, 123,
            136, 148, 160, 173, 185, 197, 210, 222
        };

        static string []JewishMonthName = {
            "",
            "Tishri", // DO NOT TRANSLATE
            "Heshvan", // DO NOT TRANSLATE
            "Kislev", // DO NOT TRANSLATE
            "Tevet", // DO NOT TRANSLATE
            "Shevat", // DO NOT TRANSLATE
            "AdarI", // DO NOT TRANSLATE
            "AdarII", // DO NOT TRANSLATE
            "Nisan", // DO NOT TRANSLATE
            "Iyyar", // DO NOT TRANSLATE
            "Sivan", // DO NOT TRANSLATE
            "Tammuz", // DO NOT TRANSLATE
            "Av", // DO NOT TRANSLATE
            "Elul" // DO NOT TRANSLATE
        };

        /************************************************************************
         * Given the year within the 19 year metonic cycle and the time of a molad
         * (new moon) which starts that year, this routine will calculate what day
         * will be the actual start of the year (Tishri 1 or Rosh Ha-Shanah).  This
         * first day of the year will be the day of the molad unless one of 4 rules
         * (called dehiyyot) delays it.  These 4 rules can delay the start of the
         * year by as much as 2 days.
         */
        static int
        Tishri1(
            int      metonicYear,
            int moladDay,
            int moladHalakim)
        {
            int tishri1;
            int dow;
            bool leapYear;
            bool lastWasLeapYear;

            tishri1 = moladDay;
            dow = tishri1 % 7;
            leapYear = metonicYear == 2 || metonicYear == 5 || metonicYear == 7
	        || metonicYear == 10 || metonicYear == 13 || metonicYear == 16
	        || metonicYear == 18;
            lastWasLeapYear = metonicYear == 3 || metonicYear == 6
	        || metonicYear == 8 || metonicYear == 11 || metonicYear == 14
	        || metonicYear == 17 || metonicYear == 0;

            /* Apply rules 2, 3 and 4. */
            if ((moladHalakim >= NOON) ||
	        ((!leapYear) && dow == TUESDAY && moladHalakim >= AM3_11_20) ||
	        (lastWasLeapYear && dow == MONDAY && moladHalakim >= AM9_32_43))
            {
	        tishri1++;
	        dow++;
	        if (dow == 7) {
	            dow = 0;
	        }
            }

            /* Apply rule 1 after the others because it can cause an additional
             * delay of one day. */
            if (dow == WEDNESDAY || dow == FRIDAY || dow == SUNDAY) {
	        tishri1++;
            }

            return(tishri1);
        }

        /************************************************************************
         * Given a metonic cycle number, calculate the date and time of the molad
         * (new moon) that starts that cycle.  Since the length of a metonic cycle
         * is a constant, this is a simple calculation, except that it requires an
         * intermediate value which is bigger that 32 bits.  Because this
         * intermediate value only needs 36 to 37 bits and the other numbers are
         * constants, the process has been reduced to just a few steps.
         */
        static void
        MoladOfMetonicCycle(
            int       metonicCycle,
            out int moladDay,
            out int moladHalakim)
        {
            int r1, r2, d1, d2;

            /* Start with the time of the first molad after creation. */
            r1 = NEW_MOON_OF_CREATION;

            /* Calculate metonicCycle * HALAKIM_PER_METONIC_CYCLE.  The upper 32
             * bits of the result will be in r2 and the lower 16 bits will be
             * in r1. */
            r1 += metonicCycle * (HALAKIM_PER_METONIC_CYCLE & 0xFFFF);
            r2 = r1 >> 16;
            r2 += metonicCycle * ((HALAKIM_PER_METONIC_CYCLE >> 16) & 0xFFFF);

            /* Calculate r2r1 / HALAKIM_PER_DAY.  The remainder will be in r1, the
             * upper 16 bits of the quotient will be in d2 and the lower 16 bits
             * will be in d1. */
            d2 = r2 / HALAKIM_PER_DAY;
            r2 -= d2 * HALAKIM_PER_DAY;
            r1 = (r2 << 16) | (r1 & 0xFFFF);
            d1 = r1 / HALAKIM_PER_DAY;
            r1 -= d1 * HALAKIM_PER_DAY;

            moladDay = (d2 << 16) | d1;
            moladHalakim = r1;
        }

        /************************************************************************
         * Given a day number, find the molad of Tishri (the new moon at the start
         * of a year) which is closest to that day number.  It's not really the
         * *closest* molad that we want here.  If the input day is in the first two
         * months, we want the molad at the start of the year.  If the input day is
         * in the fourth to last months, we want the molad at the end of the year.
         * If the input day is in the third month, it doesn't matter which molad is
         * returned, because both will be required.  This type of "rounding" allows // DO NOT TRANSLATE
         * us to avoid calculating the length of the year in most cases.
         */
        static void
        FindTishriMolad(
            int inputDay,
            out int metonicCycle,
            out int metonicYear,
            out int moladDay,
            out int moladHalakim)
        {

            /* Estimate the metonic cycle number.  Note that this may be an under
             * estimate because there are 6939.6896 days in a metonic cycle not
             * 6940, but it will never be an over estimate.  The loop below will
             * correct for any error in this estimate. */
            metonicCycle = (inputDay + 310) / 6940;

            /* Calculate the time of the starting molad for this metonic cycle. */
            MoladOfMetonicCycle(metonicCycle, out moladDay, out moladHalakim);

            /* If the above was an under estimate, increment the cycle number until
             * the correct one is found.  For modern dates this loop is about 98.6%
             * likely to not execute, even once, because the above estimate is
             * really quite close. */
            while (moladDay < inputDay - 6940 + 310) {
	        metonicCycle++;
	        moladHalakim += HALAKIM_PER_METONIC_CYCLE;
	        moladDay += moladHalakim / HALAKIM_PER_DAY;
	        moladHalakim = moladHalakim % HALAKIM_PER_DAY;
            }

            /* Find the molad of Tishri closest to this date. */
            for (metonicYear = 0; metonicYear < 18; metonicYear++) {
	        if (moladDay > inputDay - 74) {
	            break;
	        }
	        moladHalakim += HALAKIM_PER_LUNAR_CYCLE * monthsPerYear[metonicYear];
	        moladDay += moladHalakim / HALAKIM_PER_DAY;
	        moladHalakim = moladHalakim % HALAKIM_PER_DAY;
            }

        }

        /************************************************************************
         * Given a year, find the number of the first day of that year and the date
         * and time of the starting molad.
         */
        static void
        FindStartOfYear(
            int       year,
            out int      pMetonicCycle,
            out int      pMetonicYear,
            out int pMoladDay,
            out int pMoladHalakim,
            out int      pTishri1)
        {
            pMetonicCycle = (year - 1) / 19;
            pMetonicYear = (year - 1) % 19;
            MoladOfMetonicCycle(pMetonicCycle, out pMoladDay, out pMoladHalakim);

            pMoladHalakim += HALAKIM_PER_LUNAR_CYCLE * yearOffset[pMetonicYear];
            pMoladDay += pMoladHalakim / HALAKIM_PER_DAY;
            pMoladHalakim = pMoladHalakim % HALAKIM_PER_DAY;

            pTishri1 = Tishri1(pMetonicYear, pMoladDay, pMoladHalakim);
        }

        /************************************************************************
         * Given a serial day number (SDN), find the corresponding year, month and
         * day in the Jewish calendar.  The three output values will always be
         * modified.  If the input SDN is before the first day of year 1, they will
         * all be set to zero, otherwise *pYear will be > 0; *pMonth will be in the
         * range 1 to 13 inclusive; *pDay will be in the range 1 to 30 inclusive.
         */
        public static void
        SdnToJewish(
            int sdn,
            out int pYear,
            out int pMonth,
            out int pDay)
        {
            int inputDay;
            int day;
            int halakim;
            int metonicCycle;
            int metonicYear;
            int tishri1;
            int tishri1After;
            int yearLength;

            pYear = 0;
            pMonth = 0;
            pDay = 0;
            if (sdn <= SDN_OFFSET)
    	        return;
    
            inputDay = sdn - SDN_OFFSET;

            FindTishriMolad(inputDay, out metonicCycle, out metonicYear, out day, out halakim);
            tishri1 = Tishri1(metonicYear, day, halakim);

            if (inputDay >= tishri1) {
	        /* It found Tishri 1 at the start of the year. */
	        pYear = metonicCycle * 19 + metonicYear + 1;
	        if (inputDay < tishri1 + 59) {
	            if (inputDay < tishri1 + 30) {
		        pMonth = 1;
		        pDay = inputDay - tishri1 + 1;
	            } else {
		        pMonth = 2;
		        pDay = inputDay - tishri1 - 29;
	            }
	            return;
	        }

	        /* We need the length of the year to figure this out, so find
	         * Tishri 1 of the next year. */
	        halakim += HALAKIM_PER_LUNAR_CYCLE * monthsPerYear[metonicYear];
	        day += halakim / HALAKIM_PER_DAY;
	        halakim = halakim % HALAKIM_PER_DAY;
	        tishri1After = Tishri1((metonicYear + 1) % 19, day, halakim);
            } else {
	        /* It found Tishri 1 at the end of the year. */
	        pYear = metonicCycle * 19 + metonicYear;
	        if (inputDay >= tishri1 - 177) {
	            /* It is one of the last 6 months of the year. */
	            if (inputDay > tishri1 - 30) {
		        pMonth = 13;
		        pDay = inputDay - tishri1 + 30;
	            } else if (inputDay > tishri1 - 60) {
		        pMonth = 12;
		        pDay = inputDay - tishri1 + 60;
	            } else if (inputDay > tishri1 - 89) {
		        pMonth = 11;
		        pDay = inputDay - tishri1 + 89;
	            } else if (inputDay > tishri1 - 119) {
		        pMonth = 10;
		        pDay = inputDay - tishri1 + 119;
	            } else if (inputDay > tishri1 - 148) {
		        pMonth = 9;
		        pDay = inputDay - tishri1 + 148;
	            } else {
		        pMonth = 8;
		        pDay = inputDay - tishri1 + 178;
	            }
	            return;
	        } else {
	            if (monthsPerYear[(pYear - 1) % 19] == 13) {
		        pMonth = 7;
		        pDay = inputDay - tishri1 + 207;
		        if (pDay > 0) return;
		        (pMonth)--;
		        (pDay) += 30;
		        if (pDay > 0) return;
		        (pMonth)--;
		        (pDay) += 30;
	            } else {
		        pMonth = 6;
		        pDay = inputDay - tishri1 + 207;
		        if (pDay > 0) return;
		        (pMonth)--;
		        (pDay) += 30;
	            }
	            if (pDay > 0) return;
	            (pMonth)--;
	            (pDay) += 29;
	            if (pDay > 0) return;

	            /* We need the length of the year to figure this out, so find
	             * Tishri 1 of this year. */
	            tishri1After = tishri1;
	            FindTishriMolad(day - 365,
		        out metonicCycle, out metonicYear, out day, out halakim);
	            tishri1 = Tishri1(metonicYear, day, halakim);
	        }
            }

            yearLength = tishri1After - tishri1;
            day = inputDay - tishri1 - 29;
            if (yearLength == 355 || yearLength == 385) {
	        /* Heshvan has 30 days */
	        if (day <= 30) {
	            pMonth = 2;
	            pDay = day;
	            return;
	        }
	        day -= 30;
            } else {
	        /* Heshvan has 29 days */
	        if (day <= 29) {
	            pMonth = 2;
	            pDay = day;
	            return;
	        }
	        day -= 29;
            }

            /* It has to be Kislev. */
            pMonth = 3;
            pDay = day;
        }

        /************************************************************************
         * Given a year, month and day in the Jewish calendar, find the
         * corresponding serial day number (SDN).  Zero is returned when the input
         * date is detected as invalid.  The return value will be > 0 for all valid
         * dates, but there are some invalid dates that will return a positive
         * value.  To verify that a date is valid, convert it to SDN and then back
         * and compare with the original.
         */
        public static int
        JewishToSdn(
            int year,
            int month,
            int day)
        {
            int sdn;
            int      metonicCycle;
            int      metonicYear;
            int      tishri1;
            int      tishri1After;
            int moladDay;
            int moladHalakim;
            int      yearLength;
            int      lengthOfAdarIAndII;

            if (year <= 0 || day <= 0 || day > 30) {
	        return(0);
            }

            switch (month) {
            case 1:
            case 2:
	        /* It is Tishri or Heshvan - don't need the year length. */
	        FindStartOfYear(year, out metonicCycle, out metonicYear,
	            out moladDay, out moladHalakim, out tishri1);
	        if (month == 1) {
	            sdn = tishri1 + day - 1;
	        } else {
	            sdn = tishri1 + day + 29;
	        }
	        break;

            case 3:
	        /* It is Kislev - must find the year length. */

	        /* Find the start of the year. */
	        FindStartOfYear(year, out metonicCycle, out metonicYear,
	            out moladDay, out moladHalakim, out tishri1);

	        /* Find the end of the year. */
	        moladHalakim += HALAKIM_PER_LUNAR_CYCLE * monthsPerYear[metonicYear];
	        moladDay += moladHalakim / HALAKIM_PER_DAY;
	        moladHalakim = moladHalakim % HALAKIM_PER_DAY;
	        tishri1After = Tishri1((metonicYear + 1) % 19, moladDay, moladHalakim);

	        yearLength = tishri1After - tishri1;

	        if (yearLength == 355 || yearLength == 385) {
	            sdn = tishri1 + day + 59;
	        } else {
	            sdn = tishri1 + day + 58;
	        }
	        break;

            case 4:
            case 5:
            case 6:
	        /* It is Tevet, Shevat or Adar I - don't need the year length. */

	        FindStartOfYear(year + 1, out metonicCycle, out metonicYear,
	            out moladDay, out moladHalakim, out tishri1After);

	        if (monthsPerYear[(year - 1) % 19] == 12) {
                lengthOfAdarIAndII = 29;
            } else {
                lengthOfAdarIAndII = 59;
	        }

	        if (month == 4) {
	            sdn = tishri1After + day - lengthOfAdarIAndII - 237;
	        } else if (month == 5) {
	            sdn = tishri1After + day - lengthOfAdarIAndII - 208;
	        } else {
	            sdn = tishri1After + day - lengthOfAdarIAndII - 178;
	        }
	        break;

            default:
	        /* It is Adar II or later - don't need the year length. */
	        FindStartOfYear(year + 1, out metonicCycle, out metonicYear,
	            out moladDay, out moladHalakim, out tishri1After);

	        switch (month) {
	            case  7:
	                sdn = tishri1After + day - 207;
	                break;
	            case  8:
	                sdn = tishri1After + day - 178;
	                break;
	            case  9:
	                sdn = tishri1After + day - 148;
	                break;
	            case 10:
	                sdn = tishri1After + day - 119;
	                break;
	            case 11:
	                sdn = tishri1After + day - 89;
	                break;
	            case 12:
	                sdn = tishri1After + day - 60;
	                break;
	            case 13:
	                sdn = tishri1After + day - 30;
	                break;
	            default:
	                return(0);
	            }
                break;
            }
            return(sdn + SDN_OFFSET);
        }

    }
}
