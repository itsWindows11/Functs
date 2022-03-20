using Newtonsoft.Json;

namespace FunY.Models
{
    public class Fact
    {
        [JsonProperty("quote")]
        public string FactDesc { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        public bool IsFact = true;
    }

    public class JokeAPIFact
    {
        [JsonProperty("setup")]
        public string JokeStart { get; set; }

        [JsonProperty("delivery")]
        public string JokeAns { get; set; }

        [JsonProperty("joke")]
        public string Joke { get; set; }

        public bool IsFact = false;
    }
}
