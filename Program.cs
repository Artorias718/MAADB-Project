using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Threading;


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
            
            //dizionario per gli hashtag, emoji ed emoticons
            /*
                string -> emozione
                string -> nome
                int -> conteggio
            */
            Dictionary<string, Dictionary<string, int>> elemSearch = new Dictionary<string, Dictionary<string, int>>();
            string[] searchElem = splittedEmoji.Concat(splittedEmoticons).ToArray();

            foreach (Emotions em in Enum.GetValues(typeof(Emotions)))
            {

                Dictionary<string, Dictionary<string, double>> lemmi = Utils.LemmasToDictionary(lemmiArray, em);
                lemmiArray[em.ToString()] = lemmi;
                //Utils.StampaDizionario(lemmi);
                //Utils.UploadLemmiOfLexres(lemmi, em.ToString());
                //Utils.DeleteDatabase();
                //Utils.UploadLemmiOfLexresMongoDB(lemmi, em.ToString());
                //Utils.readTwitter(em, elemSearch, searchElem);
            }
            //Console.WriteLine(Utils.CalcoloPercentuali(Resources.EmoSN, Emotions.anger) + "%");
            
            //foreach (KeyValuePair<string, Dictionary<string, int>> kv in elemSearch)
                //foreach (KeyValuePair<string, int> kv1 in kv.Value)
                    //Console.WriteLine($"EMOTIZIONE: {kv.Key}\n\t\t NOME: {kv1.Key}\n\t\t VALORE: {kv1.Value}");
        }
    }
}