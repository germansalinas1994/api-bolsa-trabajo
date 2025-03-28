using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class Db_NOMBRE_BASE_Context : DbContext
{
    public Db_NOMBRE_BASE_Context()
    {
    }

    public Db_NOMBRE_BASE_Context(DbContextOptions<Db_NOMBRE_BASE_Context> options)
        : base(options)
    {
    }



    //ACA SE AGREGAN LAS TABLAS
    public virtual DbSet<Categoria> Categoria { get; set; }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("server=localhost;port=3306;database=mydb;uid=root;password=12345678;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

          modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categoria");

            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Descripcion).HasColumnName("Descripcion");
        
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
