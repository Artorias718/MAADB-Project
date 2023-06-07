using System;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Globalization;
using opennlp.tools.tokenize;
using opennlp.tools.postag;
using opennlp.tools.util;
using System.Reflection;


public class Utils
{
    public static string[] ExtractEmoticons(string fileUrl)
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

    public static Dictionary<string, string> ExtractSlagWords(string fileUrl)
    {
        int start = File.ReadAllText(fileUrl).IndexOf('{');
        int end = File.ReadAllText(fileUrl).LastIndexOf('}');

        Dictionary<string, string> SlagWords = new Dictionary<string, string>();

        string text = File.ReadAllText(fileUrl).Substring(start + 1, end - start - 1);
        string[] splittedText1 = text.Split(':');
        string[] splittedText2 = new string[0];

        for (int i = 0; i < splittedText1.Length; i++)
        {
            splittedText1[i] = splittedText1[i].Trim();
            splittedText1[i] = splittedText1[i].Replace("\r\n", "");
            //splittedText1[i] = splittedText1[i].Replace(" ", "");

            //splittedText[i] = splittedText[i].Substring(1, splittedText[i].Length - 2);

            if (!splittedText1[i].StartsWith("\'"))
            {
                splittedText1[i - 1] = splittedText1[i - 1] + ":" + splittedText1[i];

                splittedText1 = Array.FindAll(splittedText1, e => e != splittedText1[i]);
            }
        }

        foreach (string elem in splittedText1)
        {
            string[] support = elem.Split(",");

            splittedText2 = splittedText2.Concat(support).ToArray();
        }

        for (int i = 0; i < splittedText2.Length; i++)
        {
            splittedText2[i] = splittedText2[i].Replace("\r\n", "");
            //splittedText2[i] = splittedText2[i].Replace(" ", "");

            if (splittedText2[i].StartsWith(" "))
                splittedText2[i] = splittedText2[i].Substring(1);

            if (!splittedText2[i].StartsWith("\'"))
            {
                splittedText2[i - 1] = splittedText2[i - 1] + ", " + splittedText2[i];

                splittedText2 = Array.FindAll(splittedText2, e => e != splittedText2[i]); //per rimuovere elemento in posizione i
                i = i - 2;
            }
        }

        for (int i = 0; i < splittedText2.Length; i++)
            splittedText2[i] = splittedText2[i].Substring(1, splittedText2[i].Length - 2);

        for (int i = 0; i < splittedText2.Length; i = i + 2)
            SlagWords.Add(splittedText2[i], splittedText2[i + 1]);

        return SlagWords;
    }

    public static string[] ExtractEmoji(string fileUrl)
    {
        int start = File.ReadAllText(fileUrl).IndexOf('[');
        int end = File.ReadAllText(fileUrl).LastIndexOf(']');

        string text = File.ReadAllText(fileUrl).Substring(start + 1, end - start - 1);
        string[] splittedText = text.Split(',');

        for (int i = 0; i < splittedText.Length; i++)
        {
            splittedText[i] = splittedText[i].Trim();
            splittedText[i] = splittedText[i].Replace("\r\n", "");
            splittedText[i] = splittedText[i].Substring(2, splittedText[i].Length - 3);

            //Console.WriteLine(splittedText[i]);

            //splittedText[i] = splittedText[i].Substring(1, splittedText[i].Length - 2);

            //Console.WriteLine(splittedText[i]);
        }

        return splittedText;
    }

    public static void StampaDizionario(Dictionary<string, Dictionary<string, double>> d)
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

    public static void UploadLemmiOfLexres(Dictionary<string, Dictionary<string, double>> lemmi, string sentimento)
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
                              "`EmoSN` DOUBLE DEFAULT 0, " +
                              "`SentiSense` DOUBLE DEFAULT 0, " +
                              "`NRC` DOUBLE DEFAULT 0, " +
                              "`AFINN` DOUBLE, " +
                              "`ANEWARO` DOUBLE, " +
                              "`ANEWDOM` DOUBLE, " +
                              "`ANEWPLEAS` DOUBLE, " +
                              "PRIMARY KEY (`id`)" +
                              ");";

