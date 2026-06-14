using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class DSCMSControllerTests
    {
        private readonly Mock<IContentRepository> _contentRepo;
        private readonly Mock<IContentTypeRepository> _contentTypeRepo;
        private readonly Mock<ITemplateRepository> _templateRepo;
        private readonly Mock<ILogger<DSCMSController>> _logger;
        private readonly DSCMSController _controller;

        public DSCMSControllerTests()
        {
            _contentRepo = new Mock<IContentRepository>();
            _contentTypeRepo = new Mock<IContentTypeRepository>();
            _templateRepo = new Mock<ITemplateRepository>();
            _logger = new Mock<ILogger<DSCMSController>>();

            _controller = new DSCMSController(
                _contentRepo.Object,
                _contentTypeRepo.Object,
                _templateRepo.Object,
                _logger.Object);
        }

        [Fact]
        public async Task Content_ContentTypeNotFound_ReturnsWelcomeView()
        {
            _contentTypeRepo.Setup(r => r.GetByNameAsync("blog")).ReturnsAsync((ContentType?)null);

            var result = await _controller.Content("blog");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/DSCMS/Welcome.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async Task Content_ContentTypeFound_NoUrl_ReturnsContentTypeView()
        {
            var contentType = new ContentType
            {
                ContentTypeId = 1,
                Name = "blog",
                Title = "Blog",
                MultipleContentsTemplateId = 0
            };
            _contentTypeRepo.Setup(r => r.GetByNameAsync("blog")).ReturnsAsync(contentType);
            _contentRepo.Setup(r => r.GetByContentTypeIdWithDetailsAsync(1)).ReturnsAsync(new List<Content>());

            var result = await _controller.Content("blog");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ContentType>(viewResult.Model);
        }

        [Fact]
        public async Task Content_ContentTypeFound_WithUrl_ContentNotFound_ReturnsNotFound()
        {
            var contentType = new ContentType { ContentTypeId = 1, Name = "blog" };
            _contentTypeRepo.Setup(r => r.GetByNameAsync("blog")).ReturnsAsync(contentType);
            _contentRepo.Setup(r => r.GetByUrlAndContentTypeAsync("missing-post", 1)).ReturnsAsync((Content?)null);

            var result = await _controller.Content("blog", "missing-post");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Content_ContentTypeFound_WithUrl_ContentFound_NoTemplate_ReturnsRawHtml()
        {
            var contentType = new ContentType
            {
                ContentTypeId = 1,
                Name = "blog",
                DefaultSingleContentTemplateId = null
            };
            var content = new Content
            {
                ContentId = 1,
                TemplateId = 0,
                BodySource = "<p>Hello</p>",
                Title = "Test Post"
            };
            _contentTypeRepo.Setup(r => r.GetByNameAsync("blog")).ReturnsAsync(contentType);
            _contentRepo.Setup(r => r.GetByUrlAndContentTypeAsync("test-post", 1)).ReturnsAsync(content);

            var result = await _controller.Content("blog", "test-post");

            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/html", contentResult.ContentType);
            Assert.Equal("<p>Hello</p>", contentResult.Content);
        }

        [Fact]
        public async Task Content_ContentTypeFound_WithUrl_ContentFound_WithTemplate_ReturnsContentView()
        {
            var contentType = new ContentType { ContentTypeId = 1, Name = "blog" };
            var template = new Template
            {
                TemplateId = 5,
                TemplateSource = "/Views/DSCMS/Templates/Post.cshtml",
                Layout = new Layout { LayoutSource = "/Views/DSCMS/Layouts/_Blog.cshtml" }
            };
            var content = new Content
            {
                ContentId = 1,
                TemplateId = 5,
                Title = "Test Post",
                BodySource = "<p>Hello</p>"
            };
            _contentTypeRepo.Setup(r => r.GetByNameAsync("blog")).ReturnsAsync(contentType);
            _contentRepo.Setup(r => r.GetByUrlAndContentTypeAsync("test-post", 1)).ReturnsAsync(content);
            _templateRepo.Setup(r => r.GetByIdWithLayoutAsync(5)).ReturnsAsync(template);

            var result = await _controller.Content("blog", "test-post");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("/Views/DSCMS/Templates/Post.cshtml", viewResult.ViewName);
            Assert.IsType<Content>(viewResult.Model);
        }

        [Fact]
        public async Task Content_ContentTypeName_IsCaseFolded()
        {
            _contentTypeRepo.Setup(r => r.GetByNameAsync("blog")).ReturnsAsync((ContentType?)null);

            await _controller.Content("BLOG");

            _contentTypeRepo.Verify(r => r.GetByNameAsync("blog"), Times.Once);
        }
    }
}
