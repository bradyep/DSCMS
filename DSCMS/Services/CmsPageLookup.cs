using DSCMS.Repositories.Interfaces;

namespace DSCMS.Services
{
  /// <summary>
  /// Answers whether a URL path is a page <see cref="Controllers.DSCMSController"/> would actually render
  /// (a content type listing or one of its contents). Uploaded static files give way to these.
  /// </summary>
  public interface ICmsPageLookup
  {
    /// <param name="path">A URL path such as "games/shapeshifter" (leading/trailing slashes are ignored).</param>
    Task<bool> IsCmsPageAsync(string path);
  }

  public class CmsPageLookup : ICmsPageLookup
  {
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IContentRepository _contentRepository;

    public CmsPageLookup(IContentTypeRepository contentTypeRepository, IContentRepository contentRepository)
    {
      _contentTypeRepository = contentTypeRepository;
      _contentRepository = contentRepository;
    }

    public async Task<bool> IsCmsPageAsync(string path)
    {
      // Mirrors the "cms" route ({contentTypeName}/{contentUrl?}) and DSCMSController's lookups, which lowercase both parts.
      // An unknown content type falls through to the Welcome page there, which doesn't count as a page.
      var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
      if (segments.Length is 0 or > 2)
      {
        return false;
      }

      var contentTypeId = await _contentTypeRepository.GetIdByNameAsync(segments[0].ToLower());
      if (contentTypeId == null)
      {
        return false;
      }

      return segments.Length == 1
        || await _contentRepository.ExistsByUrlAndContentTypeAsync(segments[1].ToLower(), contentTypeId.Value);
    }
  }
}
