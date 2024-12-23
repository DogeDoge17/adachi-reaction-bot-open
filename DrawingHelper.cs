using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace adachi_reaction_bot
{
    public static class DrawingHelper
    {
        public static Dictionary<AdachiExpressions, string> expressionDict = new Dictionary<AdachiExpressions, string>()
        {
            {AdachiExpressions.Neutral, "b12_1_0.png"},
            {AdachiExpressions.Happy, "b12_2_0.png"},
            {AdachiExpressions.Talking, "b12_3_0.png"},
            {AdachiExpressions.Sighing, "b12_4_0.png"},
            {AdachiExpressions.Surprised, "b12_5_0.png"},
            {AdachiExpressions.Disgusted, "b12_6_0.png"},
            {AdachiExpressions.Eyeroll, "b12_7_0.png"},
            {AdachiExpressions.CrazySmile, "b12_8_0.png"},
            {AdachiExpressions.Mad, "b12_9_0.png"},
            {AdachiExpressions.Unimpressed, "b12_10_0.png"},
            {AdachiExpressions.Laughing, "b12_11_0.png"},
            {AdachiExpressions.BlushHappy, "b12_20_0.png"},
            {AdachiExpressions.BlushSuprised, "b12_21_0.png"},
            {AdachiExpressions.BlushEyeroll, "b12_22_0.png"},
            {AdachiExpressions.ShadowHappy, "b44_1_0.png"},
            {AdachiExpressions.ShadowMad, "b44_2_0.png"},
            {AdachiExpressions.ShadowNeutral, "b12_3_0.png"},
        };

        public static Image GetRandomImage()
        {
            var files = Directory.GetFiles($"{Directory.GetCurrentDirectory()}/adachi", "*.png", SearchOption.AllDirectories);
            return  Image.FromFile(files[Random.Shared.Next(0, files.Length)]);            
        }

        public static Image CustomImage(AdachiExpressions expression)
        {
            return Image.FromFile($"{Directory.GetCurrentDirectory()}/adachi/{expressionDict[expression]}");
        }

        public static string RemoveAccents(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);

            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }


        public static Color RandomColour(byte maxVal) => Color.FromArgb(Random.Shared.Next(maxVal), Random.Shared.Next(maxVal), Random.Shared.Next(maxVal));
        public static Color CustomColour(byte r, byte g, byte b) => Color.FromArgb(r, g, b);
        public static Word CustomWord(string contents, string raw = "") 
        {
            contents = RemoveAccents(contents.ToUpper() + "!");
            return new((raw == "" ? contents:raw) + "*", DrawingHelper.RemoveAccents(contents));            
        }
    }

    public enum AdachiExpressions
    {
        Neutral,
        Happy,
        Talking,
        Sighing,
        Surprised,
        Disgusted,
        Eyeroll,
        CrazySmile,
        Mad,
        Unimpressed,
        Laughing,
        BlushHappy,
        BlushSuprised,
        BlushEyeroll,
        ShadowHappy,
        ShadowMad,
        ShadowNeutral
    }
}
