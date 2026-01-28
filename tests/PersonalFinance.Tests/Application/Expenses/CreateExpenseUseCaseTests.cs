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

public sealed class CreateExpenseUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComValidacaoInvalida_RetornaFalha()
    {
        // Arrange
        var expenseRepository = Substitute.For<IExpenseRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<CreateExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<CreateExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("", "invalid") }));
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateExpenseUseCase>>();
        var useCase = new CreateExpenseUseCase(expenseRepository, categoryRepository, unitOfWork, validator, logger);

        var request = new CreateExpenseRequest { Date = new DateTime(2024, 1, 1), Amount = 10m, Description = "Lunch" };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await expenseRepository.Received(0).AddAsync(Arg.Any<Expense>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosInvalidos_RetornaFalha()
    {
        // Arrange
        var expenseRepository = Substitute.For<IExpenseRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<CreateExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<CreateExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateExpenseUseCase>>();
        var useCase = new CreateExpenseUseCase(expenseRepository, categoryRepository, unitOfWork, validator, logger);

        var request = new CreateExpenseRequest { Date = default, Amount = 10m, Description = "Lunch" };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await expenseRepository.Received(0).AddAsync(Arg.Any<Expense>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComCategoriaInexistente_RetornaNotFound()
    {
        // Arrange
        var expenseRepository = Substitute.For<IExpenseRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<CreateExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<CreateExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateExpenseUseCase>>();
        var useCase = new CreateExpenseUseCase(expenseRepository, categoryRepository, unitOfWork, validator, logger);

        var request = new CreateExpenseRequest
        {
            Date = new DateTime(2024, 1, 1),
            Amount = 10m,
            Description = "Lunch",
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.NotFound, result.ErrorCode);
        await expenseRepository.Received(0).AddAsync(Arg.Any<Expense>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_SalvaDespesaERetornaId()
    {
        // Arrange
        var expenseRepository = Substitute.For<IExpenseRepository>();
        var categoryRepository = Substitute.For<ICategoryRepository>();
        categoryRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<CreateExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<CreateExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateExpenseUseCase>>();
        var useCase = new CreateExpenseUseCase(expenseRepository, categoryRepository, unitOfWork, validator, logger);

        var request = new CreateExpenseRequest
        {
            Date = new DateTime(2024, 1, 1),
            Amount = 10m,
            Description = "Lunch",
            CategoryId = Guid.NewGuid()
        };

        Expense? addedExpense = null;
        expenseRepository.AddAsync(Arg.Do((Expense e) => addedExpense = e), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        await expenseRepository.Received(1).AddAsync(Arg.Any<Expense>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.NotNull(addedExpense);
        Assert.Equal(result.Value!.Id, addedExpense!.Id);
    }
}
