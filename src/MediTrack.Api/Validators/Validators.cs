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
    private const string PhonePattern = @"^\d{3}-\d{3}-\d{4}$";

    public CreatePatientValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("FirstName is required.")
            .MaximumLength(100)
            .WithMessage("FirstName must be at most 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("LastName is required.")
            .MaximumLength(100)
            .WithMessage("LastName must be at most 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob <= DateTime.UtcNow.AddYears(-1))
            .WithMessage("Patient must be at least 1 year old and DateOfBirth must be in the past.");

        RuleFor(x => x.Phone)
            .Matches(PhonePattern)
            .WithMessage("Phone must match the pattern XXX-XXX-XXXX (e.g., 555-123-4567).");
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
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("PatientId must be greater than 0.");

        RuleFor(x => x.DoctorId)
            .GreaterThan(0)
            .WithMessage("DoctorId must be greater than 0.");

        RuleFor(x => x.AppointmentDateTime)
            .Must(BeInTheFuture)
            .WithMessage("AppointmentDateTime must be in the future.")
            .Must(BeWithinBusinessHours)
            .WithMessage("AppointmentDateTime must be within business hours (Mon-Fri, 8AM–5PM).");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required.")
            .MinimumLength(10)
            .WithMessage("Reason must be at least 10 characters.")
            .MaximumLength(500)
            .WithMessage("Reason must be at most 500 characters.");
    }

    private static bool BeInTheFuture(DateTime dt)
    {
        return dt > DateTime.UtcNow;
    }

    private static bool BeWithinBusinessHours(DateTime dt)
    {
        // Business days: Monday..Friday
        if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
            return false;

        // Business hours: 08:00 through 17:00 (inclusive)
        var time = dt.TimeOfDay;
        var start = TimeSpan.FromHours(8);   // 08:00
        var end = TimeSpan.FromHours(17);    // 17:00
        return time >= start && time <= end;
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
