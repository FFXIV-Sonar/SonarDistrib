using Lumina;
using Lumina.Data;
using Lumina.Excel.Sheets;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SonarResources
{
    public static class LuminaExtensions
    {
        private static readonly Regex s_sheetMacroRegex = new(@"^<sheet\((?<sheetName>.+?),(?<rowId>\d+?),(?<num2>\d+?)\)>$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string ExtractTextWithSheets(this ReadOnlySeString self, GameData data, Language language) => ExtractTextWithSheets(self.AsSpan(), data, language);

        /// <summary>Extracts the text contained in this instance of <see cref="ReadOnlySeStringSpan"/>, ignoring any payload that does not have a direct equivalent string
        /// representation.</summary>
        /// <returns>The extracted text.</returns>
        public static string ExtractTextWithSheets(this ReadOnlySeStringSpan self, GameData data, Language language)
        {
            var macroExpansions = new string?[self.PayloadCount];
            var len = 0;
            var index = 0;
            foreach (var v in self)
            {
                switch (v.Type)
                {
                    case ReadOnlySePayloadType.Text:
                        len += Encoding.UTF8.GetCharCount(v.Body);
                        break;
                    case ReadOnlySePayloadType.Macro:
                        {
                            switch (v.MacroCode)
                            {
                                case MacroCode.NewLine:
                                    len += Environment.NewLine.Length;
                                    break;
                                case MacroCode.NonBreakingSpace:
                                case MacroCode.Hyphen:
                                case MacroCode.SoftHyphen:
                                    len += 1;
                                    break;
                                case MacroCode.Sheet:
                                    var expansion = ExtractMacroText(v, data, language);
                                    if (expansion is not null)
                                    {
                                        len += expansion.Length;
                                        macroExpansions[index] = expansion;
                                    }
                                    break;
                            }

                            break;
                        }
                }
                index++;
            }

            var buf = new char[len];
            var bufspan = buf.AsSpan();
            index = 0;
            foreach (var v in self)
            {
                switch (v.Type)
                {
                    case ReadOnlySePayloadType.Text:
                        bufspan = bufspan[Encoding.UTF8.GetChars(v.Body, bufspan)..];
                        break;
                    case ReadOnlySePayloadType.Macro:
                        {
                            switch (v.MacroCode)
                            {
                                case MacroCode.NewLine:
                                    Environment.NewLine.CopyTo(bufspan);
                                    bufspan = bufspan[Environment.NewLine.Length..];
                                    break;

                                case MacroCode.NonBreakingSpace:
                                    bufspan[0] = '\u00A0';
                                    bufspan = bufspan[1..];
                                    break;

                                case MacroCode.Hyphen:
                                    bufspan[0] = '-';
                                    bufspan = bufspan[1..];
                                    break;

                                case MacroCode.SoftHyphen:
                                    bufspan[0] = '\u00AD';
                                    bufspan = bufspan[1..];
                                    break;

                                case MacroCode.Sheet:
                                    var expansion = macroExpansions[index];
                                    if (expansion is not null)
                                    {
                                        expansion.CopyTo(bufspan);
                                        bufspan = bufspan[expansion.Length..];
                                    }
                                    break;
                            }

                            break;
                        }
                }
                index++;
            }

            return new(buf);
        }

        /// <summary>Not the best way to do this</summary>
        private static string? ExtractMacroText(ReadOnlySePayloadSpan v, GameData data, Language language)
        {
            var str = v.ToString(); // NOTE: This variable has no usage other than letting me debug what string it got
            var result = s_sheetMacroRegex.Match(str);
            if (result.Success)
            {
                var sheetName = result.Groups["sheetName"].ValueSpan;
                var rowId = uint.Parse(result.Groups["rowId"].ValueSpan);
                var num2 = uint.Parse(result.Groups["num2"].ValueSpan); // I don't know what this is yet but lets expect an uint
                try
                {
                    switch (sheetName)
                    {
                        case "BNpcName":
                            {
                                var sheet = data.GetExcelSheet<BNpcName>(language)!;
                                return ExtractTextWithSheets(sheet.GetRow(rowId).Singular, data, language);
                            }
                        case "EventItem":
                            {
                                var sheet = data.GetExcelSheet<EventItem>(language)!;
                                return ExtractTextWithSheets(sheet.GetRow(rowId).Name, data, language);
                            }
                        default:
                            if (Debugger.IsAttached) Debugger.Break();
                            break;
                    }
                }
                catch (Exception ex) // NOTE: Do not remove, it lets me see the exception thrown while debugging
                {
                    /* Swallow, cannot parse */
                    if (Debugger.IsAttached) Debugger.Break();
                }
            }
            else
            {
                if (Debugger.IsAttached) Debugger.Break();
            }

            Console.WriteLine($"Cannot parse {v.ToString()}"); // Inform me
            return null;
        }
    }
}
