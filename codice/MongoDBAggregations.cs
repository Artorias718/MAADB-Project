using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            // Esecuzione dell'aggregazione
            // Esecuzione dell'aggregazione
            var result = collection.Aggregate<BsonDocument>(pipeline).ToList();

            // Iterazione sui risultati
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

            // Definizione della pipeline di aggregazione
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

            // Esecuzione dell'aggregazione
            // Esecuzione dell'aggregazione
            var result = collection.Aggregate<BsonDocument>(pipeline).ToList();

            // Iterazione sui risultati
            foreach (var document in result)
            {
                string? emo = document["_id"].ToString();
                int count = document["totalCount"].AsInt32; ;
                Console.WriteLine("Emoji " + emo + "   Count " + count + "\n");

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

            // Definizione della pipeline di aggregazione
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("id", em.ToString())),
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

        //TODO 
        //generare le words clouds e gli istogrammi di conseguenza
        //ricare tutto sul relazionale
        //calcoli percentuali per le statsistiche di uso delle risorse lessicali
        //predisporre la creazione delle nuove risorse
        //presentazione
    }
}