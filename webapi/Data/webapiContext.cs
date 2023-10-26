using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using webapi.Controllers;

namespace webapi.Data
{
    public class webapiContext : DbContext
    {
        public webapiContext (DbContextOptions<webapiContext> options)
            : base(options)
        {
        }

        public DbSet<webapi.Models.Attachment> Attachment { get; set; } = default!;
    }
}
