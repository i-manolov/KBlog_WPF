using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KBlog_WPF
{
    public static class GenericFunctions
    {

        //Convert Words to numners
        public static int ConvertWordsToNumber(string NumberWord)
        {
            int Result = 0;

            // Handle game mode control commands
            switch (NumberWord)
            {
                case "ONE":
                    Result = 1;
                    break;

                case "TWO":
                    Result = 2;
                    break;

                case "THREE":
                    Result = 3;
                    break;

                case "FOUR":
                    Result = 4;
                    break;

                case "FIVE":
                    Result = 5;
                    break;

                case "SIX":
                    Result = 6;
                    break;

                case "SEVEN":
                    Result = 7;
                    break;

                case "EIGHT":
                    Result = 8;
                    break;

                case "NINE":
                    Result = 9;
                    break;

                case "ZERO":
                    Result = 0;

                    break;

            }

            return Result;
        }
    }
}
