### Basic usage
```
var html = "<html><head></head><body><div>Hello World!</div></body></html>";
var dom = Html.Parse(html, 
	new ParserOptions()
	{
		ReplaceNbsp = " ",
		TrimText = TrimType.OneTrailingSpace
	});
```

### Use LINQ queries
```
var elemText = dom.Elements.Where(el => el.Text.Contains("Hello")).FirstOrDefault();
```
### Navigate DOM Hierarchy
```
// select all child nodes from all DIV tags
var nodes = dom.Elements.Where(el => el.TagName == "div").SelectMany((el, results) => el.Children());
```

### Replace one `Element` with another
```

foreach(var elem in dom.Elements.Where(el => el.ClassNames.Contains("button")))
{
	var button = elem.AllChildren.First(el => el.TagName == "a");
	elem.ReplaceWith(button)
}
```