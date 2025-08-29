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
    public virtual DbSet<EstadoOferta> EstadoOferta { get; set; }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

          modelBuilder.Entity<Prueba>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Prueba");

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").IsRequired();
        
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
