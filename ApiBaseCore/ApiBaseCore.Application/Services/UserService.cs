using ApiBaseCore.Application.DTOs;
using ApiBaseCore.Application.Interfaces;
using ApiBaseCore.Application.Utilities;
using ApiBaseCore.Domain.Models;
using ApiBaseCore.Application.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiBaseCore.Application.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEncryptionService _encryptionService;

    public UserService(IApplicationDbContext context, IMapper mapper, IEncryptionService encryptionService)
    {
        _context = context;
        _mapper = mapper;
        _encryptionService = encryptionService;
    }

    public async Task<List<PerfilUsuarioDto>> GetUsersAsync(UserFilterDto filterDto)
    {
        var query = _context.Usuarios.AsQueryable();

        // 1. Filtering
        if (!string.IsNullOrWhiteSpace(filterDto.Filter))
        {
            var parts = filterDto.Filter.Split(new[] { ' ', '+' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                var attribute = parts[0].ToLower().Trim();
                var op = parts[1].ToLower().Trim();
                var value = string.Join(" ", parts.Skip(2)); 

                query = ApplyFilter(query, attribute, op, value);
            }
        }

        // 2. Sorting
        if (!string.IsNullOrWhiteSpace(filterDto.SortedBy))
        {
            query = ApplySort(query, filterDto.SortedBy.ToLower().Trim());
        }

        var users = await query.ToListAsync();
        return _mapper.Map<List<PerfilUsuarioDto>>(users);
    }

    public async Task<PerfilUsuarioDto> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Usuarios.FindAsync(id);
        if (user == null) throw new NotFoundException("Usuario no encontrado.");
        return _mapper.Map<PerfilUsuarioDto>(user);
    }

    public async Task<PerfilUsuarioDto> CreateUserAsync(RegistroUsuarioDto createUserDto)
    {
        // Custom Validations
        if (!string.IsNullOrEmpty(createUserDto.TaxId) && !ValidationUtilities.ValidateTaxIdRFC(createUserDto.TaxId))
        {
            throw new ValidationException("El formato del Tax ID (RFC) es inválido.");
        }

        if (!ValidationUtilities.ValidatePhoneAndresFormat(createUserDto.Phone))
        {
            throw new ValidationException("El formato del teléfono es inválido (AndresFormat). Debe ser de 10 dígitos.");
        }

        // Uniqueness Checks
        if (await _context.Usuarios.AnyAsync(u => u.Email == createUserDto.Email))
        {
            throw new ValidationException("El email ya está registrado.");
        }

        if (!string.IsNullOrEmpty(createUserDto.TaxId) && await _context.Usuarios.AnyAsync(u => u.TaxId == createUserDto.TaxId))
        {
            throw new ValidationException("El Tax ID ya está registrado.");
        }

        // Create User
        // Use AES Encryption for password as per requirement
        var encryptedPassword = _encryptionService.Encrypt(createUserDto.Password);

        var newUser = new Usuario(
            name: createUserDto.Name,
            email: createUserDto.Email,
            phone: createUserDto.Phone,
            rol: RolUsuario.User,
            taxId: createUserDto.TaxId
        );
        
        newUser.EstablecerPasswordHash(encryptedPassword);

        await _context.Usuarios.AddAsync(newUser);
        await _context.SaveChangesAsync();

        return _mapper.Map<PerfilUsuarioDto>(newUser);
    }

    public async Task<PerfilUsuarioDto> UpdateUserAsync(Guid id, UpdateUserDto updateDto)
    {
        var user = await _context.Usuarios.FindAsync(id);
        if (user == null) throw new NotFoundException("Usuario no encontrado.");

        // Validations & Updates
        if (updateDto.Email != null)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == updateDto.Email && u.Id != id))
                throw new ValidationException("El email ya existe.");
        }

        if (updateDto.TaxId != null)
        {
            if (!ValidationUtilities.ValidateTaxIdRFC(updateDto.TaxId))
                throw new ValidationException("Invalid Tax ID format.");
            if (await _context.Usuarios.AnyAsync(u => u.TaxId == updateDto.TaxId && u.Id != id))
                throw new ValidationException("Tax ID already in use.");
        }

        if (updateDto.Phone != null)
        {
             if (!ValidationUtilities.ValidatePhoneAndresFormat(updateDto.Phone))
                throw new ValidationException("Invalid Phone format.");
        }

        user.UpdateProfile(
            name: updateDto.Name ?? user.Name,
            phone: updateDto.Phone ?? user.Phone,
            taxId: updateDto.TaxId ?? user.TaxId
        );

        if (!string.IsNullOrEmpty(updateDto.Password))
        {
             var encrypted = _encryptionService.Encrypt(updateDto.Password);
             user.EstablecerPasswordHash(encrypted);
        }
        
        await _context.SaveChangesAsync();

        return _mapper.Map<PerfilUsuarioDto>(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _context.Usuarios.FindAsync(id);
        if (user == null) throw new NotFoundException("Usuario no encontrado.");

        _context.Usuarios.Remove(user);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Usuario> ApplyFilter(IQueryable<Usuario> query, string attribute, string op, string value)
    {
        switch (attribute)
        {
            case "name":
                return ApplyOp(query, u => u.Name, op, value);
            case "email":
                return ApplyOp(query, u => u.Email, op, value);
            case "phone":
                return ApplyOp(query, u => u.Phone, op, value);
            case "tax_id":
                return ApplyOpNullable(query, u => u.TaxId, op, value);
            case "id":
                if (op == "eq" && Guid.TryParse(value, out var id))
                    return query.Where(u => u.Id == id);
                return query;
            default:
                return query;
        }
    }

    private IQueryable<Usuario> ApplyOp(IQueryable<Usuario> query, Expression<Func<Usuario, string>> prop, string op, string value)
    {
        var param = prop.Parameters[0];
        var propertyBody = prop.Body;
        var valueConst = Expression.Constant(value);
        Expression? body = null;

        switch (op)
        {
            case "co": 
                body = Expression.Call(propertyBody, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, valueConst);
                break;
            case "eq":
                body = Expression.Equal(propertyBody, valueConst);
                break;
            case "sw":
                body = Expression.Call(propertyBody, typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!, valueConst);
                break;
            case "ew":
                body = Expression.Call(propertyBody, typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!, valueConst);
                break;
        }

        if (body != null)
        {
            var lambda = Expression.Lambda<Func<Usuario, bool>>(body, param);
            return query.Where(lambda);
        }
        return query;
    }

     private IQueryable<Usuario> ApplyOpNullable(IQueryable<Usuario> query, Expression<Func<Usuario, string?>> prop, string op, string value)
    {
        var param = prop.Parameters[0];
        var propertyBody = prop.Body;
        var valueConst = Expression.Constant(value);
        Expression? body = null;

        switch (op)
        {
            case "co": 
                body = Expression.Call(propertyBody, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, valueConst);
                break;
            case "eq":
                body = Expression.Equal(propertyBody, valueConst);
                break;
            case "sw":
                body = Expression.Call(propertyBody, typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!, valueConst);
                break;
            case "ew":
                body = Expression.Call(propertyBody, typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!, valueConst);
                break;
        }

        if (body != null)
        {
            var lambda = Expression.Lambda<Func<Usuario, bool>>(body, param);
            return query.Where(lambda);
        }
        return query;
    }

    private IQueryable<Usuario> ApplySort(IQueryable<Usuario> query, string sortedBy)
    {
        return sortedBy switch
        {
            "email" => query.OrderBy(u => u.Email),
            "id" => query.OrderBy(u => u.Id),
            "name" => query.OrderBy(u => u.Name),
            "phone" => query.OrderBy(u => u.Phone),
            "tax_id" => query.OrderBy(u => u.TaxId),
            "created_at" => query.OrderBy(u => u.CreatedAt),
            _ => query
        };
    }
}
