using System.Security.Claims;
using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers;

public class ContentsControllerTests
{
    private readonly Mock<IContentRepository> _contentRepo;
    private readonly Mock<IContentTypeRepository> _contentTypeRepo;
    private readonly Mock<ITemplateRepository> _templateRepo;
    private readonly Mock<ISourceTypeRepository> _sourceTypeRepo;
    private readonly Mock<ILogger<ContentsController>> _logger;
    private readonly ContentsController _controller;

    public ContentsControllerTests()
    {
        _contentRepo = new Mock<IContentRepository>();
        _contentTypeRepo = new Mock<IContentTypeRepository>();
        _templateRepo = new Mock<ITemplateRepository>();
        _sourceTypeRepo = new Mock<ISourceTypeRepository>();
        _logger = new Mock<ILogger<ContentsController>>();

        _controller = new ContentsController(
            _contentRepo.Object,
            _contentTypeRepo.Object,
            _templateRepo.Object,
            _sourceTypeRepo.Object,
            _logger.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "test-user-id") }, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithContentList()
    {
        var contents = new List<Content>
        {
            new Content 
            { 
                ContentId = 1, 
                Title = "Post 1", 
                UrlToDisplay = "post-1",
                BodySource = "Content",
                LastUpdatedDate = DateTime.Now,
                ContentType = new ContentType { Name = "Blog" }
            },
            new Content 
            { 
                ContentId = 2, 
                Title = "Post 2",
                UrlToDisplay = "post-2",
                BodySource = null,
                LastUpdatedDate = DateTime.Now,
                ContentType = new ContentType { Name = "Page" }
            }
        };
        _contentRepo.Setup(r => r.GetAllWithDetailsAsync(It.IsAny<string?>())).ReturnsAsync(contents);

        var result = await _controller.GetAll(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<ContentListDto>>(okResult.Value);
        Assert.Equal(2, dtos.Count());
        Assert.True(dtos.First().BodySourceHasContent);
        Assert.False(dtos.Last().BodySourceHasContent);
    }

    [Fact]
    public async Task GetById_ValidId_ReturnsOkWithDto()
    {
        var content = new Content 
        { 
            ContentId = 1, 
            Title = "Post 1",
            BodySource = "Test",
            BodySourceTypeId = 1,
            ContentTypeId = 2,
            CreatedBy = "user1",
            CreationDate = DateTime.Now,
            LastUpdatedBy = "user1",
            LastUpdatedDate = DateTime.Now,
            TemplateId = 3,
            UrlToDisplay = "post-1",
            ContentTypeFieldItems = new List<ContentTypeFieldItem>()
        };
        _contentRepo.Setup(r => r.GetByIdWithFieldItemsAsync(1)).ReturnsAsync(content);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ContentDetailDto>(okResult.Value);
        Assert.Equal(1, dto.ContentId);
        Assert.Equal("Post 1", dto.Title);
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsNotFound()
    {
        _contentRepo.Setup(r => r.GetByIdWithFieldItemsAsync(99)).ReturnsAsync((Content?)null);

        var result = await _controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        var dto = new ContentCreateDto
        {
            BodySource = "Test content",
            BodySourceTypeId = 1,
            ContentTypeId = 2,
            TemplateId = 3,
            Title = "New Post",
            UrlToDisplay = "new-post"
        };

        _contentRepo.Setup(r => r.AddAsync(It.IsAny<Content>()))
            .Callback<Content>(c => c.ContentId = 5)
            .Returns(Task.CompletedTask);

        var result = await _controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ContentsController.GetById), createdResult.ActionName);
        var returnedDto = Assert.IsType<ContentDetailDto>(createdResult.Value);
        Assert.Equal("New Post", returnedDto.Title);
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Title", "Required");
        var dto = new ContentCreateDto();

        var result = await _controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ValidDto_ReturnsOkWithDto()
    {
        var existingContent = new Content
        {
            ContentId = 1,
            Title = "Old Title",
            BodySource = "Old",
            BodySourceTypeId = 1,
            ContentTypeId = 2,
            CreatedBy = "user1",
            CreationDate = DateTime.Now.AddDays(-1),
            LastUpdatedBy = "user1",
            LastUpdatedDate = DateTime.Now.AddDays(-1),
            TemplateId = 3,
            UrlToDisplay = "old"
        };

        var updateDto = new ContentUpdateDto
        {
            BodySource = "Updated",
            BodySourceTypeId = 1,
            ContentTypeId = 2,
            TemplateId = 3,
            Title = "Updated Title",
            UrlToDisplay = "updated"
        };

        _contentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingContent);
        _contentRepo.Setup(r => r.UpdateAsync(It.IsAny<Content>())).Returns(Task.CompletedTask);

        var result = await _controller.Update(1, updateDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ContentDetailDto>(okResult.Value);
        Assert.Equal("Updated Title", dto.Title);
        Assert.Equal("Updated", dto.BodySource);
    }

    [Fact]
    public async Task Update_InvalidId_ReturnsNotFound()
    {
        var dto = new ContentUpdateDto
        {
            BodySource = "Test",
            BodySourceTypeId = 1,
            ContentTypeId = 2,
            TemplateId = 3,
            Title = "Test",
            UrlToDisplay = "test"
        };
        _contentRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Content?)null);

