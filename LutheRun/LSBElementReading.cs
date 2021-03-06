﻿using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LutheRun
{
    class LSBElementReading : ILSBElement
    {

        public string PostsetCmd { get; set; }
        public string ReadingTitle { get; private set; } = "";
        public string ReadingReference { get; private set; } = "";
        public string PreLiturgy { get; private set; } = "";
        public string PostLiturgy { get; private set; } = "";

        public static ILSBElement Parse(IElement element)
        {
            var caption = LSBElementCaption.Parse(element.Children.First(e => e.ClassList.Contains("lsb-caption"))) as LSBElementCaption;

            string preliturgy = "";
            string postliturgy = "";

            foreach (var child in element.Children.Where(e => e.LocalName == "lsb-content"))
            {
                bool pre = true;
                foreach (var p in child.Children)
                {
                    if (p.ClassList.Contains("lsb-responsorial"))
                    {
                        if (pre)
                        {
                            preliturgy += " " + p.StrippedText();
                        }
                        else
                        {
                            postliturgy += " " + p.StrippedText();
                        }
                    }
                    else
                    {
                        pre = false;
                    }
                }
            }

            LSBElementReading reading = new LSBElementReading();
            reading.ReadingTitle = caption.Caption;
            reading.ReadingReference = caption.SubCaption;
            reading.PreLiturgy = preliturgy;
            reading.PostLiturgy = postliturgy;

            return reading;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_READING. Title: {ReadingTitle} Reference: {ReadingReference} PreLiturgy: {PreLiturgy} PostLiturgy: {PostLiturgy}";
        }

        public string XenonAutoGen()
        {

            var postset = PostsetCmd.ExtractPostsetValues();

            string first = postset.first != -1 ? $"::postset(first={postset.first})" : "";
            string last = postset.last != -1 ? $"::postset(last={postset.last})" : "";

            bool prelit = PreLiturgy.Trim() != string.Empty;
            bool postlit = PostLiturgy.Trim() != string.Empty;

            string onreadingpostset = "";
            if (first != string.Empty && last != string.Empty && !prelit && !postlit)
            {
                // need to rewrite
                onreadingpostset = $"::postset(first={postset.first}, last={postset.last})";
            }
            else if (!prelit)
            {
                onreadingpostset = first;
            }
            else if (!postlit)
            {
                onreadingpostset = last;
            }


            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("/// <XENON_AUTO_GEN>");
            if (prelit)
            {
                sb.AppendLine($"#liturgy{{\r\n{PreLiturgy}\r\n}}{first}");
            }
            sb.AppendLine($"#reading(\"{ReadingTitle}\", \"{ReadingReference}\"){onreadingpostset}");
            if (postlit)
            {
                sb.AppendLine($"#liturgy{{\r\n{PostLiturgy}\r\n}}{last}");
            }
            //sb.AppendLine("/// </XENON_AUTO_GEN>");
            return sb.ToString();
        }
    }
}
