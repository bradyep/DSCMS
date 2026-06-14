using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class ContentTypeFieldsControllerTests
    {
        private readonly Mock<IContentTypeFieldRepository> _fieldRepo;
        private readonly Mock<IContentTypeRepository> _contentTypeRepo;
        private readonly ContentTypeFieldsController _controller;

        public ContentTypeFieldsControllerTests()
        {
            _fieldRepo = new Mock<IContentTypeFieldRepository>();
            _contentTypeRepo = new Mock<IContentTypeRepository>();
            _controller = new ContentTypeFieldsController(_fieldRepo.Object, _contentTypeRepo.Object);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_FieldNotFound_ReturnsNotFound()
        {
            _fieldRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ContentTypeField?)null);

            var result = await _controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_FieldFound_ReturnsView()
        {
            var field = new ContentTypeField { ContentTypeFieldId = 1, Name = "Title" };
            _fieldRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(field);

            var result = await _controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(field, view.Model);
        }

        [Fact]
        public async Task CreatePost_ContentTypeMissing_ReturnsNotFound()
        {
            var field = new ContentTypeField { ContentTypeId = 123, Name = "Title" };
            _contentTypeRepo.Setup(r => r.GetByIdAsync(123)).ReturnsAsync((ContentType?)null);

            var result = await _controller.Create(field);

            Assert.IsType<NotFoundResult>(result);
            _fieldRepo.Verify(r => r.AddAsync(It.IsAny<ContentTypeField>()), Times.Never);
        }

        [Fact]
        public async Task CreatePost_ValidModel_AddsAndRedirectsToContentTypeEdit()
        {
            var field = new ContentTypeField { ContentTypeId = 10, Name = "Summary" };
            _contentTypeRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new ContentType { ContentTypeId = 10, Name = "blog" });

            var result = await _controller.Create(field);

            _fieldRepo.Verify(r => r.AddAsync(field), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal("ContentTypes", redirect.ControllerName);
            Assert.Equal(10, redirect.RouteValues!["id"]);
        }

        [Fact]
        public async Task DeleteConfirmed_MissingFieldOrContentType_ReturnsNotFound()
        {
            _fieldRepo.Setup(r => r.GetByIdWithContentTypeAsync(5)).ReturnsAsync((ContentTypeField?)null);
            _fieldRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new ContentTypeField { ContentTypeFieldId = 5 });

            var result = await _controller.DeleteConfirmed(5);

            Assert.IsType<NotFoundResult>(result);
            _fieldRepo.Verify(r => r.DeleteAsync(It.IsAny<ContentTypeField>()), Times.Never);
        }

        [Fact]
        public async Task DeleteConfirmed_Valid_DeletesAndRedirects()
        {
            var withType = new ContentTypeField
            {
                ContentTypeFieldId = 5,
                ContentType = new ContentType { ContentTypeId = 20, Name = "blog" }
            };
            var direct = new ContentTypeField { ContentTypeFieldId = 5, ContentTypeId = 20, Name = "Summary" };

            _fieldRepo.Setup(r => r.GetByIdWithContentTypeAsync(5)).ReturnsAsync(withType);
            _fieldRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(direct);

            var result = await _controller.DeleteConfirmed(5);

            _fieldRepo.Verify(r => r.DeleteAsync(direct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal("ContentTypes", redirect.ControllerName);
            Assert.Equal(20, redirect.RouteValues!["id"]);
        }
    }
}
