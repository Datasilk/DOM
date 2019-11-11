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

### ParserOptions

|Property|Default|Description
|----
|ReplaceNbsp|`&nbsp;`|Replaces HTML encoded spaces with the provided string
|TrimText|`TrimType.None`|Trims spaces from `#text` nodes. **NOTE:** Parser automatically removes any duplicate spaces from `#text` nodes.

### TrimType
An `enum` used as `ParserOptions.TrimText`

|Label|Value|Description
|----
|None|0|Parser does not trim any spaces from the beginning or end of all `#text` nodes
|Right|1|Parser trims all spaces from the end of all `#text` nodes
|Left|2|Parser trims all spaces from the beginning of all `#text` nodes
|Both|3|Parser trims all spaces from the beginning and end of all `#text` nodes
|OneTrailingSpace|4|Parser trims all spaces from the beginning and end of all `#text` nodes, and if there is a space at the end of the `#text` node, it will not be removed