using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Threading;
using opennlp.tools.tokenize;
using opennlp.tools.postag;
using opennlp.tools.util;
using System.Reflection;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] splittedEmoticonPos = Utils.ExtractEmoticons("Risorse lessicali/posemoticons.txt");
            string[] splittedEmoticonNeg = Utils.ExtractEmoticons("Risorse lessicali/negemoticons.txt");
            string[] splittedEmoticons = splittedEmoticonPos.Concat(splittedEmoticonNeg).ToArray();

            Dictionary<string, string> splittedSlagWords = Utils.ExtractSlagWords("Risorse lessicali/slag_words.txt");
            //Dictionary<string, string> slagWords = Utils.swapSlagWords("Risorse lessicali/slag_words.txt");

            string[] splittedEmojiNeg = Utils.ExtractEmoji("Risorse lessicali/EmojiNeg.txt");
            string[] splittedEmojiPos = Utils.ExtractEmoji("Risorse lessicali/EmojiPos.txt");
            string[] splittedOthersEmoji = Utils.ExtractEmoji("Risorse lessicali/OthersEmoji.txt");
            string[] splittedAdditionalEmoji = Utils.ExtractEmoji("Risorse lessicali/AdditionalEmoji.txt");

            string[] splittedEmoji = splittedEmojiNeg.Concat(splittedEmojiPos).Concat(splittedOthersEmoji).Concat(splittedAdditionalEmoji).ToArray();

            //Array di dizionari dei sentimenti
            //Dictionary<sentimento, Dictionary<lemma, Dictionary<risorsa, counter>>>
            Dictionary<string, Dictionary<string, Dictionary<string, double>>> lemmiArray = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

            /* Dictionary<Tokens, Dictionary<string, int>> tokens = new Dictionary<Tokens, Dictionary<string, int>>()
             {
                 { Tokens.hashtag, new Dictionary<string, int>() },
                 { Tokens.emoticon, new Dictionary<string, int>() },
                 { Tokens.emoji, new Dictionary<string, int>() }
             };*/

            Dictionary<Emotions, Dictionary<Tokens, Dictionary<string, int>>> tokensArray = new Dictionary<Emotions, Dictionary<Tokens, Dictionary<string, int>>>();

            Dictionary<Emotions, Dictionary<string, int>> lemmaFrequencies = new Dictionary<Emotions, Dictionary<string, int>>();
            Dictionary<string, string> postags = new Dictionary<string, string>();

            //string text = File.ReadAllText("Twitter messaggi/dataset_dt_anger_60k.txt");


            Emotions em = Emotions.anticipation;
            //foreach (Emotions em in Enum.GetValues(typeof(Emotions)))
            //{
            Dictionary<string, Dictionary<string, double>> lemmi = Utils.LemmasToDictionary(em);
            Dictionary<Tokens, Dictionary<string, int>> tokens = new Dictionary<Tokens, Dictionary<string, int>>()
            {
                { Tokens.hashtag, new Dictionary<string, int>() },
                { Tokens.emoticon, new Dictionary<string, int>() },
                { Tokens.emoji, new Dictionary<string, int>() }
            };
            lemmiArray[em.ToString()] = lemmi;
            //Utils.StampaDizionario(lemmi);
            //Utils.UploadLemmiOfLexres(lemmi, em.ToString());
            //Utils.DeleteDatabase();
            //Utils.readTwitter(em, tokens, splittedEmoji, splittedEmoticons);
            Utils.TweetProcessing(em, tokens, splittedEmoji, splittedEmoticons, lemmaFrequencies, lemmi, splittedSlagWords, postags);
            tokensArray[em] = tokens;
            //Utils.UploadLemmiOfLexresMongoDB(lemmi, em.ToString(), lemmiArray, tokens);
            //}


            foreach (var kvp in tokensArray)
            {
                Emotions emotion = kvp.Key;
                Dictionary<Tokens, Dictionary<string, int>> innerDictionary = kvp.Value;

                //Console.WriteLine("Emotion: " + emotion);

                foreach (var innerKvp in innerDictionary)
                {
                    Tokens token = innerKvp.Key;
                    Dictionary<string, int> valueDictionary = innerKvp.Value;

                    //Console.WriteLine("Token: " + token);

                    foreach (var valueKvp in valueDictionary)
                    {
                        string key = valueKvp.Key;
                        int value = valueKvp.Value;

                        Console.WriteLine("Emotion: " + emotion + "Token: " + token + "Key: " + key + ", Value: " + value);
                    }
                }
            }



            //Dictionary<string, Dictionary<string, double>> lemmiw = new Dictionary<string, Dictionary<string, double>>();

            //Utils.TweetProcessing(Emotions.anger, tokens, splittedEmoji, splittedEmoticons, lemmaFrequencies, lemmiw, splittedSlagWords);

            //Console.WriteLine(Utils.CalcoloPercentuali(Resources.EmoSN, Emotions.anger) + "%");

            /*foreach (KeyValuePair<Tokens, Dictionary<string, int>> tokenEntry in tokens)
            {

                foreach (KeyValuePair<string, int> innerEntry in tokenEntry.Value)
                {
                    Console.WriteLine("Token: " + tokenEntry.Key + "Chiave: " + innerEntry.Key + ", Valore: " + innerEntry.Value);
                }
            }*/

        }
    }
}