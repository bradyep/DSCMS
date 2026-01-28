namespace DSCMS.Models
{
  /// <summary>
  /// Defines the available source types for content rendering
  /// </summary>
  public enum SourceTypeEnum
  {
    RazorFile = 1,
    InlineRazor = 2,
    Markdown = 3,
    HTML = 4,
    Text = 5
  }
}