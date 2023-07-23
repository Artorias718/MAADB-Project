using MySql.Data.MySqlClient;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Globalization;
using opennlp.tools.tokenize;
using opennlp.tools.postag;


namespace HelloWorld
{
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

            List<string> uniqueEmojiList = new List<string>();


            for (int i = 0; i < splittedText.Length; i++)
            {
                splittedText[i] = splittedText[i].Trim();
                splittedText[i] = splittedText[i].Replace("\r\n", "");
                splittedText[i] = splittedText[i].Substring(2, splittedText[i].Length - 3);
                string emoji = splittedText[i];
                uniqueEmojiList.Add(emoji);
            }

            string[] uniqueEmojiArray = uniqueEmojiList.Distinct().ToArray();

            return uniqueEmojiArray;
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

        public static void UploadPostgres(Dictionary<string, Dictionary<string, double>> lemmi, string sentimento)
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

        public static void UploadLexResourcesMongoDB(Emotions em)
        {
            string startPath = $"Risorse lessicali/{em}/";
            string endPath = $"_{em}.txt";
            string middlePath = $"_{em}";

            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "LexResources";


            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);


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
                    if (File.Exists(path))
                    {
                        string[] lines = File.ReadAllLines(path);
                        int count = lines.Length;

                        var document = new BsonDocument
                {
                    { "_id", resString + middlePath},
                    {"sentiment", em.ToString()},
                    {"num_words", count}
                };

                        collection.InsertOne(document);
                    }
                }
                catch (FileNotFoundException)
                {
                    continue;
                }

            }
        }

        public static void UploadLexResourcesWordsMongoDB(Emotions em)
        {
            var lemmi = LemmasToDictionary(em);
            var sentimento = em.ToString();

            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";

            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);

            Console.WriteLine(sentimento.ToString());

            var lex_res_collection = database.GetCollection<BsonDocument>("Lex_resources_words");

            var lems = new Dictionary<string, List<string>>();

            // Iterazione sui dati del dizionario e aggiunta delle stringhe esterne
            foreach (var outerKey in lemmi.Keys)
            {
                var outerDictionary = lemmi[outerKey];

                foreach (var innerKey in outerDictionary.Keys)
                {
                    if (!lems.ContainsKey(outerKey))
                    {
                        lems[outerKey] = new List<string>();
                    }

                    lems[outerKey].Add(innerKey);
                }
            }

            // Inserimento dei dati nella collezione
            foreach (var outerKey in lems.Keys)
            {
                var innerKeys = lems[outerKey];

                var res = new BsonDocument
                {
                    { "lemma", outerKey },
                    { "Lex_reosurces", new BsonArray(innerKeys) }
                };

                lex_res_collection.InsertOne(res);
            }

        }

        public static void UploadTweetMongoDB(TweetData data, int count)
        {
            var lemmi = LemmasToDictionary(data.Sentimento);
            var sentimento = data.Sentimento;
            var tokens = data.Tokens;

            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";

            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);

            Dictionary<string, int> innerDictionaryHashtag = tokens[Tokens.hashtag];
            Dictionary<string, int> innerDictionaryEmoji = tokens[Tokens.emoji];
            Dictionary<string, int> innerDictionaryEmoticon = tokens[Tokens.emoticon];

            BsonArray innerDocumentHashtag = new BsonArray();
            BsonArray innerDocumentEmoji = new BsonArray();
            BsonArray innerDocumentEmoticon = new BsonArray();

            foreach (var i in innerDictionaryHashtag)
            {
                for (var a = 0; a < i.Value; a++) //per aggiungere n volte un token nello stesso tweet
                {
                    innerDocumentHashtag.Add(i.Key);
                }
            }

            foreach (var i in innerDictionaryEmoji)
            {
                //sono sbagliati i conunters già quando arrivano qui, sono x2 solo per anger!!!!!!!!!!!!!!!!!!!!!!!!!!!
                for (var a = 0; a < i.Value; a++) //per aggiungere n volte un token nello stesso tweet
                {
                    innerDocumentEmoji.Add(i.Key);
                }
            }

            foreach (var i in innerDictionaryEmoticon)
            {
                for (var a = 0; a < i.Value; a++) //per aggiungere n volte un token nello stesso tweet
                {
                    innerDocumentEmoticon.Add(i.Key);
                }
            }


            var lex_res_collection = database.GetCollection<BsonDocument>("Lex_resources_words");

            var lems = new Dictionary<string, List<string>>();

            var words = new BsonArray();
            foreach (var outerKey in data.Lemmi)
            {
                var word = new BsonDocument{

                {"lemma", outerKey},
                {"pos", POS_word_tagger(outerKey)}

                };

                var filter = Builders<BsonDocument>.Filter.Eq("lemma", outerKey);
                var filteredDoc = lex_res_collection.Find(filter).FirstOrDefault();

                try
                {
                    if (filter != null)
                    {
                        // Ottieni l'ID del documento lex_res_collection e aggiungi il campo di referenza nel documento word
                        var id = filteredDoc["_id"].AsObjectId;
                        word.Add("lex_res_reference", id);
                    }

                }
                catch
                {

                }
                words.Add(word);

            }
            var document = new BsonDocument
                {
                    { "id", sentimento.ToString() },
                    { "doc_numer", count },
                    {"words", words},
                    { "hastags", innerDocumentHashtag },
                    { "emoji", innerDocumentEmoji },
                    { "emoticon", innerDocumentEmoticon }
                };

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
                            string filename = resString + $"_{em}";
                            lemmaWithScore[parola][filename] = numero;
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
                    string filename = resString + $"_{em}";


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
                                l[filename] = 1;
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

        public static List<TweetData> TweetProcessing(Emotions em, Dictionary<string, string> splittedSlagWords, string[] splittedEmoticons, string[] splittedEmoji)
        {
            List<TweetData> Tweets = new List<TweetData>();
            string tweetPath = $"Twitter messaggi/dataset_dt_{em.ToString()}_60k.txt";
            //string tweetPath = "Twitter messaggi/test.txt";
            foreach (string line in File.ReadLines(tweetPath))
            {
                TweetData Data = new TweetData(
                em,
                CreateTokensDictionary()
                );

                var tokens = Data.Tokens;

                string stopwordsText = "[,?!.;:/()& _+=<>\"]";

                char[] stopwords = stopwordsText.ToCharArray();

                WhitespaceTokenizer tokenizer = WhitespaceTokenizer.INSTANCE;

                string[] tokensNLP = tokenizer.tokenize(line);

                Data.Lemmi = tokensNLP;

                //string[] tags = POStagger(tokensNLP);

                //for (int i = 0; i < tokensNLP.Length; i++)
                // {
                //  postags.Add(tokensNLP[i], tags[i]);

                //}

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
                        if (StringToUTF32(tokeNLP).Contains(emoji))
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

                    // foreach (char stop in stopwords)
                    // {
                    //     if (tokeNLP.Contains(stop))
                    //     {
                    //         tokeNLP.Remove(stop);
                    //     }
                    // }
                }

                Tweets.Add(Data);

            }

            /*foreach (var kv in tokens)
            {
                foreach (var risorsa in kv.Value)
                {
                    Console.WriteLine($"EMOTIZIONE: {em}, TokenType: {kv.Key}, word: {risorsa.Key}, VALORE: {risorsa.Value}");
                }
            }*/

            /*foreach (string a in tokensNLPList)
            {
                Console.WriteLine(a);
            }*/

            /*foreach (var kv in lemmaFrequencies)
            {
                foreach (var risorsa in kv.Value)
                {
                    Console.WriteLine($"EMOTIZIONE: {em}, TokenType: {kv.Key}, word: {risorsa.Key}, VALORE: {risorsa.Value}");
                }
            }*/
            return Tweets;
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

        public static string POS_word_tagger(string word)
        {
            var modelPath = "opennlp-en-ud-ewt-pos-1.0-1.9.3.bin";

            java.io.InputStream inputStream = new java.io.FileInputStream(modelPath);
            // Utilizzo dell'oggetto InputStream nel costruttore di POSModel
            POSModel posModel = new POSModel(inputStream);

            // Inizializza il POSTagger con il modello POS
            POSTaggerME posTagger = new POSTaggerME(posModel);

            //Console.WriteLine("stringa: " + word);
            // Tokenizzazione del testo
            Tokenizer tokenizer = SimpleTokenizer.INSTANCE;
            string[] tokens2 = tokenizer.tokenize(word);



            // Esegui il POS tagging sui token
            string[] tags = posTagger.tag(tokens2);

            //Console.WriteLine(tags[0]);

            return tags[0];
        }

        public static string[] POStagger(string[] tokensNLP)
        {
            var modelPath = "opennlp-en-ud-ewt-pos-1.0-1.9.3.bin";

            java.io.InputStream inputStream = new java.io.FileInputStream(modelPath);
            // Utilizzo dell'oggetto InputStream nel costruttore di POSModel
            POSModel posModel = new POSModel(inputStream);

            // Inizializza il POSTagger con il modello POS
            POSTaggerME posTagger = new POSTaggerME(posModel);

            // Tokenizzazione del testo
            Tokenizer tokenizer = SimpleTokenizer.INSTANCE;


            // Esegui il POS tagging sui token
            string[] tags = posTagger.tag(tokensNLP);

            // Stampa i token e i rispettivi POS tags
            for (int i = 0; i < tokensNLP.Length; i++)
            {
                Console.WriteLine(tokensNLP[i] + " -> " + tags[i]);
            }

            //Printing the tokens 
            foreach (string token in tokensNLP)
            {
                Console.WriteLine(token);
            }

            return tags;
        }

        static string StringToUTF32(string input)
        {
            int[] utf32Codes = new int[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                utf32Codes[i] = char.ConvertToUtf32(input, i);
                if (char.IsSurrogatePair(input, i))
                    i++;  // Skip the low surrogate of the surrogate pair
            }
            string utf32Hex = "";
            foreach (int code in utf32Codes)
            {
                utf32Hex += code.ToString("X8");
            }
            string codePointHex = utf32Hex.Substring(0, 8);
            uint codePoint = uint.Parse(codePointHex, System.Globalization.NumberStyles.HexNumber);
            string unicodeEscape = "\\U" + codePoint.ToString("X8");
            return "u\"" + unicodeEscape + "\"";
        }

        public static Dictionary<Tokens, Dictionary<string, int>> CreateTokensDictionary()
        {
            return new Dictionary<Tokens, Dictionary<string, int>>
    {
        { Tokens.hashtag, new Dictionary<string, int>() },
        { Tokens.emoticon, new Dictionary<string, int>() },
        { Tokens.emoji, new Dictionary<string, int>() }
    };
        }
    }
}