using Microsoft.EntityFrameworkCore;
using MediTrack.Api.Models;

namespace MediTrack.Api.Data;

public class MediTrackDbContext : DbContext
{
    public MediTrackDbContext(DbContextOptions<MediTrackDbContext> options) : base(options) { }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Specialty> Specialties => Set<Specialty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ──── Specialty Configuration ────
        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
        });

        // ──── Doctor Configuration ────
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(d => d.LastName).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Email).IsRequired().HasMaxLength(200);
            entity.Ignore(d => d.FullName); // Computed property, not stored

            entity.HasOne(d => d.Specialty)
                  .WithMany(s => s.Doctors)
                  .HasForeignKey(d => d.SpecialtyId);
        });

        // ──── Patient Configuration ────
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Email).IsRequired().HasMaxLength(200);
            entity.Ignore(p => p.FullName);
        });

        // ──── Appointment Configuration ────
        // BUG: Missing concurrency token on Appointment — needed for double-booking prevention
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Reason).IsRequired().HasMaxLength(500);

            entity.HasOne(a => a.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);  // Don't cascade delete

            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ──── Seed Data ────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Specialties
        modelBuilder.Entity<Specialty>().HasData(
            new Specialty { Id = 1, Name = "General Practice", Description = "Primary care physician" },
            new Specialty { Id = 2, Name = "Cardiology", Description = "Heart and cardiovascular system" },
            new Specialty { Id = 3, Name = "Dermatology", Description = "Skin conditions" },
            new Specialty { Id = 4, Name = "Orthopedics", Description = "Musculoskeletal system" },
            new Specialty { Id = 5, Name = "Pediatrics", Description = "Children's health" }
        );

        // Doctors
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@meditrack.com", Phone = "555-100-0001", SpecialtyId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Doctor { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@meditrack.com", Phone = "555-100-0002", SpecialtyId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Doctor { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@meditrack.com", Phone = "555-100-0003", SpecialtyId = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Doctor { Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@meditrack.com", Phone = "555-100-0004", SpecialtyId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Doctor { Id = 5, FirstName = "Lisa", LastName = "Park", Email = "lisa.park@meditrack.com", Phone = "555-100-0005", SpecialtyId = 5, IsActive = false, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Patients
        modelBuilder.Entity<Patient>().HasData(
            new Patient { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@email.com", Phone = "555-200-0001", DateOfBirth = new DateTime(1985, 6, 15, 0, 0, 0, DateTimeKind.Utc), Address = "123 Main St", IsActive = true, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Patient { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@email.com", Phone = "555-200-0002", DateOfBirth = new DateTime(1990, 3, 22, 0, 0, 0, DateTimeKind.Utc), Address = "456 Oak Ave", IsActive = true, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Patient { Id = 3, FirstName = "Bob", LastName = "Wilson", Email = "bob.wilson@email.com", Phone = "555-200-0003", DateOfBirth = new DateTime(1978, 11, 8, 0, 0, 0, DateTimeKind.Utc), Address = "789 Pine Rd", IsActive = true, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Patient { Id = 4, FirstName = "Alice", LastName = "Brown", Email = "alice.brown@email.com", Phone = "555-200-0004", DateOfBirth = new DateTime(2000, 1, 30, 0, 0, 0, DateTimeKind.Utc), IsActive = false, CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Appointments (some in the past, some upcoming)
        modelBuilder.Entity<Appointment>().HasData(
            new Appointment { Id = 1, PatientId = 1, DoctorId = 1, AppointmentDateTime = new DateTime(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc), Reason = "Annual checkup", Status = AppointmentStatus.Completed, CreatedAt = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 2, PatientId = 2, DoctorId = 2, AppointmentDateTime = new DateTime(2025, 2, 20, 10, 30, 0, DateTimeKind.Utc), Reason = "Heart palpitations", Status = AppointmentStatus.Completed, CreatedAt = new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 3, PatientId = 1, DoctorId = 2, AppointmentDateTime = new DateTime(2025, 12, 10, 14, 0, 0, DateTimeKind.Utc), Reason = "Follow-up cardiology", Status = AppointmentStatus.Scheduled, CreatedAt = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 4, PatientId = 3, DoctorId = 3, AppointmentDateTime = new DateTime(2025, 12, 15, 11, 0, 0, DateTimeKind.Utc), Reason = "Skin rash evaluation", Status = AppointmentStatus.Scheduled, CreatedAt = new DateTime(2025, 11, 5, 0, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 5, PatientId = 2, DoctorId = 1, AppointmentDateTime = new DateTime(2025, 12, 20, 8, 30, 0, DateTimeKind.Utc), Reason = "Flu symptoms", Status = AppointmentStatus.Scheduled, CreatedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Appointment { Id = 6, PatientId = 1, DoctorId = 4, AppointmentDateTime = new DateTime(2025, 3, 5, 15, 0, 0, DateTimeKind.Utc), Reason = "Knee pain", Status = AppointmentStatus.Cancelled, CreatedAt = new DateTime(2025, 2, 20, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
