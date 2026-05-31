using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class ContentTypeFieldItemsControllerTests
    {
        private readonly Mock<IContentTypeFieldItemRepository> _itemRepo;
        private readonly Mock<IContentRepository> _contentRepo;
        private readonly Mock<IContentTypeFieldRepository> _fieldRepo;
        private readonly ContentTypeFieldItemsController _controller;

        public ContentTypeFieldItemsControllerTests()
        {
            _itemRepo = new Mock<IContentTypeFieldItemRepository>();
            _contentRepo = new Mock<IContentRepository>();
            _fieldRepo = new Mock<IContentTypeFieldRepository>();

            _controller = new ContentTypeFieldItemsController(_itemRepo.Object, _contentRepo.Object, _fieldRepo.Object);
        }

        [Fact]
        public async Task CreateGet_WithContentId_ContentMissing_ReturnsNotFound()
        {
            _contentRepo.Setup(r => r.GetAllSimpleAsync()).ReturnsAsync(new List<Content>());
            _contentRepo.Setup(r => r.GetByIdWithContentTypeAsync(5)).ReturnsAsync((Content?)null);

            var result = await _controller.Create(5);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreatePost_ContentMissing_ReturnsNotFound()
        {
            var item = new ContentTypeFieldItem { ContentId = 7, ContentTypeFieldId = 1, Value = "Hello" };
            _contentRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((Content?)null);

            var result = await _controller.Create(item);

            Assert.IsType<NotFoundResult>(result);
            _itemRepo.Verify(r => r.AddAsync(It.IsAny<ContentTypeFieldItem>()), Times.Never);
        }

        [Fact]
        public async Task CreatePost_Valid_AddsAndRedirects()
        {
            var item = new ContentTypeFieldItem { ContentId = 7, ContentTypeFieldId = 1, Value = "Hello" };
            _contentRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new Content { ContentId = 7, UrlToDisplay = "post" });

            var result = await _controller.Create(item);

            _itemRepo.Verify(r => r.AddAsync(item), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal("Contents", redirect.ControllerName);
            Assert.Equal(7, redirect.RouteValues!["id"]);
        }

        [Fact]
        public async Task DeleteConfirmed_MissingRelatedEntities_ReturnsNotFound()
        {
            _itemRepo.Setup(r => r.GetByIdWithContentAsync(3)).ReturnsAsync((ContentTypeFieldItem?)null);
            _itemRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new ContentTypeFieldItem { ContentTypeFieldItemId = 3 });

            var result = await _controller.DeleteConfirmed(3);

            Assert.IsType<NotFoundResult>(result);
            _itemRepo.Verify(r => r.DeleteAsync(It.IsAny<ContentTypeFieldItem>()), Times.Never);
        }

        [Fact]
        public async Task DeleteConfirmed_Valid_DeletesAndRedirects()
        {
            var withContent = new ContentTypeFieldItem
            {
                ContentTypeFieldItemId = 3,
                Content = new Content { ContentId = 11, UrlToDisplay = "x" }
            };
            var direct = new ContentTypeFieldItem { ContentTypeFieldItemId = 3, ContentId = 11 };

            _itemRepo.Setup(r => r.GetByIdWithContentAsync(3)).ReturnsAsync(withContent);
            _itemRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(direct);

            var result = await _controller.DeleteConfirmed(3);

            _itemRepo.Verify(r => r.DeleteAsync(direct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal("Contents", redirect.ControllerName);
            Assert.Equal(11, redirect.RouteValues!["id"]);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
