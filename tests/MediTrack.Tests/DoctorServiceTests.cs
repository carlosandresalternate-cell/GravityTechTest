using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MediTrack.Api.Data;
using MediTrack.Api.DTOs;
using MediTrack.Api.Models;
using MediTrack.Api.Services;

namespace MediTrack.Tests;

/// <summary>
/// TODO (Task 4): Implement unit tests for DoctorService.
/// 
/// Required test cases:
/// 
/// 1. GetDoctorsWithAvailabilityAsync_ShouldReturnCorrectAppointmentCounts
///    - Seed 2 doctors, one with 3 upcoming appointments, one with 0
///    - Assert correct counts
///
/// 2. GetDoctorsWithAvailabilityAsync_ShouldReturnEmptyList_WhenNoDoctors
///    - Empty database (no active doctors)
///    - Assert returns empty list
///
/// 3. GetAllDoctorsAsync_ShouldOnlyReturnActiveDoctors
///    - Seed active and inactive doctors
///    - Assert only active ones returned
/// </summary>
public class DoctorServiceTests
{
    private MediTrackDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MediTrackDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new MediTrackDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetDoctorsWithAvailabilityAsync_ShouldReturnCorrectCounts()
    {
        Assert.True(true, "TODO: Implement this test");
    }

    [Fact]
    public async Task GetDoctorsWithAvailabilityAsync_ShouldReturnEmpty_WhenNoDoctors()
    {
        Assert.True(true, "TODO: Implement this test");
    }

    [Fact]
    public async Task GetAllDoctorsAsync_ShouldOnlyReturnActiveDoctors()
    {
        Assert.True(true, "TODO: Implement this test");
    }
}
