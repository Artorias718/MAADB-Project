using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Threading;
using opennlp.tools.tokenize;




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

            Dictionary<Tokens, Dictionary<string, int>> tokens = new Dictionary<Tokens, Dictionary<string, int>>()
            {
                { Tokens.hashtag, new Dictionary<string, int>() },
                { Tokens.emoticon, new Dictionary<string, int>() },
                { Tokens.emoji, new Dictionary<string, int>() }
            };

            Dictionary<Emotions, Dictionary<string, int>> lemmaFrequencies = new Dictionary<Emotions, Dictionary<string, int>>();
            //TODO
            //aggiungere la frequenza nel db relazionale
            //continuare elaborazione testo
            string text = "Questo è un esempio di testo da tokenizzare.";

            SimpleTokenizer tokenizer = SimpleTokenizer.INSTANCE;

            String[] tokens2 = tokenizer.tokenize(text);

            //Printing the tokens 
            foreach (string token in tokens2)
            {
                Console.WriteLine(token);
            }

            foreach (Emotions em in Enum.GetValues(typeof(Emotions)))
            {

                Dictionary<string, Dictionary<string, double>> lemmi = Utils.LemmasToDictionary(em);
                lemmiArray[em.ToString()] = lemmi;
                //Utils.StampaDizionario(lemmi);
                //Utils.UploadLemmiOfLexres(lemmi, em.ToString());
                //Utils.DeleteDatabase();
                //Utils.UploadLemmiOfLexresMongoDB(lemmi, em.ToString());
                //Utils.readTwitter(em, tokens, splittedEmoji, splittedEmoticons);
                //Utils.readTwitter(Emotions.anger, tokens, splittedEmoji, splittedEmoticons, lemmaFrequencies, lemmi);

            }
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