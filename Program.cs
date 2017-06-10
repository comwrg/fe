using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace fe
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var tf = File.ReadAllLines("./template");
            var fields = "w pos y";
            var template = Fe.ReadTemplate(tf, fields);
            var fi = File.ReadAllLines("./test.txt");
            using (var fo = new StreamWriter("./test_features.txt"))
            {
                foreach (var x in Fe.ReadIter(fi, fields.Split(' '), ' '))
                {
                    Fe.ApplyTemplates(x, template);
//                    Console.WriteLine(x[0]["F"]);
                    Fe.OutputFeatures(fo, x, "y");
                }
            }

            Console.ReadKey();
        }
    }

    public static class Fe
    {
        public static List<List<KeyValuePair<string, int>>> ReadTemplate(string[] fi, string fields)
        {
            var macro = new Regex(@"%x\[(?<row>[\d-]+),(?<col>[\d]+)\]");
            var tuple = fields.Split(' ');
            var d = new Dictionary<string, string>();
            for (var i = 0; i < tuple.Length; i++)
                d.Add(i.ToString(), tuple[i]);
            // 0 w, 1 pos, 2 y
            var t = new List<List<KeyValuePair<string, int>>>();
            foreach (var item in fi)
            {
                var line = item.Trim();
                if (line.StartsWith("#"))
                {
                }
                else if (line.StartsWith("U"))
                {
                    var mc = macro.Matches(line);
                    if (mc.Count == 0)
                    {
                    }
                    else
                    {
                        var u = new List<KeyValuePair<string, int>>();
                        foreach (Match m in mc)
                            u.Add(new KeyValuePair<string, int>(d[m.Groups["col"].Value],
                                int.Parse(m.Groups["row"].Value)));
                        t.Add(u);
                    }
                }
                else if (line == "B")
                {
                }
                else if (line.StartsWith("B"))
                {
                    throw new Exception("ERROR: bigram templates not supported:" + line);
                }
            }
            return t;
        }

        public static IEnumerable<List<Dictionary<string, string>>> ReadIter(string[] fi, string[] names,
            char sep = '\t')
        {
            var x = new List<Dictionary<string, string>>();
            foreach (var l in fi)
            {
                var line = l.Trim('\n');
                if (line == string.Empty)
                {
                    yield return x;
                    x.Clear();
                }
                else
                {
                    var fields = line.Split(sep);
                    if (fields.Length < names.Length)
                    {
                        Console.WriteLine(fields.Length);
                        Console.WriteLine(names.Length);
                        throw new Exception($"Too few fields ({fields.Length}) for {names}\n{line}");
                    }
                    var item = new Dictionary<string, string>();
                    item.Add("F","");
                    for (var i = 0; i < names.Length; i++)
                        item.Add(names[i], fields[i]);

                    x.Add(item);
                }
            }
        }

        public static void ApplyTemplates(List<Dictionary<string, string>> x,
            List<List<KeyValuePair<string, int>>> templates)
        {
            foreach (var template in templates)
            {
                // name = '|'.join(['%s[%d]' % (f, o) for f, o in template])
                var list = new List<string>();
                foreach (var item in template)
                    list.Add(string.Format("{0}[{1}]", item.Key, item.Value));
                var name = string.Join("|", list.ToArray());
//                Console.WriteLine(name);
                for (var t = 0; t < x.Count; t++)
                {
                    var values = new List<string>();
                    foreach (var item in template)
                    {
//                        Console.WriteLine(item.Value);
                        var p = t + item.Value;
                        if (p < 0 || p >= x.Count)
                        {
                            values.Clear();
                            break;
                        }
//                        Console.WriteLine(item.Value);
//                        Console.WriteLine(x[p][item.Key]);
                        values.Add(x[p][item.Key]);
                    }

                    if (values.Count != 0)
                        x[t]["F"] += string.Format("{0}={1}\n", name, string.Join("|", values.ToArray()));
                }
            }
        }

        public static void OutputFeatures(StreamWriter fo, List<Dictionary<string, string>> x, string field = "")
        {
            for (var t = 0; t < x.Count; t++)
            {
                if (field != string.Empty)
                    fo.Write(x[t][field]);
//                Console.WriteLine(x[t]["F"]);
                foreach (var a in x[t]["F"].TrimEnd('\n').Split('\n'))
                    fo.Write(string.Format("\t{0}", a.Replace(":", "__COLON__")));
                fo.Write("\r\n");
//                Console.ReadKey();
            }
            fo.Write("\r\n");
        }
    }
}