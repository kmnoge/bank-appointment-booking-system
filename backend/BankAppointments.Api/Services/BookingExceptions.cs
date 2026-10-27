namespace BankAppointments.Api.Services;

public sealed class BookingValidationException(string message) : Exception(message);

public sealed class SlotUnavailableException(string message) : Exception(message);
