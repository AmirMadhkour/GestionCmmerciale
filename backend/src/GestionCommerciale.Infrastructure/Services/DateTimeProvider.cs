using GestionCommerciale.Application.Common.Interfaces;

namespace GestionCommerciale.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}