# DOM
A C# class library for parsing HTML into a searchable, filterable DOM tree

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