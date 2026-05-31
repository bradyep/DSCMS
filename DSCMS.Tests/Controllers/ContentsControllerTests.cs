using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class ContentsControllerTests
    {
        private readonly Mock<IContentRepository> _contentRepo;
        private readonly Mock<IContentTypeRepository> _contentTypeRepo;
        private readonly Mock<ITemplateRepository> _templateRepo;
        private readonly Mock<ISourceTypeRepository> _sourceTypeRepo;
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<ILogger<ContentsController>> _logger;
        private readonly ContentsController _controller;

        public ContentsControllerTests()
        {
            _contentRepo = new Mock<IContentRepository>();
            _contentTypeRepo = new Mock<IContentTypeRepository>();
            _templateRepo = new Mock<ITemplateRepository>();
            _sourceTypeRepo = new Mock<ISourceTypeRepository>();
            _userRepo = new Mock<IUserRepository>();
            _logger = new Mock<ILogger<ContentsController>>();

            _controller = new ContentsController(
                _contentRepo.Object,
                _contentTypeRepo.Object,
                _templateRepo.Object,
                _sourceTypeRepo.Object,
                _userRepo.Object,
                _logger.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithAllContents()
        {
            var contents = new List<Content>
            {
                new Content { ContentId = 1, Title = "Post 1" },
                new Content { ContentId = 2, Title = "Post 2" }
            };
            _contentRepo.Setup(r => r.GetAllWithDetailsAsync(It.IsAny<string?>())).ReturnsAsync(contents);
            _contentTypeRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContentType>());

            var result = await _controller.Index("");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Content>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ContentNotFound_ReturnsNotFound()
        {
            _contentRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Content?)null);

            var result = await _controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ContentFound_ReturnsViewWithContent()
        {
            var content = new Content { ContentId = 1, Title = "Post 1" };
            _contentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(content);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Content>(viewResult.Model);
            Assert.Equal(1, model.ContentId);
        }

        [Fact]
        public async Task Edit_NullId_ReturnsNotFound()
        {
            var result = await _controller.Edit((int?)null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ContentNotFound_ReturnsNotFound()
        {
            _contentRepo.Setup(r => r.GetByIdWithFieldItemsAsync(99)).ReturnsAsync((Content?)null);

            var result = await _controller.Edit(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ContentFound_ReturnsViewWithContent()
        {
            var content = new Content { ContentId = 5, Title = "My Post" };
            _contentRepo.Setup(r => r.GetByIdWithFieldItemsAsync(5)).ReturnsAsync(content);
            _contentTypeRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContentType>());
            _templateRepo.Setup(r => r.GetByIsForMultipleContentsAsync(It.IsAny<int>())).ReturnsAsync(new List<Template>());
            _sourceTypeRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SourceType>());
            _userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ApplicationUser>());

            var result = await _controller.Edit(5);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Content>(viewResult.Model);
            Assert.Equal(5, model.ContentId);
        }

        [Fact]
        public async Task Delete_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ContentNotFound_ReturnsNotFound()
        {
            _contentRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Content?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ContentFound_ReturnsViewWithContent()
        {
            var content = new Content { ContentId = 3, Title = "To Delete" };
            _contentRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(content);

            var result = await _controller.Delete(3);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Content>(viewResult.Model);
        }

        [Fact]
        public async Task DeleteConfirmed_CallsDeleteAndRedirects()
        {
            var content = new Content { ContentId = 3, Title = "To Delete" };
            _contentRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(content);

            var result = await _controller.DeleteConfirmed(3);

            _contentRepo.Verify(r => r.DeleteAsync(content), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
    }
}
