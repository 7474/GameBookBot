using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GameBookBot
{
    public class GameRepository
    {
        //public GameRepository()
        //{

        //}

        public async Task<Game> GetGame(string name)
        {
            return (await GetGames()).FirstOrDefault(x => x.Title == name);
        }
        public async Task<IEnumerable<Game>> GetGames()
        {

            var game = JsonConvert.DeserializeObject<Game>(await ReadFile(HttpContext.Current.Server.MapPath("~/App_Data/hello.json")));
            return new List<Game>()
            {
                game
            };
        }

        public static async Task<string> ReadFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }

    public class Game
    {
        public string Title { get; set; }
        public IList<Paragraph> Paragraphs { get; set; }
        private IDictionary<string, Paragraph> _paragraphDic;
        [JsonIgnore]
        public IDictionary<string, Paragraph> ParagraphDic
        {
            get
            {
                if (_paragraphDic == null)
                {
                    _paragraphDic = Paragraphs.ToDictionary(x => x.Id);
                }
                return _paragraphDic;
            }
        }

        public Game()
        {
            Paragraphs = new List<Paragraph>();
        }
        public Game(string filePath)
            : this()
        {
        }

        public Paragraph NextParagraph(GameContext context, string message)
        {
            context.IsMessagePerseFailed = false;
            if (string.IsNullOrEmpty(context.CurrentParagraphId))
            {
                context.CurrentParagraphId = Paragraphs.First().Id;
            }
            else
            {
                var current = ParagraphDic[context.CurrentParagraphId];
                var nextParatraphId = current.GetNextParagraphId(context, message);
                if (string.IsNullOrEmpty(nextParatraphId))
                {
                    context.IsMessagePerseFailed = true;
                }
                else
                {
                    context.CurrentParagraphId = nextParatraphId;
                }
            }
            return ParagraphDic[context.CurrentParagraphId];
        }
    }

    [Serializable]
    public class GameContext
    {
        public string CurrentParagraphId { get; set; }
        public IDictionary<string, string> Data { get; set; }
        public bool IsMessagePerseFailed { get; set; }

        public GameContext()
        {
            Data = new Dictionary<string, string>();
        }
    }

    public class Paragraph
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public IList<Option> Options { get; set; }

        public string GetMessage(GameContext context)
        {
            if (IsGameOver(context))
            {
                return $"{GetFormatedText(context)}\n\nゲームオーバー！";
            }
            else
            {
                var optionsText = string.Join(Environment.NewLine, GetChoosableOptions(context).Select(x => $"- {x.Id}: {x.Text}"));
                return $"{GetFormatedText(context)}\n\n{ optionsText }";
            }
        }

        public string GetNextParagraphId(GameContext context, string message)
        {
            // XXX 自然言語的な対応
            return GetChoosableOptions(context).FirstOrDefault(x => message.StartsWith(x.Id))?.Id;
        }

        public bool IsGameOver(GameContext context)
        {
            return !GetChoosableOptions(context).Any();
        }

        private IList<Option> GetChoosableOptions(GameContext context)
        {
            // XXX 状況に応じての選択可否
            return Options;
        }

        private string GetFormatedText(GameContext context)
        {
            // XXX 変数展開などの加工
            return Text;
        }
    }

    public class Option
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }

}