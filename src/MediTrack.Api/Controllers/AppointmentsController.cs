using Microsoft.AspNetCore.Mvc;
using MediTrack.Api.DTOs;
using MediTrack.Api.Services;

namespace MediTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Get upcoming appointments, optionally filtered by doctor.
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetUpcoming([FromQuery] int? doctorId)
    {
        var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(doctorId);
        return Ok(appointments);
    }

    /// <summary>
    /// Get an appointment by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentResponse>> GetById(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }

    /// <summary>
    /// Book a new appointment.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> Book([FromBody] BookAppointmentRequest request)
    {
        try
        {
            var appointment = await _appointmentService.BookAppointmentAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an appointment.
    /// </summary>
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var result = await _appointmentService.CancelAppointmentAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reschedule an appointment.
    /// TODO: Implement (Task 2c)
    /// </summary>
    [HttpPut("{id:int}/reschedule")]
    public async Task<ActionResult<AppointmentResponse>> Reschedule(
        int id,
        [FromBody] RescheduleAppointmentRequest request)
    {
        // TODO: Call _appointmentService.RescheduleAppointmentAsync
        // Return appropriate status codes:
        //   200 OK with the updated appointment
        //   404 if appointment not found
        //   400 if appointment is not in Scheduled status
        //   409 if the new time slot is already taken

        var rescheduledAppointment = await _appointmentService.RescheduleAppointmentAsync(id, request);
        
        if (rescheduledAppointment is null)
        {
            return NotFound();
        }

        return Ok(rescheduledAppointment);
    }
}
