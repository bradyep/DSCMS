using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class TemplatesControllerTests
    {
        private readonly Mock<ITemplateRepository> _templateRepo;
        private readonly Mock<ILayoutRepository> _layoutRepo;
        private readonly TemplatesController _controller;

        public TemplatesControllerTests()
        {
            _templateRepo = new Mock<ITemplateRepository>();
            _layoutRepo = new Mock<ILayoutRepository>();
            _controller = new TemplatesController(_templateRepo.Object, _layoutRepo.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithTemplates()
        {
            var templates = new List<Template>
            {
                new Template { TemplateId = 1, Name = "Blog Post" },
                new Template { TemplateId = 2, Name = "Blog Index" }
            };
            _templateRepo.Setup(r => r.GetAllWithLayoutAsync()).ReturnsAsync(templates);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Template>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_TemplateNotFound_ReturnsNotFound()
        {
            _templateRepo.Setup(r => r.GetByIdWithLayoutAsync(99)).ReturnsAsync((Template?)null);

            var result = await _controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_TemplateFound_ReturnsViewWithTemplate()
        {
            var template = new Template { TemplateId = 1, Name = "Blog Post" };
            _templateRepo.Setup(r => r.GetByIdWithLayoutAsync(1)).ReturnsAsync(template);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Template>(viewResult.Model);
            Assert.Equal(1, model.TemplateId);
        }

        [Fact]
        public async Task Edit_NullId_ReturnsNotFound()
        {
            var result = await _controller.Edit((int?)null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_TemplateNotFound_ReturnsNotFound()
        {
            _templateRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Template?)null);

            var result = await _controller.Edit(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_TemplateFound_ReturnsViewWithTemplate()
        {
            var template = new Template { TemplateId = 1, Name = "Blog Post" };
            _templateRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);
            _layoutRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Layout>());

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Template>(viewResult.Model);
            Assert.Equal(1, model.TemplateId);
        }

        [Fact]
        public async Task Delete_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_TemplateNotFound_ReturnsNotFound()
        {
            _templateRepo.Setup(r => r.GetByIdWithLayoutAsync(99)).ReturnsAsync((Template?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_TemplateFound_ReturnsViewWithTemplate()
        {
            var template = new Template { TemplateId = 1, Name = "Blog Post" };
            _templateRepo.Setup(r => r.GetByIdWithLayoutAsync(1)).ReturnsAsync(template);

            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Template>(viewResult.Model);
        }

        [Fact]
        public async Task DeleteConfirmed_TemplateExists_CallsDeleteAndRedirects()
        {
            var template = new Template { TemplateId = 1, Name = "Blog Post" };
            _templateRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);

            var result = await _controller.DeleteConfirmed(1);

            _templateRepo.Verify(r => r.DeleteAsync(template), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
    }
}
