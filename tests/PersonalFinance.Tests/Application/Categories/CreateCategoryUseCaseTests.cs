using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Application.Categories.UseCases;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Domain.Entities;
using Errors = PersonalFinance.Shared.Results.Errors;

namespace PersonalFinance.Tests.Application.Categories;

public sealed class CreateCategoryUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComValidacaoInvalida_RetornaFalha()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<CreateCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<CreateCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("", "invalid") }));
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateCategoryUseCase>>();
        var useCase = new CreateCategoryUseCase(repository, invalidator, unitOfWork, validator, logger);

        var request = new CreateCategoryRequest { Name = "Food", ColorHex = "#FF112233" };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await repository.Received(0).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComCorInvalida_RetornaFalha()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<CreateCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<CreateCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateCategoryUseCase>>();
        var useCase = new CreateCategoryUseCase(repository, invalidator, unitOfWork, validator, logger);

        var request = new CreateCategoryRequest { Name = "Food", ColorHex = "#123" };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await repository.Received(0).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_SalvaCategoriaERetornaId()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<CreateCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<CreateCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CreateCategoryUseCase>>();
        var useCase = new CreateCategoryUseCase(repository, invalidator, unitOfWork, validator, logger);

        var request = new CreateCategoryRequest { Name = "Food", ColorHex = "#FF112233" };
        Category? addedCategory = null;
        repository.AddAsync(Arg.Do((Category c) => addedCategory = c), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        await repository.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        invalidator.Received(1).Invalidate();
        Assert.NotNull(addedCategory);
        Assert.Equal(result.Value!.Id, addedCategory!.Id);
    }
}
