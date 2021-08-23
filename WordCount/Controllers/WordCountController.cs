using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WordCount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WordCountController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<WordCount>> GetWordCountsAsync([FromForm] IFormFile file)
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                var text = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
                var matches = Regex
                    .Matches(text, @"\b[\w']+-?[\w']*\b")
                    .Cast<Match>()
                    .Select(m => Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(m.Value));

                var wordCounts = new Dictionary<string, int>();
                foreach (var word in matches)
                {
                    if (wordCounts.ContainsKey(word))
                        wordCounts[word]++;
                    else
                        wordCounts.Add(word, 1);
                }

                var topWords = wordCounts.OrderByDescending(key => key.Value).Take(10);

                return topWords.Select(kv => new WordCount(kv.Key, kv.Value)).ToArray();
            }
        }
    }
}
