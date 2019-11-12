using System;
using System.Collections.Generic;
using System.Linq;

namespace Datasilk.Core.DOM
{
    public static class Extensions
    {
        public static IEnumerable<Element> SelectMany<TSource, TResult>(this IEnumerable<Element> source, Func<Element, IEnumerable<Element>> selector)
        {
            var items = new List<Element>();
            foreach (Element item in source)
            {
                foreach (IEnumerable<Element> result in selector(item))
                {
                    items.AddRange(result);
                }
            }
            return items.AsEnumerable();
        }
    }
}
