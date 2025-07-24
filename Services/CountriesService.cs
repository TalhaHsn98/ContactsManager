using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ApplicationDbContext _db;

        public CountriesService(ApplicationDbContext db)
        {

            _db = db;
           
        }
        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddREquest)
        {

            if (countryAddREquest == null) { 
                throw new ArgumentNullException(nameof(countryAddREquest));
            }

            if (countryAddREquest.CountryName == null) { 
                throw new ArgumentException(nameof(countryAddREquest.CountryName)); 
            }

            if (await _db.Countries.Where(temp => temp.CountryName == countryAddREquest.CountryName).CountAsync() > 0) {
                throw new ArgumentException("Given Country already in the list");
            }


            Country country = countryAddREquest.ToCountry();
            // guid
            country.CountryID = Guid.NewGuid();
            _db.Countries.Add(country);
            await _db.SaveChangesAsync();
            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            return await _db.Countries.Select(country => country.ToCountryResponse()).ToListAsync();
        }

        public async Task<CountryResponse> GetCountryByCountryID(Guid? countryID)
        {
            if (countryID == null) return null;

            Country? country_from_the_list = await _db.Countries.FirstOrDefaultAsync(temp => temp.CountryID == countryID);

            if (country_from_the_list == null) return null;

            return country_from_the_list.ToCountryResponse();
        
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            MemoryStream memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            int countriesInserted = 0;
            ExcelPackage.License.SetNonCommercialPersonal("Talha");


            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets["Countries"];

                int rowCount = workSheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    string? cellValue = Convert.ToString(workSheet.Cells[row, 1].Value);

                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        string? countryName = cellValue;

                        if (_db.Countries.Where(temp => temp.CountryName == countryName).Count() == 0)
                        {
                            Country country = new Country() { CountryName = countryName };
                            _db.Countries.Add(country);
                            await _db.SaveChangesAsync();

                            countriesInserted++;
                        }
                    }
                }
            }

            return countriesInserted;
        }
    }
}
