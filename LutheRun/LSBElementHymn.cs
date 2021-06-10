﻿using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LutheRun
{
    class LSBElementHymn : ILSBElement, IDownloadWebResource
    {

        public string Caption { get; private set; } = "";
        public string SubCaption { get; private set; } = "";
        public string Copyright { get; private set; } = "";

        private bool IsText { get; set; } = false;
        private int imageIndex = 0;

        public IEnumerable<HymnImageLine> Images => ImageUrls;

        private List<HymnTextVerse> TextVerses = new List<HymnTextVerse>();
        private List<HymnImageLine> ImageUrls = new List<HymnImageLine>();

        public static LSBElementHymn Parse(IElement element)
        {
            // all hymns have caption and subcaption
            var res = new LSBElementHymn();
            res.Caption = element.QuerySelectorAll(".caption-text").FirstOrDefault()?.TextContent ?? "";
            res.SubCaption = element.QuerySelectorAll(".subcaption-text").FirstOrDefault()?.TextContent ?? "";

            // then parse the lsb-content (could be either text or image)
            var content = element.Children.Where(c => c.LocalName == "lsb-content").FirstOrDefault();

            foreach (var child in content.Children)
            {
                if (child.ClassList.Contains("numbered-stanza"))
                {
                    res.IsText = true;
                    HymnTextVerse verse = new HymnTextVerse();
                    verse.Number = child.QuerySelectorAll(".stanza-number").FirstOrDefault()?.TextContent ?? "";
                    //var s = Regex.Replace(child.TextContent, @"^\d+", "");
                    //verse.Lines = s.Split(new string[] { "\r\n", "\r", "\n", Environment.NewLine, ".", ",", ":", ";", "!", "?", "    " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    //res.TextVerses.Add(verse);
                    verse.Lines = child.ParagraphText();
                    res.TextVerses.Add(verse);
                }
                else if (child.ClassList.Contains("image"))
                {
                    HymnImageLine imageline = new HymnImageLine();
                    var picture = child.Children.First();
                    var sources = picture?.Children.Where(c => c.LocalName == "source");
                    foreach (var source in sources)
                    {
                        if (source.Attributes["media"].Value == "print")
                        {
                            imageline.PrintURL = source.Attributes["srcset"].Value;
                        }
                        else if (source.Attributes["media"].Value == "screen")
                        {
                            string src = source.Attributes["srcset"].Value;
                            var urls = src.Split(", ");
                            foreach (var url in urls)
                            {
                                if (url.Contains("retina"))
                                {
                                    string s = url.Trim();
                                    if (s.EndsWith(" 2x"))
                                    {
                                        s = s.Substring(0, s.Length - 3);
                                    }
                                    imageline.RetinaScreenURL = s.Trim();
                                }
                                else
                                {
                                    imageline.ScreenURL = url.Trim();
                                }
                            }
                        }
                    }
                    imageline.InferedName = $"Hymn_{res.Caption.Replace(",", "")}_{res.imageIndex++}";
                    res.ImageUrls.Add(imageline);
                }
                else if (child.ClassList.Contains("copyright"))
                {
                    res.Copyright = res.Copyright + child.StrippedText();
                }
            }

            return res;
        }

        public string DebugString()
        {
            if (IsText)
            {
                return $"///XENON DEBUG::Parsed as LSB_ELEMENT_HYMN-TEXT. Caption:'{Caption}' SubCaption:'{SubCaption}''";
            }
            return "";
        }

        public string XenonAutoGen()
        {
            if (IsText)
            {
                return XenonAutoGenTextHymn();
            }
            else
            {
                return XenonAutoGenImageHymn();
            }
        }


        private string XenonAutoGenTextHymn()
        {
            StringBuilder sb = new StringBuilder();
            var match = Regex.Match(Caption, @"(?<number>\d+)?(?<name>.*)");
            string title = "Hymn";
            string name = match.Groups["name"]?.Value.Trim() ?? "";
            string number = "LSB " + match.Groups["number"]?.Value.Trim() ?? "";
            string tune = "";
            string copyright = Copyright;
            sb.Append($"#texthymn(\"{title}\", \"{name}\", \"{tune}\", \"{number}\", \"{copyright}\") {{\r\n");

            foreach (var verse in TextVerses)
            {
                string verseinsert = verse.Number != string.Empty ? $"(Verse {verse.Number})" : "";
                sb.AppendLine($"#verse{verseinsert} {{");
                foreach (var line in verse.Lines)
                {
                    sb.AppendLine(line.Trim());
                }
                sb.AppendLine("}");
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string XenonAutoGenImageHymn()
        {
            StringBuilder sb = new StringBuilder();
            var match = Regex.Match(Caption, @"(?<number>\d+)?(?<name>.*)");
            string title = "Hymn";
            string name = match.Groups["name"]?.Value.Trim() ?? "";
            string number = "LSB " + match.Groups["number"]?.Value.Trim() ?? "";
            string tune = "";
            string copyright = Copyright;

            sb.AppendLine("/// <XENON_AUTO_GEN>");
            //sb.AppendLine($"/// \"{title}\", \"{name}\", \"{tune}\", \"{number}\", \"{copyright}\"");
            sb.AppendLine($"#stitchedimage(\"{title}\", \"{name}\", \"{number}\", \"{copyright}\") {{");
            //sb.AppendLine("/// URLS::");
            foreach (var imageline in ImageUrls)
            {
                //sb.AppendLine($"/// img={imageline.RetinaScreenURL}");
                sb.AppendLine($"{imageline.InferedName};");
            }
            sb.AppendLine("}");
            sb.AppendLine("/// </XENON_AUTO_GEN>");


            return sb.ToString();
        }

        public Task GetResourcesFromWeb()
        {
            var tasks = ImageUrls.Select(async imageurls =>
            {
                // use screenurls
                try
                {
                    System.Net.WebRequest request = System.Net.WebRequest.Create(imageurls.ScreenURL);
                    System.Net.WebResponse response = await request.GetResponseAsync();
                    System.IO.Stream responsestream = response.GetResponseStream();
                    imageurls.Bitmap = new Bitmap(responsestream);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed trying to download: {imageurls.ScreenURL}\r\n{ex}");
                }
            });
            return Task.WhenAll(tasks);
        }

        class HymnTextVerse
        {
            public string Number { get; set; } = "";
            public List<string> Lines { get; set; } = new List<string>();
        }

        internal class HymnImageLine
        {
            public string LocalPath { get; set; } = "";
            public string PrintURL { get; set; } = "";
            public string ScreenURL { get; set; } = "";
            public string RetinaScreenURL { get; set; } = "";
            public string InferedName { get; set; } = "";
            public Bitmap Bitmap { get; set; }
        }


    }

}
