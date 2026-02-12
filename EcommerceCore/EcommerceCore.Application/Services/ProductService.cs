using AutoMapper;
using AutoMapper.QueryableExtensions;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace EcommerceCore.Application.Services;

public class ProductService(IApplicationDbContext context, IFileStorageService fileStorageService, IMapper mapper)
    : IProductService
{
    /// <summary>
    /// Obtiene una lista paginada de productos, con opción de búsqueda y filtrado.
    /// </summary>
    /// <param name="filter">DTO con los criterios de filtrado.</param>
    /// <returns>Un objeto <see cref="PaginatedResult{ProductDto}"/> que contiene la lista de productos.</returns>
    public async Task<PaginatedResult<ProductDto>> GetAllAsync(ProductFilterDto filter)
    {
        // Inicia la consulta sobre la colección de productos.
        var query = context.Products.AsQueryable();

        // Aplica filtro de búsqueda si se proporciona un término.
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(filter.SearchTerm) || p.Description.Contains(filter.SearchTerm));
        }

        // Filter by Category
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(p => p.Category == filter.Category);
        }

        // Filter by Price Range
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        // Cuenta el total de elementos que coinciden con la consulta.
        var totalCount = await query.CountAsync();

        // Aplica paginación y proyecta los resultados a DTOs.
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize) // Salta los elementos de páginas anteriores.
            .Take(filter.PageSize) // Toma solo los elementos de la página actual.
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider) // Proyecta la entidad a DTO usando AutoMapper.
            .ToListAsync(); // Ejecuta la consulta y obtiene la lista.

        // Retorna el resultado paginado.
        return new PaginatedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// Obtiene un producto por su identificador único.
    /// </summary>
    /// <param name="id">El ID del producto a buscar.</param>
    /// <returns>Un <see cref="ProductDto"/> si se encuentra el producto; de lo contrario, null.</returns>
    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        // Busca el producto por ID.
        var p = await context.Products.FindAsync(keyValues: id);
        // Mapea el producto a DTO si se encuentra, de lo contrario retorna null.
        return p == null ? null : mapper.Map<ProductDto>(source: p);
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="dto">El DTO con los datos para crear el producto.</param>
    /// <returns>El <see cref="ProductDto"/> del producto recién creado.</returns>
    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        // Crea una nueva instancia de Product a partir del DTO.
        var product = new Product(name: dto.Name,
            description: dto.Description,
            price: dto.Price,
            stock: dto.Stock,
            imageUrl: dto.ImageUrl,
            category: dto.Category);
        // Agrega el nuevo producto al contexto de la base de datos.
        context.Products.Add(entity: product);
        // Guarda los cambios en la base de datos.
        await context.SaveChangesAsync();

        // Mapea la entidad Product creada a ProductDto y la retorna.
        return mapper.Map<ProductDto>(source: product);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">El ID del producto a actualizar.</param>
    /// <param name="dto">El DTO con los datos actualizados del producto.</param>
    /// <exception cref="KeyNotFoundException">Se lanza si el producto no se encuentra.</exception>
    public async Task UpdateAsync(int id, UpdateProductDto dto)
    {
        // Busca el producto por ID.
        var product = await context.Products.FindAsync(keyValues: id);
        // Si el producto no se encuentra, lanza una excepción.
        if (product == null) throw new KeyNotFoundException(message: "Producto no encontrado.");

        // Actualiza los detalles del producto usando el método de la entidad.
        product.UpdateDetails(name: dto.Name,
            description: dto.Description,
            price: dto.Price,
            imageUrl: dto.ImageUrl,
            category: dto.Category);

        // Calcular diferencia de stock para actualizar.
        var stockDiff = dto.Stock - product.Stock;
        if (stockDiff != 0)
        {
            // Actualiza el stock del producto.
            product.UpdateStock(quantity: stockDiff);
        }

        // Guarda los cambios en la base de datos.
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un producto por su identificador único.
    /// </summary>
    /// <param name="id">El ID del producto a eliminar.</param>
    /// <exception cref="KeyNotFoundException">Se lanza si el producto no se encuentra.</exception>
    public async Task DeleteAsync(int id)
    {
        // Busca el producto por ID.
        var product = await context.Products.FindAsync(keyValues: id);
        // Si el producto no se encuentra, lanza una excepción.
        if (product == null) throw new KeyNotFoundException(message: "Producto no encontrado.");

        // Eliminar imagen si existe y no es una URL externa (placeholder).
        if (!string.IsNullOrEmpty(value: product.ImageUrl) && !product.ImageUrl.StartsWith(value: "http"))
        {
            await fileStorageService.DeleteFileAsync(fileRoute: product.ImageUrl);
        }

        // Marca el producto para ser eliminado del contexto de la base de datos.
        context.Products.Remove(entity: product);
        // Guarda los cambios en la base de datos.
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Sube una imagen para un producto específico y actualiza su URL de imagen.
    /// </summary>
    /// <param name="id">El ID del producto al que se le asociará la imagen.</param>
    /// <param name="file">El archivo de imagen a subir.</param>
    /// <returns>La URL de la imagen subida.</returns>
    /// <exception cref="KeyNotFoundException">Se lanza si el producto no se encuentra.</exception>
    public async Task<string> UploadImageAsync(int id, IFormFile file)
    {
        // Busca el producto por ID.
        var product = await context.Products.FindAsync(keyValues: id);
        // Si el producto no se encuentra, lanza una excepción.
        if (product == null) throw new KeyNotFoundException(message: "Producto no encontrado.");

        // Borrar imagen anterior si no es una URL externa (placeholder).
        if (!string.IsNullOrEmpty(value: product.ImageUrl) && !product.ImageUrl.StartsWith(value: "http"))
        {
            await fileStorageService.DeleteFileAsync(fileRoute: product.ImageUrl);
        }

        // Genera un nombre de archivo único para la imagen.
        var fileExtension = Path.GetExtension(path: file.FileName);
        var uniqueFileName = $"products/{Guid.NewGuid()}{fileExtension}";

        string fileUrl;
        // Abre el stream del archivo y lo guarda usando el servicio de almacenamiento.
        await using (var stream = file.OpenReadStream())
        {
            fileUrl = await fileStorageService.SaveFileAsync(fileStream: stream,
                fileName: uniqueFileName);
        }

        // Actualiza la URL de la imagen del producto.
        product.UpdateDetails(name: product.Name,
            description: product.Description,
            price: product.Price,
            imageUrl: fileUrl,
            category: product.Category);
        // Guarda los cambios en la base de datos.
        await context.SaveChangesAsync();

        // Retorna la URL de la imagen subida.
        return fileUrl;
    }
}
