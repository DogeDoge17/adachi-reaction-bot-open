using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adachi_reaction_bot
{
    public class Language
    {
        public string code = "";
        public string[] words = { };
        public double chance = 100;
        int wordCount { get { return words.Length; } }

        public Language(string path, string code, double chance)
        {
            this.chance = chance;
            this.code = code;

            words = File.OpenText(path).ReadToEnd().Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).Select(wr => wr.Trim()).ToArray();
        }

        public bool RollChance()
        {
            if (chance < 0.0 || chance > 100.0)
                throw new ArgumentOutOfRangeException(nameof(chance), "Percentage must be between 0.0 and 100");

            double randomNumber = Random.Shared.NextDouble() * 100.0;
            return randomNumber < chance;
        }

        public static Language RollChances(Language[] langs)
        {
            for (int i = 0; i < langs.Length; i++)
            {
                if (langs[i].RollChance())
                    return langs[i];            
            }

            return langs[0];
        }

        public Word GetWord()
        {
            Word williamRobinson;
            williamRobinson.raw = words[Random.Shared.Next(0, words.Length)];
            williamRobinson.formatted = DrawingHelper.RemoveAccents(williamRobinson.raw);
            return williamRobinson;
        }
    }

    public struct Word
    {
        public string raw = "";
        public string formatted = "";

        public Word(string raw, string formatted)
        {
            this.raw = raw;
            this.formatted = formatted;
        }

        public Word() { }
    }

}
