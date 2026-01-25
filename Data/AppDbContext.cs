using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PraOndeFoi.Models;

namespace PraOndeFoi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Conta> Contas { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<Recorrencia> Recorrencias { get; set; }
        public DbSet<Assinatura> Assinaturas { get; set; }
        public DbSet<OrcamentoMensal> OrcamentosMensais { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TransacaoTag> TransacaoTags { get; set; }
        public DbSet<AnexoTransacao> AnexosTransacao { get; set; }
        public DbSet<MetaFinanceira> MetasFinanceiras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Conta)
                .WithOne(c => c.Usuario)
                .HasForeignKey<Conta>(c => c.UsuarioId);

            modelBuilder.Entity<Conta>()
                .HasIndex(c => c.UsuarioId)
                .IsUnique();

            modelBuilder.Entity<Conta>()
                .HasMany(c => c.Transacoes)
                .WithOne(t => t.Conta)
                .HasForeignKey(t => t.ContaId);

            modelBuilder.Entity<Transacao>()
                .HasIndex(t => new { t.ContaId, t.DataTransacao });

            modelBuilder.Entity<Transacao>()
                .HasIndex(t => new { t.ContaId, t.Tipo, t.DataTransacao });

            modelBuilder.Entity<Transacao>()
                .HasIndex(t => new { t.ContaId, t.CategoriaId, t.DataTransacao });

            modelBuilder.Entity<Transacao>()
                .HasOne(t => t.Categoria)
                .WithMany()
                .HasForeignKey(t => t.CategoriaId);

            modelBuilder.Entity<Tag>()
                .HasOne(t => t.Conta)
                .WithMany()
                .HasForeignKey(t => t.ContaId);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => new { t.ContaId, t.Nome })
                .IsUnique();

            modelBuilder.Entity<TransacaoTag>()
                .HasKey(tt => new { tt.TransacaoId, tt.TagId });

            modelBuilder.Entity<TransacaoTag>()
                .HasOne(tt => tt.Transacao)
                .WithMany(t => t.Tags)
                .HasForeignKey(tt => tt.TransacaoId);

            modelBuilder.Entity<TransacaoTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.Transacoes)
                .HasForeignKey(tt => tt.TagId);

            modelBuilder.Entity<AnexoTransacao>()
                .HasOne(a => a.Transacao)
                .WithMany(t => t.Anexos)
                .HasForeignKey(a => a.TransacaoId);

            modelBuilder.Entity<AnexoTransacao>()
                .HasIndex(a => a.TransacaoId);

            modelBuilder.Entity<Recorrencia>()
                .HasOne(r => r.Conta)
                .WithMany()
                .HasForeignKey(r => r.ContaId);

            modelBuilder.Entity<Recorrencia>()
                .HasIndex(r => new { r.ContaId, r.Ativa, r.ProximaExecucao });

            modelBuilder.Entity<Recorrencia>()
                .HasOne(r => r.Categoria)
                .WithMany()
                .HasForeignKey(r => r.CategoriaId);

            modelBuilder.Entity<Assinatura>()
                .HasOne(a => a.Conta)
                .WithMany()
                .HasForeignKey(a => a.ContaId);

            modelBuilder.Entity<Assinatura>()
                .HasIndex(a => new { a.ContaId, a.Ativa, a.ProximaCobranca });

            modelBuilder.Entity<Assinatura>()
                .HasOne(a => a.Categoria)
                .WithMany()
                .HasForeignKey(a => a.CategoriaId);

            modelBuilder.Entity<OrcamentoMensal>()
                .HasOne(o => o.Conta)
                .WithMany()
                .HasForeignKey(o => o.ContaId);

            modelBuilder.Entity<OrcamentoMensal>()
                .HasIndex(o => new { o.ContaId, o.Mes, o.Ano, o.CategoriaId })
                .IsUnique();

            modelBuilder.Entity<OrcamentoMensal>()
                .HasOne(o => o.Categoria)
                .WithMany()
                .HasForeignKey(o => o.CategoriaId);

            modelBuilder.Entity<MetaFinanceira>()
                .HasOne(m => m.Conta)
                .WithMany()
                .HasForeignKey(m => m.ContaId);

            modelBuilder.Entity<MetaFinanceira>()
                .HasIndex(m => new { m.ContaId, m.DataInicio });

            modelBuilder.Entity<MetaFinanceira>()
                .HasOne(m => m.Categoria)
                .WithMany()
                .HasForeignKey(m => m.CategoriaId);

            modelBuilder.Entity<Categoria>()
                .HasData(
                    new Categoria { Id = 1, Nome = "Salário", Predefinida = true },
                    new Categoria { Id = 2, Nome = "Freelance", Predefinida = true },
                    new Categoria { Id = 3, Nome = "Aluguel", Predefinida = true },
                    new Categoria { Id = 4, Nome = "Alimentação", Predefinida = true },
                    new Categoria { Id = 5, Nome = "Transporte", Predefinida = true },
                    new Categoria { Id = 6, Nome = "Saúde", Predefinida = true },
                    new Categoria { Id = 7, Nome = "Educação", Predefinida = true },
                    new Categoria { Id = 8, Nome = "Lazer", Predefinida = true },
                    new Categoria { Id = 9, Nome = "Investimentos", Predefinida = true },
                    new Categoria { Id = 10, Nome = "Assinaturas", Predefinida = true }
                );
        }

    }
}