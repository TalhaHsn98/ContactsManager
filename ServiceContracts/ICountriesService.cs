using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Country entity
    /// </summary>
    public interface ICountriesService
    {
        CountryResponse AddCountry(CountryAddRequest? countryAddREquest);

        /// <summary>
        /// Returns all countries from the list
        /// </summary>
        /// <returns>All countries as the list</returns>
        List<CountryResponse> GetAllCountries();

        CountryResponse GetCountryByCountryID(Guid? countryID);
    }
}
