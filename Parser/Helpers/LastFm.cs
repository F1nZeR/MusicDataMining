using System.IO;
using IF.Lastfm.Core.Api;

namespace Parser.Helpers
{
    public class LastFm
    {
        private static LastfmClient _client;
        public static LastfmClient Client
        {
            get
            {
                if (_client != null) return _client;

                var config = File.ReadAllText("lastfm.txt").Split(';');
                return _client = new LastfmClient(config[0], config[1]);
            }
        }
    }
}