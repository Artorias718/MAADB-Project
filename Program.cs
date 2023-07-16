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

            string[] splittedEmoji = splittedEmojiNeg.Concat(splittedEmojiPos).Concat(splittedOthersEmoji).Concat(splittedAdditionalEmoji).ToArray();

            Dictionary<string, Dictionary<string, Dictionary<string, double>>> lemmiArray = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

            Dictionary<Emotions, Dictionary<Tokens, Dictionary<string, int>>> tokensArray = new Dictionary<Emotions, Dictionary<Tokens, Dictionary<string, int>>>();

            Dictionary<Emotions, Dictionary<string, int>> lemmaFrequencies = new Dictionary<Emotions, Dictionary<string, int>>();
            Dictionary<string, string> postags = new Dictionary<string, string>();


            //Emotions em = Emotions.anger;
            foreach (Emotions em in Enum.GetValues(typeof(Emotions)))
            {
                // TweetData Data = new TweetData(
                //     LemmasToDictionary(em),
                //     em.ToString(),
                //     LemmasToDictionary(em),
                //     CreateTokensDictionary(),
                //     new Dictionary<string, int>()
                // );

                //Utils.StampaDizionario(lemmi);
                //Utils.UploadPostgres(lemmi, em.ToString());
                //Utils.DeleteDatabase();

                UploadLexResourcesMongoDB(em);

                UploadLexResourcesWordsMongoDB(em);
                //TODO 
                //cambiare il nome della risorsa con la referenza a LexResource
                //Aggiungere la seconda referenza in Tweet
                //Riprovare le aggregations

                List<TweetData> ProcessedTweets = TweetProcessing(em, splittedSlagWords, splittedEmoticons, splittedEmoji);

                foreach (TweetData tweet in ProcessedTweets)
                {
                    UploadTweetMongoDB(tweet);
                }



                //lemmiArray[em.ToString()] = Data.Lemmi;
                //tokensArray[em] = Data.Tokens;
            }

        }
    }
}