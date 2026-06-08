using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class TemplatesControllerTests
    {
        private readonly Mock<ITemplateRepository> _templateRepo;
        private readonly Mock<ILayoutRepository> _layoutRepo;
        private readonly Mock<ILogger<TemplatesController>> _logger;
        private readonly TemplatesController _controller;

        public TemplatesControllerTests()
        {
            _templateRepo = new Mock<ITemplateRepository>();
            _layoutRepo = new Mock<ILayoutRepository>();
            _logger = new Mock<ILogger<TemplatesController>>();
            _controller = new TemplatesController(_templateRepo.Object, _layoutRepo.Object, _logger.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithTemplates()
        {
            var templates = new List<Template>
            {
                new Template { TemplateId = 1, Name = "Blog Post" },
                new Template { TemplateId = 2, Name = "Blog Index" }
            };
            _templateRepo.Setup(r => r.GetAllWithLayoutAsync()).ReturnsAsync(templates);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<TemplateListDto>>(ok.Value);
            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public async Task GetById_TemplateNotFound_ReturnsNotFound()
        {
            _templateRepo.Setup(r => r.GetByIdWithLayoutAsync(99)).ReturnsAsync((Template?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetById_TemplateFound_ReturnsOkWithDto()
        {
            var template = new Template { TemplateId = 1, Name = "Blog Post", TemplateSource = "/src", SourceTypeId = 1 };
            _templateRepo.Setup(r => r.GetByIdWithLayoutAsync(1)).ReturnsAsync(template);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<TemplateDetailDto>(ok.Value);
            Assert.Equal(1, dto.TemplateId);
        }

        [Fact]
        public async Task Delete_TemplateNotFound_ReturnsNotFound()
        {
            _templateRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Template?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_TemplateExists_CallsDeleteAndReturnsNoContent()
        {
            var template = new Template { TemplateId = 1, Name = "Blog Post" };
            _templateRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);

            var result = await _controller.Delete(1);

            _templateRepo.Verify(r => r.DeleteAsync(template), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Create_ValidDto_ReturnsCreatedAtAction()
        {
            var dto = new TemplateCreateDto { Name = "New Template", TemplateSource = "/src", SourceTypeId = 1 };
            _templateRepo.Setup(r => r.AddAsync(It.IsAny<Template>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), created.ActionName);
        }

        [Fact]
        public async Task GetFormOptions_ReturnsOkWithLayouts()
        {
            var layouts = new List<Layout> { new Layout { LayoutId = 1, Name = "Main" } };
            _layoutRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(layouts);

            var result = await _controller.GetFormOptions();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<TemplateFormOptionsDto>(ok.Value);
            Assert.Single(dto.Layouts);
        }
    }
}
