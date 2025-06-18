using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;

namespace CRUDExample.Controllers
{
    [Route("persons")]
    public class PersonsController : Controller
    {

        private readonly ICountriesService _countryService;
        private readonly IPersonsService _personsService;

        public PersonsController(ICountriesService countriesService, IPersonsService personsService)
        {
            _countryService = countriesService;
            _personsService = personsService;
        }

        [Route("[action]")]
        [Route("/")]
        public IActionResult Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            //Search
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                    { nameof(PersonResponse.PersonName), "Person Name" },
                    { nameof(PersonResponse.Email), "Email" },
                    { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                    { nameof(PersonResponse.Gender), "Gender" },
                    { nameof(PersonResponse.CountryID), "Country" },
                    { nameof(PersonResponse.Address), "Address" }
            };
            List<PersonResponse> persons = _personsService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //Sort
            List<PersonResponse> sortedPersons = _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedPersons); //Views/Persons/Index.cshtml
        }


        [Route("[action]")]
        [HttpGet]
        public IActionResult Create()
        {
            List<CountryResponse> countries = _countryService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp => 
            new SelectListItem { Text = temp.CountryName, Value = temp.CountryID.ToString()});
            return View();
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = _countryService.GetAllCountries();
                ViewBag.Countries = countries;

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            //call the service method
            PersonResponse personResponse = _personsService.AddPerson(personAddRequest);

            //navigate to Index() action method (it makes another get request to "persons/index"
            return RedirectToAction("Index", "Persons");
        }


        [HttpGet]
        [Route("[action]/{PersonId}")]

        public IActionResult Edit(Guid PersonId)
        {
            PersonResponse? personResponse = _personsService.GetPersonByPersonID(PersonId);

            if(personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();
            List<CountryResponse> countries = _countryService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
            new SelectListItem { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            return View(personUpdateRequest);

        }

        [HttpPost]
        [Route("[action]/{PersonId}")]
        public IActionResult Edit(PersonUpdateRequest personUpdateRequest) {

            PersonResponse? personResponse = _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null) { 
                
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid) 
            {
                PersonResponse updatedPerson = _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index");
            }
            else
            {

                List<CountryResponse> countries = _countryService.GetAllCountries();
                ViewBag.Countries = countries.Select(temp =>
                new SelectListItem { Text = temp.CountryName, Value = temp.CountryID.ToString() });

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(personResponse.ToPersonUpdateRequest());

            }
        }

        [HttpGet]
        [Route("[action]/{PersonId}")]

        public IActionResult Delete(Guid PersonId) 
        {
            PersonResponse? personResponse = _personsService.GetPersonByPersonID(PersonId);

            if (personResponse == null) {
                return RedirectToAction("Index");
            }

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{PersonId}")]
        public IActionResult Delete(PersonUpdateRequest personUpdateRequest) {
            PersonResponse? personResponse = _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null) {
                return RedirectToAction("Index");
            }

            _personsService.DeletePerson(personResponse.PersonID);

            return RedirectToAction("Index");
        }



    }
}
