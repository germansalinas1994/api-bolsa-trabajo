using System;
using System.Collections.Generic;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public partial class DbBolsaTrabajoContext : DbContext
{
    public DbBolsaTrabajoContext(DbContextOptions<DbBolsaTrabajoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Carrera> Carrera { get; set; }

    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<EstadoOferta> EstadoOferta { get; set; }

    public virtual DbSet<EstadoPostulacion> EstadoPostulacion { get; set; }

    public virtual DbSet<EstadoValidacion> EstadoValidacion { get; set; }

    public virtual DbSet<Genero> Genero { get; set; }

    public virtual DbSet<Localidad> Localidad { get; set; }

    public virtual DbSet<Modalidad> Modalidad { get; set; }

    public virtual DbSet<Notificacion> Notificacion { get; set; }

    public virtual DbSet<Oferta> Oferta { get; set; }

    public virtual DbSet<OfertaCarrera> OfertaCarrera { get; set; }

    public virtual DbSet<OfertaCategoria> OfertaCategoria { get; set; }

    public virtual DbSet<OfertaHistorial> OfertaHistorial { get; set; }

    public virtual DbSet<Pais> Pais { get; set; }

    public virtual DbSet<PerfilCandidato> PerfilCandidato { get; set; }

    public virtual DbSet<PerfilEmpresa> PerfilEmpresa { get; set; }

    public virtual DbSet<Postulacion> Postulacion { get; set; }

    public virtual DbSet<PostulacionHistorial> PostulacionHistorial { get; set; }

    public virtual DbSet<Provincia> Provincia { get; set; }

    public virtual DbSet<Rol> Rol { get; set; }

    public virtual DbSet<TipoContrato> TipoContrato { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Oferta>(e =>
        {
            e.HasOne(x => x.PerfilEmpresa)
            .WithMany()
            .HasForeignKey(x => x.IdPerfilEmpresa);

            e.HasOne(x => x.Modalidad)
            .WithMany()
            .HasForeignKey(x => x.IdModalidad);

            e.HasOne(x => x.TipoContrato)
            .WithMany()
            .HasForeignKey(x => x.IdTipoContrato);

            e.HasOne(x => x.Localidad)
            .WithMany()
            .HasForeignKey(x => x.IdLocalidad);
        });

        modelBuilder.Entity<Localidad>(e =>
       {
           e.HasOne(x => x.Provincia)
           .WithMany()
           .HasForeignKey(x => x.IdProvincia);

       });

        modelBuilder.Entity<Notificacion>(e =>
      {
          e.HasOne(x => x.Postulacion)
          .WithMany()
          .HasForeignKey(x => x.IdPostulacion);

          e.HasOne(x => x.Usuario)
          .WithMany()
          .HasForeignKey(x => x.IdUsuario);
      });

        modelBuilder.Entity<OfertaCarrera>(e =>
      {
          e.HasOne(x => x.Carrera)
          .WithMany()
          .HasForeignKey(x => x.IdCarrera);

          e.HasOne(x => x.Oferta)
          .WithMany()
          .HasForeignKey(x => x.IdOferta);
      });

        modelBuilder.Entity<OfertaCategoria>(e =>
        {
            e.HasOne(x => x.Categoria)
            .WithMany()
            .HasForeignKey(x => x.IdCategoria);

            e.HasOne(x => x.Oferta)
            .WithMany()
            .HasForeignKey(x => x.IdOferta);
        });

        modelBuilder.Entity<OfertaHistorial>(e =>
      {
          e.HasOne(x => x.EstadoOferta)
          .WithMany()
          .HasForeignKey(x => x.IdEstadoOferta);

          e.HasOne(x => x.Oferta)
          .WithMany()
          .HasForeignKey(x => x.IdOferta);
      });

        modelBuilder.Entity<PerfilCandidato>(e =>
       {
           e.HasOne(x => x.Genero)
           .WithMany()
           .HasForeignKey(x => x.IdGenero);

           e.HasOne(x => x.Usuario)
           .WithMany()
           .HasForeignKey(x => x.IdUsuario);

           e.HasOne(x => x.Carrera)
           .WithMany()
           .HasForeignKey(x => x.IdCarrera);
       });

        modelBuilder.Entity<PerfilEmpresa>(e =>
      {
          e.HasOne(x => x.EstadoValidacion)
          .WithMany()
          .HasForeignKey(x => x.IdEstadoValidacion);

          e.HasOne(x => x.Usuario)
          .WithMany()
          .HasForeignKey(x => x.IdUsuario);
      });

        modelBuilder.Entity<Postulacion>(e =>
     {
         e.HasOne(x => x.Oferta)
         //si quisiera mapear una relacion inversa para traer la coleccion directamente
         .WithMany(o => o.Postulaciones)
        //  .WithMany()
         .HasForeignKey(x => x.IdOferta);

         e.HasOne(x => x.PerfilCandidato)
         .WithMany()
         .HasForeignKey(x => x.IdPerfilCandidato);
     });

        modelBuilder.Entity<PostulacionHistorial>(e =>
           {
               e.HasOne(x => x.EstadoPostulacion)
               .WithMany()
               .HasForeignKey(x => x.IdEstadoPostulacion);

               e.HasOne(x => x.Postulacion)
               .WithMany(p => p.Historial)
               .HasForeignKey(x => x.IdPostulacion);
           });

        modelBuilder.Entity<Provincia>(e =>
       {
           e.HasOne(x => x.Pais)
           .WithMany()
           .HasForeignKey(x => x.IdPais);

       });

        modelBuilder.Entity<Usuario>(e =>
      {
          e.HasOne(x => x.Rol)
          .WithMany()
          .HasForeignKey(x => x.IdRol);

      });



        OnModelCreatingPartial(modelBuilder);

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