            cmd.ExecuteNonQuery();
        }


        string outerDictQuery = "INSERT INTO tavoletta (lemma, sentimento, EmoSN, SentiSense, AFINN, ANEWARO, ANEWDOM, ANEWPLEAS)" +
                                            "VALUES (@lemma, @sentimento, @EmoSN, @SentiSense, @AFINN, @ANEWARO, @ANEWDOM, @ANEWPLEAS);";
        //string innerDictQuery = "INSERT INTO tavoletta (risorsa, value, lemma_id) VALUES (@EmoSN, @SentiSense, @NRC, @AFINN, @ANEW, @Frquency);";


        using (MySqlCommand command = new MySqlCommand(outerDictQuery, conn))
        {


            foreach (KeyValuePair<string, Dictionary<string, double>> outerPair in lemmi)
            {

                command.Parameters.AddWithValue("@lemma", outerPair.Key);
                command.Parameters.AddWithValue("@sentimento", sentimento);

                // Aggiungi il valore di default 
                command.Parameters.AddWithValue("@EmoSN", 0);
                command.Parameters.AddWithValue("@SentiSense", 0);
                command.Parameters.AddWithValue("@AFINN", null);
                command.Parameters.AddWithValue("@ANEWARO", null);
                command.Parameters.AddWithValue("@ANEWDOM", null);
                command.Parameters.AddWithValue("@ANEWPLEAS", null);



                foreach (KeyValuePair<string, double> innerPair in outerPair.Value)
                {
                    Resources resource;
                    ResourcesWithScore resourceWithScore;

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

                    if (Enum.TryParse(innerPair.Key, out resourceWithScore))
                    {
                        switch (resourceWithScore)
                        {
                            case ResourcesWithScore.afinn:
                                command.Parameters["@AFINN"].Value = innerPair.Value;
                                break;
                            case ResourcesWithScore.anewAro:
                                command.Parameters["@ANEWARO"].Value = innerPair.Value;
                                break;
                            case ResourcesWithScore.anewDom:
                                command.Parameters["@ANEWDOM"].Value = innerPair.Value;
                                break;
                            case ResourcesWithScore.anewPleas:
                                command.Parameters["@ANEWPLEAS"].Value = innerPair.Value;
                                break;
                        }
                    }


                }

                command.ExecuteNonQuery();

                command.Parameters.RemoveAt("@lemma");
                command.Parameters.RemoveAt("@sentimento");
                command.Parameters.RemoveAt("@EmoSN");
                command.Parameters.RemoveAt("@SentiSense");
                command.Parameters.RemoveAt("@AFINN");
                command.Parameters.RemoveAt("@ANEWARO");
                command.Parameters.RemoveAt("@ANEWDOM");
                command.Parameters.RemoveAt("@ANEWPLEAS");


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

    public static void UploadLemmiOfLexresMongoDB(Dictionary<string, Dictionary<string, double>> lemmi, string sentimento)
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

    public static Dictionary<string, Dictionary<string, double>> LemmasToDictionary(Emotions em)
    {

        Dictionary<string, Dictionary<string, double>> lemmi = new Dictionary<string, Dictionary<string, double>>(); // Creazione di un nuovo dizionario per ogni iterazione
        Dictionary<string, Dictionary<string, double>> lemmaWithScore = new Dictionary<string, Dictionary<string, double>>();

        //TODO
        //Qui assegna tutti gli score all'etichetta "afinn" manulamente, per avere anche quella devo trasformare lemmawithScore in un 
        // Dictionary<string, Dictionary<string, int>>
        // <lemma<risorsa:score>>

        //Console.WriteLine(em);
        string startPath = $"Risorse lessicali/{em}/";
        string endPath = $"_{em}.txt";
        string startPathConScore = $"Risorse lessicali/ConScore/";
        string endPathWithScore = $"_tab.tsv";


        //popola lemmaWithscore con le risorse lessicali con lo score in afinn.txt
        using (StreamReader reader = new StreamReader(startPathConScore + "afinn.txt"))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] lineParts = line.Split('\t');
                string parola = lineParts[0];
                double numero = double.Parse(lineParts[1]);
                if (!lemmaWithScore.ContainsKey(parola))
                {
                    lemmaWithScore[parola] = new Dictionary<string, double>();
                }
                lemmaWithScore[parola]["afinn"] = numero;

            }
        }
        //popola lemmaWithscore con le risorse lessicali con lo score negli altri 3 files
        foreach (ResourcesWithScore res in ResourcesWithScore.GetValues(typeof(ResourcesWithScore)))
        {
            if (res == ResourcesWithScore.nuova_risorsa)
            {
                continue;

            }

            try
            {
                string resString = res.ToString();
                string path = startPathConScore + resString + endPathWithScore;

                // Leggi tutti i lemmi presenti nella risorsa lessicale
                using (StreamReader reader = new StreamReader(path))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] lineParts = line.Split('\t');
                        string parola = lineParts[0];

                        double numero = double.Parse(lineParts[1], CultureInfo.GetCultureInfo("en-US")); if (!lemmaWithScore.ContainsKey(parola))
                        {
                            lemmaWithScore[parola] = new Dictionary<string, double>();
                        }

                        lemmaWithScore[parola][resString] = numero;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                continue;
            }

        }

        //popola il dizionario del lemma sia con parole delle risorse normali che con parole delle risorse con score
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
                    string? lemma = reader.ReadLine();


                    while (lemma != null)
                    {
                        // Rimuovi l'endline
                        lemma = lemma.Replace("\n", "");

                        // Rimuovi le parole composte
                        if (!lemma.Contains("_"))
                        {
                            //carica nel dizionario i lemmi senza score
                            Dictionary<string, double> l = lemmi.ContainsKey(lemma) ? lemmi[lemma] : new Dictionary<string, double>();
                            l[resString] = 1;
                            lemmi[lemma] = l;
                            //se il lemma è presente nel lemmaWithScore, aggiunge anche gli scores delle altre risorse
                            if (lemmaWithScore.ContainsKey(lemma))
                            {
                                foreach (var dict in lemmaWithScore[lemma])
                                {
                                    l[dict.Key] = dict.Value;
                                    lemmi[lemma] = l;
                                }

                            }
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

    public static void TweetProcessing(Emotions em,
                                        Dictionary<Tokens, Dictionary<string, int>> tokens,
                                        string[] splittedEmoji, string[] splittedEmoticons,
                                        Dictionary<Emotions, Dictionary<string, int>> lemmaFrequencies,
                                        Dictionary<string, Dictionary<string, double>> lemmi,
                                        Dictionary<string, string> splittedSlagWords)
    {
        //string tweetPath = $"Twitter messaggi/dataset_dt_{em}_60k.txt";
        string tweetPath = "Twitter messaggi/test.txt";
        Dictionary<string, int> innerDictLemmiFreq = new Dictionary<string, int>();

        foreach (string l in lemmi.Keys)
        {
            innerDictLemmiFreq[l] = 0;

        }
        lemmaFrequencies[em] = innerDictLemmiFreq;

        string text = File.ReadAllText(tweetPath);

        //string stopwordsText = "[,?!.;:/()& _+=<>\"]";

        //char[] stopwords = stopwordsText.ToCharArray();

        //tokenizzazione del tweet
        WhitespaceTokenizer tokenizer = WhitespaceTokenizer.INSTANCE;
        string[] tokensNLP = tokenizer.tokenize(text);
        List<string> tokensNLPList = new List<string>(tokensNLP);

        //rimozione username e url
        tokensNLPList.Remove("USERNAME");
        tokensNLPList.Remove("URL");

        //sostituzione slag words
        foreach (var kv in splittedSlagWords)
        {
            if (tokensNLPList.Contains(kv.Key))
            {
                int index = tokensNLPList.IndexOf(kv.Key);

                if (index != -1)
                {
                    tokensNLPList.RemoveAt(index);
                    tokensNLPList.InsertRange(index, tokenizer.tokenize(kv.Value));
                }
            }

        }

        //popolamento dizionario dei tokens
        foreach (string tokeNLP in tokensNLPList)
        {
            if (tokeNLP.Contains('#'))
            {

                if (tokens[Tokens.hashtag].ContainsKey(tokeNLP))
                {
                    tokens[Tokens.hashtag][tokeNLP]++;
                }
                else
                {
                    tokens[Tokens.hashtag].Add(tokeNLP, 1);
                }
            }

            foreach (string emo in splittedEmoticons)
            {
                if (tokeNLP.Contains(emo))
                {
                    if (tokens[Tokens.emoticon].ContainsKey(emo))
                    {
                        tokens[Tokens.emoticon][emo]++;
                    }
                    else
                    {
                        tokens[Tokens.emoticon].Add(emo, 1);
                    };
                }

            }

            foreach (string emoji in splittedEmoji)
            {
                if (tokeNLP.Contains(emoji))
                {
                    if (tokens[Tokens.emoji].ContainsKey(emoji))
                    {
                        tokens[Tokens.emoji][emoji]++;
                    }
                    else
                    {
                        tokens[Tokens.emoji].Add(emoji, 1);
                    }
                }


            }
            foreach (string lem in lemmi.Keys)
            {
                if (tokeNLP.Contains(lem))
                {
                    innerDictLemmiFreq[lem]++;
                }

            }

        }


        foreach (var kv in tokens)
        {
            foreach (var risorsa in kv.Value)
            {

                Console.WriteLine($"EMOTIZIONE: {em}, TokenType: {kv.Key}, word: {risorsa.Key}, VALORE: {risorsa.Value}");

            }
        }
        /*foreach (string a in tokensNLPList)
        {
            Console.WriteLine(a);
        }*/

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

    public static int CalcoloPercentuali(Resources res, Emotions em)
    {
        string startPath = $"Risorse lessicali/{em}/";
        string endPath = $"_{em}.txt";
        string tweetPath = $"Twitter messaggi/dataset_dt_anger_60k.txt";

        int N_twitter_words = 0;
        int N_lex_words = 0;
        List<string> lemmiPresenti = new List<string>();
        int N_shared_words = 0;

        try
        {
            string resString = res.ToString();
            string path = startPath + resString + endPath;

            // Leggi tutti i lemmi presenti nella risorsa lessicale
            using (StreamReader reader = new StreamReader(path))
            {
                string? lemma = reader.ReadLine();

                while (lemma != null)
                {
                    // Rimuovi l'endline
                    lemma = lemma.Replace("\n", "");
                    N_lex_words++;
                    // Rimuovi le parole composte
                    if (!lemma.Contains("_"))
                    {
                        using (StreamReader readerTweet = new StreamReader(tweetPath))
                        {
                            string? rigaTweet;
                            while ((rigaTweet = readerTweet.ReadLine()) != null)
                            {
                                string[] words = rigaTweet.Split(' '); // Dividi la riga in parole utilizzando lo spazio come separatore

                                foreach (string word in words)
                                {
                                    // Esegui le operazioni desiderate con ogni parola
                                    N_twitter_words++;
                                    if (lemma == word && !lemmiPresenti.Contains(lemma))
                                    {
                                        lemmiPresenti.Add(lemma);
                                        //fare che se il lemma compare aumenti di 1 ma poi non lo faccia pi+ per quel lemma
                                    }
                                }
                            }
                        }
                    }
                    lemma = reader.ReadLine();

                }
            }
        }
        catch (FileNotFoundException) { }

        N_shared_words = lemmiPresenti.Count;

        int perc_presence_lex_res = N_shared_words / N_lex_words;
        int perc_presence_twitter = N_shared_words / N_twitter_words;

        return perc_presence_lex_res;


    }

    public static string[] POStagger(string text)
    {

        var modelPath = "opennlp-en-ud-ewt-pos-1.0-1.9.3.bin";

        java.io.InputStream inputStream = new java.io.FileInputStream(modelPath);
        // Utilizzo dell'oggetto InputStream nel costruttore di POSModel
        POSModel posModel = new POSModel(inputStream);

        // Inizializza il POSTagger con il modello POS
        POSTaggerME posTagger = new POSTaggerME(posModel);

        // Tokenizzazione del testo
        Tokenizer tokenizer = SimpleTokenizer.INSTANCE;
        string[] tokens2 = tokenizer.tokenize(text);

        // Esegui il POS tagging sui token
        string[] tags = posTagger.tag(tokens2);

        // Stampa i token e i rispettivi POS tags
        for (int i = 0; i < tokens2.Length; i++)
        {
            Console.WriteLine(tokens2[i] + " -> " + tags[i]);
        }

        //Printing the tokens 
        foreach (string token in tokens2)
        {
            Console.WriteLine(token);
        }
        return tags;
    }



}







