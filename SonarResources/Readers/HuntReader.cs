using DryIocAttributes;
using Humanizer;
using Lumina;
using Lumina.Excel.GeneratedSheets2;
using Sonar.Data.Details;
using Sonar.Data.Rows;
using Sonar.Enums;
using SonarResources.Lumina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sonar;

namespace SonarResources.Readers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class HuntReader
    {
        private LuminaManager Luminas { get; }
        private SonarDb Db { get; }
        
        public HuntReader(LuminaManager luminas, SonarDb db, ZoneReader _)
        {
            this.Luminas = luminas;
            this.Db = db;

            Console.WriteLine("Reading all hunts");
            foreach (var entry in this.Luminas.GetAllLuminasEntries())
            {
                Program.WriteProgress(this.Read(entry) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this.Db.Hunts.Count})");

            Console.WriteLine("Setting hunt timers");
            this.SetAllHuntTimers();

            Console.WriteLine("Setting hunt zones");
            this.SetAllHuntZones();
        }

        private bool Read(LuminaEntry lumina)
        {
            var nmSheet = lumina.Data.GetExcelSheet<NotoriousMonster>(lumina.LuminaLanguage)?
                .Where(nm => nm.BNpcName.Row != 0 && nm.BNpcBase.Row != 0);
            if (nmSheet is null) return false;

            var bNpcName = lumina.Data.GetExcelSheet<BNpcName>(lumina.LuminaLanguage);
            if (bNpcName is null) return false;

            var result = false;
            foreach (var nm in nmSheet)
            {
                var nmId = nm.RowId;
                var rank = GetHuntRank(nm);
                var expansion = GetHuntExpansion(nmId);
                var level = GetHuntLevel(nmId);

                var id = nm.BNpcName.Row;
                if (!this.Db.Hunts.TryGetValue(id, out var hunt))
                {
                    this.Db.Hunts[id] = hunt = new()
                    {
                        Id = id,
                        Rank = rank,
                        Expansion = expansion,
                        Level = level,
                        SpawnTimers = GetDefaultSpawnTimers(rank),
                    };
                }

                if (!hunt.Name.ContainsKey(lumina.SonarLanguage))
                {
                    var name = bNpcName.GetRow(nm.BNpcName.Row)?.Singular?.ToTextString()?.Transform(To.TitleCase);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        hunt.Name[lumina.SonarLanguage] = name;
                        result = true;
                    }
                }
            }
            return result;
        }

        private void SetAllHuntTimers()
        {
            // ARR Timers (A Ranks)
            this.SetHuntTimers("Vogaal Ja", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Unktehi", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Nahn", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 30, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Hellsclaw", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Marberry", new(new(SonarConstants.EarthHour * 4, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Cornu", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Sabotender Bailarina", new(new(SonarConstants.EarthHour * 4, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Alectryon", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Maahes", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Zanig'oh", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Dalvag's Final Flame", new(new(SonarConstants.EarthHour * 4, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Forneus", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Ghede Ti Malice", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Melt", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Girtab", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Marraco", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));
            this.SetHuntTimers("Kurrea", new(new(SonarConstants.EarthHour * 3 + SonarConstants.EarthMinute * 20, SonarConstants.EarthHour * 5)));

            // ARR Timers (S Ranks)
            this.SetHuntTimers("Croque-Mitaine", new(new(SonarConstants.EarthHour * 65, SonarConstants.EarthHour * 75), new(SonarConstants.EarthHour * 39, SonarConstants.EarthHour * 45)));
            this.SetHuntTimers("Croakadile", new(new(SonarConstants.EarthHour * 50), new(SonarConstants.EarthHour * 30)));
            this.SetHuntTimers("Bonnacon", new(new(SonarConstants.EarthHour * 65, SonarConstants.EarthHour * 75), new(SonarConstants.EarthHour * 39, SonarConstants.EarthHour * 45)));
            this.SetHuntTimers("The Garlok", new(new(SonarConstants.EarthHour * 42, SonarConstants.EarthHour * 48), new(SonarConstants.EarthHour * 21, SonarConstants.EarthHour * 29)));
            this.SetHuntTimers("Nandi", new(new(SonarConstants.EarthHour * 47, SonarConstants.EarthHour * 53), new(SonarConstants.EarthHour * 28, SonarConstants.EarthHour * 32)));
            this.SetHuntTimers("Chernobog", new(new(SonarConstants.EarthHour * 65, SonarConstants.EarthHour * 71), new(SonarConstants.EarthHour * 39, SonarConstants.EarthHour * 43)));
            this.SetHuntTimers("Brontes", new(new(SonarConstants.EarthHour * 66, SonarConstants.EarthHour * 78), new(SonarConstants.EarthHour * 39, SonarConstants.EarthHour * 47)));
            this.SetHuntTimers("Zona Seeker", new(new(SonarConstants.EarthHour * 57, SonarConstants.EarthHour * 63), new(SonarConstants.EarthHour * 34, SonarConstants.EarthHour * 38)));
            this.SetHuntTimers("Lampalagua", new(new(SonarConstants.EarthHour * 66, SonarConstants.EarthHour * 78), new(SonarConstants.EarthHour * 39, SonarConstants.EarthHour * 47)));
            this.SetHuntTimers("Nunyunuwi", new(new(SonarConstants.EarthHour * 44, SonarConstants.EarthHour * 54), new(SonarConstants.EarthHour * 26, SonarConstants.EarthHour * 33)));
            this.SetHuntTimers("Minhocao", new(new(SonarConstants.EarthHour * 57, SonarConstants.EarthHour * 63), new(SonarConstants.EarthHour * 34, SonarConstants.EarthHour * 38)));
            this.SetHuntTimers("Laideronnette", new(new(SonarConstants.EarthHour * 42, SonarConstants.EarthHour * 48), new(SonarConstants.EarthHour * 25, SonarConstants.EarthHour * 29)));
            this.SetHuntTimers("Mindflayer", new(new(SonarConstants.EarthHour * 50), new(SonarConstants.EarthHour * 30)));
            this.SetHuntTimers("Wulgaru", new(new(SonarConstants.EarthHour * 67, SonarConstants.EarthHour * 78), new(SonarConstants.EarthHour * 39, SonarConstants.EarthHour * 47)));
            this.SetHuntTimers("Thousand-cast Theda", new(new(SonarConstants.EarthHour * 57, SonarConstants.EarthHour * 63), new(SonarConstants.EarthHour * 34, SonarConstants.EarthHour * 38)));
            this.SetHuntTimers("Safat", new(new(SonarConstants.EarthHour * 60, SonarConstants.EarthHour * 84), new(SonarConstants.EarthHour * 36, SonarConstants.EarthHour * 51)));
            this.SetHuntTimers("Agrippa The Mighty", new(new(SonarConstants.EarthHour * 60, SonarConstants.EarthHour * 84), new(SonarConstants.EarthHour * 36, SonarConstants.EarthHour * 51)));
        }

        private void SetAllHuntZones()
        {
            // TODO: Can this be automated somehow?
            
            // Hunt zones (ARR)
            this.SetHuntZones("White Joker", "Central Shroud");
            this.SetHuntZones("Forneus", "Central Shroud");
            this.SetHuntZones("Laideronnette", "Central Shroud");

            this.SetHuntZones("Stinging Sophie", "East Shroud");
            this.SetHuntZones("Melt", "East Shroud");
            this.SetHuntZones("Wulgaru", "East Shroud");

            this.SetHuntZones("Monarch Ogrefly", "South Shroud");
            this.SetHuntZones("Ghede Ti Malice", "South Shroud");
            this.SetHuntZones("mindflayer", "South Shroud");

            this.SetHuntZones("Phecda", "North Shroud");
            this.SetHuntZones("Girtab", "North Shroud");
            this.SetHuntZones("Thousand-cast Theda", "North Shroud");

            this.SetHuntZones("Sewer Syrup", "Western Thanalan");
            this.SetHuntZones("Alectryon", "Western Thanalan");
            this.SetHuntZones("Zona Seeker", "Western Thanalan");

            this.SetHuntZones("Ovjang", "Central Thanalan");
            this.SetHuntZones("Sabotender Bailarina", "Central Thanalan");
            this.SetHuntZones("Brontes", "Central Thanalan");

            this.SetHuntZones("Gatling", "Eastern Thanalan");
            this.SetHuntZones("Maahes", "Eastern Thanalan");
            this.SetHuntZones("Lampalagua", "Eastern Thanalan");

            this.SetHuntZones("Albin the Ashen", "Southern Thanalan");
            this.SetHuntZones("Zanig'oh", "Southern Thanalan");
            this.SetHuntZones("Nunyunuwi", "Southern Thanalan");

            this.SetHuntZones("Flame Sergeant Dalvag", "Northern Thanalan");
            this.SetHuntZones("Dalvag's Final Flame", "Northern Thanalan");
            this.SetHuntZones("Minhocao", "Northern Thanalan");

            this.SetHuntZones("Skogs Fru", "Middle La Noscea");
            this.SetHuntZones("Vogaal Ja", "Middle La Noscea");
            this.SetHuntZones("Croque-mitaine", "Middle La Noscea");

            this.SetHuntZones("Barbastelle", "Lower La Noscea");
            this.SetHuntZones("Unktehi", "Lower La Noscea");
            this.SetHuntZones("Croakadile", "Lower La Noscea");

            this.SetHuntZones("Bloody Mary", "Eastern La Noscea");
            this.SetHuntZones("Hellsclaw", "Eastern La Noscea");
            this.SetHuntZones("the Garlok", "Eastern La Noscea");

            this.SetHuntZones("Dark Helmet", "Western La Noscea");
            this.SetHuntZones("Nahn", "Western La Noscea");
            this.SetHuntZones("Bonnacon", "Western La Noscea");

            this.SetHuntZones("Myradrosh", "Upper La Noscea");
            this.SetHuntZones("Marberry", "Upper La Noscea");
            this.SetHuntZones("Nandi", "Upper La Noscea");

            this.SetHuntZones("Vuokho", "Outer La Noscea");
            this.SetHuntZones("Cornu", "Outer La Noscea");
            this.SetHuntZones("Chernobog", "Outer La Noscea");

            this.SetHuntZones("Naul", "Coerthas Central Highlands");
            this.SetHuntZones("Marraco", "Coerthas Central Highlands");
            this.SetHuntZones("Safat", "Coerthas Central Highlands");

            this.SetHuntZones("Leech King", "Mor Dhona");
            this.SetHuntZones("Kurrea", "Mor Dhona");
            this.SetHuntZones("Agrippa the Mighty", "Mor Dhona");

            // Hunt Zones (HW)
            this.SetHuntZones("Alteci", "Coerthas Western Highlands");
            this.SetHuntZones("Kreutzet", "Coerthas Western Highlands");
            this.SetHuntZones("Mirka", "Coerthas Western Highlands");
            this.SetHuntZones("Lyuba", "Coerthas Western Highlands");
            this.SetHuntZones("kaiser behemoth", "Coerthas Western Highlands");

            this.SetHuntZones("Gnath cometdrone", "The Dravanian Forelands");
            this.SetHuntZones("Thextera", "The Dravanian Forelands");
            this.SetHuntZones("Pylraster", "The Dravanian Forelands");
            this.SetHuntZones("Lord of the Wyverns", "The Dravanian Forelands");
            this.SetHuntZones("Senmurv", "The Dravanian Forelands");

            this.SetHuntZones("Pterygotus", "The Dravanian Hinterlands");
            this.SetHuntZones("false gigantopithecus", "The Dravanian Hinterlands");
            this.SetHuntZones("Slipkinx Steeljoints", "The Dravanian Hinterlands");
            this.SetHuntZones("Stolas", "The Dravanian Hinterlands");
            this.SetHuntZones("the Pale Rider", "The Dravanian Hinterlands");

            this.SetHuntZones("Scitalis", "The Churning Mists");
            this.SetHuntZones("the Scarecrow", "The Churning Mists");
            this.SetHuntZones("Bune", "The Churning Mists");
            this.SetHuntZones("Agathos", "The Churning Mists");
            this.SetHuntZones("Gandarewa", "The Churning Mists");

            this.SetHuntZones("Squonk", "The Sea of Clouds");
            this.SetHuntZones("Sanu Vali of Dancing Wings", "The Sea of Clouds");
            this.SetHuntZones("Enkelados", "The Sea of Clouds");
            this.SetHuntZones("Sisiutl", "The Sea of Clouds");
            this.SetHuntZones("Bird of Paradise", "The Sea of Clouds");

            this.SetHuntZones("Lycidas", "Azys Lla");
            this.SetHuntZones("Omni", "Azys Lla");
            this.SetHuntZones("Campacti", "Azys Lla");
            this.SetHuntZones("stench blossom", "Azys Lla");
            this.SetHuntZones("Leucrotta", "Azys Lla");

            // Hunt Zones (SB)
            this.SetHuntZones("Gauki Strongblade", "The Ruby Sea");
            this.SetHuntZones("Guhuo Niao", "The Ruby Sea");
            this.SetHuntZones("Funa Yurei", "The Ruby Sea");
            this.SetHuntZones("Oni Yumemi", "The Ruby Sea");
            this.SetHuntZones("Okina", "The Ruby Sea");

            this.SetHuntZones("Deidar", "Yanxia");
            this.SetHuntZones("Gyorai Quickstrike", "Yanxia");
            this.SetHuntZones("Gajasura", "Yanxia");
            this.SetHuntZones("Angada", "Yanxia");
            this.SetHuntZones("Gamma", "Yanxia");

            this.SetHuntZones("Kurma", "The Azim Steppe");
            this.SetHuntZones("Aswang", "The Azim Steppe");
            this.SetHuntZones("Girimekhala", "The Azim Steppe");
            this.SetHuntZones("Sum", "The Azim Steppe");
            this.SetHuntZones("Orghana", "The Azim Steppe");

            this.SetHuntZones("Shadow-dweller Yamini", "The Fringes");
            this.SetHuntZones("Ouzelum", "The Fringes");
            this.SetHuntZones("Orcus", "The Fringes");
            this.SetHuntZones("Erle", "The Fringes");
            this.SetHuntZones("Udumbara", "The Fringes");

            this.SetHuntZones("Gwas-y-neidr", "The Peaks");
            this.SetHuntZones("Buccaboo", "The Peaks");
            this.SetHuntZones("Vochstein", "The Peaks");
            this.SetHuntZones("Aqrabuamelu", "The Peaks");
            this.SetHuntZones("Bone Crawler", "The Peaks");

            this.SetHuntZones("Manes", "The Lochs");
            this.SetHuntZones("Kiwa", "The Lochs");
            this.SetHuntZones("Mahisha", "The Lochs");
            this.SetHuntZones("Luminare", "The Lochs");
            this.SetHuntZones("Salt and Light", "The Lochs");

            // Hunt Zones (ShB)
            this.SetHuntZones("La Velue", "Lakeland");
            this.SetHuntZones("Itzpapalotl", "Lakeland");
            this.SetHuntZones("Nuckelavee", "Lakeland");
            this.SetHuntZones("Nariphon", "Lakeland");
            this.SetHuntZones("Tyger", "Lakeland");

            this.SetHuntZones("Coquecigrue", "Kholusia");
            this.SetHuntZones("Indomitable", "Kholusia");
            this.SetHuntZones("Li'l Murderer", "Kholusia");
            this.SetHuntZones("Huracan", "Kholusia");
            this.SetHuntZones("forgiven pedantry", "Kholusia");

            this.SetHuntZones("Worm of the Well", "Amh Araeng");
            this.SetHuntZones("Juggler Hecatomb", "Amh Araeng");
            this.SetHuntZones("Maliktender", "Amh Araeng");
            this.SetHuntZones("Sugaar", "Amh Araeng");
            this.SetHuntZones("Tarchia", "Amh Araeng");

            this.SetHuntZones("Domovoi", "Il Mheg");
            this.SetHuntZones("Vulpangue", "Il Mheg");
            this.SetHuntZones("the mudman", "Il Mheg");
            this.SetHuntZones("O Poorest Pauldia", "Il Mheg");
            this.SetHuntZones("Aglaope", "Il Mheg");

            this.SetHuntZones("Mindmaker", "The Rak'tika Greatwood");
            this.SetHuntZones("Pachamama", "The Rak'tika Greatwood");
            this.SetHuntZones("Supay", "The Rak'tika Greatwood");
            this.SetHuntZones("Grassman", "The Rak'tika Greatwood");
            this.SetHuntZones("Ixtab", "The Rak'tika Greatwood");

            this.SetHuntZones("Gilshs Aath Swiftclaw", "The Tempest");
            this.SetHuntZones("Deacon", "The Tempest");
            this.SetHuntZones("Rusalka", "The Tempest");
            this.SetHuntZones("Baal", "The Tempest");
            this.SetHuntZones("Gunitt", "The Tempest");

            this.SetHuntZones("forgiven gossip", "Lakeland", "Kholusia", "Amh Araeng", "Il Mheg", "The Rak'tika Greatwood", "The Tempest");
            this.SetHuntZones("forgiven rebellion", "Lakeland", "Kholusia", "Amh Araeng", "Il Mheg", "The Rak'tika Greatwood", "The Tempest");

            // Hunt Zones (EW)
            this.SetHuntZones("green Archon", "Labyrinthos");
            this.SetHuntZones("ü-u-ü-u", "Labyrinthos");
            this.SetHuntZones("hulder", "Labyrinthos");
            this.SetHuntZones("Storsie", "Labyrinthos");
            this.SetHuntZones("Burfurlur the Canny", "Labyrinthos");

            this.SetHuntZones("Ker shroud", "Labyrinthos", "Thavnair", "Garlemald", "Mare Lamentorum", "Elpis", "Ultima Thule");
            this.SetHuntZones("Ker", "Labyrinthos", "Thavnair", "Garlemald", "Mare Lamentorum", "Elpis", "Ultima Thule");

            this.SetHuntZones("Vajrakumara", "Thavnair");
            this.SetHuntZones("Iravati", "Thavnair");
            this.SetHuntZones("Yilan", "Thavnair");
            this.SetHuntZones("Sugriva", "Thavnair");
            this.SetHuntZones("sphatika", "Thavnair");

            this.SetHuntZones("warmonger", "Garlemald");
            this.SetHuntZones("Emperor's rose", "Garlemald");
            this.SetHuntZones("Minerva", "Garlemald");
            this.SetHuntZones("Aegeiros", "Garlemald");
            this.SetHuntZones("Armstrong", "Garlemald");

            this.SetHuntZones("daphnia magna", "Mare Lamentorum");
            this.SetHuntZones("genesis rock", "Mare Lamentorum");
            this.SetHuntZones("mousse princess", "Mare Lamentorum");
            this.SetHuntZones("lunatender queen", "Mare Lamentorum");
            this.SetHuntZones("Ruminator", "Mare Lamentorum");

            this.SetHuntZones("Yumcax", "Elpis");
            this.SetHuntZones("Shockmaw", "Elpis");
            this.SetHuntZones("petalodus", "Elpis");
            this.SetHuntZones("Gurangatch", "Elpis");
            this.SetHuntZones("Ophioneus", "Elpis");

            this.SetHuntZones("level cheater", "Ultima Thule");
            this.SetHuntZones("Oskh Rhei", "Ultima Thule");
            this.SetHuntZones("Arch-Eta", "Ultima Thule");
            this.SetHuntZones("Fan Ail", "Ultima Thule");
            this.SetHuntZones("Narrow-rift", "Ultima Thule");

            // Hunt Zones (DT)
            this.SetHuntZones("Mad Maguey", "Urqopacha");
            this.SetHuntZones("Chupacabra", "Urqopacha");
            this.SetHuntZones("Queen Hawk", "Urqopacha");
            this.SetHuntZones("Nechuciho", "Urqopacha");
            this.SetHuntZones("Kirlirger the Abhorrent", "Urqopacha");

            this.SetHuntZones("The Slammer", "Kozama'uka");
            this.SetHuntZones("Go'ozoabek'be", "Kozama'uka");
            this.SetHuntZones("The Raintriller", "Kozama'uka");
            this.SetHuntZones("Pkuucha", "Kozama'uka");
            this.SetHuntZones("Ihnuxokiy", "Kozama'uka");

            this.SetHuntZones("Leafscourge Hadoll Ja", "Yak T'el");
            this.SetHuntZones("Xty'iinbek", "Yak T'el");
            this.SetHuntZones("Starcrier", "Yak T'el");
            this.SetHuntZones("Rrax Yity'a", "Yak T'el");
            this.SetHuntZones("Neyoozoteel", "Yak T'el");

            this.SetHuntZones("Nopalitender Fabuloso", "Shaaloani");
            this.SetHuntZones("Uktena", "Shaaloani");
            this.SetHuntZones("Yehehetoaua'pyo", "Shaaloani");
            this.SetHuntZones("Keheniheyamewi", "Shaaloani");
            this.SetHuntZones("Sansheya", "Shaaloani");

            this.SetHuntZones("Gallowsbeak", "Heritage Found");
            this.SetHuntZones("Gargant", "Heritage Found");
            this.SetHuntZones("Heshuala", "Heritage Found");
            this.SetHuntZones("Urna Variabilis", "Heritage Found");
            this.SetHuntZones("Atticus the Primogenitor", "Heritage Found");

            this.SetHuntZones("Jewel Bearer", "Living Memory");
            this.SetHuntZones("13th Child", "Living Memory");
            this.SetHuntZones("Sally the Sweeper", "Living Memory");
            this.SetHuntZones("Cat's Eye", "Living Memory");
            this.SetHuntZones("The Forecaster", "Living Memory");

            this.SetHuntZones("Crystal Incarnation", "Urqopacha", "Kozama'uka", "Yak T'el", "Shaaloani", "Heritage Found", "Living Memory");
            this.SetHuntZones("Arch Aethereater", "Urqopacha", "Kozama'uka", "Yak T'el", "Shaaloani", "Heritage Found", "Living Memory");
        }

        private static SpawnTimers GetDefaultSpawnTimers(HuntRank rank) // If things were this easy but... ARR why?!
        {
            if (rank == HuntRank.B) return new(new(SonarConstants.EarthSecond * 5));
            if (rank == HuntRank.A) return new(new(SonarConstants.EarthHour * 4, SonarConstants.EarthHour * 6));
            if (rank == HuntRank.S) return new(new(SonarConstants.EarthHour * 84, SonarConstants.EarthHour * 132), new(SonarConstants.EarthHour * 50, SonarConstants.EarthHour * 80));
            return new();
        }

        private void SetHuntZones(string huntName, params string[] zoneNames)
        {
            var hunt = this.Db.Hunts.Values.First(h => h.Name.ToString().Equals(huntName, StringComparison.InvariantCultureIgnoreCase));
            foreach (var zoneName in zoneNames)
            {
                var zone = this.Db.Zones.Values.First(z => z.Name.ToString().Equals(zoneName, StringComparison.InvariantCultureIgnoreCase));
                hunt.ZoneIds.Add(zone.Id);
                zone.HuntIds.Add(hunt.Id);
            }
        }

        private void SetHuntTimers(string name, SpawnTimers timers)
        {
            var hunt = this.Db.Hunts.Values.First(h => h.Name.ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase));
            hunt.SpawnTimers = timers;
        }

        private static HuntRank GetHuntRank(NotoriousMonster hunt)
        {
            // Stolen from https://github.com/quisquous/cactbot/blob/master/util/gen_hunt_data.py
            if (hunt.BNpcBase.Row == 10422) return HuntRank.SS; // Forgiven Rebellion
            if (hunt.BNpcBase.Row == 10755) return HuntRank.SSMinion; // Forgiven Gossip

            if (hunt.BNpcName.Row == 10615) return HuntRank.SS; // Ker
            if (hunt.BNpcName.Row == 10616) return HuntRank.SSMinion; // Ker Shroud

            if (hunt.Rank == 3) return HuntRank.S;
            if (hunt.Rank == 2) return HuntRank.A;

            return HuntRank.B;
        }

        private static ExpansionPack GetHuntExpansion(uint nmId)
        {
            if (nmId >= 1 && nmId <= 51) return ExpansionPack.ARealmReborn;
            if (nmId >= 52 && nmId <= 81) return ExpansionPack.Heavensward;
            if (nmId >= 82 && nmId <= 111) return ExpansionPack.Stormblood;
            if (nmId >= 112 && nmId <= 171) return ExpansionPack.Shadowbringers;
            if (nmId >= 172 && nmId <= 231) return ExpansionPack.Endwalker;
            if (nmId >= 232 && nmId <= 9999 /* TODO */) return ExpansionPack.Dawntrail;
            return ExpansionPack.Unknown;
        }

        public static int GetHuntLevel(uint nmId)
        {
            return GetHuntExpansion(nmId) switch
            {
                ExpansionPack.ARealmReborn => 50,
                ExpansionPack.Heavensward => 60,
                ExpansionPack.Stormblood => 70,
                ExpansionPack.Shadowbringers => 80,
                ExpansionPack.Endwalker => 90,
                ExpansionPack.Dawntrail => 100,
                _ => 0
            };
        }
    }
}
