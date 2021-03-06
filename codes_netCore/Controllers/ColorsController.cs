﻿using codes_netCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace codes_netCore.Controllers
{
    public class ColorsController : Controller
    {
        private readonly ModelContext _context;

        public ColorsController(ModelContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Colors.ToList());
        }

        public IActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        public IActionResult Create([Bind("Id,Hex")] Color color)
        {
            if (ModelState.IsValid)
            {
                if (_context.Colors.Where(c => c.Hex == color.Hex).FirstOrDefault() == null)
                {
                    _context.Add(color);
                    _context.SaveChanges();
                    return new StatusCodeResult(StatusCodes.Status200OK);
                }
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }
    }
}
