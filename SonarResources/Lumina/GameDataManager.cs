using DryIocAttributes;
using Lumina;
using Lumina.Data;
using Lumina.Excel.Sheets;
using Sonar.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SonarResources.Lumina
{
    [ExportEx]
    [SingletonReuse]
    public sealed class GameDataManager : IDisposable, IAsyncDisposable
    {
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

        private readonly CancellationTokenSource _cts = new();
        private ImmutableList<GameData> _datas = [];
        private ImmutableList<GameDataEntry> _entries = [];

        public GameDataManager()
        {
            new Thread(this.FileHandleThread).Start();
        }

        public IEnumerable<GameDataEntry> Entries => this._entries;
        public IEnumerable<GameData> Datas => this._datas;

        private void FileHandleThread()
        {
            var spinWait = new SpinWait();
            while (!this._cts.IsCancellationRequested)
            {
                foreach (var data in this._datas) data.ProcessFileHandleQueue();
                spinWait.SpinOnce();
            }
        }

        public void Add(GameData data)
        {
            var languages = GetLanguages(data).ToList();
            if (languages.Count == 0) return;

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

                var entry = new GameDataEntry(data, languagePair.LuminaLanguage, languagePair.SonarLanguage);
                if (ImmutableInterlocked.Update(ref this._entries, static (entries, entry) => entries.Add(entry), entry))
                {
                    Console.WriteLine($"Added {languagePair.LangCode} language for {data.DataPath.FullName}: {languagePair.LuminaLanguage} => {languagePair.SonarLanguage}");
                    added = true;
                }
            }
            if (added) ImmutableInterlocked.Update(ref this._datas, (datas, data) => datas.Add(data), data);
        }

        private static IEnumerable<Language> GetLanguages(GameData lumina)
        {
            foreach (var language in Enum.GetValues<Language>())
            {
                var sheet = lumina.GetExcelSheet<PlaceName>(language);
                if (sheet?.Language == language) yield return language;
            }
        }

        public void Clear()
        {
            this._entries = [];
            this._datas = [];
        }

        public void Dispose()
        {
            this._cts.Cancel();
            this._cts.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await this._cts.CancelAsync().ConfigureAwait(false);
            this._cts.Dispose();
        }
    }
}
