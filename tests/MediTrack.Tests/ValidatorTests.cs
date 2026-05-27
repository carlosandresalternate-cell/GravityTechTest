using FluentValidation.TestHelper;
using MediTrack.Api.DTOs;
using MediTrack.Api.Validators;

namespace MediTrack.Tests;

/// <summary>
/// TODO (Task 4): Implement unit tests for validators.
/// 
/// Required test cases for CreatePatientValidator:
/// 
/// 1. Valid patient passes validation
/// 2. Empty FirstName fails
/// 3. Empty LastName fails
/// 4. Invalid email format fails
/// 5. Future DateOfBirth fails
/// 6. DateOfBirth less than 1 year ago fails
/// 7. Invalid phone format fails (not matching XXX-XXX-XXXX)
/// 8. Valid phone format passes (matching XXX-XXX-XXXX)
///
/// Required test cases for BookAppointmentValidator:
/// 
/// 1. Valid request passes
/// 2. PatientId of 0 fails
/// 3. DoctorId of 0 fails
/// 4. Past AppointmentDateTime fails
/// 5. Weekend AppointmentDateTime fails (Saturday/Sunday)
/// 6. Before 8AM AppointmentDateTime fails
/// 7. After 5PM AppointmentDateTime fails
/// 8. Reason shorter than 10 chars fails
/// 9. Reason longer than 500 chars fails
/// </summary>
public class ValidatorTests
{
    // Hint: Use FluentValidation's TestHelper for cleaner assertions:
    //
    // var validator = new CreatePatientValidator();
    // var model = new CreatePatientRequest(...);
    // var result = validator.TestValidate(model);
    // result.ShouldNotHaveAnyValidationErrors();
    //   -- or --
    // result.ShouldHaveValidationErrorFor(x => x.FirstName);

    [Fact]
    public void CreatePatient_ValidRequest_ShouldPass()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }

    [Fact]
    public void CreatePatient_EmptyFirstName_ShouldFail()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }

    [Fact]
    public void CreatePatient_FutureDateOfBirth_ShouldFail()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }

    [Fact]
    public void CreatePatient_InvalidEmail_ShouldFail()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }

    [Fact]
    public void CreatePatient_InvalidPhoneFormat_ShouldFail()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }

    [Fact]
    public void BookAppointment_ValidRequest_ShouldPass()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }

    [Fact]
    public void BookAppointment_PastDate_ShouldFail()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }

    [Fact]
    public void BookAppointment_WeekendDate_ShouldFail()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }

    [Fact]
    public void BookAppointment_OutsideBusinessHours_ShouldFail()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }

    [Fact]
    public void BookAppointment_ShortReason_ShouldFail()
    {
        Assert.True(true, "TODO: Implement after completing Task 3 (validators)");
    }
}
