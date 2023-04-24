using System.IO;

namespace HelloWorld
{
    public class Utils
    {

        public static string[] ExtractEmoji(string fileUrl)
        {
            int start = File.ReadAllText(fileUrl).IndexOf('[');
            int end = File.ReadAllText(fileUrl).LastIndexOf(']');

            string text = File.ReadAllText(fileUrl).Substring(start + 1, end - start - 1);
            string[] splittedText = text.Split(',');


            for (int i = 0; i < splittedText.Length; i++)
            {

                splittedText[i] = splittedText[i].Trim();

                if (splittedText[i][0].Equals('\\'))
                {
                    splittedText[i] = splittedText[i].Remove(0, 1).Replace("\r\n", "");
                }

                //Console.WriteLine("***" + splittedText[i] + "***");

                splittedText[i] = splittedText[i].Substring(1, splittedText[i].Length - 2);

                //Console.WriteLine(splittedText[i]);

            }
            return splittedText;
        }


    }
}
