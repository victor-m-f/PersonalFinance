using NSubstitute;
using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Application.Categories.Responses;
using PersonalFinance.Application.Categories.UseCases;

namespace PersonalFinance.Tests.Application.Categories;

public sealed class FilterCategoriesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComFiltrosComEspacos_NormalizaParametros()
    {
        // Arrange
        var repository = Substitute.For<ICategoryReadRepository>();
        repository.FilterAsync(
                Arg.Any<Guid?>(),
                Arg.Any<bool>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<bool>(),
                Arg.Any<PageRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(PagedResult<CategoryListItemResponse>.FromAll(Array.Empty<CategoryListItemResponse>())));

        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<FilterCategoriesUseCase>>();
        var useCase = new FilterCategoriesUseCase(repository, logger);

        var request = new FilterCategoriesRequest
        {
            ParentId = Guid.NewGuid(),
            IncludeAll = true,
            Name = "  Food  ",
            SortBy = "  Name  ",
            SortDescending = true,
            Page = new PageRequest { PageNumber = 0, PageSize = 500 }
        };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await repository.Received(1).FilterAsync(
            request.ParentId,
            true,
            "Food",
            "Name",
            true,
            Arg.Is<PageRequest>(page => page.PageNumber == 1 && page.PageSize == PageRequest.MaxPageSize),
            Arg.Any<CancellationToken>());
    }
}
