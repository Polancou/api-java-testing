using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[ApiController]
[Route(template: "api/v{version:apiVersion}/[controller]")]
[ApiVersion(version: "1.0")]
public class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>
    /// Obtiene la lista completa de productos con filtros.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> GetProducts([FromQuery] ProductFilterDto filter)
    {
        // Obtiene todos los productos de forma asíncrona, aplicando filtros de búsqueda y paginación.
        var products = await productService.GetAllAsync(filter);
        return Ok(value: products); // Retorna la lista paginada de productos con un código 200 OK.
    }

    /// <summary>
    /// Obtiene un producto específico por su ID.
    /// </summary>
    /// <param name="id">El ID del producto a buscar.</param>
    [HttpGet(template: "{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        // Busca un producto por su identificador único.
        var product = await productService.GetByIdAsync(id: id);

        if (product == null)
        {
            return NotFound(); // Retorna 404 Not Found si el producto no se encuentra.
        }

        return product; // Retorna el producto encontrado con un código 200 OK.
    }

    /// <summary>
    /// Crea un nuevo producto (Requiere rol Admin).
    /// </summary>
    /// <param name="productDto">Los datos del nuevo producto a crear.</param>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto productDto)
    {
        // Crea un nuevo producto utilizando los datos proporcionados.
        var createdProduct = await productService.CreateAsync(dto: productDto);
        // Retorna 201 Created, incluyendo la ubicación del nuevo recurso y el objeto creado.
        return CreatedAtAction(actionName: nameof(GetProduct),
            routeValues: new
            {
                id = createdProduct.Id
            },
            value: createdProduct);
    }

    /// <summary>
    /// Actualiza un producto existente (Requiere rol Admin).
    /// </summary>
    /// <param name="id">El ID del producto a actualizar.</param>
    /// <param name="productDto">Los datos actualizados del producto.</param>
    [Authorize(Roles = "Admin")]
    [HttpPut(template: "{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto productDto)
    {
        try
        {
            // Intenta actualizar el producto con el ID y los datos proporcionados.
            await productService.UpdateAsync(id: id,
                dto: productDto);
            return NoContent(); // Retorna 204 No Content si la actualización fue exitosa.
        }
        catch (KeyNotFoundException)
        {
            return NotFound(); // Retorna 404 Not Found si el producto no existe.
        }
        catch (InvalidOperationException ex)
        {
            // Captura errores de lógica de negocio o validación.
            return BadRequest(error: ex.Message); // Retorna 400 Bad Request con el mensaje de error.
        }
    }

    /// <summary>
    /// Elimina un producto (Requiere rol Admin).
    /// </summary>
    /// <param name="id">El ID del producto a eliminar.</param>
    [Authorize(Roles = "Admin")]
    [HttpDelete(template: "{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            // Intenta eliminar el producto con el ID proporcionado.
            await productService.DeleteAsync(id: id);
            return NoContent(); // Retorna 204 No Content si la eliminación fue exitosa.
        }
        catch (KeyNotFoundException)
        {
            return NotFound(); // Retorna 404 Not Found si el producto no existe.
        }
    }

    /// <summary>
    /// Sube una imagen para un producto (Requiere rol Admin).
    /// </summary>
    /// <param name="id">El ID del producto al que se asociará la imagen.</param>
    /// <param name="file">El archivo de imagen a subir.</param>
    [Authorize(Roles = "Admin")]
    [HttpPost(template: "{id}/image")]
    public async Task<IActionResult> UploadProductImage(int id, IFormFile file)
    {
        // Verifica si se ha proporcionado un archivo o si está vacío.
        if (file == null || file.Length == 0)
            return BadRequest(error: new { message = "No se ha proporcionado ningún archivo." });

        // Verifica el tamaño del archivo para asegurar que no exceda el límite de 5MB.
        if (file.Length > 1024 * 1024 * 5) // Límite de 5MB
            return BadRequest(error: new { message = "El archivo debe ser menor a 5 MB." });

        try
        {
            // Sube la imagen y asocia su URL al producto.
            var imageUrl = await productService.UploadImageAsync(id: id,
                file: file);
            return Ok(value: new { imageUrl }); // Retorna 200 OK con la URL de la imagen subida.
        }
        catch (KeyNotFoundException)
        {
            return NotFound(); // Retorna 404 Not Found si el producto no existe.
        }
    }
}
