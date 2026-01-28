using NSubstitute;
using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Application.Expenses.Responses;
using PersonalFinance.Application.Expenses.UseCases;

namespace PersonalFinance.Tests.Application.Expenses;

public sealed class FilterExpensesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComFiltrosComEspacos_NormalizaParametros()
    {
        // Arrange
        var repository = Substitute.For<IExpenseReadRepository>();
        repository.FilterAsync(
                Arg.Any<DateTime?>(),
                Arg.Any<DateTime?>(),
                Arg.Any<decimal?>(),
                Arg.Any<decimal?>(),
                Arg.Any<Guid?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<bool>(),
                Arg.Any<PageRequest?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(PagedResult<ExpenseListItemResponse>.FromAll(Array.Empty<ExpenseListItemResponse>())));

        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<FilterExpensesUseCase>>();
        var useCase = new FilterExpensesUseCase(repository, logger);

        var request = new FilterExpensesRequest
        {
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31),
            MinAmount = 10m,
            MaxAmount = 100m,
            CategoryId = Guid.NewGuid(),
            DescriptionSearch = "  market  ",
            SortBy = "  Date  ",
            SortDescending = true,
            Page = new PageRequest { PageNumber = 0, PageSize = 500 }
        };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await repository.Received(1).FilterAsync(
            request.StartDate,
            request.EndDate,
            request.MinAmount,
            request.MaxAmount,
            request.CategoryId,
            "market",
            "Date",
            true,
            Arg.Is<PageRequest>(page => page.PageNumber == 1 && page.PageSize == PageRequest.MaxPageSize),
            Arg.Any<CancellationToken>());
    }
}
