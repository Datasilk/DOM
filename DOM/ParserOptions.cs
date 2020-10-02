namespace Datasilk.Core.DOM
{
    public enum TrimType
    {
        None = 0,
        Right = 1,
        Left = 2,
        Both = 3,
        OneTrailingSpace = 4
    }

    public class ParserOptions
    {
        public string ReplaceNbsp { get; set; } = "&nbsp;";
        public TrimType TrimText { get; set; } = TrimType.OneTrailingSpace;
        public bool ForceOneTrailingSpace = false;
    }
}
