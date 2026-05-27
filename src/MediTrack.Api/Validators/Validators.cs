using FluentValidation;
using MediTrack.Api.DTOs;

namespace MediTrack.Api.Validators;

/// <summary>
/// TODO (Task 3): Complete this validator.
/// 
/// Rules:
///   - FirstName: required, max 100 chars
///   - LastName: required, max 100 chars
///   - Email: required, valid email format
///   - DateOfBirth: must be in the past, patient must be at least 1 year old
///   - Phone: must match pattern XXX-XXX-XXXX (e.g., "555-123-4567")
/// </summary>
public class CreatePatientValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientValidator()
    {
        // TODO: Add validation rules
        // Example:
        // RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
    }
}

/// <summary>
/// TODO (Task 3): Complete this validator.
/// 
/// Rules:
///   - PatientId: must be greater than 0
///   - DoctorId: must be greater than 0
///   - AppointmentDateTime: must be in the future
///   - AppointmentDateTime: must be within business hours (Mon-Fri, 8AM-5PM)
///   - Reason: required, between 10 and 500 characters
/// </summary>
public class BookAppointmentValidator : AbstractValidator<BookAppointmentRequest>
{
    public BookAppointmentValidator()
    {
        // TODO: Add validation rules
    }
}

/// <summary>
/// TODO (Task 3 — bonus): Validator for reschedule request.
/// </summary>
public class RescheduleAppointmentValidator : AbstractValidator<RescheduleAppointmentRequest>
{
    public RescheduleAppointmentValidator()
    {
        // TODO: Add validation rules
        // - NewDateTime must be in the future
        // - NewDateTime must be within business hours
    }
}
