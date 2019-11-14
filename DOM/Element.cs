using System.Collections.Generic;
using System.Linq;

namespace Datasilk.Core.DOM
{
    public class Element
    {
        public Html Dom;
        public bool IsSelfClosing; // <tag/>
        public bool IsClosing; // </tag>
        public bool IsRemoved = false; // if True, flagged for removal from DOM.
        public int Index;
        public int ParentIndex;
        public int[] Indexes;
        public string TagName;
        public string Text;
        public string Id;
        public List<string> ClassNames = new List<string>();
        public List<int> ChildrenIndexes = new List<int>();
        public Dictionary<string, string> Attributes;
        public Dictionary<string, string> Styles;

        private int _nextSibling = -1;
        private int _firstChild = -1;
        private bool _nofirstChild = false;

        public Element(Html parser)
        {
            Dom = parser;
        }

        public List<Element> Children()
        {
            return _Children();
        }

        private List<Element> _Children(bool traverse = false)
        {
            if (traverse)
            {
                var children = _Children();
                children.AddRange(_Children().SelectMany((c, results) => c._Children(true)).ToList());
                return children;
            }
            else
            {
                return ChildrenIndexes.Select(c => Dom.Elements[c]).ToList();
            }
        }

        public List<Element> AllChildren()
        {
            return _Children(true);
        }

        public Element FirstChild
        {
            get
            {
                if (_firstChild >= 0)
                {
                    return Dom.Elements[_firstChild];
                }
                if (ChildrenIndexes != null)
                {
                    if (ChildrenIndexes.Count > 0)
                    {
                        _firstChild = ChildrenIndexes[0];
                        return Dom.Elements[_firstChild];
                    }

                }
                if (!_nofirstChild && Index < Dom.Elements.Count - 1)
                {
                    var hierarchy = string.Join(">", Indexes);
                    for (var x = Index + 1; x < Dom.Elements.Count; x++)
                    {
                        if (Dom.Elements[x].Indexes.Length == Indexes.Length + 1)
                        {
                            var childhier = string.Join(">", Dom.Elements[x].Indexes);
                            if (childhier.IndexOf(hierarchy) >= 0)
                            {
                                _firstChild = x;
                                return Dom.Elements[x];
                            }
                            else
                            {
                                break;
                            }
                        }
                        else if (Dom.Elements[x].Indexes.Length < Indexes.Length + 1)
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
                if (_nextSibling >= 0) { return Dom.Elements[_nextSibling]; }
                var hierarchy = string.Join(">", Indexes);
                var len = Indexes.Length;
                for (var x = Index + 1; x < Dom.Elements.Count; x++)
                {
                    var elem = Dom.Elements[x];
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
                if (Indexes.Length < 2) { return null; }
                return Dom.Elements[Indexes[Indexes.Length - 2]];
            }
        }

        public List<Element> Parents()
        {
            if(Indexes.Length > 1)
            {
                return Indexes.SkipLast(1).Select(i => Dom.Elements[i]).ToList();
            }
            else
            {
                return new List<Element>();
            }
        }

        public void ReplaceWith(Element element)
        {
            element.IsRemoved = true;
            element.ParentIndex = ParentIndex;
            Parent.ChildrenIndexes = Parent.ChildrenIndexes.Select(i => i == Index ? element.Index : i).ToList();
            var children = AllChildren();
            foreach(var child in children)
            {
                child.IsRemoved = true;
            }
            element.IsRemoved = false;
            children = element.AllChildren();
            foreach (var child in children)
            {
                child.IsRemoved = false;
            }
        }

        public int HierarchyTagIndex(string tag)
        {
            for (var x = Indexes.Length - 1; x >= 0; x--)
            {
                if (Dom.Elements[Indexes[x]].TagName == tag)
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
                tags.Add(Dom.Elements[Indexes[x]].TagName);
            }
            return tags;
        }
    }
}
