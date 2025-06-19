using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Entities
{
    public class PersonDbContext : DbContext
    {

        public PersonDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Country> Countries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");


            string countriesjson =  System.IO.File.ReadAllText("countries.json");

            List<Country> countries= System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesjson);

            foreach(Country country in countries)
            {
                modelBuilder.Entity<Country>().HasData(country);
            }


            string personjson = System.IO.File.ReadAllText("persons.json");

            List<Person> persons =  System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personjson);

            foreach(Person person in persons)
            {
                modelBuilder.Entity<Person>().HasData(person);
            }
        }


    }


}
