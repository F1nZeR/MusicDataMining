using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Parser
{
    public class LyricsEngine
    {
        //Substring method, but with starting index and ending index too.
        private static string Slice(string source, int start, int end)
        {
            if (end < 0)
            {
                end = source.Length + end;
            }
            var len = end - start;
            return source.Substring(start, len);
        }

        //Method replaces first letter of all words to UPPERCASE and replaces all spaces with underscores.
        private static string Sanitize(string s)
        {
            var array = s.Trim().ToCharArray();
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            for (var i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array).Trim().Replace(' ', '_');
        }

        public static string PullLyrics(string strArtist, string strSongTitle, bool isAgain = false)
        {
            string sLyrics;
            try
            {
                if (isAgain)
                {
                    var regex = new Regex(Regex.Escape("(") + ".*" + Regex.Escape(")"));
                    strSongTitle = regex.Replace(strSongTitle, string.Empty).Trim();
                }

                var wc = new WebClient();
                var sUrl = @"http://lyrics.wikia.com/index.php?title=" + Sanitize(strArtist) + ":" + Sanitize(strSongTitle) +
                              "&action=edit";
                //Set encoding to UTF8 to handle accented characters.
                wc.Encoding = Encoding.UTF8;
                sLyrics = wc.DownloadString(sUrl);
                //Get surrounding tags.
                var iStart = sLyrics.IndexOf("&lt;lyrics>") + 12;
                var iEnd = sLyrics.IndexOf("&lt;/lyrics>") - 1;
                //Replace webpage standard newline feed with carriage return + newline feed, which is standard on Windows.
                sLyrics = Slice(sLyrics, iStart, iEnd).Replace("n", Environment.NewLine).TrimEnd();
                //If Lyrics Wikia is suggesting a redirect, pull lyrics for that.
                if (sLyrics.Contains("#REDIRECT"))
                {
                    iStart = sLyrics.IndexOf("#REDIRECT [[") + 12;
                    iEnd = sLyrics.IndexOf("]]", iStart);
                    strArtist = Slice(sLyrics, iStart, iEnd).Split(':')[0];
                    strSongTitle = Slice(sLyrics, iStart, iEnd).Split(':')[1];
                    PullLyrics(strArtist, strSongTitle);
                }
                //If lyrics weren't found 
                else if (sLyrics.Contains("&lt;!-- PUT LYRICS HERE (a\r\nd delete this e\r\ntire li\r\ne) -->"))
                {
                    sLyrics = null;
                }

                if (sLyrics != null)
                {
                    var regex = new Regex("\r\n");
                    sLyrics = regex.Replace(sLyrics, "n").Replace("\n", " ");
                }
            }
            catch (Exception)
            {
                sLyrics = null;
            }

            if (sLyrics == null && !isAgain)
            {
                return PullLyrics(strArtist, strSongTitle, true);
            }

            return sLyrics;
        }
    }
}