        var result = await _controller.Update(99, dto);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_ConcurrencyException_ThrowsOrReturnsNotFound()
    {
        var existingContent = new Content
        {
            ContentId = 1,
            Title = "Test",
            BodySourceTypeId = 1,
            ContentTypeId = 2,
            CreatedBy = "user1",
            CreationDate = DateTime.Now,
            LastUpdatedBy = "user1",
            LastUpdatedDate = DateTime.Now,
            TemplateId = 3,
            UrlToDisplay = "test"
        };

        var dto = new ContentUpdateDto
        {
            BodySource = "Updated",
            BodySourceTypeId = 1,
            ContentTypeId = 2,
            TemplateId = 3,
            Title = "Updated",
            UrlToDisplay = "updated"
        };

        _contentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingContent);
        _contentRepo.Setup(r => r.UpdateAsync(It.IsAny<Content>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        _contentRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(false);

        var result = await _controller.Update(1, dto);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        var content = new Content { ContentId = 3, Title = "To Delete" };
        _contentRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(content);
        _contentRepo.Setup(r => r.DeleteAsync(content)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(3);

        Assert.IsType<NoContentResult>(result);
        _contentRepo.Verify(r => r.DeleteAsync(content), Times.Once);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsNotFound()
    {
        _contentRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Content?)null);

        var result = await _controller.Delete(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetFormOptions_ReturnsOkWithOptions()
    {
        var contentTypes = new List<ContentType> 
        { 
            new ContentType { ContentTypeId = 1, Name = "Blog", DefaultSingleContentTemplateId = 5 },
            new ContentType { ContentTypeId = 2, Name = "Page" }
        };
        var templates = new List<Template> 
        { 
            new Template { TemplateId = 5, Name = "Default" } 
        };
        var sourceTypes = new List<SourceType> 
        { 
            new SourceType { SourceTypeId = 1, Description = "HTML" } 
        };

        _contentTypeRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(contentTypes);
        _templateRepo.Setup(r => r.GetByIsForMultipleContentsAsync(0)).ReturnsAsync(templates);
        _sourceTypeRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(sourceTypes);

        var result = await _controller.GetFormOptions();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ContentFormOptionsDto>(okResult.Value);
        Assert.Equal(2, dto.ContentTypes.Count);
        Assert.Single(dto.Templates);
        Assert.Single(dto.SourceTypes);
        Assert.Single(dto.DefaultTemplateLookup);
        Assert.Equal(5, dto.DefaultTemplateLookup[1]);
    }
}
