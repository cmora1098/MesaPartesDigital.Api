using Microsoft.EntityFrameworkCore;
using MesaPartesDigital.Models;

namespace MesaPartesDigital.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TipoDocPer> TipoDocPers { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<TipoPersona> TipoPersonas { get; set; }
        //public DbSet<PersonaBusquedaDto> PersonaBusquedas { get; set; }
        public DbSet<PersonaNaturalDto> PersonaNaturales { get; set; }
        public DbSet<PersonaJuridicaDto> PersonaJuridicas { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🔽 Le decimos a EF que este modelo no tiene una tabla física con Primary Key
            //modelBuilder.Entity<PersonaBusquedaDto>().HasNoKey(); 
            modelBuilder.Entity<PersonaNaturalDto>().HasNoKey();
            modelBuilder.Entity<PersonaJuridicaDto>().HasNoKey();
        }

        //public async Task<List<PersonaBusquedaDto>> ObtenerPersonaPorDocumentoAsync(int iCodTipoDocPer, string vDocPer)
        //{
        //    return await this.PersonaBusquedas
        //        .FromSqlInterpolated($"EXEC [dbo].[USP_Persona_ObtenerPorDocumento] @iCodTipoDocPer={iCodTipoDocPer}, @vDocPer={vDocPer}")
        //        .ToListAsync();
        //}

         public async Task<List<PersonaNaturalDto>> ObtenerPersonaPorDocumentoAsync(int iCodTipoDocPer, string vDocPer)
        {
            return await this.PersonaNaturales
                .FromSqlInterpolated($"EXEC [dbo].[USP_Persona_ObtenerPorDocumento] @iCodTipoDocPer={iCodTipoDocPer}, @vDocPer={vDocPer}")
                .ToListAsync();
        }

        public async Task<List<PersonaJuridicaDto>> ObtenerPersonaJuridicaPorRucAsync(int iCodTipoDocPer, string vDocPer)
        {
            return await this.PersonaJuridicas
                .FromSqlInterpolated($"EXEC [dbo].[USP_PersonaJuridica_ObtenerPorRUC] @iCodTipoDocPer={iCodTipoDocPer}, @vDocPer={vDocPer}")
                .ToListAsync();
        }


    }
}