using System.IO;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            HashSet<string> posemoticons = new HashSet<string>(File.ReadAllLines("Risorse lessicali/posemoticons.txt"));
            HashSet<string> negemoticons = new HashSet<string>(File.ReadAllLines("Risorse lessicali/negemoticons.txt"));

            Console.WriteLine("sono scemo!");
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
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(nameFile);
                //string input = sr.ReadToEnd();
                
                //Read the first line of text
                line = sr.ReadToEnd();
                
                int i = line.IndexOf("_");
                while(i >= 0)
                {
                    Console.WriteLine("prima del while1 " + i);
                    int j = i;

                    while(!line[j].Equals(' '))
                    {
                        j--;
                    }
                    Console.WriteLine("dopo il while1 " + j);
                    Console.WriteLine("prima del while2 " + i);
                    int k=i;
                    while(!line[k].Equals(' ') && k<line.Length-1)
                    {
                        k++;
                    }

                    Console.WriteLine("dopo il while2 " + k);
                    
                    String line2 = line.Remove(j+1, k-j);
                    Console.WriteLine(line2);
                    String a = line;
                    sr.Close();
                    using(StreamWriter writer = new StreamWriter(nameFile, true))
                    {
                        {
                            string output = line.Replace(a, line2);
                            writer.Write(output);
                        }
                        writer.Close();
                    }

                    i = line.IndexOf("_", i+1);
                }

                //Continue to read until you reach end of file
                /*
                while (line != '\n')
                {
                    //Console.WriteLine(line.IndexOf("_"));
                    //write the line to console window
                    //Console.WriteLine(line);
                    //Read the next line
                    line = sr.ReadLine();
                }
                */
                //close the file
                sr.Close();
                //Console.ReadLine();
            }
            catch(Exception e)
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