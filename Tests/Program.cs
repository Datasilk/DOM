using System;
using System.IO;
using System.Linq;
using Datasilk.Core.DOM;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = AppContext.BaseDirectory;

            //load HTML file from https://www.markentingh.com landing page
            var html = File.ReadAllText(path + "Html\\test.html");

            //parse DOM
            var dom = Html.Parse(html);

            //get a list of bars that are colored
            var elems = dom.Elements.Where(el => el.ClassNames.Contains("bar")).SelectMany((el, results) => el.Children().Where(c => c.ClassNames.Any(a => a.IndexOf("color_") >= 0))).ToList();

            //replace all project elements with the child H5 tag
            var projects = dom.Elements.Where(el => el.ClassNames.Contains("project"));
            foreach(var project in projects)
            {
                var child = project.AllChildren().FirstOrDefault(a => a.TagName == "h5");
                if(child != null)
                {
                    project.ReplaceWith(child);
                }
            }

            Console.Write(dom.Render());
        }
    }
}
