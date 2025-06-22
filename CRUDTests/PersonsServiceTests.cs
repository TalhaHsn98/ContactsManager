using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CRUDTests
{
    public class PersonsServiceTests
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countryService;
        private readonly ITestOutputHelper _testOutputHelper;

        //constructor
        public PersonsServiceTests(ITestOutputHelper testOutputHelper)
        {
            _countryService = new CountriesService(new PersonDbContext(new DbContextOptionsBuilder<PersonDbContext>().Options));

            _personsService = new PersonsService(new PersonDbContext(new DbContextOptionsBuilder<PersonDbContext>().Options), _countryService);

            _testOutputHelper = testOutputHelper;
        }



        #region AddPerson

        [Fact]
        public async Task AddPerson_nullValue()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;

            //Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async() =>
            {
                await _personsService.AddPerson(personAddRequest);
            });
        }


        [Fact]
        public async Task AddPerson_PersonNameNull()
        {
            //Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest() { PersonName = null };

            //Act
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _personsService.AddPerson(personAddRequest);
            });
        }


        //When we supply proper person details, it should insert the person into the persons list; and it should return an object of PersonResponse, which includes with the newly generated person id
        [Fact]
        public async Task AddPerson_ProperPersonDetails()
        {
            //Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest() { PersonName = "Person name...", Email = "person@example.com", Address = "sample address", CountryID = Guid.NewGuid(), Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"), ReceiveNewsLetters = true };

            //Act
            PersonResponse person_response_from_add = await _personsService.AddPerson(personAddRequest);

            List<PersonResponse> persons_list = await _personsService.GetAllPersons();

            //Assert
            Assert.True(person_response_from_add.PersonID != Guid.Empty);

            Assert.Contains(person_response_from_add, persons_list);
        }

        #endregion

        #region GetPersonByPersonID

        [Fact]
        public async Task GetPersonByPersonID_NullPersonID()
        {
            Guid? personID = null;

            PersonResponse? Response_from_get_Method = await _personsService.GetPersonByPersonID(personID);

            //Assert
            Assert.Null(Response_from_get_Method);
        }

        //If we supply a valid person ID then the method in the service shall respond with the 
        //valid person details

        [Fact]

        public async Task GetPersonByPersonID_ValidPersonID()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "Canada" };

            CountryResponse Respone_from_Add_Country = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest? personAddRequest = new PersonAddRequest() { PersonName = "abc", Email = "person@example.com", Address = "sample address", CountryID = Respone_from_Add_Country.CountryID, Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2000-01-01"), ReceiveNewsLetters = true };

            PersonResponse Response_from_add_Method = await _personsService.AddPerson(personAddRequest);

            Guid PersonID_StoredinDatabase = Response_from_add_Method.PersonID;

            PersonResponse? Response_from_Get_Method = await _personsService.GetPersonByPersonID(PersonID_StoredinDatabase);

            //Assert
            Assert.Equal(Response_from_add_Method, Response_from_Get_Method);

        }
        #endregion

        #region GetAllPerson

        [Fact]

        public async Task GetAllPerson_EmptyList()
        {
            List<PersonResponse> personResponse_FromGetAll = await _personsService.GetAllPersons();

            //Assert
            Assert.Empty(personResponse_FromGetAll);
        }

        //If we add few persons, then the getallperson method shall return us all the persons we stored in our list or database
        [Fact]
        public async Task GetAllPersons_AddfewPersons()
        {
            CountryAddRequest countryrequest_1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryrequest_2 = new CountryAddRequest() { CountryName = "Canada" };

            CountryResponse CountryResponse_1 = await _countryService.AddCountry(countryrequest_1);
            CountryResponse CountryResponse_2 = await _countryService.AddCountry(countryrequest_2);

            PersonAddRequest? personAddRequest_1 = new PersonAddRequest() { PersonName = "abc2", Email = "person2@example.com", Address = "sample 2 address", CountryID = CountryResponse_1.CountryID, Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2005-01-01"), ReceiveNewsLetters = true };
            PersonAddRequest? personAddRequest_2 = new PersonAddRequest() { PersonName = "abc3", Email = "person3@example.com", Address = "sample 3 address", CountryID = CountryResponse_2.CountryID, Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2005-01-01"), ReceiveNewsLetters = true };
            PersonAddRequest? personAddRequest_3 = new PersonAddRequest() { PersonName = "abc4", Email = "person4@example.com", Address = "sample 4 address", CountryID = CountryResponse_2.CountryID, Gender = GenderOptions.Male, DateOfBirth = DateTime.Parse("2004-01-01"), ReceiveNewsLetters = true };

            List<PersonAddRequest> personaddrequests = new List<PersonAddRequest> { personAddRequest_1, personAddRequest_2, personAddRequest_3 };

            
            List<PersonResponse> responselists_fromAddMethod = new List<PersonResponse>();

            


            foreach(PersonAddRequest personaddreq in personaddrequests)
            {
                PersonResponse resp = await _personsService.AddPerson(personaddreq);
                responselists_fromAddMethod.Add(resp);
            }

            //ExpectedOutput

            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personresponse in responselists_fromAddMethod)
            {
                _testOutputHelper.WriteLine(personresponse.ToString());
            }

            List<PersonResponse> responselists_GetMethod = await _personsService.GetAllPersons();
            
            // Print Actual
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personresponse in responselists_GetMethod)
            {
                _testOutputHelper.WriteLine(personresponse.ToString());
            }


            foreach (PersonResponse response in responselists_GetMethod)
            {
                Assert.Contains(response, responselists_fromAddMethod);
            }
                
        }


        #endregion

        #region GetFilteredPersons

        //If the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            CountryAddRequest country_request_1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest country_request_2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse country_response_1 = await _countryService.AddCountry(country_request_1);
            CountryResponse country_response_2 = await _countryService.AddCountry(country_request_2);

            PersonAddRequest person_request_1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = country_response_1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest person_request_2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = country_response_2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest person_request_3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = country_response_2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> person_requests = new List<PersonAddRequest>() { person_request_1, person_request_2, person_request_3 };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in person_requests)
            {
                PersonResponse person_response = await _personsService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> persons_list_from_search = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                Assert.Contains(person_response_from_add, persons_list_from_search);
            }
        }


        //First we will add few persons; and then we will search based on person name with some search string. It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName()
        {
            //Arrange
            CountryAddRequest country_request_1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest country_request_2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse country_response_1 = await _countryService.AddCountry(country_request_1);
            CountryResponse country_response_2 = await _countryService.AddCountry(country_request_2);

            PersonAddRequest person_request_1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = country_response_1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest person_request_2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = country_response_2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest person_request_3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = country_response_2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> person_requests = new List<PersonAddRequest>() { person_request_1, person_request_2, person_request_3 };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in person_requests)
            {
                PersonResponse person_response = await _personsService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> persons_list_from_search = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "ma");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                if (person_response_from_add.PersonName != null)
                {
                    if (person_response_from_add.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Contains(person_response_from_add, persons_list_from_search);
                    }
                }
            }
        }

        #endregion

        #region GetSortedPersons
        [Fact]
        public async Task GetSortedPersons()
        {
            //Arrange
            CountryAddRequest country_request_1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest country_request_2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse country_response_1 = await _countryService.AddCountry(country_request_1);
            CountryResponse country_response_2 = await _countryService.AddCountry(country_request_2);

            PersonAddRequest person_request_1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = country_response_1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest person_request_2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = country_response_2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest person_request_3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = country_response_2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> person_requests = new List<PersonAddRequest>() { person_request_1, person_request_2, person_request_3 };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in person_requests)
            {
                PersonResponse person_response = await _personsService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            List<PersonResponse> allpersons = await _personsService.GetAllPersons();

            //Act
            List<PersonResponse> persons_list_from_SortedMethod = await _personsService.GetSortedPersons(allpersons, nameof(Person.PersonName), SortOrderOptions.DESC);

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_SortedMethod)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            person_response_list_from_add = person_response_list_from_add.OrderByDescending(temp => temp.PersonName).ToList();

            //Assert

            for (int i = 0; i < persons_list_from_SortedMethod.Count; i++)
            {
                Assert.Equal(person_response_list_from_add[i], persons_list_from_SortedMethod[i]);
            }            

        }
        #endregion

        #region UpdatePerson
        [Fact]
        public async Task UpdatePerson_NullPerson()
        {
            PersonUpdateRequest? personUpdateRequest = null;

            //Assert and Act
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            });
        }

        [Fact]
        public async Task UpdatePerson_InvalidPerson()
        {
            PersonUpdateRequest personUpdateRequest = new PersonUpdateRequest() { PersonID = Guid.NewGuid() };
            
            await Assert.ThrowsAsync<ArgumentException>(async() =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            });
        }



        [Fact]
        public async Task UpdatePerson_PersonNameNull()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "USA" };

            CountryResponse response_AddCountry = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest() { PersonName = "John", CountryID = response_AddCountry.CountryID, Address = "Abc road", DateOfBirth = DateTime.Parse("2000-01-01"), Email = "abc@example.com", Gender = GenderOptions.Male, ReceiveNewsLetters = true };

            PersonResponse response_AddPerson = await _personsService.AddPerson(personAddRequest);

            PersonUpdateRequest personUpdateReq = response_AddPerson.ToPersonUpdateRequest();

            personUpdateReq.PersonName = null;


            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _personsService.UpdatePerson(personUpdateReq);
            });

        }


        [Fact]
        public async Task UpdatePerson_ValidPerson()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "USA" };

            CountryResponse country_response_from_add = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest() { PersonName = "John", CountryID = country_response_from_add.CountryID, Address = "Abc road", DateOfBirth = DateTime.Parse("2000-01-01"), Email = "abc@example.com", Gender = GenderOptions.Male, ReceiveNewsLetters = true };

            PersonResponse response_AddPerson = await _personsService.AddPerson(personAddRequest);

            PersonUpdateRequest personUpdateReq = response_AddPerson.ToPersonUpdateRequest();
            personUpdateReq.PersonName = "Tal";
            personUpdateReq.Email = "tal@g.com";

            PersonResponse Response_from_update_method = await _personsService.UpdatePerson(personUpdateReq);

            PersonResponse? Response_from_Get_method = await _personsService.GetPersonByPersonID(Response_from_update_method.PersonID);


            Assert.Equal(Response_from_update_method, Response_from_Get_method);
            

        }



        #endregion

        #region DeletePerson


        //if a valid person Id is passed the method deletes and returns true
        [Fact]
        public async Task DeletePerson_ValidPerson()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "USA" };

            CountryResponse response_AddCountry = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest() { PersonName = "John", CountryID = response_AddCountry.CountryID, Address = "Abc road", DateOfBirth = DateTime.Parse("2000-01-01"), Email = "abc@example.com", Gender = GenderOptions.Male, ReceiveNewsLetters = true };

            PersonResponse response_from_addPerson = await _personsService.AddPerson(personAddRequest);

            bool isDeleted = await _personsService.DeletePerson(response_from_addPerson.PersonID);

            //Assert
            Assert.True(isDeleted);
        }


        //if a invalid person Id is passed the method deletes and returns false
        [Fact]
        public async Task DeletePerson_inValidPerson()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "USA" };

            CountryResponse response_AddCountry = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest() { PersonName = "John", CountryID = response_AddCountry.CountryID, Address = "Abc road", DateOfBirth = DateTime.Parse("2000-01-01"), Email = "abc@example.com", Gender = GenderOptions.Male, ReceiveNewsLetters = true };

            PersonResponse response_from_addPerson = await _personsService.AddPerson(personAddRequest);

            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);
        }
        #endregion
    }
}