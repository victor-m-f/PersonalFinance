using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Application.Expenses.UseCases;
using Errors = PersonalFinance.Shared.Results.Errors;

namespace PersonalFinance.Tests.Application.Expenses;

public sealed class DeleteExpenseUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComValidacaoInvalida_RetornaFalha()
    {
        // Arrange
        var repository = Substitute.For<IExpenseRepository>();
        var validator = Substitute.For<IValidator<DeleteExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<DeleteExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("", "invalid") }));
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<DeleteExpenseUseCase>>();
        var useCase = new DeleteExpenseUseCase(repository, validator, logger);

        var request = new DeleteExpenseRequest { Id = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await repository.Received(0).DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComDespesaInexistente_RetornaNotFound()
    {
        // Arrange
        var repository = Substitute.For<IExpenseRepository>();
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        var validator = Substitute.For<IValidator<DeleteExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<DeleteExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<DeleteExpenseUseCase>>();
        var useCase = new DeleteExpenseUseCase(repository, validator, logger);

        var request = new DeleteExpenseRequest { Id = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.NotFound, result.ErrorCode);
        await repository.Received(0).DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ComDespesaValida_DeletaComSucesso()
    {
        // Arrange
        var repository = Substitute.For<IExpenseRepository>();
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        repository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));
        var validator = Substitute.For<IValidator<DeleteExpenseRequest>>();
        validator.ValidateAsync(Arg.Any<DeleteExpenseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<DeleteExpenseUseCase>>();
        var useCase = new DeleteExpenseUseCase(repository, validator, logger);

        var request = new DeleteExpenseRequest { Id = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await repository.Received(1).DeleteByIdAsync(request.Id, Arg.Any<CancellationToken>());
    }
}
