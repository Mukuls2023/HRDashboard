using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gNxt_HR_Dashboard.Helper
{
    public class NumberToWordsConverter
    {

        private static readonly string[] Units = {
        "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
        "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
    };

        private static readonly string[] Tens = {
        "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
    };

        private static readonly string[] Thousands = {
        "", "Thousand", "Million"
    };

        public static string Convert(int number)
        {
            if (number == 0)
            {
                return Units[0];
            }

            if (number < 0)
            {
                return "Minus " + Convert(-number);
            }

            int thousandCounter = 0;
            string words = "";

            while (number > 0)
            {
                int chunk = number % 1000;

                if (chunk > 0)
                {
                    string chunkWords = ConvertChunk(chunk);
                    words = chunkWords + (thousandCounter > 0 ? " " + Thousands[thousandCounter] : "") + " " + words;
                }

                number /= 1000;
                thousandCounter++;
            }

            return words.Trim();
        }

        private static string ConvertChunk(int number)
        {
            string words = "";

            if (number >= 100)
            {
                words += Units[number / 100] + " Hundred";
                number %= 100;
            }

            if (number > 0)
            {
                if (!string.IsNullOrEmpty(words))
                {
                    words += " ";
                }

                if (number < 20)
                {
                    words += Units[number];
                }
                else
                {
                    words += Tens[number / 10];

                    if ((number % 10) > 0)
                    {
                        words += "-" + Units[number % 10];
                    }
                }
            }

            return words;
        }
    }


}