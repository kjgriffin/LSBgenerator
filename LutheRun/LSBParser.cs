﻿using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LutheRun
{
    public class LSBParser
    {

        private StringBuilder stringBuilder = new StringBuilder();

        private List<ILSBElement> serviceElements = new List<ILSBElement>();

        public string XenonText { get => stringBuilder.ToString(); }

        private string ServiceFileName;


        public void Serviceify()
        {
            serviceElements = Serviceifier.AddAdditionalInferedElements(Serviceifier.RemoveUnusedElement(serviceElements));
        }


        public string XenonDebug()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var se in serviceElements)
            {
                sb.AppendLine(se.DebugString());
            }
            return sb.ToString();
        }

        public async Task ParseHTML(string path)
        {
            ServiceFileName = path;
            string html = "";
            try
            {
                using (TextReader reader = new StreamReader(path))
                {
                    html = await reader.ReadToEndAsync();
                    await ParseHTMLIntoDOM(html);
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task ParseHTMLIntoDOM(string html)
        {
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(html);

            var bulletin = document.All.Where(e => e.Id == "bulletin_surface").FirstOrDefault();
            if (bulletin != null)
            {
                var service = bulletin.Children.First().Children.First().Children.First().Children.Where(e => e.LocalName == "lsb-service-element").ToList();
                ParseDOMElements(service);
            }

        }

        private void ParseDOMElements(List<AngleSharp.Dom.IElement> dom)
        {
            List<ILSBElement> elements = new List<ILSBElement>();
            serviceElements.Clear();
            foreach (var e in dom)
            {
                _ParseLSBServiceElement(e);
            }
        }

        private void ParseLSBServiceElement(AngleSharp.Dom.IElement element)
        {
            /*
            bool checkforselection = false;
            if (element.ClassList.Contains("option-group"))
            {
                checkforselection = true;
            }
            if (element.Children.All(x => x.LocalName == "lsb-service-element"))
            {
                bool allseperate = true;
                if (checkforselection)
                {
                    allseperate = false;
                    if (element.Children.All(x => !x.ClassList.Contains("selected")))
                    {
                        allseperate = true;
                    }
                }
                if (allseperate)
                {
                    foreach (var child in element.Children)
                    {
                        ParseLSBServiceElement(child);
                    }
                }
                return;
            }
            */

            if (element.LocalName == "lsb-content")
            {
                if (element.Children.Any(c => c.ClassList.Contains("lsb-responsorial")))
                {
                    serviceElements.Add(LSBElementLiturgy.Parse(element));
                }
                foreach (var imageline in element.Children.Where(c => c.ClassList.Contains("image")))
                {
                    serviceElements.Add(LSBElementLiturgySung.Parse(imageline));
                }
            }
            else
            {

                if (element.ClassList.Contains("heading"))
                {
                    serviceElements.Add(LSBElementHeading.Parse(element));
                }
                else if (element.ClassList.Contains("caption"))
                {
                    // TODO: do something different for anthem-titles, sermon-titles, postlude/preludes. Ignore others???
                    serviceElements.Add(LSBElementCaption.Parse(element));
                }
                else if (element.ClassList.Contains("static"))
                {
                    if (ParseAsPrefab(element))
                    {
                        return;
                    }

                    if (ParsePropperAsFullMusic(element))
                    {
                        return;
                    }

                    // is liturgy responsorial if it contains lsb-content elements
                    foreach (var child in element.Children.Where(c => c.LocalName == "lsb-content"))
                    {
                        ParseLSBServiceElement(child);
                    }
                }
                else if (element.ClassList.Contains("reading"))
                {
                    serviceElements.Add(LSBElementReading.Parse(element));
                }
                else if (element.ClassList.Contains("hymn"))
                {
                    serviceElements.Add(LSBElementHymn.Parse(element));
                }
                else if (element.ClassList.Contains("option-group") || element.ClassList.Contains("group"))
                {
                    var singlechild = element.Children.Where(c => c.LocalName == "lsb-service-element");
                    if (singlechild.Count() == 1)
                    {
                        if (singlechild.First().ClassList.Contains("static"))
                        {
                            ParseLSBServiceElement(singlechild.First());
                            return;
                        }
                    }

                    // select all sub lsb-elements what are group selected
                    var selectedgroup = element.Children.Where(c => c.LocalName == "lsb-service-element" && c.ClassList.Contains("selected"));
                    foreach (var selectedelement in selectedgroup)
                    {
                        if (selectedelement.ClassList.Contains("group"))
                        {
                            foreach (var lsbserviceelement in selectedelement.Children.Where(c => c.LocalName == "lsb-service-element"))
                            {
                                ParseLSBServiceElement(lsbserviceelement);
                            }
                        }
                        else if (selectedelement.ClassList.Contains("static"))
                        {
                            ParseLSBServiceElement(selectedelement);
                        }
                        else if (selectedelement.ClassList.Contains("prayer"))
                        {
                            foreach (var c in selectedelement.Children.Where(x => x.LocalName == "lsb-content"))
                            {
                                ParseLSBServiceElement(c);
                            }
                        }
                    }
                }
                else if (element.ClassList.Contains("proper"))
                {
                    if (ParseAsPrefab(element))
                    {
                        return;
                    }
                    if (ParsePropperAsFullMusic(element) == true)
                    {
                        return;
                    }
                }
                else if (element.ClassList.Contains("prayer"))
                {
                    if (ParseAsPrefab(element))
                    {
                        return;
                    }
                    foreach (var c in element.Children.Where(x => x.LocalName == "lsb-content"))
                    {
                        ParseLSBServiceElement(c);
                    }
                }
                else
                {
                    serviceElements.Add(LSBElementUnknown.Parse(element));
                }
            }
        }





        private bool _ParseLSBServiceElement(IElement element)
        {
            if (element.ClassList.Contains("caption"))
            {
                serviceElements.Add(LSBElementCaption.Parse(element));
                return true;
            }
            else if (element.ClassList.Contains("heading"))
            {
                serviceElements.Add(LSBElementHeading.Parse(element));
                return true;
            }
            else if (element.ClassList.Contains("hymn"))
            {
                serviceElements.Add(LSBElementHymn.Parse(element));
                return true;
            }
            else if (element.ClassList.Contains("reading"))
            {
                serviceElements.Add(LSBElementReading.Parse(element));
                return true;
            }
            else if (element.ClassList.Contains("prayer"))
            {
                return _ParseLSBPrayerElement(element);
            }
            else if (element.ClassList.Contains("proper"))
            {
                return _ParseLSBProperElement(element);
            }
            else if (element.ClassList.Contains("static"))
            {
                return _ParseLSBStaticElement(element);
            }
            else if (element.ClassList.Contains("option-group"))
            {
                return _ParseLSBOptionGroupElement(element);
            }
            else if (element.ClassList.Contains("group"))
            {
                return _ParseLSBGroupElement(element);
            }
            serviceElements.Add(LSBElementUnknown.Parse(element));
            return false;
        }

        private bool _ParseLSBPrayerElement(IElement element)
        {
            // capture known prayers we have prefab slides for
            if (ParseAsPrefab(element))
            {
                return true;
            }
            // otherwise just spit it out as liturgy
            return _ParseLSBElementIntoLiturgy(element);
        }

        private bool _ParseLSBProperElement(IElement element)
        {
            if (ParseAsPrefab(element))
            {
                return true;
            }
            if (ParsePropperAsFullMusic(element))
            {
                return true;
            }
            if (ParsePropperAsIntroit(element))
            {
                return true;
            }
            return false;
        }

        private bool _ParseLSBStaticElement(IElement element)
        {
            if (ParseAsPrefab(element))
            {
                return true;
            }
            if (ParsePropperAsFullMusic(element))
            {
                return true;
            }
            return _ParseLSBElementIntoLiturgy(element);
        }

        private bool _ParseLSBOptionGroupElement(IElement element)
        {
            // only parse selected element if in option group
            var selectedelement = element.Children.Where(c => c.LocalName == "lsb-service-element" && c.ClassList.Contains("selected"));
            foreach (var elem in selectedelement)
            {
                _ParseLSBServiceElement(elem);
            }
            return true;
        }

        private bool _ParseLSBGroupElement(IElement element)
        {
            var subelements = element.Children.Where(c => c.LocalName == "lsb-service-element");
            foreach (var elem in subelements)
            {
                _ParseLSBServiceElement(elem);
            }
            return true;
        }

        private bool _ParseLSBElementIntoLiturgy(IElement element)
        {
            foreach (var content in element.Children.Where(x => x.LocalName == "lsb-content"))
            {
                _ParseLSBContentIntoLiturgy(content);
            }
            return true;
        }


        private bool _ParseLSBContentIntoLiturgy(IElement contentelement)
        {
            List<string> ltext = new List<string>();
            foreach (var child in contentelement.Children.Where(c => c.ClassList.Contains("lsb-responsorial") || c.ClassList.Contains("lsb-responsorial-continued") || c.ClassList.Contains("image")))
            {
                if (child.ClassList.Contains("lsb-responsorial") || child.ClassList.Contains("lsb-responsorial-continued"))
                {
                    var l = LSBElementLiturgy.Parse(child) as LSBElementLiturgy;
                    ltext.Add(l.LiturgyText);
                }
                else if (child.ClassList.Contains("image"))
                {
                    if (ltext.Any())
                    {
                        serviceElements.Add(LSBElementLiturgy.Create(ltext.StringTogether(Environment.NewLine)));
                        ltext.Clear();
                    }
                    serviceElements.Add(LSBElementLiturgySung.Parse(child));
                }
            }
            if (ltext.Any())
            {
                serviceElements.Add(LSBElementLiturgy.Create(ltext.StringTogether(Environment.NewLine)));
                ltext.Clear();
            }
            return true;
        }



        private bool ParsePropperAsIntroit(IElement element)
        {
            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;
            if (caption == null || caption?.Caption == string.Empty)
            {
                return false;
            }

            if (!caption.Caption.ToLower().Contains("introit"))
            {
                return false;
            }

            serviceElements.Add(LSBElementIntroit.Parse(element));

            return true;
        }


        private bool ParsePropperAsFullMusic(IElement element)
        {
            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;
            if (caption != null && caption?.Caption != string.Empty)
            {
                // reasonably sure this might have something interesting
                // we're expecting multiple images in the content
                foreach (var content in element.Children.Where(c => c.LocalName == "lsb-content"))
                {
                    var images = content.Children.Where(c => c.ClassList.Contains("image"));
                    if (images.Count() > 1)
                    {
                        LSBElementHymn hymn = LSBElementHymn.Parse(element);
                        // check that we really want this as a hymn
                        // biggest clue is separate text lines
                        if (!hymn.HasSeperateTextLines())
                        {
                            if (hymn.Lines <= 2)
                            {
                                // lets do it as litimage instead
                                return false;
                            }
                            int variance = hymn.LineWidthVariance(Path.GetDirectoryName(ServiceFileName));
                            if (variance > 10 * hymn.Lines)
                            {
                                // pretty sure it's not one thing, but lots of sung liturgy
                                return false;
                            }
                        }

                        serviceElements.Add(hymn);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ParseAsPrefab(IElement element)
        {
            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;
            if (caption != null && caption?.Caption != string.Empty)
            {
                string ctext = Regex.Replace(caption.Caption, @"[^\w ]", "");
                // first check if is known element (Creed/Prayer)
                Dictionary<string, string> prefabs = new Dictionary<string, string>()
                {
                    ["Apostles Creed"] = "apostlescreed",
                    ["Nicene Creed"] = "nicenecreed",
                    ["Lords Prayer"] = "lordsprayer",
                };
                if (prefabs.Keys.Contains(ctext))
                {
                    // use a prefab instead
                    serviceElements.Add(new LSBElementIsPrefab(prefabs[ctext], element.StrippedText()));
                    return true;
                }
            }
            return false;
        }

        public void CompileToXenon()
        {
            stringBuilder.Clear();
            stringBuilder.Append($"\r\n////////////////////////////////////\r\n// XENON AUTO GEN: From Service File '{System.IO.Path.GetFileName(ServiceFileName)}'\r\n////////////////////////////////////\r\n\r\n");
            foreach (var se in serviceElements)
            {
                stringBuilder.AppendLine(se.XenonAutoGen());
            }
        }

        public Task LoadWebAssets(Action<Bitmap, string, string> addImageAsAsset)
        {
            IEnumerable<IDownloadWebResource> resources = serviceElements.Select(s => s as IDownloadWebResource).Where(s => s != null);
            IEnumerable<Task> tasks = resources.Select(async s =>
            {
                await s.GetResourcesFromLocalOrWeb(Path.GetDirectoryName(ServiceFileName));
                foreach (var image in s.Images)
                {
                    addImageAsAsset(image.Bitmap, image.RetinaScreenURL, image.InferedName);
                }
            });
            return Task.WhenAll(tasks);
        }

    }
}
