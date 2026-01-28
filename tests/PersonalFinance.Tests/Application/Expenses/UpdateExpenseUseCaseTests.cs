using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Application.Expenses.UseCases;
using PersonalFinance.Domain.Entities;
using Errors = PersonalFinance.Shared.Results.Errors;

namespace PersonalFinance.Tests.Application.Expenses;

public sealed class UpdateExpenseUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComValidacaoInvalida_RetornaFalha()
    {
        // Arrange
        var repository = Substitute.For<IExpenseRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<UpdateExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("", "invalid") }));
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateExpenseUseCase>>();
        var useCase = new UpdateExpenseUseCase(repository, unitOfWork, validator, logger);

        var request = new UpdateExpenseRequest { Id = Guid.NewGuid(), Date = new DateTime(2024, 1, 1), Amount = 10m };

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
        var repository = Substitute.For<IExpenseRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Expense?>(null));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<UpdateExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateExpenseUseCase>>();
        var useCase = new UpdateExpenseUseCase(repository, unitOfWork, validator, logger);

        var request = new UpdateExpenseRequest { Id = Guid.NewGuid(), Date = new DateTime(2024, 1, 1), Amount = 10m };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.NotFound, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosInvalidos_RetornaFalha()
    {
        // Arrange
        var expense = Expense.Create(new DateTime(2024, 1, 1), 10m, "Lunch", null).Value!;
        var repository = Substitute.For<IExpenseRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Expense?>(expense));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<UpdateExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateExpenseUseCase>>();
        var useCase = new UpdateExpenseUseCase(repository, unitOfWork, validator, logger);

        var request = new UpdateExpenseRequest { Id = expense.Id, Date = new DateTime(2024, 1, 1), Amount = 0m };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_AtualizaESalva()
    {
        // Arrange
        var expense = Expense.Create(new DateTime(2024, 1, 1), 10m, "Lunch", null).Value!;
        var repository = Substitute.For<IExpenseRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Expense?>(expense));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var validator = Substitute.For<IValidator<UpdateExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateExpenseUseCase>>();
        var useCase = new UpdateExpenseUseCase(repository, unitOfWork, validator, logger);

        var request = new UpdateExpenseRequest { Id = expense.Id, Date = new DateTime(2024, 1, 2), Amount = 20m, Description = "Dinner" };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(20m, expense.Amount);
        Assert.Equal("Dinner", expense.Description);
    }
}
