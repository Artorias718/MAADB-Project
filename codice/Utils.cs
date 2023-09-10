using MySql.Data.MySqlClient;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Globalization;
using opennlp.tools.tokenize;
using opennlp.tools.postag;
using WordCloudSharp;
using System.Drawing;
using System.Text.RegularExpressions;
using static HelloWorld.MongoDBAggregations;
using System;
using System.Diagnostics;
using System.Text.Json;

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

        public static int checkPresence(string l, string em, string nome_risorsa)
        {
            string startPath = $"Risorse lessicali/{em}/";
            string endPath = $"_{em}.txt";

            int isPresent = 0;

            if (File.Exists(startPath + nome_risorsa + endPath))
            {
                using (StreamReader reader = new StreamReader(startPath + nome_risorsa + endPath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] lineParts = line.Split('\t');
                        string parola = lineParts[0];
                        if (l == parola)
                        {
                            isPresent = 1;
                        }
                    }
                }
            }
            return isPresent;
        }

        public static double? getScore(string l, string nome_risorsa)
        {

            string startPathConScore = $"Risorse lessicali/ConScore/";
            string endPathWithScore = $"_tab.tsv";

            double? score = null;
            string path;

            if(nome_risorsa.Equals("afinn"))
            {
                path = startPathConScore + nome_risorsa + ".txt";
            }
            else
            {
                path = startPathConScore + nome_risorsa + endPathWithScore;
            }

            if (File.Exists(path))
            {
                // Leggi tutti i lemmi presenti nella risorsa lessicale
                using (StreamReader reader = new StreamReader(path))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] lineParts = line.Split('\t');
                        string parola = lineParts[0];
                        if (l == parola)
                        {
                            score = double.Parse(lineParts[1], CultureInfo.GetCultureInfo("en-US"));
                        }

                    }
                }
            }

            return score;
        }

        static bool checkTableExists(MySqlConnection connection, string tableName)
        {
            // Eseguire una query per verificare se la tabella esiste
            using (MySqlCommand cmd = new MySqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.Text;
                
                // Eseguire una query per verificare se la tabella esiste
                cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables " + 
                                    $"WHERE table_schema = '{connection.Database}' " + 
                                    $"AND table_name = '{tableName}'";

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                
                return count > 0;
            }
        }

        public static void createTablesPostgres()
        {   
            MySqlConnection conn = new MySqlConnection("server=localhost;user=alfredo;pwd=password;database=maadb");
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            conn.Open();
/*
            if(checkTableExists(conn, "tavoletta"))
                Console.WriteLine($"La tabella 'tavoletta' esiste nel database '{conn.Database}'.");
            else
            {
                Console.WriteLine($"La tabella 'tavoletta' NON esiste nel database '{conn.Database}'.");

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
                */
            if(checkTableExists(conn, "percentages"))
                Console.WriteLine($"La tabella 'percentages' esiste nel database '{conn.Database}'.");
            else
            {
                Console.WriteLine($"La tabella 'percentages' NON esiste nel database '{conn.Database}'.");

                cmd.CommandText = "CREATE TABLE percentages (" +
                                  "`sentiment` VARCHAR(50), " +
                                  "`perc_presence_emo_sn` DOUBLE DEFAULT 0," +
                                  "`perc_presence_sn_in_tokens` DOUBLE DEFAULT 0, " +
                                  "`perc_presence_senti_sense` DOUBLE DEFAULT 0, " +
                                  "`perc_presence_sense_tokens` DOUBLE DEFAULT 0, " +
                                  "`perc_presence_nrc` DOUBLE DEFAULT 0, " +
                                  "`perc_presence_nrc_tokens` DOUBLE DEFAULT 0" +
                                  ");";
                
                cmd.ExecuteNonQuery();
            }

            if(checkTableExists(conn, "common_words"))
                Console.WriteLine($"La tabella 'common_words' esiste nel database '{conn.Database}'.");
            else
            {
                Console.WriteLine($"La tabella 'common_words' NON esiste nel database '{conn.Database}'.");

                cmd.CommandText = "CREATE TABLE common_words (" +
                                  "`sentiment` VARCHAR(50), " +
                                  "`shared_words_emo_sn` INT(11) DEFAULT 0," +
                                  "`total_words_emo_sn` INT(11) DEFAULT 0, " +
                                  "`shared_words_senti_sense` INT(11) DEFAULT 0, " +
                                  "`total_words_senti_sense` INT(11) DEFAULT 0, " +
                                  "`shared_words_nrc` INT(11) DEFAULT 0, " +
                                  "`total_words_nrc` INT(11) DEFAULT 0," +
                                  "`total_tokens` INT(11) DEFAULT 0" +
                                  ");";
                
                cmd.ExecuteNonQuery();
            }

            if(checkTableExists(conn, "sentiment"))
                Console.WriteLine($"La tabella 'sentiment' esiste nel database '{conn.Database}'.");
            else
            {
                Console.WriteLine($"La tabella 'sentiment' NON esiste nel database '{conn.Database}'.");

                cmd.CommandText = "CREATE TABLE sentiment (" +
                                  "`id` INT(11) PRIMARY KEY AUTO_INCREMENT," +
                                  "`name` VARCHAR(30)" +
                                  ") AUTO_INCREMENT=0;";
                
                cmd.ExecuteNonQuery();
            }

            if(checkTableExists(conn, "lex_res_totals"))
                Console.WriteLine($"La tabella 'lex_res_totals' esiste nel database '{conn.Database}'.");
            else
            {
                Console.WriteLine($"La tabella 'lex_res_totals' NON esiste nel database '{conn.Database}'.");

                cmd.CommandText = "CREATE TABLE lex_res_totals (" +
                                  "`sentiment_id` INT(11), " +
                                  "`emo_sn` DOUBLE DEFAULT 0, " +
                                  "`senti_sense` DOUBLE DEFAULT 0, " +
                                  "`nrc` DOUBLE DEFAULT 0, " +
                                  "foreign key (`sentiment_id`) references sentiment(`id`) " +
                                  ");";
                
                cmd.ExecuteNonQuery();
            }

            if(checkTableExists(conn, "tweet"))
                Console.WriteLine($"La tabella 'tweet' esiste nel database '{conn.Database}'.");
            else
            {
                Console.WriteLine($"La tabella 'tweet' NON esiste nel database '{conn.Database}'.");

                cmd.CommandText = "CREATE TABLE tweet (" +
                                  "`id` INT(11) PRIMARY KEY AUTO_INCREMENT," +
                                  "`text` VARCHAR(300)," +
                                  "`sentiment_id` INT(11)," +
                                  "foreign key (`sentiment_id`) references sentiment(`id`) " +
                                  ") AUTO_INCREMENT=0;";
                
                cmd.ExecuteNonQuery();
            }

            if(checkTableExists(conn, "token"))
                Console.WriteLine($"La tabella 'token' esiste nel database '{conn.Database}'.");
            else
            {
                Console.WriteLine($"La tabella 'token' NON esiste nel database '{conn.Database}'.");

                cmd.CommandText = "CREATE TABLE token (" +
                                  "`id` INT(11) PRIMARY KEY AUTO_INCREMENT," +
                                  "`token` VARCHAR(300)," +
                                  "`type` VARCHAR(20)," +
                                  "`frequency` INT(11) DEFAULT 0," +
                                  "`sentiment_id` INT(11)," +
                                  "foreign key (`sentiment_id`) references sentiment(`id`) " +
                                  ") AUTO_INCREMENT=0;";
                
                cmd.ExecuteNonQuery();
            }

            if(checkTableExists(conn, "lex_res"))
                Console.WriteLine($"La tabella 'lex_res' esiste nel database '{conn.Database}'.");
            else
            {
                Console.WriteLine($"La tabella 'lex_res' NON esiste nel database '{conn.Database}'.");

                cmd.CommandText = "CREATE TABLE lex_res (" +
                                  "`id` INT(11) PRIMARY KEY AUTO_INCREMENT," +
                                  "`lemma` VARCHAR(300)," +
                                  "`emo_sn` INT(11) DEFAULT 0," +
                                  "`senti_sense` INT(11) DEFAULT 0," +
                                  "`nrc` INT(11) DEFAULT 0," +
                                  "`sentiment_id` INT(11)," +
                                  "foreign key (`sentiment_id`) references sentiment(`id`), " +
                                  "`frequency` INT(11) DEFAULT 0," +
                                  "`AFINN` DOUBLE, " +
                                  "`ANEWARO` DOUBLE, " +
                                  "`ANEWDOM` DOUBLE, " +
                                  "`ANEWPLEAS` DOUBLE " +
                                  ") AUTO_INCREMENT=0;";
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }

        public static void UploadPostgres(Dictionary<string, Dictionary<string, double>> lemmi, string sentimento, List<TweetData> ProcessedTweets)
        {
            MySqlConnection conn = new MySqlConnection("server=localhost;user=alfredo;pwd=password;database=maadb");
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            conn.Open();
            MySqlCommand command;

            string insertSentiment = "INSERT INTO sentiment (name) " +
                                        "VALUES (@name)";
            
            command = new MySqlCommand(insertSentiment, conn);
            command.Parameters.AddWithValue("@name", sentimento);
            command.ExecuteNonQuery();
            command.Parameters.RemoveAt("@name");

            string insertLexRes = "INSERT INTO lex_res (lemma, emo_sn, senti_sense, nrc, sentiment_id, frequency, AFINN, ANEWARO, ANEWDOM, ANEWPLEAS) " +
                                        "SELECT @lemma, @emo_sn, @senti_sense, @nrc, s.id, @frequency, @AFINN, @ANEWARO, @ANEWDOM, @ANEWPLEAS "+
                                        "FROM sentiment s " +
                                        "WHERE s.name = @name";
            //string innerDictQuery = "INSERT INTO tavoletta (risorsa, value, lemma_id) VALUES (@EmoSN, @SentiSense, @NRC, @AFINN, @ANEW, @Frquency);";

            string[] chiaviDizionarioEsterno = new string[lemmi.Keys.Count];
            lemmi.Keys.CopyTo(chiaviDizionarioEsterno, 0);
            //command.ExecuteNonQuery();

            using (command = new MySqlCommand(insertLexRes, conn))
            {
                foreach (string l in chiaviDizionarioEsterno)
                {
                    command.Parameters.AddWithValue("@lemma", l);

                    // Aggiungi il valore di default 
                    command.Parameters.AddWithValue("@frequency", 0);
                    command.Parameters.AddWithValue("@emo_sn", checkPresence(l, sentimento, "EmoSN"));
                    command.Parameters.AddWithValue("@senti_sense", checkPresence(l, sentimento, "SentiSense"));
                    command.Parameters.AddWithValue("@nrc", checkPresence(l, sentimento, "NRC"));
                    command.Parameters.AddWithValue("@AFINN", getScore(l, "afinn"));
                    command.Parameters.AddWithValue("@ANEWARO", getScore(l, "anewAro"));
                    command.Parameters.AddWithValue("@ANEWDOM", getScore(l, "anewDom"));
                    command.Parameters.AddWithValue("@ANEWPLEAS", getScore(l, "anewPleas"));

                    command.Parameters.AddWithValue("@name", sentimento);

                    command.ExecuteNonQuery();

                    command.Parameters.RemoveAt("@lemma");
                    command.Parameters.RemoveAt("@frequency");
                    command.Parameters.RemoveAt("@emo_sn");
                    command.Parameters.RemoveAt("@nrc");
                    command.Parameters.RemoveAt("@senti_sense");
                    command.Parameters.RemoveAt("@AFINN");
                    command.Parameters.RemoveAt("@ANEWARO");
                    command.Parameters.RemoveAt("@ANEWDOM");
                    command.Parameters.RemoveAt("@ANEWPLEAS");
                    command.Parameters.RemoveAt("@name");
                }
            }

            string insertLexResTot = "INSERT INTO lex_res_totals (sentiment_id, emo_sn, senti_sense, nrc) " +
                                        "SELECT s.id, @emo_sn, @senti_sense, @nrc "+
                                        "FROM sentiment s " +
                                        "WHERE s.name = @name";

            string startPath = $"Risorse lessicali/{sentimento}/";
            string endPath = $"_{sentimento}.txt";

            command = new MySqlCommand(insertLexResTot, conn);

            command.Parameters.AddWithValue("@emo_sn", File.Exists(startPath + "EmoSN" + endPath) ? File.ReadAllLines(startPath + "EmoSN" + endPath).Length : 0);
            command.Parameters.AddWithValue("@senti_sense", File.Exists(startPath + "SentiSense" + endPath) ? File.ReadAllLines(startPath + "SentiSense" + endPath).Length : 0);
            command.Parameters.AddWithValue("@nrc", File.Exists(startPath + "NRC" + endPath) ? File.ReadAllLines(startPath + "NRC" + endPath).Length : 0);
            command.Parameters.AddWithValue("@name", sentimento);

            command.ExecuteNonQuery();

            command.Parameters.RemoveAt("@emo_sn");
            command.Parameters.RemoveAt("@nrc");
            command.Parameters.RemoveAt("@senti_sense");
            command.Parameters.RemoveAt("@name");

            string insertTweet = "INSERT INTO tweet (text, sentiment_id) " +
                                        "SELECT @text, s.id "+
                                        "FROM sentiment s " +
                                        "WHERE s.name = @name";

            command = new MySqlCommand(insertTweet, conn);

            string insertToken = "INSERT INTO token (token, type, frequency, sentiment_id) " +
                                        "SELECT @token, @type, @frequency, s.id "+
                                        "FROM sentiment s " +
                                        "WHERE s.name = @name";

            MySqlCommand command1 = new MySqlCommand(insertToken, conn);

            string tweetComplete = "";

            foreach (TweetData tweet in ProcessedTweets)
            {
                foreach (string lemma in tweet.Lemmi)
                    tweetComplete = string.Concat(tweetComplete, " ", lemma);
                
                command.Parameters.AddWithValue("@text", tweetComplete);
                command.Parameters.AddWithValue("@name", sentimento);

                command.ExecuteNonQuery();

                command.Parameters.RemoveAt("@text");
                command.Parameters.RemoveAt("@name");

                tweetComplete = "";

                foreach (Tokens tok in tweet.Tokens.Keys)
                    foreach (string lemma in tweet.Tokens[tok].Keys)
                    {
                        command1.Parameters.AddWithValue("@token", lemma);
                        command1.Parameters.AddWithValue("@type", Enum.GetName(typeof(Tokens), tok));
                        command1.Parameters.AddWithValue("@frequency", tweet.Tokens[tok][lemma]);
                        command1.Parameters.AddWithValue("@name", sentimento);

                        command1.ExecuteNonQuery();

                        command1.Parameters.RemoveAt("@token");
                        command1.Parameters.RemoveAt("@type");
                        command1.Parameters.RemoveAt("@frequency");
                        command1.Parameters.RemoveAt("@name");
                    }
            }

            conn.Close();
        }

        public static void DeleteDatabase()
        {
            MySqlConnection conn = new MySqlConnection("server=localhost;user=alfredo;pwd=password;database=maadb");
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            conn.Open();

            cmd.CommandText = "drop table common_words;" +
                                "drop table lex_res;" +
                                "drop table lex_res_totals;" +
                                "drop table percentages;" +
                                "drop table token;" +
                                "drop table tweet;" + 
                                "drop table sentiment;";

            cmd.ExecuteNonQuery();

            conn.Close();

            Console.WriteLine("Eliminate tutte le tabelle.");
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

        public static BsonDocument UploadTweetMongoDB(TweetData data, int count, IMongoDatabase database)
        {
            //var lemmi = LemmasToDictionary(data.Sentimento);
            var sentimento = data.Sentimento;
            var tokens = data.Tokens;

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

            Console.WriteLine("Elaborazione tokens finita" + DateTime.Now);

            var lex_res_collection = database.GetCollection<BsonDocument>("Lex_resources_words");

            var lems = new Dictionary<string, List<string>>();

            var words = new BsonArray();

            foreach (var outerKey in data.Lemmi)
            {
                var word = new BsonDocument { };
                if (count < 60)
                {
                    word = new BsonDocument{

                    {"lemma", outerKey},
                    {"pos", POS_word_tagger(outerKey)}
                        };
                }
                else
                {
                    word = new BsonDocument{

                    {"lemma", outerKey}
                    //{"pos", POS_word_tagger(outerKey)}  
                        };
                }

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

            return document;
        }

        public static Dictionary<string, Dictionary<string, double>> LemmasToDictionary(Emotions em)
        {
            Dictionary<string, Dictionary<string, double>> lemmi = new Dictionary<string, Dictionary<string, double>>(); // Creazione di un nuovo dizionario per ogni iterazione
            Dictionary<string, Dictionary<string, double>> lemmaWithScore = new Dictionary<string, Dictionary<string, double>>();

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

                            double numero = double.Parse(lineParts[1], CultureInfo.GetCultureInfo("en-US"));
                            if (!lemmaWithScore.ContainsKey(parola))
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

        public static List<TweetData> TweetProcessing(Emotions em, 
                                                    Dictionary<string, string> splittedSlagWords, 
                                                    string[] splittedEmoticons,
                                                    string[] splittedEmoji)
        {
            List<TweetData> Tweets = new List<TweetData>();
            string tweetPath = $"Twitter messaggi/dataset_dt_{em}_60k.txt";
            //string tweetPath = "Twitter messaggi/test.txt";
            foreach (string line in File.ReadLines(tweetPath))
            {
                TweetData Data = new TweetData(
                                            em,
                                            CreateTokensDictionary()
                                            );

                var tokens = Data.Tokens;

                string stopwordsText = "[,?!.;:/()& _+=<>\"]";

                string[] negWords = new string[] {"not", "rather", "hardly", "couldn\'t", "wasn\'t", "didn\'t", "wouldn\'t", "shouldn\'t",
                                                    "weren\'t", "don\'t", "doesn\'t", "haven\'t", "hasn\'t", "won\'t", "wont", "hadn\'t"};

                char[] stopwords = stopwordsText.ToCharArray();

                WhitespaceTokenizer tokenizer = WhitespaceTokenizer.INSTANCE;

                string[] tokensNLP = tokenizer.tokenize(line);

                List<string> tokensNLPList = new List<string>(tokensNLP);

                //rimozione username e url
                tokensNLPList.RemoveAll(item => item == "USERNAME" || item == "URL");

                //rimozione parole che negano
                tokensNLPList.RemoveAll(negWords.Contains);
                
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
                }

                //Pulisce tokensNLPList da stopwords and emojis
                for (int i = 0; i < tokensNLPList.Count; i++)
                {
                    string tokeNLP = tokensNLPList[i];

                    foreach (char stop in stopwords)
                    {
                        if (tokeNLP.Contains(stop))
                        {
                            tokeNLP = tokeNLP.Replace(stop.ToString(), "");
                        }
                    }

                    tokensNLPList[i] = tokeNLP;
                }
                tokensNLPList.RemoveAll(string.IsNullOrEmpty);

                for (int i = 0; i < tokensNLPList.Count; i++)
                {
                    string tokeNLP = tokensNLPList[i];

                    foreach (string emo in splittedEmoji)
                    {
                        if (StringToUTF32(tokeNLP).Contains(emo))
                        {
                            tokensNLPList[i] = "";
                        }
                    }

                }

                tokensNLPList.RemoveAll(string.IsNullOrEmpty);

                Data.Lemmi = tokensNLPList.ToArray();

                Tweets.Add(Data);

            }
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


        public static void CalcoloPercPresTwitter(Emotions em, object res)
        {
            string reso = "";
            if (res is Resources)
            {
                // Gestisci il caso in cui reso è di tipo Resources
                Resources resource = (Resources)res;
                // Effettua le operazioni specifiche per Resources
                // Ad esempio, esegui un conteggio di parole con em e resource
                reso = resource.ToString();
            }
            else if (res is ResourcesWithScore)
            {
                // Gestisci il caso in cui reso è di un altro tipo diverso da Resources
                ResourcesWithScore resource = (ResourcesWithScore)res;
                // Effettua le operazioni specifiche per AltResourceType
                // Ad esempio, esegui un conteggio di parole con em e altResource
                reso = resource.ToString();
            }
            int N_twitter_words = getTweetTotalWords(em);
            int N_shared_words = getSharedWordsCount(em, reso);


            double perc_presence_twitter = (double)N_shared_words / (double)N_twitter_words;
            Console.WriteLine(Math.Round(perc_presence_twitter * 100, 2) + " %");
            //return perc_presence_twitter;

        }

        public static void CalcoloPercPresLexs(Emotions em, object res)
        {
            string reso = "";
            if (res is Resources)
            {
                // Gestisci il caso in cui reso è di tipo Resources
                Resources resource = (Resources)res;
                // Effettua le operazioni specifiche per Resources
                // Ad esempio, esegui un conteggio di parole con em e resource
                reso = resource.ToString();
            }
            else if (res is ResourcesWithScore)
            {
                // Gestisci il caso in cui reso è di un altro tipo diverso da Resources
                ResourcesWithScore resource = (ResourcesWithScore)res;
                // Effettua le operazioni specifiche per AltResourceType
                // Ad esempio, esegui un conteggio di parole con em e altResource
                reso = resource.ToString();
            }
            int N_shared_words_unique = getUniqueLemmasInTweets(em, reso);
            int N_lex_words = getLexResTotalElements(em, reso);


            double perc_presence_lex = (double)N_shared_words_unique / (double)N_lex_words;

            Console.WriteLine(Math.Round(perc_presence_lex * 100, 2) + " %");
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

        public static void generateWordCloud(IList<string> words, IList<int> frequenze)
        {

            var wordCloud = new WordCloud(1300, 1000, useRank: true, fontColor: Color.Blue, allowVerical: true, fontname: "YouYuan");

            var image = wordCloud.Draw(words, frequenze);

            image.Save("word_cloud.png");
        }

        public static void generateWordsFrequenciesFiles(List<BsonDocument> docs)
        {
            List<string> words = new List<string>();
            List<int> frequenze = new List<int>();

            string[] commonWords = new string[]
                {
                    "the", "be", "to", "of", "and", "a", "in", "that", "have", "i",
                    "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
                    "this", "but", "his", "by", "from", "they", "we", "say", "her", "she",
                    "or", "an", "will", "my", "one", "all", "would", "there", "their", "what",
                    "so", "up", "out", "if", "about", "who", "get", "which", "go", "me",
                    "when", "make", "can", "like", "time", "no", "just", "him", "know", "take",
                    "people", "into", "year", "your", "good", "some", "could", "them", "see", "other",
                    "than", "then", "now", "look", "only", "come", "its", "over", "think", "also",
                    "back", "after", "use", "two", "how", "USERNAME","our", "work", "first", "well", "way", "is", "it's","im","i'm"
                };

            foreach (var document in docs)
            {
                string? word = document["_id"].ToString();
                int count = document["totalCount"].AsInt32;

                string lowercaseWord = word.ToLower();

                if (!commonWords.Contains(lowercaseWord))
                {
                    words.Add(word);
                    frequenze.Add(count);
                }
            }

            // Sort the words and frequencies in descending order based on frequencies
            var sortedData = words.Zip(frequenze, (w, f) => new { Word = w, Frequency = f })
                                  .OrderByDescending(item => item.Frequency)
                                  .Take(500)
                                  .ToList();

            string filePath = "Data_words.csv";

            // Create the file CSV and write the data
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var item in sortedData)
                {
                    string line = $"{item.Word};{item.Frequency};";
                    writer.WriteLine(line);
                }
            }
        }

        public static void generateEmojiFrequenciesFiles(List<BsonDocument> docs)
        {
            List<string> words = new List<string>();
            List<int> frequenze = new List<int>();

            foreach (var document in docs)
            {
                string? word = document["_id"].ToString();
                int count = document["totalCount"].AsInt32;

                string lowercaseWord = word.ToLower();

                words.Add(ConvertUnicodeToEmoji(word));
                frequenze.Add(count);
            }

            // Sort the words and frequencies in descending order based on frequencies
            var sortedData = words.Zip(frequenze, (w, f) => new { Word = w, Frequency = f })
                                  .OrderByDescending(item => item.Frequency)
                                  .Take(500)
                                  .ToList();

            string filePath = "Data_emoji.csv";

            // Create the file CSV and write the data
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var item in sortedData)
                {
                    string line = string.Format("{0},{1}", item.Frequency, item.Word);
                    writer.WriteLine(line);
                }
            }
        }

        public static void generateEmoticonsFrequenciesFiles(List<BsonDocument> docs)
        {
            List<string> words = new List<string>();
            List<int> frequenze = new List<int>();

            foreach (var document in docs)
            {
                string? word = document["_id"].ToString();
                int count = document["totalCount"].AsInt32;

                string lowercaseWord = word.ToLower();

                words.Add(word);
                frequenze.Add(count);


            }

            // Sort the words and frequencies in descending order based on frequencies
            var sortedData = words.Zip(frequenze, (w, f) => new { Word = w, Frequency = f })
                                  .OrderByDescending(item => item.Frequency)
                                  .Take(500)
                                  .ToList();

            string filePath = "Data_emoti.csv";

            // Create the file CSV and write the data
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var item in sortedData)
                {
                    string line = string.Format("{0},{1}", item.Frequency, item.Word);
                    writer.WriteLine(line);
                }
            }
        }

        public static string ConvertUnicodeToEmoji(string input)
        {
            string output = Regex.Replace(input, @"\\U([0-9A-Fa-f]{8})", match =>
            {
                string unicodeHex = match.Groups[1].Value;
                int unicodeValue = int.Parse(unicodeHex, System.Globalization.NumberStyles.HexNumber);
                string emoji = char.ConvertFromUtf32(unicodeValue);
                return $"{emoji}";
            });

            return output;
        }
    
        public static void TweetSerializer(List<TweetData> par)
        {
            // Specifica il percorso del tuo script Python
            string pythonScriptPath = "codice\\script.py";

            string json = JsonSerializer.Serialize(par);

            string filePath = "input.json";

            // Scrivere la stringa JSON in un file
            File.WriteAllText(filePath, json);

        }

        public static List<TweetData> SerializingBack()
        {
            // Specifica il percorso del tuo script Python
            string inputFilePath = "output.json"; // Sostituisci con il percorso del file JSON di output

            // Leggi il JSON dal file di output
            string jsonString = File.ReadAllText(inputFilePath);
            var data = JsonSerializer.Deserialize<List<TweetData>>(jsonString);

            return data;

        }

        public static void PrintTweetDataList(List<TweetData> tweetList)
        {
            foreach (var tweetData in tweetList)
            {
                Console.WriteLine("Lemmi:");
                foreach (var lemma in tweetData.Lemmi)
                {
                    Console.WriteLine(lemma);
                }
                Console.WriteLine("Sentimento: " + tweetData.Sentimento);
                Console.WriteLine("Tokens:");
                foreach (var tokenType in tweetData.Tokens)
                {
                    Console.WriteLine(tokenType.Key + ":");
                    foreach (var token in tokenType.Value)
                    {
                        Console.WriteLine($"{token.Key}: {token.Value}");
                    }
                }
            }
        }

        public static void RunPythonScript(string pythonScriptPath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = pythonScriptPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = psi };

            process.Start();

            process.WaitForExit();
        }
    }
}