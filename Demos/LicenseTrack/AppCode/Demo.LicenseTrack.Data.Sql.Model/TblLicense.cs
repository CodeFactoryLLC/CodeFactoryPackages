using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Demo.LicenseTrack.Data.Sql.Model;

[Table("TblLicense")]
public partial class TblLicense
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    [StringLength(1024)]
    public string License { get; set; } = null!;

    public DateTime ExpirationDate { get; set; }
}
