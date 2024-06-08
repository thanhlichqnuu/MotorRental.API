﻿using Microsoft.EntityFrameworkCore;
using MotorRental.Application.IRepository;
using MotorRental.Entities;
using MotorRental.Infrastructure.Data.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MotorRental.Infrastructure.Data.Repository
{
    public class MotorRepository : IMotorRepository
    {
        private readonly ApplicationDbContext _db;
    

        public MotorRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Motorbike> Add(Motorbike motorbike)
        {
            // process user
            var existingUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == motorbike.User.Id);
            motorbike.User = existingUser;
            if (existingUser == null)
            {
                return null;
            }

            // process company
            var existingCompany = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(u => u.Name.ToLower() == motorbike.Company.Name.ToLower());
            motorbike.Company = existingCompany != null ? existingCompany : motorbike.Company;

            if (existingCompany == null)
            {
                Company obj = new Company() { Name = motorbike.Company.Name };
                var objCreated = await _db.Companies.AddAsync(obj);
                _db.SaveChanges();
                motorbike.Company = obj;
            }


            // avoid insert parent when insert child
            _db.Entry(motorbike.Company).State = EntityState.Unchanged;
            _db.Entry(motorbike.User).State = EntityState.Unchanged;


            await _db.Motorbikes.AddAsync(motorbike);
            await _db.SaveChangesAsync();

            //motorbike.User.Motorbikes = [];
            //motorbike.Company.Motorbikes = [];

            return motorbike;
        }

        public async Task<Motorbike?> UpdateAsync(Motorbike motorbike)
        {
           
            _db.Entry(motorbike).CurrentValues.SetValues(motorbike);
            await _db.SaveChangesAsync();

            motorbike.User.Motorbikes = [];
            motorbike.Company.Motorbikes = [];

            return motorbike;
            
        }
    }
}