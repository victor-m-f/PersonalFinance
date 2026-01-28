using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Application.Expenses.UseCases;
using PersonalFinance.Domain.Entities;
using Errors = PersonalFinance.Shared.Results.Errors;

namespace PersonalFinance.Tests.Application.Expenses;

public sealed class AssignExpenseCategoryUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComValidacaoInvalida_RetornaFalha()
    {
        // Arrange
        var expenseRepository = Substitute.For<IExpenseRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<AssignExpenseCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<AssignExpenseCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("", "invalid") }));
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<AssignExpenseCategoryUseCase>>();
        var useCase = new AssignExpenseCategoryUseCase(expenseRepository, categoryRepository, unitOfWork, validator, logger);

        var request = new AssignExpenseCategoryRequest { Id = Guid.NewGuid(), CategoryId = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComDespesaInexistente_RetornaNotFound()
    {
        // Arrange
        var expenseRepository = Substitute.For<IExpenseRepository>();
        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Expense?>(null));
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<AssignExpenseCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<AssignExpenseCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<AssignExpenseCategoryUseCase>>();
        var useCase = new AssignExpenseCategoryUseCase(expenseRepository, categoryRepository, unitOfWork, validator, logger);

        var request = new AssignExpenseCategoryRequest { Id = Guid.NewGuid(), CategoryId = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.NotFound, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComCategoriaInexistente_RetornaNotFound()
    {
        // Arrange
        var expense = Expense.Create(new DateTime(2024, 1, 1), 10m, "Lunch", null).Value!;
        var expenseRepository = Substitute.For<IExpenseRepository>();
        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Expense?>(expense));
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<AssignExpenseCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<AssignExpenseCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<AssignExpenseCategoryUseCase>>();
        var useCase = new AssignExpenseCategoryUseCase(expenseRepository, categoryRepository, unitOfWork, validator, logger);

        var request = new AssignExpenseCategoryRequest { Id = expense.Id, CategoryId = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.NotFound, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_AssignaCategoriaESalva()
    {
        // Arrange
        var expense = Expense.Create(new DateTime(2024, 1, 1), 10m, "Lunch", null).Value!;
        var expenseRepository = Substitute.For<IExpenseRepository>();
        expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Expense?>(expense));
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var validator = Substitute.For<IValidator<AssignExpenseCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<AssignExpenseCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<AssignExpenseCategoryUseCase>>();
        var useCase = new AssignExpenseCategoryUseCase(expenseRepository, categoryRepository, unitOfWork, validator, logger);

        var categoryId = Guid.NewGuid();
        var request = new AssignExpenseCategoryRequest { Id = expense.Id, CategoryId = categoryId };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(categoryId, expense.CategoryId);
    }
}
