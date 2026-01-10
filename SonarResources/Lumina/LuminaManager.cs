using DryIoc.FastExpressionCompiler.LightExpression;
using DryIoc.ImTools;
using DryIocAttributes;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Sonar.Enums;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarResources.Lumina
{
    [ExportEx]
    [SingletonReuse]
    public sealed class LuminaManager : IDisposable
    {
        private readonly Lock _lock = new();
        private readonly ImmutableList<GameData>.Builder _luminas = ImmutableList.CreateBuilder<GameData>();
        private readonly ImmutableList<LuminaEntry>.Builder _entries = ImmutableList.CreateBuilder<LuminaEntry>();
        private readonly CancellationTokenSource _cts = new();

        private static readonly List<(string LangCode, Language LuminaLanguage, SonarLanguage SonarLanguage)> s_languagePairs =
        [
            ("EN", Language.English, SonarLanguage.English),
            ("JP", Language.Japanese, SonarLanguage.Japanese),
            ("DE", Language.German, SonarLanguage.German),
            ("FR", Language.French, SonarLanguage.French),
            ("CN", Language.ChineseSimplified, SonarLanguage.ChineseSimplified),
            //("CN+", Language.ChineseTraditional, SonarLanguage.ChineseTraditional), // Not used
            ("KR", Language.Korean, SonarLanguage.Korean),
            ("TW", Language.TraditionalChinese, SonarLanguage.ChineseTraditional),
        ];

        public LuminaManager()
        {
            new Thread(this.FileHandleThread).Start();
        }

        private void FileHandleThread()
        {
            var spinWait = new SpinWait();
            while (!this._cts.IsCancellationRequested)
            {
                foreach (var lumina in this._luminas) lumina.ProcessFileHandleQueue();
                spinWait.SpinOnce();
            }
        }

        public void LoadLumina(string sqPath)
        {
            this.AddLumina(new GameData(sqPath, new()
            {
                CacheFileResources = true,
                PanicOnSheetChecksumMismatch = false,
                LoadMultithreaded = true,
            }));
        }

        public void AddLumina(GameData lumina)
        {
            var languages = GetLanguages(lumina).ToList();
            if (languages.Count == 0) return;

            lock (this._lock)
            {
                var added = false;
                foreach (var language in languages)
                {
                    var index = s_languagePairs.FindIndex(pair => pair.LuminaLanguage == language);
                    if (index is -1)
                    {
                        Console.WriteLine($"Language pair could not be found for {language}");
                        continue;
                    }
                    var languagePair = s_languagePairs[index];
                    Console.WriteLine($"Found {languagePair.LangCode} language: {languagePair.LuminaLanguage} => {languagePair.SonarLanguage}");
                    lock (this._entries) this._entries.Add(new(lumina, languagePair.LuminaLanguage, languagePair.SonarLanguage));
                    added = true;
                }
                if (added) lock (this._luminas) this._luminas.Add(lumina);
            }
        }

        public static IEnumerable<Language> GetLanguages(GameData lumina)
        {
            foreach (var language in Enum.GetValues<Language>())
            {
                var sheet = lumina.GetExcelSheet<PlaceName>(language);
                if (sheet?.Language == language) yield return language;
            }
        }

        public IEnumerable<LuminaEntry> GetAllLuminasEntries()
        {
            lock (this._lock) return this._entries.ToImmutable();
        }

        public IEnumerable<GameData> GetAllDatas()
        {
            lock (this._lock) return this._luminas.ToImmutable();
        }

        public void Clear()
        {
            lock (this._lock)
            {
                this._entries.Clear();
                this._luminas.Clear();
            }
        }

        public void Dispose()
        {
            this._cts.Cancel();
            this._cts.Dispose();
        }
    }
}
