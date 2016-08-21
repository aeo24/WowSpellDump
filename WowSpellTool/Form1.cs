using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using mshtml;

namespace WowSpellTool
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> bannedClasses = new Dictionary<string, string>
        {
            { "Restoration","Rogue" },
            { "Tenacity", "Hunter" },
            { "Cunning", "Hunter" },
            { "Ferocity", "Hunter" },
            { "Demonology", "Shaman" },
            { "Protection", "Monk" },
       };


        List <string> classCheck = new List<string>(new string[] {"Priest","Rogue","Mage","Druid","Warrior","Warlock","Hunter","Shaman","Paladin","Monk","DeathKnight","Pet","DemonHunter"});
        MultikeyDictionary<string, string, List<Tuple<string, string>>> classMap = new MultikeyDictionary<string, string, List<Tuple<string, string>>>();

        public Form1()
        {
      
            InitializeComponent();
            string line;
            var url = "https://raw.githubusercontent.com/simulationcraft/simc/legion-dev/SpellDataDump/allspells.txt";
            var webRequest = WebRequest.Create(url);
            var response = webRequest.GetResponse();
            var content = response.GetResponseStream();
            var reader = new StreamReader(content);
            var spellList = new List<Tuple<string, string, string, string>>();
            List<string> tmpclassesList = new List<string>();
            List<string> tmpspecList = new List<string>();
            HashSet<string> specList = new HashSet<string>();
            while ((line = reader.ReadLine()) != null)
            {
                string spellId = "";
                string spellname = "";

                if (line.StartsWith("Name") && !line.Contains("Item") && !line.Contains("Passive") &&
                    !line.Contains("Hidden"))
                {
                    line = line.Replace(",", "");
                    int start = line.IndexOf("(id");
                    int end = line.IndexOf(")", start);
                    spellId = line.Substring(start + 4, end - (start + 4));
                    var nameStart = line.IndexOf(":") + 1;
                    spellname = line.Substring(nameStart, start - nameStart);
                    spellname = spellname.Replace(" ", "");
                    spellname = spellname.Replace("-", "");
                    spellname = Regex.Replace(spellname, "[^0-9a-zA-Z]+", "");
                    line = reader.ReadLine();
                    if (line.StartsWith("Class"))
                    {

                        line = line.Substring(line.IndexOf(":") + 2);
                        line = line.Replace("Death Knight", "DeathKnight");
                        line = line.Replace("Demon Hunter", "DemonHunter");

                        line = line.Replace(", ", ",");
                        line = line.Replace(" ", ",");
                       
                        string[] values = line.Split(',');
                        if (values.Length == 1)
                        {
                            Array.Resize(ref values, values.Length + 1);
                            values[values.Length - 1] = "General";
                        }
                        foreach (string v in values)
                        {
                            if (classCheck.Contains(v))
                            {
                                tmpclassesList.Add(v);
                            }
                            else
                            {
                                tmpspecList.Add(v);
                            }
                        }
                    }
                }
                if (spellId.Length > 0)
                {           
                    foreach (string classn in tmpclassesList)
                    {
                        foreach (string spec in tmpspecList)
                        {
                            spellList.Add((Tuple.Create(spellId, classn, spellname, spec)));
                            specList.Add(spec);

                        }
                    }
                }
                tmpclassesList = new List<string>();
                tmpspecList = new List<string>();
            }
            reader.Close();
            foreach (string c in classCheck)
            {
                foreach (string s in specList)
                {
                    List<Tuple<string, string>> spells = new List<Tuple<string, string>>();
                    foreach (var t in spellList)
                    {
                       
                        if (t.Item2.Equals(c) && t.Item4.Equals(s))
                        {
                            spells.Add(Tuple.Create(t.Item1, t.Item3));
                        }
                      
                    }
                    if (spells.Count > 0)
                        classMap.Add(c, s.Length > 0 ? s : "General", spells);
                }
            }
            StreamWriter file = new StreamWriter("C:\\Logs\\Spells.txt");
            foreach (string c in classCheck)
            {
                
                file.WriteLine("public static class " + c);
                file.WriteLine("{");
                foreach (string s in specList)
                {
                    var name = s.Length > 0 ? s : "General";
                    string value ="";

                    if (bannedClasses.TryGetValue(s, out value))
                    {
                        if(value.Equals(c))
                            continue;
                    }
                    var classlist = classMap[c, name];
                    if(classlist == null)
                        continue;
                    file.WriteLine("\tpublic static class " + name );
                    file.WriteLine("\t{");
                    var dupeList = new List<string>();
                    foreach (var list in classlist)
                    {
                        string count = "";
                        if(dupeList.Contains(list.Item2))
                            count = dupeList.Count(r => r == list.Item2).ToString();

                        file.WriteLine("\t\tpublic static WowSpell " + list.Item2 + (count.Length > 0 ? count : "") +" => Wow.Spells[" + list.Item1 + "];\n");
                        dupeList.Add(list.Item2);
                    }
                    file.WriteLine("\t}\n ");
                }
                file.WriteLine("}\n ");
            }
            file.Close();

        }

        public class MultikeyDictionary<K1, K2, V> : Dictionary<KeyValuePair<K1, K2>, V>
        {
            public MultikeyDictionary()
            {
                
            }
            public V this[K1 index1, K2 index2]
            {
               
                get {
                    V value;
                    this.TryGetValue(new KeyValuePair<K1, K2>(index1, index2), out value);
                    return value;
                }
                set { this[new KeyValuePair<K1, K2>(index1, index2)] = value; }
            }

            public bool Remove(K1 index1, K2 index2)
            {
                return base.Remove(new KeyValuePair<K1, K2>(index1, index2));
            }

            public void Add(K1 index1, K2 index2, V value)
            {
                try
                {
                    base.Add(new KeyValuePair<K1, K2>(index1, index2), value);
                }
                catch (Exception)
                {

                    
                }
                
            }
        }

    }

}
