using codes_netCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codes_netCore.Controllers
{
    public class CountriesController : Controller
    {
        private readonly ModelContext _context;

        public CountriesController(ModelContext context)
        {
            _context = context;
        }

        readonly string greyColorHEX = "#808080";

        public ActionResult Main()
        {
            List<Country> countries = new List<Country>();
            foreach (var country in _context.Countries)
            {
                countries.Add(new Country() { Id = country.Id, Name = $"{country.Code} {country.Name}" });
            }
            ViewBag.CountryId = new SelectList(countries, "Id", "Name");

            return View();
        }

        public ActionResult CodesTable(int countryId, string R = "0")
        {
            List<BaseTable> _uiCodesTable = new List<BaseTable>();
            BaseTable table = null;
            // init table with default values
            for (int i = 0; i < 100; ++i)
            {
                table = new BaseTable
                {
                    R = R,
                    AB = i < 10 ? $"0{i}" : $"{i}"
                };
                for (int j = 0; j < 10; ++j)
                {
                    table.codes[j] = new CodeDt() { code = $"{table.AB}{j}" };
                }
                _uiCodesTable.Add(table);
            }

            Country country = _context.Countries.Find(countryId);
            ICollection<Code> _countryCodes = country.Codes;
            if (_countryCodes.Count > 0)
            {
                // paint cells with roots colors
                if (R.Length > 1)
                {
                    foreach (var ABrow in _uiCodesTable)
                    {
                        IEnumerable<Code> rootCodes = null;
                        string RAB;
                        for (int i = 1; i < R.Length; i++)
                        {
                            RAB = R + ABrow.AB;
                            if (i > 1)
                            {
                                RAB = RAB.Remove(RAB.Length - i + 1);
                            }
                            RAB = RAB.Substring(RAB.Length - 3);
                            rootCodes = _countryCodes.Where(code => code.R == R.Remove(R.Length - i) && (code.Value != null && code.Value.Equals(RAB)) || code.Value == null);

                            if (rootCodes.Count() > 0)
                                break;
                        }

                        if (rootCodes.Count() > 0)
                        {
                            foreach (var rootCode in rootCodes)
                            {
                                if (rootCode.Value == null)
                                {
                                    for (int k = 0; k < 10; k++)
                                    {
                                        ABrow.codes[k].colorHEX = rootCode.Network.Color.Hex;
                                        ABrow.codes[k].id = -rootCode.Id;
                                    }
                                }
                                else
                                {
                                    char lastDigit = rootCode.Value[rootCode.Value.Length - 1] == ' ' ?
                                                              rootCode.Value[rootCode.Value.Length - 2] :
                                                               rootCode.Value[rootCode.Value.Length - 1];
                                    for (int i = 0; i < 10; ++i)
                                    {
                                        if (lastDigit == i.ToString()[0])
                                        {
                                            for (int k = 0; k < 10; k++)
                                            {
                                                ABrow.codes[k].colorHEX = rootCode.Network.Color.Hex;
                                                ABrow.codes[k].id = -rootCode.Id;
                                            }
                                        }
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (R.Length == 1)
                {
                    // check if R = root and paint if true
                    var _rootCode = _countryCodes.Where(r => r.R == R).FirstOrDefault();
                    if (_rootCode != null && _rootCode.Value == null)
                    {
                        foreach (var ABrow in _uiCodesTable)
                        {
                            for (int k = 0; k < 10; k++)
                            {
                                ABrow.codes[k].colorHEX = _rootCode.Network.Color.Hex;
                                ABrow.codes[k].id = -_rootCode.Id;
                            }
                        }
                    }
                    return PartialView(_uiCodesTable);
                }

                // fill table with codes
                IEnumerable<Code> codesOfR = _countryCodes.Where(code => code.R == R);
                if (codesOfR.Count() > 0)
                {
                    foreach (var ABrow in _uiCodesTable)
                    {
                        foreach (var cell in ABrow.codes)
                        {
                            Code code = codesOfR.FirstOrDefault(_code => _code.Value == cell.code);
                            if (code != null)
                            {
                                cell.colorHEX = code.Network.Color.Hex;
                                cell.id = code.Id;
                            }
                        }
                    }
                }

                // paint cells with inherited codes colors
                IEnumerable<Code> inheritedCodes = null;

                foreach (var ABrow in _uiCodesTable)
                {
                    foreach (var cell in ABrow.codes)
                    {
                        inheritedCodes = _countryCodes.Where(code => $"{code.R}{code.Value}".StartsWith(R + cell.code));
                        if (inheritedCodes.Count() == 0) continue;
                        string colorHEX = null;
                        foreach (var code in inheritedCodes)
                        {
                            if (cell.colorHEX == "#FFFFFF")
                            {
                                colorHEX = greyColorHEX;
                                break;
                            }
                            if (colorHEX == null)
                                colorHEX = code.Network.Color.Hex;
                            else if (colorHEX != code.Network.Color.Hex)
                            {
                                colorHEX = greyColorHEX;
                                break;
                            }
                        }
                        cell.colorHEX = colorHEX;
                    }
                }
            }
            return PartialView(_uiCodesTable);
        }

        public ActionResult CodesList(int countryId)
        {
            Country country = _context.Countries.Find(countryId);
            if (country == null) return new StatusCodeResult(StatusCodes.Status400BadRequest);
            List<Code> codes = new List<Code>();
            foreach (var code in country.Codes.ToList())
            {
                codes.Add(new Code() { Value = $"{code.Country.Code}{code.R}{code.Value}" });
            }
            return PartialView(codes);
        }

        [HttpGet]
        public ActionResult CountryDropDown()
        {
            List<Country> countries = new List<Country>();
            foreach (var country in _context.Countries)
            {
                countries.Add(new Country() { Id = country.Id, Name = $"{country.Code} {country.Name}" });
            }
            ViewBag.CountryId = new SelectList(countries, "Id", "Name");

            return PartialView();
        }

        [HttpGet]
        public ActionResult RegExp()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult RegExp(int? id)
        {
            var codes = _context.Networks.Find(id)?.Codes;
            if (codes == null) return new StatusCodeResult(StatusCodes.Status404NotFound);
            string codeRegExp = $"^{codes.First().Country.Code}(";
            foreach (var item in codes)
            {
                codeRegExp += $"{item.R}{item.Value}" + "|";
            }
            codeRegExp = codeRegExp.TrimEnd('|');
            codeRegExp += ").*";

            return File(Encoding.UTF8.GetBytes(codeRegExp), "text/plain", $"{codes.First().Country.Name.Trim()} {codes.First().Network.Name.Trim()}.txt");
        }

        [HttpPost]
        public ActionResult ExportCodes(int? id)
        {
            if (id == null)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

            string country = _context.Countries.Find(id).Name.Trim();
            var codes = _context.Countries.Find(id).Codes;

            if (codes == null)
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }

            List<CodeDT> list = new List<CodeDT>();
            foreach (var item in codes)
            {
                list.Add(new CodeDT() { Value = $"{item.Country.Code}{item.R}{item.Value}", Country = item.Country.Name, Network = item.Network.Name });
            }
            return ExportToExcel(list, $"{country} {DateTime.Now}");
        }

        FileStreamResult ExportToExcel(IEnumerable<CodeDT> dataSet, string fileName)
        {
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells[1, 1].LoadFromCollection(dataSet, true);

            return File(new MemoryStream(excel.GetAsByteArray()), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
        }

        public IActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,ISO3166_1_a2,Name,Code")] Country country)
        {
            if (ModelState.IsValid)
            {
                if (_context.Countries.Where(c => c.Name == country.Name || c.ISO3166_1_a2 == country.ISO3166_1_a2 || c.Code == country.Code).FirstOrDefault() == null)
                {
                    _context.Add(country);
                    await _context.SaveChangesAsync();
                    return new StatusCodeResult(StatusCodes.Status200OK);
                }
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }

        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }
    }
}
