using Microsoft.VisualBasic.CompilerServices;
using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Threading;
using opennlp.tools.tokenize;
using opennlp.tools.postag;
using opennlp.tools.util;
using System.Reflection;
using static HelloWorld.Utils;
using static HelloWorld.MongoDBAggregations;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] splittedEmoticonPos = ExtractEmoticons("Risorse lessicali/posemoticons.txt");
            string[] splittedEmoticonNeg = ExtractEmoticons("Risorse lessicali/negemoticons.txt");
            string[] splittedEmoticons = splittedEmoticonPos.Concat(splittedEmoticonNeg).ToArray();

            Dictionary<string, string> splittedSlagWords = ExtractSlagWords("Risorse lessicali/slag_words.txt");
            //Dictionary<string, string> slagWords = Utils.swapSlagWords("Risorse lessicali/slag_words.txt");

            string[] splittedEmojiNeg = ExtractEmoji("Risorse lessicali/EmojiNeg.txt");
            string[] splittedEmojiPos = ExtractEmoji("Risorse lessicali/EmojiPos.txt");
            string[] splittedOthersEmoji = ExtractEmoji("Risorse lessicali/OthersEmoji.txt");
            string[] splittedAdditionalEmoji = ExtractEmoji("Risorse lessicali/AdditionalEmoji.txt");

            string[] splittedEmoji = splittedEmojiNeg.Concat(splittedEmojiPos).Concat(splittedOthersEmoji).Concat(splittedAdditionalEmoji).Distinct().ToArray();

            Dictionary<string, Dictionary<string, Dictionary<string, double>>> lemmiArray = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

            Dictionary<Emotions, Dictionary<Tokens, Dictionary<string, int>>> tokensArray = new Dictionary<Emotions, Dictionary<Tokens, Dictionary<string, int>>>();

            Dictionary<Emotions, Dictionary<string, int>> lemmaFrequencies = new Dictionary<Emotions, Dictionary<string, int>>();
            Dictionary<string, string> postags = new Dictionary<string, string>();

            //Emotions em = Emotions.anger;
            foreach (Emotions em in Enum.GetValues(typeof(Emotions)))
            {
                List<TweetData> ProcessedTweets = TweetProcessing(em, splittedSlagWords, splittedEmoticons, splittedEmoji);
                int count = 1;
                //var lemmi = LemmasToDictionary(em);

                //Utils.createTablesPostgres();
                //Utils.UploadPostgres(lemmi, em.ToString(), ProcessedTweets);

                //Utils.DeleteDatabase();

                //********************************************  Funzioni per il caricamento risorse lessicali su mongoDB  

                //UploadLexResourcesMongoDB(em);
                //UploadLexResourcesWordsMongoDB(em);

                //********************************************  Funzioni NLP 
                //Utils.TweetSerializer(ProcessedTweets);
                //RunPythonScript("codice\\script.py");

                //ProcessedTweets = Utils.SerializingBack();

                // //********************************************  Funzioni per il caricamento tweets su mongoDB    

                // List<BsonDocument> documentsToInsert = new List<BsonDocument>();

                // string connectionString = "mongodb://localhost:27017";
                // MongoClient client = new MongoClient(connectionString);

                // string databaseName = "Twitter";
                // string collectionName = "Tweet";

                // IMongoDatabase database = client.GetDatabase(databaseName);

                // var collection = database.GetCollection<BsonDocument>(collectionName);

                // foreach (TweetData tweet in ProcessedTweets)
                // {

                //     //tweet.Lemmi = Utils2.Lemming(tweet.Lemmi);

                //     BsonDocument d = UploadTweetMongoDB(tweet, count, database);
                //     documentsToInsert.Add(d);
                //     Console.WriteLine("\nDocumenti elaborati" + count);

                //     count++;
                // }

                // collection.InsertMany(documentsToInsert);

                //***********************************************   Funzioni per il caricamento su MySql 

                //UploadPostgres(lemmi, em.ToString()); 

                //***********************************************   Funzioni per il calcllo dei dati delle words clouds 

                // getHashtagFrequencies(em);
                // getEmojiFrequencies(em);
                // getEmoticonsFrequencies(em);
                // getWordsFrequencies(em);

                //*********************************************** Funzioni per il calcolo percentuali per istogrammi 

                CalcoloPercPresLexs(Emotions.anger, Resources.EmoSN);
                CalcoloPercPresLexs(Emotions.anger, Resources.sentisense);
                CalcoloPercPresLexs(Emotions.anger, Resources.NRC);

                //CalcoloPercentuale(Emotions.joy, Resources.EmoSN);

                //Utils2.Lemming("cats");
            }
        }
    }
}