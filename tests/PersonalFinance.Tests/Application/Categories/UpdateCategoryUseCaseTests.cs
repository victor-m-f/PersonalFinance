using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Application.Categories.UseCases;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;
using Errors = PersonalFinance.Shared.Results.Errors;

namespace PersonalFinance.Tests.Application.Categories;

public sealed class UpdateCategoryUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ComValidacaoInvalida_RetornaFalha()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<UpdateCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("", "invalid") }));
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateCategoryUseCase>>();
        var useCase = new UpdateCategoryUseCase(repository, invalidator, unitOfWork, validator, logger);

        var request = new UpdateCategoryRequest { Id = Guid.NewGuid(), Name = "Food", ColorHex = "#FF112233" };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComCategoriaInexistente_RetornaNotFound()
    {
        // Arrange
        var repository = Substitute.For<ICategoryRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Category?>(null));
        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<UpdateCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateCategoryUseCase>>();
        var useCase = new UpdateCategoryUseCase(repository, invalidator, unitOfWork, validator, logger);

        var request = new UpdateCategoryRequest { Id = Guid.NewGuid(), Name = "Food", ColorHex = "#FF112233" };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.NotFound, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComParentInexistente_RetornaNotFound()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233").Value!;
        var category = Category.Create("Food", color, null).Value!;

        var repository = Substitute.For<ICategoryRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Category?>(category));
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));

        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<UpdateCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateCategoryUseCase>>();
        var useCase = new UpdateCategoryUseCase(repository, invalidator, unitOfWork, validator, logger);

        var request = new UpdateCategoryRequest
        {
            Id = category.Id,
            Name = "Food",
            ColorHex = "#FF112233",
            ParentId = Guid.NewGuid()
        };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.NotFound, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComCicloDeParent_RetornaFalha()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233").Value!;
        var category = Category.Create("Food", color, null).Value!;

        var repository = Substitute.For<ICategoryRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Category?>(category));
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<UpdateCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateCategoryUseCase>>();
        var useCase = new UpdateCategoryUseCase(repository, invalidator, unitOfWork, validator, logger);

        var request = new UpdateCategoryRequest
        {
            Id = category.Id,
            Name = "Food",
            ColorHex = "#FF112233",
            ParentId = category.Id
        };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComNomeInvalido_RetornaFalha()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233").Value!;
        var category = Category.Create("Food", color, null).Value!;

        var repository = Substitute.For<ICategoryRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Category?>(category));

        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<UpdateCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateCategoryUseCase>>();
        var useCase = new UpdateCategoryUseCase(repository, invalidator, unitOfWork, validator, logger);

        var request = new UpdateCategoryRequest
        {
            Id = category.Id,
            Name = " ",
            ColorHex = "#FF112233",
            ParentId = null
        };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.ValidationError, result.ErrorCode);
        await unitOfWork.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        invalidator.Received(0).Invalidate();
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_AtualizaESalva()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233").Value!;
        var category = Category.Create("Food", color, null).Value!;

        var repository = Substitute.For<ICategoryRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Category?>(category));

        var invalidator = Substitute.For<ICategorySuggestionCacheInvalidator>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var validator = Substitute.For<IValidator<UpdateCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<UpdateCategoryUseCase>>();
        var useCase = new UpdateCategoryUseCase(repository, invalidator, unitOfWork, validator, logger);

        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var request = new UpdateCategoryRequest
        {
            Id = category.Id,
            Name = "Groceries",
            ColorHex = "#FFAA2233",
            ParentId = null
        };

        // Act
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        invalidator.Received(1).Invalidate();
        Assert.Equal("Groceries", category.Name);
    }
}
