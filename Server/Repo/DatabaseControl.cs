using Microsoft.EntityFrameworkCore;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Repo
{
    public static class DatabaseControl
    {
        public static List<Phone> GetPhonesForView()
        {
            using (DbAppContext ctx = new DbAppContext())
            {
                return ctx.Phones.Include(p => p.CompanyEntity).ToList();
            }
        }

        public static void AddPhone(Phone phone)
        {
            using (DbAppContext ctx = new DbAppContext())
            {
                ctx.Phones.Add(phone);
                ctx.SaveChanges();
            }
        }

        public static void UpdatePhone(Phone phone)
        {
            if (phone == null) return;
            using (DbAppContext ctx = new DbAppContext())
            {
                Phone _phone = ctx.Phones.FirstOrDefault(p => p.Id == phone.Id);

                _phone.Title = phone.Title;
                _phone.Price = phone.Price;
                _phone.CompanyId = phone.CompanyId;

                ctx.SaveChanges();
            }
        }

        public static void DeletePhone(Phone phone)
        {
            if (phone == null) return;
            using(DbAppContext ctx = new DbAppContext())
            {
                ctx.Phones.Remove(phone);
                ctx.SaveChanges();
            }
        }

        public static List<Company> GetCompanies()
        {
            using (DbAppContext ctx = new DbAppContext())
            {
                return ctx.Companies.ToList();
            }
        }
    }
}
