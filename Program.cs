using System.IO;


namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            HashSet<string> posemoticons = new HashSet<string>(File.ReadAllLines("Risorse lessicali/posemoticons.txt"));
            HashSet<string> negemoticons = new HashSet<string>(File.ReadAllLines("Risorse lessicali/negemoticons.txt"));

            string[] splittedTextPos = Utils.ExtractEmoji("Risorse lessicali/posemoticons.txt");
            string[] splittedTextNeg = Utils.ExtractEmoji("Risorse lessicali/negemoticons.txt");
            string[] splittedText = splittedTextPos.Concat(splittedTextNeg).ToArray();

            Parsing.readTwitter("fare.txt", splittedText);



        }

    }

    public static class Parsing
    {
        public static void readTwitter(string nameFile, string[] splittedText)
        {

            String line;
            try
            {
                StreamReader sr = new StreamReader(nameFile);

                line = sr.ReadToEnd();
                int i = line.IndexOf("_");
                int head;
                int tail;

                while (i > 0)
                {
                    Console.WriteLine("Underscore: " + i);
                    int j = i;

                    while (!line[j].Equals(' '))
                    {
                        j--;
                    }
                    head = j;

                    Console.WriteLine("Head: " + head);
                    j = i;
                    while (!line[j].Equals(' ') && j < line.Length - 1)
                    {
                        j++;
                    }
                    tail = j;
                    Console.WriteLine("Tail: " + j);

                    //salva la parola che stiamo considerando, per controllare se è un emoji
                    String selected_word = line.Substring(head + 1, tail - head - 1);


                    bool isEmoji = false;

                    //conftronta ogni emoji con la parola, sia per le emoji positive che per quelle negative
                    foreach (String emoji in splittedText)
                    {
                        // Console.WriteLine("Confronta " + selected_word + " con " + emoji + " Risultato: " + selected_word.Equals(emoji));
                        if (selected_word.Equals(emoji))
                        {
                            isEmoji = true;

                        }

                    }

                    if (!isEmoji)
                    {
                        String res = line.Remove(head + 1, tail - head);
                        sr.Close();

                        File.WriteAllText("fare.txt", File.ReadAllText("fare.txt").Replace(line, res));
                        line = res; //salva la nuova frase per continuare a iterare
                    }


                    i = line.IndexOf("_", i + 1);
                }

                sr.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }

        }
    }
}