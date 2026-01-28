using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Application.Categories.UseCases;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using Errors = PersonalFinance.Shared.Results.Errors;

namespace PersonalFinance.Tests.Application.Categories;

public sealed class DeleteCategoryUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComValidacaoInvalida_RetornaFalha()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var validator = Substitute.For<IValidator<DeleteCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<DeleteCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("", "invalid") }));
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<DeleteCategoryUseCase>>();
        var useCase = new DeleteCategoryUseCase(repository, invalidator, validator, logger);

        var request = new DeleteCategoryRequest { Id = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await repository.Received(0).DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComCategoriaInexistente_RetornaNotFound()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var validator = Substitute.For<IValidator<DeleteCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<DeleteCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<DeleteCategoryUseCase>>();
        var useCase = new DeleteCategoryUseCase(repository, invalidator, validator, logger);

        var request = new DeleteCategoryRequest { Id = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.NotFound, result.ErrorCode);
        await repository.Received(0).DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComCategoriaComFilhos_RetornaConflict()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        repository.HasChildrenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var validator = Substitute.For<IValidator<DeleteCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<DeleteCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<DeleteCategoryUseCase>>();
        var useCase = new DeleteCategoryUseCase(repository, invalidator, validator, logger);

        var request = new DeleteCategoryRequest { Id = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Conflict, result.ErrorCode);
        await repository.Received(0).DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComCategoriaEmUso_RetornaConflict()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        repository.HasChildrenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        repository.IsInUseByExpensesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var validator = Substitute.For<IValidator<DeleteCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<DeleteCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<DeleteCategoryUseCase>>();
        var useCase = new DeleteCategoryUseCase(repository, invalidator, validator, logger);

        var request = new DeleteCategoryRequest { Id = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Conflict, result.ErrorCode);
        await repository.Received(0).DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComCategoriaValida_DeletaEInvalidaCache()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        repository.HasChildrenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        repository.IsInUseByExpensesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        repository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var validator = Substitute.For<IValidator<DeleteCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<DeleteCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<DeleteCategoryUseCase>>();
        var useCase = new DeleteCategoryUseCase(repository, invalidator, validator, logger);

        var request = new DeleteCategoryRequest { Id = Guid.NewGuid() };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await repository.Received(1).DeleteByIdAsync(request.Id, Arg.Any<CancellationToken>());
        invalidator.Received(1).Invalidate();
    }
}
