using Microsoft.EntityFrameworkCore;
using ClyvoVetApi.Models;

namespace ClyvoVetApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Dono> Donos { get; set; } = null!;
    public DbSet<Funcionario> Funcionarios { get; set; } = null!;
    public DbSet<Pet> Pets { get; set; } = null!;
    public DbSet<Consulta> Consultas { get; set; } = null!;
    public DbSet<Medicamento> Medicamentos { get; set; } = null!;
    public DbSet<ConsultaMedicamento> ConsultasMedicamentos { get; set; } = null!;
    public DbSet<Vacina> Vacinas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. DONOS
        modelBuilder.Entity<Dono>(entity =>
        {
            entity.ToTable("DONOS");
            entity.HasKey(e => e.Id).HasName("DONOS_PK");
            
            entity.Property(e => e.Id).HasColumnName("dono_id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).HasColumnName("dono_nome").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Telefone).HasColumnName("dono_telefone").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasColumnName("dono_email").HasMaxLength(50).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_DONO_EMAIL");
        });

        // 2. FUNCIONARIOS
        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.ToTable("FUNCIONARIOS");
            entity.HasKey(e => e.Id).HasName("FUNCIONARIOS_PK");

            entity.Property(e => e.Id).HasColumnName("funcionario_id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).HasColumnName("funcionario_nome").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Setor).HasColumnName("funcionario_setor").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Cargo).HasColumnName("funcionario_cargo").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasColumnName("funcionario_email").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Telefone).HasColumnName("funcionario_telefone").HasMaxLength(20).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_FUNC_EMAIL");
        });

        // 3. PETS
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.ToTable("PETS");
            entity.HasKey(e => e.Id).HasName("PETS_PK");

            entity.Property(e => e.Id).HasColumnName("pet_id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).HasColumnName("pet_nome").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DataNascimento).HasColumnName("pet_data_nascimento").IsRequired();
            entity.Property(e => e.Peso).HasColumnName("pet_peso").HasColumnType("DECIMAL(5, 2)").IsRequired();
            entity.Property(e => e.Especie).HasColumnName("pet_especie").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Raca).HasColumnName("pet_raca").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DonoId).HasColumnName("dono_id").IsRequired();

            entity.HasOne(e => e.Dono)
                .WithMany(d => d.Pets)
                .HasForeignKey(e => e.DonoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("PETS_DONOS_FK");
        });

        // 4. CONSULTAS
        modelBuilder.Entity<Consulta>(entity =>
        {
            entity.ToTable("CONSULTAS");
            entity.HasKey(e => e.Id).HasName("CONSULTAS_PK");

            entity.Property(e => e.Id).HasColumnName("consulta_id").ValueGeneratedOnAdd();
            entity.Property(e => e.Tipo).HasColumnName("consulta_tipo").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Valor).HasColumnName("consulta_valor").HasColumnType("DECIMAL(10, 2)").IsRequired();
            entity.Property(e => e.Descricao).HasColumnName("consulta_descricao").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Status).HasColumnName("consulta_status").HasMaxLength(1).IsFixedLength().IsRequired();
            entity.Property(e => e.Data).HasColumnName("consulta_data").IsRequired();
            entity.Property(e => e.PetId).HasColumnName("pet_id").IsRequired();
            entity.Property(e => e.FuncionarioId).HasColumnName("funcionario_id").IsRequired();

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Consultas)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("CONSULTAS_PETS_FK");

            entity.HasOne(e => e.Funcionario)
                .WithMany(f => f.Consultas)
                .HasForeignKey(e => e.FuncionarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("CONSULTAS_FUNCIONARIOS_FK");

            // Check Constraint for Status (A, C, R)
            entity.ToTable(t => t.HasCheckConstraint("CK_CONSULTA_STATUS", "consulta_status IN ('A', 'C', 'R')"));
        });

        // 5. MEDICAMENTOS
        modelBuilder.Entity<Medicamento>(entity =>
        {
            entity.ToTable("MEDICAMENTOS");
            entity.HasKey(e => e.Id).HasName("MEDICAMENTOS_PK");

            entity.Property(e => e.Id).HasColumnName("medicamento_id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).HasColumnName("medicamento_nome").HasMaxLength(100).IsRequired();
        });

        // 6. CONSULTAS_MEDICAMENTOS
        modelBuilder.Entity<ConsultaMedicamento>(entity =>
        {
            entity.ToTable("CONSULTAS_MEDICAMENTOS");
            entity.HasKey(e => e.Id).HasName("CONSULTAS_MEDICAMENTOS_PK");

            entity.Property(e => e.Id).HasColumnName("consulta_medicamento_id").ValueGeneratedOnAdd();
            entity.Property(e => e.ConsultaId).HasColumnName("consulta_id").IsRequired();
            entity.Property(e => e.MedicamentoId).HasColumnName("medicamento_id").IsRequired();
            entity.Property(e => e.Dosagem).HasColumnName("cm_dosagem").HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Consulta)
                .WithMany(c => c.ConsultasMedicamentos)
                .HasForeignKey(e => e.ConsultaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("CM_CONSULTAS_FK");

            entity.HasOne(e => e.Medicamento)
                .WithMany(m => m.ConsultasMedicamentos)
                .HasForeignKey(e => e.MedicamentoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("CM_MEDICAMENTOS_FK");
        });

        // 7. VACINAS
        modelBuilder.Entity<Vacina>(entity =>
        {
            entity.ToTable("VACINAS");
            entity.HasKey(e => e.Id).HasName("VACINAS_PK");

            entity.Property(e => e.Id).HasColumnName("vacina_id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).HasColumnName("vacina_nome").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Data).HasColumnName("vacina_data").IsRequired();
            entity.Property(e => e.Status).HasColumnName("vacina_status").HasMaxLength(1).IsFixedLength().IsRequired();
            entity.Property(e => e.PetId).HasColumnName("pet_id").IsRequired();

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Vacinas)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("VACINAS_PETS_FK");

            // Check Constraint for Status (P, A)
            entity.ToTable(t => t.HasCheckConstraint("CK_VACINA_STATUS", "vacina_status IN ('P', 'A')"));
        });
    }
}