using TestWork.Core.Models;

namespace TestWork.Core.Interfaces.Subscription;

public interface IApartmentPriceParser
{
    Task<ParsedApartmentModel> GetApartmentAsync(string apartmentUrl);
}
