using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class ContentTypesControllerTests
    {
        private readonly Mock<IContentTypeRepository> _contentTypeRepo;
        private readonly Mock<ITemplateRepository> _templateRepo;
        private readonly ContentTypesController _controller;

        public ContentTypesControllerTests()
        {
            _contentTypeRepo = new Mock<IContentTypeRepository>();
            _templateRepo = new Mock<ITemplateRepository>();
            _controller = new ContentTypesController(_contentTypeRepo.Object, _templateRepo.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithAllContentTypes()
        {
            var contentTypes = new List<ContentType>
            {
                new ContentType { ContentTypeId = 1, Name = "blog" },
                new ContentType { ContentTypeId = 2, Name = "news" }
            };
            _contentTypeRepo.Setup(r => r.GetAllWithTemplateAsync()).ReturnsAsync(contentTypes);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<ContentType>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ContentTypeNotFound_ReturnsNotFound()
        {
            _contentTypeRepo.Setup(r => r.GetByIdWithTemplateAsync(99)).ReturnsAsync((ContentType?)null);

            var result = await _controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ContentTypeFound_ReturnsViewWithModel()
        {
            var contentType = new ContentType { ContentTypeId = 1, Name = "blog" };
            _contentTypeRepo.Setup(r => r.GetByIdWithTemplateAsync(1)).ReturnsAsync(contentType);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ContentType>(viewResult.Model);
            Assert.Equal(1, model.ContentTypeId);
        }

        [Fact]
        public async Task Edit_NullId_ReturnsNotFound()
        {
            var result = await _controller.Edit((int?)null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ContentTypeNotFound_ReturnsNotFound()
        {
            _contentTypeRepo.Setup(r => r.GetByIdWithFieldsAsync(99)).ReturnsAsync((ContentType?)null);

            var result = await _controller.Edit(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ContentTypeFound_ReturnsViewWithModel()
        {
            var contentType = new ContentType { ContentTypeId = 1, Name = "blog" };
            _contentTypeRepo.Setup(r => r.GetByIdWithFieldsAsync(1)).ReturnsAsync(contentType);
            _templateRepo.Setup(r => r.GetByIsForMultipleContentsAsync(It.IsAny<int>())).ReturnsAsync(new List<Template>());

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ContentType>(viewResult.Model);
            Assert.Equal(1, model.ContentTypeId);
        }

        [Fact]
        public async Task Delete_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ContentTypeNotFound_ReturnsNotFound()
        {
            _contentTypeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ContentType?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ContentTypeFound_ReturnsViewWithModel()
        {
            var contentType = new ContentType { ContentTypeId = 2, Name = "news" };
            _contentTypeRepo.Setup(r => r.GetByIdWithTemplateAsync(2)).ReturnsAsync(contentType);

            var result = await _controller.Delete(2);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ContentType>(viewResult.Model);
        }

        [Fact]
        public async Task DeleteConfirmed_CallsDeleteAndRedirects()
        {
            var contentType = new ContentType { ContentTypeId = 2, Name = "news" };
            _contentTypeRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(contentType);

            var result = await _controller.DeleteConfirmed(2);

            _contentTypeRepo.Verify(r => r.DeleteAsync(contentType), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
    }
}
