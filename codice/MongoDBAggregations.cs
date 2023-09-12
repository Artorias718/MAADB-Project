using MongoDB.Driver;
using MongoDB.Bson;
using static HelloWorld.Utils;

namespace HelloWorld
{
    public class MongoDBAggregations
    {
        public static void getHashtagFrequencies(Emotions em)
        {

            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";


            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);

            // Definizione della pipeline di aggregazione
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("id", em.ToString())),
                new BsonDocument("$unwind", "$hastags"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$hastags" },
                    { "totalCount", new BsonDocument("$sum", 1) }
                })
            };

            var result = collection.Aggregate<BsonDocument>(pipeline).ToList();

            foreach (var document in result)
            {
                string? hashtag = document["_id"].ToString();
                int count = document["totalCount"].AsInt32; ;
            }

        }

        public static void getEmojiFrequencies(Emotions em)
        {

            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";


            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);

            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("id", em.ToString())),
                new BsonDocument("$unwind", "$emoji"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$emoji" },
                    { "totalCount", new BsonDocument("$sum", 1) }
                })
            };

            var result = collection.Aggregate<BsonDocument>(pipeline).ToList();

            // Iterazione sui risultati
            foreach (var document in result)
            {
                string? emo = document["_id"].ToString();
                int count = document["totalCount"].AsInt32; ;
            }
            generateEmojiFrequenciesFiles(result);


        }

        public static void getEmoticonsFrequencies(Emotions em)
        {

            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";


            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);

            // Definizione della pipeline di aggregazione
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("id", em.ToString())),
                new BsonDocument("$unwind", "$emoticon"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$emoticon" },
                    { "totalCount", new BsonDocument("$sum", 1) }
                })
            };

            var result = collection.Aggregate<BsonDocument>(pipeline).ToList();

            // Iterazione sui risultati
            foreach (var document in result)
            {
                string? emo = document["_id"].ToString();
                int count = document["totalCount"].AsInt32; ;
                Console.WriteLine("Emoticon " + emo + "   Count " + count + "\n");

            }

            generateEmoticonsFrequenciesFiles(result);



        }

        public static void getWordsFrequencies(Emotions em)
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";


            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);

            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("sentiment", em.ToString())),
                new BsonDocument("$unwind", "$words"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$words.lemma" },
                    { "totalCount", new BsonDocument("$sum", 1) }
                })
            };

            var result = collection.Aggregate<BsonDocument>(pipeline).ToList();

            generateWordsFrequenciesFiles(result);

            //generateWordCloud(words, frequenze);
            //Console.WriteLine("Word " + word + "   Count " + count + "\n");

        }

        public static int getTweetTotalWords(Emotions em)
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";

            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);

            var pipeline = new List<BsonDocument>
        {
            BsonDocument.Parse("{ $match: { id: \"" + em.ToString() + "\" } }"),
            BsonDocument.Parse("{ $group: { _id: \"$id\", totalWords: { $sum: { $size: \"$words\" } } } }"),
            BsonDocument.Parse("{ $project: { _id: 0, id: \"$_id\", totalWords: 1 } }")
        };

            var results = collection.Aggregate<BsonDocument>(pipeline).FirstOrDefault();
            return results.GetValue("totalWords").AsInt32; ;
        }

        public static int getSharedWordsCount(Emotions em, string reso)
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";

            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);
            string res_name = reso + "_" + em.ToString();
            //Console.WriteLine("********************" + res_name);

            var pipeline = new BsonDocument[]
        {
            BsonDocument.Parse("{ $match: { id: \"" + em.ToString() + "\" } }"),
            BsonDocument.Parse("{ $unwind: \"$words\" }"),
            BsonDocument.Parse("{ $match: { \"words.lex_res_reference\": { $exists: true } } }"),
            BsonDocument.Parse("{ $lookup: { from: \"Lex_resources_words\", localField: \"words.lex_res_reference\", foreignField: \"_id\", as: \"lexResourceWord\" } }"),
            BsonDocument.Parse("{ $unwind: \"$lexResourceWord\" }"),
            BsonDocument.Parse("{ $unwind: \"$lexResourceWord.Lex_reosurces\" }"),
            BsonDocument.Parse("{ $group: { _id: \"$lexResourceWord.Lex_reosurces\", count: { $sum: 1 } } }"),
            BsonDocument.Parse("{ $match: { _id: \"" + res_name + "\" } }") // Filtra per il nome della risorsa cercata
        };

            var results = collection.Aggregate<BsonDocument>(pipeline).FirstOrDefault();
            //Console.WriteLine(results.GetValue("count").AsInt32);

            return results.GetValue("count").AsInt32;
        }

        public static int getUniqueLemmasInTweets(Emotions em, string reso)
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";

            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);
            string res_name = reso + "_" + em.ToString();
            //Console.WriteLine("********************" + res_name);

            var pipeline = new BsonDocument[]
           {
            new BsonDocument("$match", new BsonDocument("id", em.ToString())),
            new BsonDocument("$unwind", "$words"),
            new BsonDocument("$match", new BsonDocument("words.lex_res_reference", new BsonDocument("$exists", true))),
             new BsonDocument("$lookup", new BsonDocument
             {
                 { "from", "Lex_resources_words" },
                 { "localField", "words.lex_res_reference" },
                 { "foreignField", "_id" },
                 { "as", "lexResourceWord" }
             }),
            new BsonDocument("$unwind", "$lexResourceWord"),
            new BsonDocument("$match", new BsonDocument("lexResourceWord.Lex_reosurces", res_name)),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "totalLemmas", new BsonDocument("$sum", 1) },
                { "uniqueLemmas", new BsonDocument("$addToSet", "$words.lemma") }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "totalLemmas", 1 },
                { "uniqueLemmasCount", new BsonDocument("$size", "$uniqueLemmas") }
            })
           };

            var aggregateOptions = new AggregateOptions { AllowDiskUse = true };
            var results = collection.Aggregate<BsonDocument>(pipeline, aggregateOptions).ToList().FirstOrDefault();

            if (results != null)
            {
                return results.GetValue("uniqueLemmasCount").AsInt32;
            }
            else
            {
                return 0;
            }
        }

        public static int getLexResTotalElements(Emotions em, string reso)
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "LexResources";

            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);
            string res_name = reso + "_" + em.ToString();

            var pipeline = new BsonDocument[]
{
            new BsonDocument("$match", new BsonDocument
            {
                { "_id", res_name }
            })
};

            var aggregateOptions = new AggregateOptions { AllowDiskUse = true };
            var results = collection.Aggregate<BsonDocument>(pipeline, aggregateOptions).FirstOrDefault();

            return results.GetValue("num_words").AsInt32;
        }

        public static void getNewResources(Emotions em)
        {

            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);

            string databaseName = "Twitter";
            string collectionName = "Tweet";


            IMongoDatabase database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);
            var pipeline = new List<BsonDocument>
                {
                    new BsonDocument
                    {
                        { "$match", new BsonDocument("id", em.ToString()) }
                    },
                    new BsonDocument("$unwind", "$words"),
                    new BsonDocument
                    {
                        { "$match", new BsonDocument("words.lex_res_reference", new BsonDocument("$exists", false)) }
                    },
                    new BsonDocument
                    {
                        { "$group", new BsonDocument
                            {
                                { "_id", "$words.lemma" },
                                { "frequenza", new BsonDocument("$sum", 1) }
                            }
                        }
                    },
                    new BsonDocument
                    {
                        { "$match", new BsonDocument("frequenza", new BsonDocument("$gt", 800)) }
                    },
                    new BsonDocument
                    {
                        { "$project", new BsonDocument
                            {
                                { "_id", 0 },
                                { "parola", "$_id" },
                                { "frequenza", 1 }
                            }
                        }
                    }
                };

            var result = collection.Aggregate<BsonDocument>(pipeline).ToList();

            string nomeFile = $"nuova_risorsa_{em.ToString()}.txt";

            // Creazione del file e scrittura dei risultati
            using (StreamWriter writer = new StreamWriter(nomeFile))
            {
                foreach (var document in result)
                {
                    string parola = document.GetValue("parola").AsString;
                    int frequenza = document.GetValue("frequenza").AsInt32;
                    writer.WriteLine($"{parola}, {frequenza}");
                }
            }

            Console.WriteLine("Risultato salvato in nuova_risorsa.txt");
        }

    }
}
