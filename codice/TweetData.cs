using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloWorld
{
    public class TweetData
    {
        public string[] Lemmi { get; set; }
        public Emotions Sentimento { get; set; }
        public Dictionary<Tokens, Dictionary<string, int>> Tokens { get; set; }


        public TweetData(
        Emotions sentimento,
        Dictionary<Tokens, Dictionary<string, int>> tokens)
        {
            Sentimento = sentimento;
            Tokens = tokens;
        }
    }
}