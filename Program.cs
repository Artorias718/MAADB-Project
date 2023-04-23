using System.IO;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            HashSet<string> posemoticons = new HashSet<string>(File.ReadAllLines("Risorse lessicali/posemoticons.txt"));
            HashSet<string> negemoticons = new HashSet<string>(File.ReadAllLines("Risorse lessicali/negemoticons.txt"));
            Parsing.readTwitter("fare.txt", posemoticons, negemoticons);

        }

    }
    public static class Parsing
    {
        public static void readTwitter(string nameFile, HashSet<String> posemoticons, HashSet<String> negemoticons)
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
                    String selected_word = line.Substring(head + 1, tail - head);

                    //conftronta ogni emoji con la parola, sia per le emoji positive che per quelle negative
                    foreach (String emoji in posemoticons.Union(negemoticons))
                    {
                        if (!selected_word.Equals(emoji))
                        {
                            //se la parola NON è un emoji, la rimuove 
                            String res = line.Remove(head + 1, tail - head);
                            sr.Close();

                            File.WriteAllText("fare.txt", File.ReadAllText("fare.txt").Replace(line, res));
                            line = res; //salva la nuova frase per continuare a iterare
                            break; //appena trova una corrispondenza con una emoji esce dal foreach
                        }


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