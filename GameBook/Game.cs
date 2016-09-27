using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBook
{
    public class Game
    {
        public IDictionary<string, Paragraph> Paragraphs { get; set; }

        public Paragraph NextParagraph(GameContext context)
        {
            // XXX
            return Paragraphs.First().Value;
        }
    }

    public class GameContext
    {
        public string CurrentParagraphId { get; set; }
        public IDictionary<string, string> Data { get; set; }

        public GameContext()
        {
            Data = new Dictionary<string, string>();
        }
    }

    public class Paragraph
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public IList<Option> NextParagraphs { get; set; }

        private string GetMessage(GameContext context)
        {
            return Text;
        }

        private IList<Option> GetChoosableOptions(GameContext context)
        {
            return NextParagraphs;
        }

        public bool IsGameOver(GameContext context)
        {
            return !GetChoosableOptions(context).Any();
        }
    }

    public class Option
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }
}
