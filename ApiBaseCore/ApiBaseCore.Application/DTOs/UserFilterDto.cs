namespace ApiBaseCore.Application.DTOs;

public class UserFilterDto
{
    /// <summary>
    /// Property to sort by. Allowed values: email, id, name, phone, tax_id, created_at.
    /// Default sort direction is ascending unless logic specifies otherwise.
    /// </summary>
    public string? SortedBy { get; set; }

    /// <summary>
    /// Filter string in format: attribute+op+value
    /// Examples: name+co+user, email+ew+mail.com
    /// Ops: co (contains), eq (equals), sw (starts with), ew (ends with)
    /// </summary>
    public string? Filter { get; set; }
}
