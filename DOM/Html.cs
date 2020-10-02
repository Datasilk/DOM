using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datasilk.Core.DOM
{
    public class Html
    {
        public string RawHtml;
        public List<Element> Elements;
        public string DocumentType = "html";
        private ParserOptions Options;

        public Html(string htm, ParserOptions options = null)
        {
            RawHtml = htm;
            Options = options != null ? options : new ParserOptions();
            Elements = new List<Element>();
            ParseHtml(htm);
        }

        public static Html Parse(string html, ParserOptions options = null)
        {
            return new Html(html, options);
        }

        private void ParseHtml(string htm)
        {
            if (htm.Length <= 3) { return; }
            bool isInScript = false;
            bool foundTag = false;
            int s1, s2, s3, xs = -1;
            int parentElement = -1;
            string str1, schar, strTag, strText, docType = "html";
            var hierarchy = new List<string>();
            var hierarchyIndexes = new List<int>();
            var tagNameChars = new string[] { "/", "!", "?" };

            for (var x = 0; x < htm.Length; x++)
            {
                //find HTML tag
                var domTag = new Element(this);

                if (foundTag == false && xs == 0)
                {
                    //no tags found in htm, create text tag and exit
                    var textTag = new Element(this)
                    {
                        TagName = "#text",
                        Text = CleanText(htm)
                    };
                    AddTag(textTag, parentElement, true, false, hierarchy, hierarchyIndexes);
                    break;
                }
                else if (xs == -1)
                {
                    xs = x;
                }
                else if (foundTag == true)
                {
                    xs = x;
                }

                var isClosingTag = false;
                var isSelfClosing = false;
                var isComment = false;
                foundTag = false;
                if (isInScript == true)
                {
                    //find closing script tag
                    //TODO: make sure </script> tag isn't in a 
                    //      javascript string, but instead is the
                    //      actual closing tag for the script
                    x = htm.IndexOf("</script>", x);
                    if (x == -1) { break; }
                    schar = htm.Substring(x, 9).ToString();
                }
                else
                {
                    //find next html tag
                    x = htm.IndexOf('<', x);
                    if (x == -1) { break; }
                    schar = htm.Substring(x, 3).ToString();
                }
                if (schar[0] == '<')
                {
                    if (OnlyAlphabet(schar[1].ToString(), tagNameChars))
                    {
                        //found HTML tag
                        s1 = htm.IndexOf(">", x + 2);
                        s2 = htm.IndexOf("<", x + 2);
                        if (s1 >= 0)
                        {
                            //check for comment
                            if (htm.Substring(x + 1, 3) == "!--")
                            {
                                s1 = htm.IndexOf("-->", x + 1);
                                if (s1 < 0)
                                {
                                    s1 = htm.Length - 1;
                                }
                                else
                                {
                                    s1 += 2;
                                }
                                s2 = -1;
                                isSelfClosing = true;
                                isComment = true;
                            }

                            //check for broken tag
                            if (s2 < s1 && s2 >= 0) { continue; }

                            //found end of tag
                            foundTag = true;
                            strTag = htm.Substring(x + 1, s1 - (x + 1));

                            //check for self-closing tag
                            str1 = strTag.Substring(strTag.Length - 1, 1);
                            if (str1 == "/" || (str1 == "?" && schar[1] == '?')) { isSelfClosing = true; }
                            if (Elements.Count == 0)
                            {
                                if (strTag.IndexOf("?xml") == 0) { docType = "xml"; }
                            }
                            DocumentType = docType;

                            //check for attributes
                            domTag.ClassNames = new List<string>();
                            if (isComment == true)
                            {
                                domTag.TagName = "!--";
                                domTag.Text = strTag.Substring(3, strTag.Length - 5);
                            }
                            else
                            {
                                s3 = strTag.IndexOf(" ");
                                if (s3 < 0)
                                {
                                    //tag has no attributes
                                    if (isSelfClosing)
                                    {
                                        if (strTag.Length > 1)
                                        {
                                            domTag.TagName = strTag.Substring(0, strTag.Length - 2).ToLower();
                                        }
                                    }
                                    else
                                    {
                                        //tag has no attributes & no forward-slash
                                        domTag.TagName = strTag.ToLower();
                                    }
                                }
                                else
                                {
                                    //tag has attributes
                                    domTag.TagName = strTag.Substring(0, s3).ToLower();
                                    domTag.Attributes = GetAttributes(strTag);
                                    domTag.Styles = new Dictionary<string, string>();

                                    //set up class name list
                                    if (domTag.Attributes.ContainsKey("class"))
                                    {
                                        domTag.ClassNames = new List<string>(domTag.Attributes["class"].Split(' '));
                                    }
                                    else { domTag.ClassNames = new List<string>(); }

                                    //set up style dictionary
                                    if (domTag.Attributes.ContainsKey("style"))
                                    {
                                        var domStyle = new List<string>(domTag.Attributes["style"].Split(';'));
                                        foreach (string keyval in domStyle)
                                        {
                                            var styleKeyVal = keyval.Trim().Split(new char[] { ':' }, 2);
                                            if (styleKeyVal.Length == 2)
                                            {
                                                var kv = styleKeyVal[0].Trim().ToLower();
                                                if (domTag.Styles.ContainsKey(kv) == false)
                                                {
                                                    domTag.Styles.Add(kv, styleKeyVal[1].Trim());
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                            if (domTag.TagName != "")
                            {
                                //check if tag is script
                                if (docType == "html")
                                {
                                    if (isInScript == true)
                                    {
                                        isInScript = false;
                                    }
                                    else if (domTag.TagName == "script" && isSelfClosing == false)
                                    {
                                        isInScript = true;
                                    }

                                    //check if tag is self-closing even if it
                                    //doesn't include a forward-slash at the end
                                    switch (domTag.TagName)
                                    {
                                        case "br":
                                        case "img":
                                        case "input":
                                        case "link":
                                        case "meta":
                                        case "hr":
                                            isSelfClosing = true;
                                            break;
                                    }
                                }

                                if (domTag.TagName.Substring(0, 1) == "!")
                                {
                                    //comments & doctype are self-closing tags
                                    isSelfClosing = true;
                                }

                                if (schar[1] == '/')
                                {
                                    //found closing tag
                                    isClosingTag = true;
                                }

                                //extract text before beginning of tag
                                strText = htm.Substring(xs, x - xs).Trim();
                                if (strText != "")
                                {
                                    var textTag = new Element(this)
                                    {
                                        TagName = "#text",
                                        Text = CleanText(strText)
                                    };
                                    AddTag(textTag, parentElement, true, false, hierarchy, hierarchyIndexes);
                                }

                                //check if domTag is unusable
                                if (domTag.TagName == "" || domTag.TagName == null)
                                {
                                    foundTag = false;
                                    continue;
                                }

                                //add tag to array
                                parentElement = AddTag(domTag, parentElement, isSelfClosing, isClosingTag, hierarchy, hierarchyIndexes);
                                //parentElement = pelem;
                                if (isClosingTag == true)
                                {
                                    //go back one parent if this tag is a closing tag
                                    if (parentElement >= 0)
                                    {
                                        if (Elements[parentElement].TagName != domTag.TagName.Replace("/", ""))
                                        {
                                            //not the same tag as the current parent tag, add missing closing tag
                                            if (Elements[parentElement].ParentIndex >= 0)
                                            {
                                                if (Elements[Elements[parentElement].ParentIndex].TagName == domTag.TagName.Replace("/", ""))
                                                {
                                                    //replace unknown closing tag with missing closing tag
                                                    domTag.TagName = "/" + Elements[Elements[parentElement].ParentIndex].TagName;
                                                }
                                                else
                                                {
                                                    //skip this closing tag because it doesn't have an opening tag
                                                    //Elements.RemoveAt(Elements.Count - 1);
                                                    x = xs = s1;
                                                    continue;
                                                }
                                            }
                                        }
                                        parentElement = Elements[parentElement].ParentIndex;
                                        if (hierarchy.Count > 0)
                                        {
                                            hierarchy.RemoveAt(hierarchy.Count - 1);
                                            hierarchyIndexes.RemoveAt(hierarchyIndexes.Count - 1);
                                        }
                                    }
                                }
                            }
                            x = xs = s1;
                        }
                    }
                }
            }
            //finally, add last text tag (if possible)
            if (xs < htm.Length - 1)
            {
                if (htm.Substring(xs).Trim().Replace("\r", "").Replace("\n", "").Length > 0)
                {
                    var textTag = new Element(this)
                    {
                        TagName = "#text",
                        Text = CleanText(htm.Substring(xs))
                    };
                    AddTag(textTag, parentElement, true, false, hierarchy, hierarchyIndexes);
                }
            }
        }

        public Dictionary<string, string> GetAttributes(string tag)
        {
            var attrs = new Dictionary<string, string>();
            int s1, s2, s3, s4, s5;
            string attrName, str2;
            string[] arr;
            s1 = tag.IndexOf(" ");
            if (s1 >= 1)
            {
                for (var x = s1; x < tag.Length; x++)
                {
                    if (x >= tag.Length - 3) { break; }
                    //look for attribute name
                    s2 = tag.IndexOf("=", x);
                    s3 = tag.IndexOf(" ", x);
                    s4 = tag.IndexOf("\"", x);
                    s5 = tag.IndexOf("'", x);
                    if (s4 < 0) { s4 = tag.Length + 1; }
                    if (s5 < 0) { s5 = tag.Length + 2; }
                    if (s3 < s2 && s3 < s4 && s3 < s5)
                    {
                        //found a space first, then equal sign (=), then quotes
                        attrName = tag.Substring(s3 + 1, s2 - (s3 + 1)).ToLower();
                        //find type of quotes to use
                        if (s4 < s5 && s4 < tag.Length)
                        {
                            //use double quotes
                            arr = tag.Substring(s4 + 1).Replace("\\\"", "{{q}}").Split('"');
                            str2 = arr[0].Replace("{{q}}", "\\\"");
                            if (!attrs.ContainsKey(attrName))
                            {
                                attrs.Add(attrName, str2);
                            }
                            x = s4 + str2.Length + 1;
                        }
                        else if (s5 < tag.Length)
                        {
                            //use single quotes
                            arr = tag.Substring(s5 + 1).Replace("\\'", "{{q}}").Split('\'');
                            str2 = arr[0].Replace("{{q}}", "\\'");
                            if (!attrs.ContainsKey(attrName))
                            {
                                attrs.Add(attrName, str2);
                            }
                            x = s5 + str2.Length + 1;
                        }
                    }
                }
            }
            return attrs;
        }

        private int AddTag(Element domTag, int parentElement, bool isSelfClosing, bool isClosingTag, List<string> hierarchy, List<int> hierarchyIndexes)
        {
            domTag.ParentIndex = parentElement;
            domTag.Index = Elements.Count;
            if (domTag.Attributes == null)
            {
                domTag.Attributes = new Dictionary<string, string>();
            }
            if (hierarchyIndexes != null)
            {
                if (hierarchyIndexes.Count > 0)
                {
                    if (hierarchyIndexes[hierarchyIndexes.Count - 1] != domTag.Index)
                    {
                        parentElement = hierarchyIndexes[hierarchyIndexes.Count - 1];
                    }
                    else if (hierarchyIndexes.Count > 1)
                    {
                        parentElement = hierarchyIndexes[hierarchyIndexes.Count - 2];
                    }

                }
            }

            if (parentElement > -1)
            {
                Element parent = Elements[parentElement];
                if (parent.ChildrenIndexes == null)
                {
                    parent.ChildrenIndexes = new List<int>();
                }
                parent.ChildrenIndexes.Add(Elements.Count);
                Elements[parentElement] = parent;
            }

            //make current tag the parent
            if (isSelfClosing == false && isClosingTag == false)
            {
                parentElement = Elements.Count;
                hierarchy.Add(domTag.TagName);
                hierarchyIndexes.Add(parentElement);
            }

            domTag.IsSelfClosing = isSelfClosing;
            domTag.IsClosing = isClosingTag;
            domTag.Indexes = hierarchyIndexes.ToArray();
            Elements.Add(domTag);
            return parentElement;
        }

        private string atab = "    ";

        public string Render(bool useLineBreaks = false, bool useTabs = false)
        {
            var html = new StringBuilder();
            var tabs = "";
            if(Elements.Count == 0) { return ""; }

            //get all root elements
            foreach(var root in Elements.Where(e => e.ParentIndex == -1))
            {
                RenderElement(root, html, tabs, useLineBreaks, useTabs);
            }
            return html.ToString();
        }

        private void RenderElement(Element el, StringBuilder html, string tabs, bool useLineBreaks = false, bool useTabs = false)
        {
            if (el.IsRemoved) { return; }
            if (el.TagName == "#text")
            {
                //text node
                if (useTabs) { html.Append(tabs + atab); }
                html.Append(el.Text);
                if (useLineBreaks) { html.Append("\n"); }
            }
            else
            {
                //element node;
                if (useTabs) { html.Append(tabs + atab); }
                if (el.TagName == "!--")
                {
                    html.Append("<!--" + (useLineBreaks ? "\n" : "") + el.Text + (useLineBreaks ? "\n" : "") + "-->");
                }
                else
                {
                    if (el.Attributes != null && el.Attributes.Count > 0)
                    {
                        html.Append("<" + el.TagName + " " + string.Join(' ', el.Attributes.Select(a => a.Key + "=\"" + a.Value + "\"").ToArray()) + (el.IsSelfClosing ? "/" : "") + ">");
                    }
                    else
                    {
                        html.Append("<" + el.TagName + (el.IsSelfClosing ? "/" : "") + ">");
                    }
                    if (!el.IsSelfClosing)
                    {
                        tabs += atab;
                    }
                    else if (el.IsClosing)
                    {
                        if (tabs.Length == atab.Length || tabs.Length == 0)
                        {
                            tabs = "";
                        }
                        else
                        {
                            tabs = tabs.Substring(0, tabs.Length - 1 - atab.Length);
                        }
                    }
                }

                if (useLineBreaks) { html.Append("\n"); }
                foreach(var child in el.Children())
                {
                    RenderElement(child, html, tabs, useLineBreaks, useTabs);
                }
            }
        }

        #region "Helpers"

        private string CleanText(string text)
        {
            if (Options.ReplaceNbsp != "&nbsp;") { text = text.Replace("&nbsp;", Options.ReplaceNbsp); }
            text = NormalizeWhiteSpaces(text);
            if (Options.TrimText != TrimType.None) {
                switch (Options.TrimText)
                {
                    case TrimType.Left:
                        text = text.TrimStart();
                        break;
                    case TrimType.Right:
                        text = text.TrimEnd();
                        break;

                    case TrimType.Both:
                        text = text.Trim();
                        break;
                    case TrimType.OneTrailingSpace:
                        if(Options.ForceOneTrailingSpace || text.Substring(text.Length - 1, 1) == " ")
                        {
                            text = text.Trim() + " ";
                        }
                        else
                        {
                            text.Trim();
                        }
                        break;
                }
                
            }
            return text.Trim();
        }

        private string NormalizeWhiteSpaces(string input)
        {
            int len = input.Length,
                index = 0,
                i = 0;
            var src = input.ToCharArray();
            bool skip = false;
            char ch;
            for (; i < len; i++)
            {
                ch = src[i];
                switch (ch)
                {
                    case '\u0020': //space
                    case '\u00A0': //non-breaking space
                        //remove extra spaces
                        if (skip) continue;
                        src[index++] = ch;
                        skip = true;
                        continue;
                    case '\u1680': //ogham space mark
                    case '\u2000': //en quad
                    case '\u2001': //em quad
                    case '\u2002': //en space
                    case '\u2003': //em space
                    case '\u2004': //three-per-em space
                    case '\u2005': //four-per-em space
                    case '\u2006': //six-per-em space
                    case '\u2007': //figure space
                    case '\u2008': //punctuation space
                    case '\u2009': //thin space
                    case '\u200A': //hair space
                    case '\u202F': //narrow no-break space
                    case '\u205F': //medium mathmatical space
                    case '\u3000': //ideographic space
                    case '\u2028': //line separator
                    case '\u2029': //paragraph separator
                    case '\u0009': //character tabulation
                    case '\u000A': //line feed (LF)
                    case '\u000B': //line tabulation
                    case '\u000C': //form feed (FF)
                    case '\u000D': //carriage return (CR)
                    case '\u0085': //next line (NEL)
                        //ignore all other space-like characters
                        continue;
                    default:
                        skip = false;
                        src[index++] = ch;
                        continue;
                }
            }

            return new string(src, 0, index);
        }

        private bool OnlyAlphabet(string myStr, params string[] exceptionList)
        {
            for (int x = 0; x <= myStr.Length - 1; x++)
            {
                var result = false;
                if (Asc(myStr.Substring(x, 1)) >= Asc("a") & Asc(myStr.Substring(x, 1)) <= Asc("z"))
                {
                    result = true;
                }
                if (Asc(myStr.Substring(x, 1)) >= Asc("A") & Asc(myStr.Substring(x, 1)) <= Asc("Z"))
                {
                    result = true;
                }
                if (exceptionList.Length >= 0)
                {
                    for (int y = exceptionList.GetLowerBound(0); y <= exceptionList.GetUpperBound(0); y++)
                    {
                        if (myStr.Substring(x, 1) == exceptionList[y])
                        {
                            result = true;
                        }
                    }
                }
                if (result == false)
                {
                    return false;
                }
            }
            return true;
        }

        private int Asc(string character)
        {
            string c = character.ToString();
            if (character.Length > 1) { c = c.Substring(0, 1); }

            return Encoding.ASCII.GetBytes(character)[0];
        }
        #endregion
    }
}
