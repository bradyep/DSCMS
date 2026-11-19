using DSCMS.Repositories.Interfaces;
using DSCMS.Services;
using Moq;

namespace DSCMS.Tests.Services
{
    public class CmsPageLookupTests
    {
        private readonly Mock<IContentTypeRepository> _contentTypeRepo = new();
        private readonly Mock<IContentRepository> _contentRepo = new();
        private readonly CmsPageLookup _lookup;

        public CmsPageLookupTests()
        {
            _contentTypeRepo.Setup(r => r.GetIdByNameAsync("games")).ReturnsAsync(3);
            _contentRepo.Setup(r => r.ExistsByUrlAndContentTypeAsync("shapeshifter", 3)).ReturnsAsync(true);
            _lookup = new CmsPageLookup(_contentTypeRepo.Object, _contentRepo.Object);
        }

        [Theory]
        [InlineData("games")]
        [InlineData("Games/")]
        [InlineData("games/shapeshifter")]
        [InlineData("/GAMES/Shapeshifter/")]
        public async Task ExistingTypeOrContent_IsCmsPage(string path)
        {
            Assert.True(await _lookup.IsCmsPageAsync(path));
        }

        [Theory]
        [InlineData("ruffle")]                       // unknown type: DSCMSController shows the Welcome page
        [InlineData("games/idemon-unknown")]         // known type, unknown content: DSCMSController returns 404
        [InlineData("games/shapeshifter/assets")]    // deeper than the cms route
        [InlineData("")]
        public async Task OtherPaths_AreNotCmsPages(string path)
        {
            Assert.False(await _lookup.IsCmsPageAsync(path));
        }
    }
}
