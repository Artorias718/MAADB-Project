using System.IO;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            HashSet<string> posemoticons = new HashSet<string>(File.ReadAllLines("Risorse lessicali/posemoticons.txt"));
            HashSet<string> negemoticons = new HashSet<string>(File.ReadAllLines("Risorse lessicali/negemoticons.txt"));
            Parsing.readTwitter("fare.txt");
        }

    }
    public static class Parsing
    {
        public static void readTwitter(string nameFile)
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

                    String res = line.Remove(head + 1, tail - head);
                    Console.WriteLine(res);
                    sr.Close();

                    File.WriteAllText("fare.txt", File.ReadAllText("fare.txt").Replace(line, res));
                    line = res;

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