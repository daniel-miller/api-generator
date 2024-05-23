namespace ApiGenerator.Utility
{
    public static class Inflector
    {
        public static string ConvertFirstLetterToUppercase(string input)
        {
            return string.Concat(input[0].ToString().ToUpper(), input.Substring(1));
        }

        public static string ConvertFirstLetterToLowercase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            char[] charArray = input.ToCharArray();
            charArray[0] = char.ToLower(charArray[0]);
            return new string(charArray);
        }

        public static string PluralizeNoun(string singularNoun)
        {
            if (string.IsNullOrEmpty(singularNoun))
            {
                return singularNoun;
            }

            // Define some basic rules for pluralization
            if (singularNoun.EndsWith("y") && !singularNoun.EndsWith("ay"))
            {
                // Replace "y" with "ies" for words ending in "y"
                return singularNoun.Substring(0, singularNoun.Length - 1) + "ies";
            }
            else if (singularNoun.EndsWith("s") || singularNoun.EndsWith("x") || singularNoun.EndsWith("z") || singularNoun.EndsWith("sh") || singularNoun.EndsWith("ch"))
            {
                // Add "es" for words ending in "s," "x," "z," "sh," or "ch"
                return singularNoun + "es";
            }
            else
            {
                // Add "s" for all other cases
                return singularNoun + "s";
            }
        }
    }
}
