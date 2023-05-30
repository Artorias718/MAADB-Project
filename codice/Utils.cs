using System.Data;
using MySql.Data.MySqlClient;
using MongoDB.Driver;
using MongoDB.Bson;

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

    public static void StampaDizionario(Dictionary<string, Dictionary<string, int>> d)
    {
        foreach (var kv in d)
        {
            Console.WriteLine("Lemma: " + kv.Key);
            Console.WriteLine("Risorse:");
            foreach (var risorsa in kv.Value)
            {

                Console.WriteLine(risorsa.Key + ": " + risorsa.Value);

            }
            Console.WriteLine();
        }
    }

    public static void UploadLemmiOfLexres(Dictionary<string, Dictionary<string, int>> lemmi, string sentimento)
    {
        MySqlConnection conn = new MySqlConnection("server=localhost;user=artorias;pwd=password;database=dibby");
        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = conn;
        conn.Open();

        // Eseguire una query per verificare se la tabella esiste
        cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{conn.Database}' AND table_name = 'tavoletta'";

        int tableCount = Convert.ToInt32(cmd.ExecuteScalar());

        if (tableCount > 0)
        {
            Console.WriteLine($"La tabella 'tavoletta' esiste nel database '{conn.Database}'.");
        }
        else
        {
            Console.WriteLine($"La tabella 'tavoletta' non esiste nel database '{conn.Database}'.");

            cmd.CommandText = "CREATE TABLE tavoletta (" +
                              "`id` INT NOT NULL AUTO_INCREMENT, " +
                              "`lemma` VARCHAR(255) NOT NULL, " +
                              "`sentimento` VARCHAR(255), " +
                              "`EmoSN` INT DEFAULT 0, " +
                              "`SentiSense` INT DEFAULT 0, " +
                              "`NRC` INT DEFAULT 0, " +
                              "`AFINN` INT, " +
                              "`ANEW` INT, " +
                              "`Frquency` INT, " +
                              "PRIMARY KEY (`id`)" +
                              ");";

            cmd.ExecuteNonQuery();
        }


        string outerDictQuery = "INSERT INTO tavoletta (lemma, sentimento, EmoSN, SentiSense) VALUES (@lemma, @sentimento, @EmoSN, @SentiSense);";
        //string innerDictQuery = "INSERT INTO tavoletta (risorsa, value, lemma_id) VALUES (@EmoSN, @SentiSense, @NRC, @AFINN, @ANEW, @Frquency);";



        using (MySqlCommand command = new MySqlCommand(outerDictQuery, conn))
        {


            foreach (KeyValuePair<string, Dictionary<string, int>> outerPair in lemmi)
            {

                command.Parameters.AddWithValue("@lemma", outerPair.Key);
                command.Parameters.AddWithValue("@sentimento", sentimento);

                // Aggiungi il valore di default per @EmoSN e @SentiSense
                command.Parameters.AddWithValue("@EmoSN", 0);
                command.Parameters.AddWithValue("@SentiSense", 0);

                foreach (KeyValuePair<string, int> innerPair in outerPair.Value)
                {
                    Resources resource;

                    if (Enum.TryParse(innerPair.Key, out resource))
                    {
                        switch (resource)
                        {
                            case Resources.EmoSN:
                                command.Parameters["@EmoSN"].Value = innerPair.Value;
                                break;
                            case Resources.sentisense:
                                command.Parameters["@SentiSense"].Value = innerPair.Value;
                                break;
                        }
                    }


                    command.ExecuteNonQuery();

                }
                command.Parameters.RemoveAt("@lemma");
                command.Parameters.RemoveAt("@sentimento");
                command.Parameters.RemoveAt("@EmoSN");
                command.Parameters.RemoveAt("@SentiSense");
            }
            conn.Close();

        }
    }

    public static void DeleteDatabase()
    {
        MySqlConnection conn = new MySqlConnection("server=localhost;user=artorias;pwd=password;database=dibby");
        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = conn;
        conn.Open();

        cmd.CommandText = "DROP TABLE inner_dict, outer_dict;";
        cmd.ExecuteNonQuery();

        conn.Close();

    }

    public static void UploadLemmiOfLexresMongoDB(Dictionary<string, Dictionary<string, int>> lemmi, string sentimento)
    {
        string connectionString = "mongodb://localhost:27017";
        MongoClient client = new MongoClient(connectionString);

        string databaseName = "testName";
        string collectionName = "testcollection2";

        // Ottieni il riferimento al database
        IMongoDatabase database = client.GetDatabase(databaseName);

        // Ottieni il riferimento alla collezione
        var collection = database.GetCollection<BsonDocument>(collectionName);

        /* Salva ogni lemma come un documento 
        foreach (var item in lemmi)
        {
            var document = new BsonDocument();
            document.Add("lemma", item.Key);

            var innerDict = new BsonDocument();
            foreach (var innerItem in item.Value)
            {
                innerDict.Add(innerItem.Key, innerItem.Value);
            }

            document.Add("risorse", innerDict);

            collection.InsertOne(document);
        }
        */
        var document = new BsonDocument();

        document.Add("sentimento", sentimento);

        foreach (var outerKey in lemmi.Keys)
        {
            var outerDict = lemmi[outerKey];
            var innerDoc = new BsonDocument();

            foreach (var innerKey in outerDict.Keys)
            {
                var innerValue = outerDict[innerKey];
                innerDoc.Add(innerKey, innerValue);
            }

            document.Add(outerKey, innerDoc);
        }

        // Inserisci il documento nel database
        collection.InsertOne(document);


    }

    public static Dictionary<string, Dictionary<string, int>> LemmasToDictionary(Dictionary<string, Dictionary<string, Dictionary<string, int>>> lemmiArray, Emotions em)
    {

        Dictionary<string, Dictionary<string, int>> lemmi = new Dictionary<string, Dictionary<string, int>>(); // Creazione di un nuovo dizionario per ogni iterazione

        //Console.WriteLine(em);
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
                            //Console.WriteLine("ID" + lemmiArray[(int)em]);

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
                continue;
            }

        }
        return lemmi;



    }

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







