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
            string[] splittedTextPos = Utils.ExtractEmoji("Risorse lessicali/posemoticons.txt");
            string[] splittedTextNeg = Utils.ExtractEmoji("Risorse lessicali/negemoticons.txt");
            string[] splittedText = splittedTextPos.Concat(splittedTextNeg).ToArray();

            //Array di dizionari dei sentimenti
            //Dictionary<sentimento, Dictionary<lemma, Dictionary<risorsa, counter>>>
            Dictionary<string, Dictionary<string, Dictionary<string, double>>> lemmiArray = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

            //funzione per riconosce gli slang, inuput solo il tweet, restituisce avetuali slangs


            foreach (Emotions em in Enum.GetValues(typeof(Emotions)))
            {

                Dictionary<string, Dictionary<string, double>> lemmi = Utils.LemmasToDictionary(lemmiArray, em);
                lemmiArray[em.ToString()] = lemmi;
                //Utils.StampaDizionario(lemmi);
                //Utils.UploadLemmiOfLexres(lemmi, em.ToString());
                //Utils.DeleteDatabase();
                Utils.UploadLemmiOfLexresMongoDB(lemmi, em.ToString());

            }

        }

    }
}