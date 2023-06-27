using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloWorld
{
    public class SentimentData
    {
        public Dictionary<string, Dictionary<string, double>> Lemmi { get; set; }
        public string Sentimento { get; set; }
        public Dictionary<string, Dictionary<string, double>> LemmiArray { get; set; }
        public Dictionary<Tokens, Dictionary<string, int>> Tokens { get; set; }
        public Dictionary<string, int> LemmaFrequencies { get; set; }


        public SentimentData(
        Dictionary<string, Dictionary<string, double>> lemmi,
        string sentimento,
        Dictionary<string, Dictionary<string, double>> lemmiArray,
        Dictionary<Tokens, Dictionary<string, int>> tokens,
        Dictionary<string, int> lemmaFrequencies)
        {
            Lemmi = lemmi;
            Sentimento = sentimento;
            LemmiArray = lemmiArray;
            Tokens = tokens;
            LemmaFrequencies = lemmaFrequencies;
        }
    }
}