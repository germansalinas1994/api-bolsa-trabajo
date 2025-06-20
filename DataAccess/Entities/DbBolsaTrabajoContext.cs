using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class DbBolsaTrabajoContext : DbContext
{
    public DbBolsaTrabajoContext()
    {
    }

    public DbBolsaTrabajoContext(DbContextOptions<DbBolsaTrabajoContext> options)
        : base(options)
    {
    }



    //ACA SE AGREGAN LAS TABLAS
    public virtual DbSet<Prueba> Prueba { get; set; }



//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseSqlServer("Server=db-utn.ctziwii5swvc0.us-east-1.rds.amazonaws.com,1433;Database=dbBolsaTrabajo;User Id=admin;Password=Utnfrlp2025;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

          modelBuilder.Entity<Prueba>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Prueba");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre).HasColumnName("name");
        
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
