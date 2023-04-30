using System.IO;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] splittedTextPos = Utils.ExtractEmoji("Risorse lessicali/posemoticons.txt");
            string[] splittedTextNeg = Utils.ExtractEmoji("Risorse lessicali/negemoticons.txt");
            string[] splittedText = splittedTextPos.Concat(splittedTextNeg).ToArray();

            Utils.UploadLemmiOfLexres();

            //Parsing.readTwitter("fare.txt", splittedText);

            //per ogni emozione accede a ogni risorsa (EMOsn, NRC ecc.)
            foreach (Emotions em in Enum.GetValues(typeof(Emotions)))
            {
                //Console.WriteLine(em);
                /*
                    string 1: parola del sentimento che stiamo analizzando
                    string 2: nome risorsa del file (EmoSN, NRC, sentisense)
                    int: occorenze della parola
                */
                Dictionary<string, Dictionary<string, int>> lemmi = new Dictionary<string, Dictionary<string, int>>();
                string startPath = $"Risorse lessicali/{em}/";
                string endPath = $"_{em}.txt";

                foreach (Resources res in Resources.GetValues(typeof(Resources)))
                {
                    if (res == Resources.nuova_risorsa)
                    {
                        continue;
                    }

                    try
                    {
                        string resString = res.ToString();
                        string path = startPath + resString + endPath;

                        // Leggi tutti i lemmi presenti nella risorsa lessicale
                        using (StreamReader reader = new StreamReader(path))
                        {
                            string lemma = reader.ReadLine();
                            while (lemma != null)
                            {
                                // Rimuovi l'endline
                                lemma = lemma.Replace("\n", "");

                                // Rimuovi le parole composte
                                if (!lemma.Contains("_"))
                                {
                                    Dictionary<string, int> l = lemmi.ContainsKey(lemma) ? lemmi[lemma] : new Dictionary<string, int>();
                                    l[resString] = 1;
                                    lemmi[lemma] = l;
                                }

                                lemma = reader.ReadLine();

                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        // Continua se il file non è stato trovato
                        continue;
                    }

                }
                //var lemmiList = lemmi.Select(kv => new { lemma = kv.Key, risorse = kv.Value }).ToList();

                /*foreach (var kv in lemmi)
                {
                    Console.WriteLine("Lemma: " + kv.Key);
                    Console.WriteLine("Risorse:");
                    foreach (var risorsa in kv.Value)
                    {

                        Console.WriteLine(risorsa.Key + ": " + risorsa.Value);

                    }
                    Console.WriteLine();
                }*/
                //Utils.StampaDizionario(lemmi);


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
}