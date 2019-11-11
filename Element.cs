using System.Collections.Generic;

namespace Datasilk.Core.DOM
{
    public class Element
    {
        public Html Parser;
        public bool IsSelfClosing; // <tag/>
        public bool IsClosing; // </tag>
        public int Index;
        public int ParentIndex;
        public int[] Indexes;
        public string TagName;
        public string Text;
        public string Id;
        public List<string> ClassNames;
        public List<int> ChildenIndexes;
        public Dictionary<string, string> Attributes;
        public Dictionary<string, string> Styles;

        private int _nextSibling = -1;
        private int _firstChild = -1;
        private bool _nofirstChild = false;

        public Element(Html parser)
        {
            Parser = parser;
        }

        public List<Element> Children(int limit = 0)
        {
            var items = new List<Element>();
            if (ChildenIndexes == null) { return items; }
            foreach (var x in ChildenIndexes)
            {
                items.Add(Parser.Elements[x]);
                if (limit > 0) { if (x + 1 == limit) { return items; } }
            }
            return items;
        }

        public Element FirstChild
        {
            get
            {
                if (_firstChild >= 0)
                {
                    return Parser.Elements[_firstChild];
                }
                if (ChildenIndexes != null)
                {
                    if (ChildenIndexes.Count > 0)
                    {
                        _firstChild = ChildenIndexes[0];
                        return Parser.Elements[_firstChild];
                    }

                }
                if (!_nofirstChild && Index < Parser.Elements.Count - 1)
                {
                    var hierarchy = string.Join(">", Indexes);
                    for (var x = Index + 1; x < Parser.Elements.Count; x++)
                    {
                        if (Parser.Elements[x].Indexes.Length == Indexes.Length + 1)
                        {
                            var childhier = string.Join(">", Parser.Elements[x].Indexes);
                            if (childhier.IndexOf(hierarchy) >= 0)
                            {
                                _firstChild = x;
                                return Parser.Elements[x];
                            }
                            else
                            {
                                break;
                            }
                        }
                        else if (Parser.Elements[x].Indexes.Length < Indexes.Length + 1)
                        {
                            break;
                        }

                    }
                    _nofirstChild = true;
                }



                return null;
            }
        }

        public Element NextSibling
        {
            get
            {
                if (_nextSibling >= 0) { return Parser.Elements[_nextSibling]; }
                var hierarchy = string.Join(">", Indexes);
                var len = Indexes.Length;
                for (var x = Index + 1; x < Parser.Elements.Count; x++)
                {
                    var elem = Parser.Elements[x];
                    if (elem.Indexes.Length == len)
                    {
                        var child = string.Join(">", elem.Indexes);
                        if (hierarchy == child)
                        {
                            return elem;
                        }
                        else { return null; }
                    }
                    else if (elem.Indexes.Length < len) { return null; }
                }
                return null;
            }
        }

        public Element Parent
        {
            get
            {
                if (Indexes.Length == 0) { return null; }
                return Parser.Elements[Indexes[Indexes.Length - 1]];
            }
        }

        public int HierarchyTagIndex(string tag)
        {
            for (var x = Indexes.Length - 1; x >= 0; x--)
            {
                if (Parser.Elements[Indexes[x]].TagName == tag)
                {
                    return x;
                }
            }
            return -1;
        }

        public bool HasTagInHierarchy(string tag)
        {
            return HierarchyTagIndex(tag) >= 0;
        }

        public List<string> HierarchyTags()
        {
            var tags = new List<string>();
            for (var x = 0; x < Indexes.Length; x++)
            {
                tags.Add(Parser.Elements[Indexes[x]].TagName);
            }
            return tags;
        }
    }
}
