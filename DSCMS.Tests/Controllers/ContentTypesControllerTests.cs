using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class ContentTypesControllerTests
    {
        private readonly Mock<IContentTypeRepository> _contentTypeRepo;
        private readonly Mock<ITemplateRepository> _templateRepo;
        private readonly Mock<ILogger<ContentTypesController>> _logger;
        private readonly ContentTypesController _controller;

        public ContentTypesControllerTests()
        {
            _contentTypeRepo = new Mock<IContentTypeRepository>();
            _templateRepo = new Mock<ITemplateRepository>();
            _logger = new Mock<ILogger<ContentTypesController>>();
            _controller = new ContentTypesController(_contentTypeRepo.Object, _templateRepo.Object, _logger.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithAllContentTypes()
        {
            var contentTypes = new List<ContentType>
            {
                new ContentType { ContentTypeId = 1, Name = "blog", Title = "Blog" },
                new ContentType { ContentTypeId = 2, Name = "news", Title = "News" }
            };
            _contentTypeRepo.Setup(r => r.GetAllWithTemplateAsync()).ReturnsAsync(contentTypes);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<ContentTypeListDto>>(ok.Value);
            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public async Task GetById_ContentTypeNotFound_ReturnsNotFound()
        {
            _contentTypeRepo.Setup(r => r.GetByIdWithTemplateAsync(99)).ReturnsAsync((ContentType?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetById_ContentTypeFound_ReturnsOkWithDto()
        {
            var contentType = new ContentType { ContentTypeId = 1, Name = "blog", Title = "Blog" };
            _contentTypeRepo.Setup(r => r.GetByIdWithTemplateAsync(1)).ReturnsAsync(contentType);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ContentTypeDetailDto>(ok.Value);
            Assert.Equal(1, dto.ContentTypeId);
        }

        [Fact]
        public async Task Delete_ContentTypeNotFound_ReturnsNotFound()
        {
            _contentTypeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ContentType?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ContentTypeFound_CallsDeleteAndReturnsNoContent()
        {
            var contentType = new ContentType { ContentTypeId = 2, Name = "news", Title = "News" };
            _contentTypeRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(contentType);

            var result = await _controller.Delete(2);

            _contentTypeRepo.Verify(r => r.DeleteAsync(contentType), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Create_ValidDto_ReturnsCreatedAtAction()
        {
            var dto = new ContentTypeCreateDto { Name = "news", Title = "News", ItemsPerPage = 10, MultipleContentsTemplateId = 1 };
            _contentTypeRepo.Setup(r => r.AddAsync(It.IsAny<ContentType>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), created.ActionName);
        }

        [Fact]
        public async Task GetFormOptions_ReturnsOkWithTemplateLists()
        {
            var multipleTemplates = new List<Template> { new Template { TemplateId = 1, Name = "List" } };
            var singleTemplates = new List<Template> { new Template { TemplateId = 2, Name = "Detail" } };
            _templateRepo.Setup(r => r.GetByIsForMultipleContentsAsync(1)).ReturnsAsync(multipleTemplates);
            _templateRepo.Setup(r => r.GetByIsForMultipleContentsAsync(0)).ReturnsAsync(singleTemplates);

            var result = await _controller.GetFormOptions();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ContentTypeFormOptionsDto>(ok.Value);
            Assert.Single(dto.MultipleContentsTemplates);
            Assert.Equal(2, dto.SingleContentTemplates.Count); // 1 blank + 1 real
        }
    }
}
