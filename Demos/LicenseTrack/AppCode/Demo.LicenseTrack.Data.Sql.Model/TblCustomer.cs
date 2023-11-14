using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Demo.LicenseTrack.Data.Sql.Model;

[Table("TblCustomer")]
public partial class TblCustomer
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [StringLength(100)]
    public string MiddleName { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(512)]
    public string? Address { get; set; }

    [StringLength(512)]
    public string? Address2 { get; set; }

    [StringLength(255)]
    public string? City { get; set; }

    [StringLength(10)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }
}
