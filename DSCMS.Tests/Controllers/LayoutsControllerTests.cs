using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class LayoutsControllerTests
    {
        private readonly Mock<ILayoutRepository> _layoutRepo;
        private readonly Mock<ILogger<LayoutsController>> _logger;
        private readonly LayoutsController _controller;

        public LayoutsControllerTests()
        {
            _layoutRepo = new Mock<ILayoutRepository>();
            _logger = new Mock<ILogger<LayoutsController>>();
            _controller = new LayoutsController(_layoutRepo.Object, _logger.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithLayouts()
        {
            var layouts = new List<Layout>
            {
                new Layout { LayoutId = 1, Name = "Bootstrap Blog", LayoutSource = "/Views/DSCMS/Layouts/_Blog.cshtml" }
            };
            _layoutRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(layouts);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<LayoutListDto>>(ok.Value);
            Assert.Single(dtos);
        }

        [Fact]
        public async Task GetById_LayoutNotFound_ReturnsNotFound()
        {
            _layoutRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Layout?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetById_LayoutFound_ReturnsOkWithDto()
        {
            var layout = new Layout { LayoutId = 1, Name = "My Layout", LayoutSource = "/src", SourceTypeId = 1 };
            _layoutRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(layout);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<LayoutDetailDto>(ok.Value);
            Assert.Equal(1, dto.LayoutId);
        }

        [Fact]
        public async Task Delete_LayoutNotFound_ReturnsNotFound()
        {
            _layoutRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Layout?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_LayoutExists_CallsDeleteAndReturnsNoContent()
        {
            var layout = new Layout { LayoutId = 1, Name = "Old Layout" };
            _layoutRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(layout);

            var result = await _controller.Delete(1);

            _layoutRepo.Verify(r => r.DeleteAsync(layout), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Create_ValidDto_ReturnsCreatedAtAction()
        {
            var dto = new LayoutCreateDto { Name = "New Layout", LayoutSource = "/src", SourceTypeId = 1 };
            _layoutRepo.Setup(r => r.AddAsync(It.IsAny<Layout>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), created.ActionName);
        }
    }
}
