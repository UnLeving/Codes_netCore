﻿using codes_netCore.Models;
using codes_netCore.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace codes_netCore.Controllers
{
    public class CodesController : Controller
    {
        private readonly ModelContext _context;

        public CodesController(ModelContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddCodes([Bind("CountryId,NetworkId,R,Values")] Codes codes)
        {
            if (ModelState.IsValid)
            {
                bool _isNewCodeAdded = false;

                foreach (var code in codes.Values)
                {
                    var codeInDb = _context.Codes.Where(c => c.R == codes.R && c.Value == code).FirstOrDefault();
                    if (codeInDb == null)
                    {
                        #region Reduce codes
                        if (codes.R.Length == 1)
                        {
                            var _Rcodes = _context.Codes.Where(c => c.R == codes.R);
                            // R = root, nothing to add
                            if (_Rcodes.Count() != 0 && _Rcodes.FirstOrDefault().Value == null)
                                break;

                            // all codes of specific R and network
                            var _codesByNetworkAndR = _Rcodes.Where(c => c.NetworkId == codes.NetworkId);

                            if (_codesByNetworkAndR != null)
                            {
                                // all entries d** and dd*
                                var _entriesCodes = _codesByNetworkAndR.Where(c => c.Value.StartsWith(code.Remove(code.Length - 2)));
                                if (codes.Values.Count() == 1)
                                {
                                    if (_entriesCodes != null)
                                    {
                                        // filter codes to find AB codes of one network // 3 -> 2
                                        List<Code> _threeDigitsCodes = new List<Code>();
                                        foreach (var c in _entriesCodes)
                                            if (c.Value.Length == 3 && c.Value.StartsWith(code.Remove(code.Length - 1)))
                                                _threeDigitsCodes.Add(c);

                                        if (_threeDigitsCodes.Count > 0)
                                        {
                                            if (_threeDigitsCodes.Count == 9)
                                            {
                                                // check if DB contains collapsed codes of specific network. Ex.: [country][R][00*]
                                                // 2 -> 1
                                                List<Code> _twoDigitsCodes = new List<Code>();
                                                foreach (var _code in _entriesCodes)
                                                    if (_code.Value.Length == 2 && _code.Value.StartsWith(code.Remove(code.Length - 2)))
                                                        _twoDigitsCodes.Add(_code);

                                                if (_twoDigitsCodes.Count() == 9)
                                                {
                                                    // check if DB contains collapsed codes by R. Ex.: [country][R]

                                                    // 1 -> R
                                                    List<Code> _oneDigitCodes = new List<Code>();
                                                    foreach (var _code in _entriesCodes)
                                                        if (_code.Value.Length == 1)
                                                            _oneDigitCodes.Add(_code);

                                                    if (_oneDigitCodes.Count == 99)
                                                    {
                                                        _context.Codes.RemoveRange(_oneDigitCodes);
                                                        _context.Codes.Add(new Code()
                                                        {
                                                            CountryId = codes.CountryId,
                                                            NetworkId = codes.NetworkId,
                                                            R = codes.R,
                                                            Value = null
                                                        });
                                                        _isNewCodeAdded = true;
                                                    }
                                                    else
                                                    {
                                                        _context.Codes.RemoveRange(_twoDigitsCodes);
                                                        _context.Codes.RemoveRange(_threeDigitsCodes);
                                                        // collapse codes to one digit code (value)
                                                        _context.Codes.Add(new Code()
                                                        {
                                                            CountryId = codes.CountryId,
                                                            NetworkId = codes.NetworkId,
                                                            R = codes.R,
                                                            Value = code.Remove(code.Length - 2)
                                                        });
                                                        _isNewCodeAdded = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _context.Codes.RemoveRange(_threeDigitsCodes);
                                                    _context.Codes.Add(new Code()
                                                    {
                                                        CountryId = codes.CountryId,
                                                        NetworkId = codes.NetworkId,
                                                        R = codes.R,
                                                        Value = code.Remove(code.Length - 1)
                                                    });
                                                    _isNewCodeAdded = true;
                                                }
                                            }
                                            else
                                            {
                                                _context.Codes.Add(new Code() { CountryId = codes.CountryId, NetworkId = codes.NetworkId, R = codes.R, Value = code });
                                                _isNewCodeAdded = true;
                                            }
                                        }
                                        else
                                        {
                                            _context.Codes.Add(new Code() { CountryId = codes.CountryId, NetworkId = codes.NetworkId, R = codes.R, Value = code });
                                            _isNewCodeAdded = true;
                                        }
                                    }
                                }
                                else if (codes.Values.Count() == 10)
                                {
                                    // 2 -> 1
                                    List<Code> _twoDigitsCodes = new List<Code>();
                                    List<Code> _threeDigitsCodes = new List<Code>();
                                    foreach (var _code in _entriesCodes)
                                        if (_code.Value.Length == 2 && _code.Value.StartsWith(code.Remove(code.Length - 2)))
                                            _twoDigitsCodes.Add(_code);
                                        else if (_code.Value.Length == 3 && _code.Value.StartsWith(code.Remove(code.Length - 2)))
                                            _threeDigitsCodes.Add(_code);

                                    if (_twoDigitsCodes.Count == 9)
                                    {
                                        // 1 -> R
                                        List<Code> _oneDigitCodes = new List<Code>();
                                        foreach (var _code in _entriesCodes)
                                            if (_code.Value.Length == 1)
                                                _oneDigitCodes.Add(_code);

                                        if (_oneDigitCodes.Count == 9)
                                        {
                                            _context.Codes.RemoveRange(_oneDigitCodes);
                                            if (_threeDigitsCodes.Count > 0)
                                                _context.Codes.RemoveRange(_threeDigitsCodes);
                                            _context.Codes.Add(new Code()
                                            {
                                                CountryId = codes.CountryId,
                                                NetworkId = codes.NetworkId,
                                                R = codes.R,
                                                Value = null
                                            });
                                            _isNewCodeAdded = true;
                                        }
                                        else
                                        {
                                            _context.Codes.RemoveRange(_twoDigitsCodes);
                                            if (_threeDigitsCodes.Count > 0)
                                                _context.Codes.RemoveRange(_threeDigitsCodes);
                                            _context.Codes.Add(new Code()
                                            {
                                                CountryId = codes.CountryId,
                                                NetworkId = codes.NetworkId,
                                                R = codes.R,
                                                Value = code.Remove(code.Length - 2)
                                            });
                                            _isNewCodeAdded = true;
                                        }
                                    }
                                    else
                                    {
                                        _context.Codes.Add(new Code()
                                        {
                                            CountryId = codes.CountryId,
                                            NetworkId = codes.NetworkId,
                                            R = codes.R,
                                            Value = code.Remove(code.Length - 1)
                                        });
                                        _isNewCodeAdded = true;
                                    }
                                    break;
                                }
                                else if (codes.Values.Count() == 500)
                                {
                                    // 1 -> R
                                    List<Code> _oneDigitCodes = new List<Code>();
                                    byte _count = 0;
                                    foreach (var _code in _codesByNetworkAndR)
                                        if (_code.Value?.Length == 1)
                                            _oneDigitCodes.Add(_code);
                                    for (int i = 0; i < 5; i++)
                                    {
                                        if (_oneDigitCodes.Find(c => c.Value == i.ToString()) != null)
                                            ++_count;
                                    }

                                    // check if from one table
                                    if (_oneDigitCodes.Count == 5 && _count == 5)
                                    {
                                        _context.RemoveRange(_Rcodes);
                                        _context.Add(new Code()
                                        {
                                            CountryId = codes.CountryId,
                                            NetworkId = codes.NetworkId,
                                            R = codes.R,
                                            Value = null
                                        });
                                        _isNewCodeAdded = true;
                                    }
                                    else
                                    {
                                        // 3 -> 1
                                        _context.RemoveRange(_oneDigitCodes);
                                        for (int i = 0; i < codes.Values.Count(); i += 100)
                                        {
                                            _context.Codes.Add(new Code()
                                            {
                                                CountryId = codes.CountryId,
                                                NetworkId = codes.NetworkId,
                                                R = codes.R,
                                                Value = codes.Values[i].Remove(codes.Values[i].Length - 2)
                                            });
                                        }

                                        _isNewCodeAdded = true;
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                _context.Codes.Add(new Code() { CountryId = codes.CountryId, NetworkId = codes.NetworkId, R = codes.R, Value = code });
                                _isNewCodeAdded = true;
                            }
                        }
                        else
                        {
                            if (codes.Values.Count() == 1)
                            {
                                if (codes.R.Remove(codes.R.Length - 1).Length > 0)
                                {
                                    string _zone = codes.R;
                                    string _code = code;
                                    Dictionary<string, string> _keyValuePairs = new Dictionary<string, string>();
                                    while (_zone.Remove(_zone.Length - 1).Length > 0)
                                    {
                                        _code = $"{_zone[_zone.Length - 1]}{_code.Remove(_code.Length - 1)}";
                                        _keyValuePairs.Add(_zone.Remove(_zone.Length - 1), _code);
                                        _zone = _zone.Remove(_zone.Length - 1);
                                    }
                                    // retreive root code
                                    Code _rootCode = null;
                                    foreach (var item in _keyValuePairs)
                                    {
                                        _rootCode = _context.Codes.Where(c => c.R == item.Key && c.Value == item.Value).FirstOrDefault();
                                    }

                                    if (_rootCode != null)
                                    {
                                        int _networkId = 0;
                                        // expand _rootCode
                                        string t = $"{codes.R}{code}".Remove(0, $"{_rootCode.R}{_rootCode.Value}".Length);
                                        for (int i = 0; i <= 9; i++)
                                        {
                                            // codes painting
                                            if (i.ToString()[0] != t[0])
                                                _networkId = _rootCode.NetworkId;
                                            else
                                                _networkId = codes.NetworkId;

                                            _context.Codes.Add(new Code()
                                            {
                                                CountryId = codes.CountryId,
                                                NetworkId = _networkId,
                                                R = $"{_rootCode.R}{_rootCode.Value[0]}",
                                                Value = $"{_rootCode.Value[1]}{_rootCode.Value[2]}{i}"
                                            });
                                        }
                                        // add new code
                                        _context.Codes.Add(new Code()
                                        {
                                            CountryId = codes.CountryId,
                                            NetworkId = codes.NetworkId,
                                            R = codes.R,
                                            Value = code
                                        });

                                        // delete _rootCode
                                        if (_context.Codes.Remove(_rootCode) != null)
                                        {
                                            _isNewCodeAdded = true;
                                            break;
                                        }
                                    }
                                }

                                var _inLineDBCodes = _context.Codes.Where(c =>
                                c.NetworkId == codes.NetworkId &&
                                c.R == codes.R &&
                                c.Value.StartsWith(code.Remove(code.Length - 1)));
                                if (_inLineDBCodes.Count() == 9)
                                {
                                    string _parentCode = codes.R[codes.R.Length - 1] + code.Remove(code.Length - 1);
                                    _context.Codes.Add(new Code()
                                    {
                                        CountryId = codes.CountryId,
                                        NetworkId = codes.NetworkId,
                                        R = codes.R.Remove(codes.R.Length - 1),
                                        Value = _parentCode
                                    });
                                    _isNewCodeAdded = true;
                                    _context.Codes.RemoveRange(_inLineDBCodes);
                                    break;
                                }
                            }
                            else if (codes.Values.Count() == 10)
                            {
                                string _parentCode = codes.R[codes.R.Length - 1] + code.Remove(code.Length - 1);
                                _context.Codes.Add(new Code()
                                {
                                    CountryId = codes.CountryId,
                                    NetworkId = codes.NetworkId,
                                    R = codes.R.Remove(codes.R.Length - 1),
                                    Value = _parentCode
                                });
                                _isNewCodeAdded = true;
                                break;
                            }

                            _context.Codes.Add(new Code() { CountryId = codes.CountryId, NetworkId = codes.NetworkId, R = codes.R, Value = code });
                            _isNewCodeAdded = true;
                        }
                        #endregion

                    }
                    else
                    {//reassign code
                        if (codeInDb.NetworkId == codes.NetworkId) continue;
                        codeInDb.NetworkId = codes.NetworkId;
                        _context.Entry(codeInDb).State = EntityState.Modified;
                        _isNewCodeAdded = true;
                    }
                }
                if (_isNewCodeAdded == false)
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);

                _context.SaveChanges();
                return new StatusCodeResult(StatusCodes.Status200OK);
            }
            return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }

        [HttpPost]
        public IActionResult DeleteInheritedCode([Bind("Id,CountryId,R,Value")] Code code)
        {
            int id = -code.Id;
            Code rootCode = _context.Codes.Find(id);
            if (rootCode == null)
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            if (rootCode.R.Length == 1)
            {
                if (rootCode.Value == null)
                {
                    string AB = null;

                    // add ddd
                    AB = code.Value.Remove(2);
                    for (int i = 0; i < 10; i++)
                    {
                        if (code.Value[2] == i.ToString()[0])
                            continue;
                        _context.Codes.Add(new Code() { Value = $"{AB}{i}", R = rootCode.R, CountryId = rootCode.CountryId, NetworkId = rootCode.NetworkId });
                    }

                    // add dd
                    AB = AB[0].ToString();
                    for (int i = 0; i < 10; i++)
                    {
                        if (code.Value[1] == i.ToString()[0])
                            continue;
                        _context.Codes.Add(new Code() { Value = $"{AB}{i}", R = rootCode.R, CountryId = rootCode.CountryId, NetworkId = rootCode.NetworkId });
                    }

                    // add d
                    for (int i = 0; i < 10; i++)
                    {
                        if (AB == i.ToString())
                            continue;
                        _context.Codes.Add(new Code() { Value = i.ToString(), R = rootCode.R, CountryId = rootCode.CountryId, NetworkId = rootCode.NetworkId });
                    }
                }
                else if (rootCode.Value.Length == 1)
                {
                    string AB = null;

                    // add ddd
                    AB = code.Value.Remove(2);
                    for (int i = 0; i < 10; i++)
                    {
                        if (code.Value[2] == i.ToString()[0])
                            continue;
                        _context.Codes.Add(new Code() { Value = $"{AB}{i}", R = rootCode.R, CountryId = rootCode.CountryId, NetworkId = rootCode.NetworkId });
                    }

                    // add dd
                    AB = AB[0].ToString();
                    for (int i = 0; i < 10; i++)
                    {
                        if (code.Value[1] == i.ToString()[0])
                            continue;
                        _context.Codes.Add(new Code() { Value = $"{AB}{i}", R = rootCode.R, CountryId = rootCode.CountryId, NetworkId = rootCode.NetworkId });
                    }

                }
                else if (rootCode.Value.Length == 2)
                {
                    // add ddd
                    if (code.Value != "0")
                    {
                        string AB = code.Value.Remove(2);

                        for (int i = 0; i < 10; i++)
                        {
                            if (code.Value[2] == i.ToString()[0])
                                continue;
                            _context.Codes.Add(new Code()
                            {
                                Value = $"{AB}{i}",
                                R = rootCode.R,
                                CountryId = rootCode.CountryId,
                                NetworkId = rootCode.NetworkId
                            });
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (code.Value[code.Value.Length - 1] == i.ToString()[0])
                        continue;

                    _context.Codes.Add(new Code()
                    {
                        CountryId = code.CountryId,
                        NetworkId = rootCode.NetworkId,
                        R = code.R,
                        Value = code.Value.Remove(code.Value.Length - 1) + i
                    });
                }
            }
            var c = _context.Codes.Remove(rootCode);
            if (c != null)
            {
                _context.SaveChanges();
                return new StatusCodeResult(StatusCodes.Status200OK);
            }
            else
                return new StatusCodeResult(StatusCodes.Status400BadRequest);

        }

        [HttpPost]
        public IActionResult DeleteColumnMixedCodes([Bind("Ids,R,C,Codes")] DeleteMixedCodesViewModel deleteMixedCodes)//int?[] ids, string R, string[] codes)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (deleteMixedCodes.R.Length != 1)
                return BadRequest();
            bool _isAnyCodeDeleted = false;
            int _id = 0;
            Code code;

            if (deleteMixedCodes.Ids[0] < 0)
            {
                _id = -deleteMixedCodes.Ids[0];
                code = _context.Codes.Find(_id);
                if (code == null)
                    return NotFound();

                if (code.Value == null)
                {
                    int T = 0;
                    if (int.Parse(deleteMixedCodes.Codes[0][0].ToString()) >= 5)
                        T = 50;

                    for (int i = 0 + T; i < 50 + T; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (deleteMixedCodes.C == j.ToString())
                                continue;
                            string AB = i < 10 ? "0" + i : i.ToString();
                            _context.Codes.Add(new Code() { Value = $"{AB}{j}", R = code.R, CountryId = code.CountryId, NetworkId = code.NetworkId });
                        }
                    }

                    for (int i = 5 - (T == 0 ? 0 : T / 10); i < 10 - (T == 0 ? 0 : T / 10); i++)
                    {
                        _context.Codes.Add(new Code() { Value = $"{i}", R = code.R, CountryId = code.CountryId, NetworkId = code.NetworkId });
                    }

                    if (_context.Codes.Remove(code) != null)
                    {
                        _context.SaveChanges();
                        return Ok();
                    }
                }
            }

            for (int j = 0; j < deleteMixedCodes.Ids.Length - 1; ++j)
            {
                if (deleteMixedCodes.Ids[j] == 0)
                    continue;
                if (_id == -deleteMixedCodes.Ids[j] || _id == deleteMixedCodes.Ids[j])
                    continue;

                // if negetive - root code
                if (deleteMixedCodes.Ids[j] < 0)
                {
                    _id = -deleteMixedCodes.Ids[j];
                    code = _context.Codes.Find(_id);
                    if (code == null)
                        continue;

                    if (code.Value == null)
                    {
                        string AB = null;

                        // add ddd
                        AB = deleteMixedCodes.Codes[j].Remove(2);
                        for (int k = 0; k < 50; ++k)
                            for (int i = 0; i < 10; i++)
                            {
                                if (deleteMixedCodes.Codes[j][2] == i.ToString()[0])
                                    continue;
                                _context.Codes.Add(new Code() { Value = $"{AB}{i}", R = code.R, CountryId = code.CountryId, NetworkId = code.NetworkId });
                            }

                        //// add dd
                        //AB = AB[0].ToString();
                        //for (int i = 0; i < 10; i++)
                        //{
                        //    if (deleteMixedCodes.Codes[j][1] == i.ToString()[0])
                        //        continue;
                        //    _context.Codes.Add(new Code() { Value = $"{AB}{i}", R = code.R, CountryId = code.CountryId, NetworkId = code.NetworkId });
                        //}

                        //// add d
                        //for (int i = 0; i < 10; i++)
                        //{
                        //    if (AB == i.ToString())
                        //        continue;
                        //    _context.Codes.Add(new Code() { Value = i.ToString(), R = code.R, CountryId = code.CountryId, NetworkId = code.NetworkId });
                        //}
                    }
                    else if (code.Value.Length == 1)
                    {
                        string AB = null;

                        // add ddd
                        AB = deleteMixedCodes.Codes[j].Remove(2);
                        for (int i = 0; i < 10; i++)
                        {
                            if (deleteMixedCodes.Codes[j][2] == i.ToString()[0])
                                continue;
                            _context.Codes.Add(new Code() { Value = $"{AB}{i}", R = code.R, CountryId = code.CountryId, NetworkId = code.NetworkId });
                        }

                        // add dd
                        AB = AB[0].ToString();
                        for (int i = 0; i < 10; i++)
                        {
                            if (deleteMixedCodes.Codes[j][1] == i.ToString()[0])
                                continue;
                            _context.Codes.Add(new Code() { Value = $"{AB}{i}", R = code.R, CountryId = code.CountryId, NetworkId = code.NetworkId });
                        }

                    }
                    else if (code.Value.Length == 2)
                    {
                        // add ddd
                        if (code.Value != "0")
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                if (deleteMixedCodes.Codes[j][2] == i.ToString()[0])
                                    continue;
                                _context.Codes.Add(new Code()
                                {
                                    Value = $"{code.Value}{i}",
                                    R = code.R,
                                    CountryId = code.CountryId,
                                    NetworkId = code.NetworkId
                                });
                            }
                        }
                    }
                }
                // if positive - code
                else
                {
                    _id = deleteMixedCodes.Ids[j];
                    code = _context.Codes.Find(_id);
                    if (code == null)
                        continue;
                }

                if (_context.Codes.Remove(code) != null)
                    _isAnyCodeDeleted = true;
            }
            if (_isAnyCodeDeleted)
            {
                _context.SaveChanges();
                return Ok();
            }
            else
                return NotFound();

        }

        [HttpPost]
        public IActionResult Delete(int?[] ids)
        {
            if (ids == null)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            bool _isAnyCodeDeleted = false;
            Code code;
            foreach (var id in ids)
            {
                code = _context.Codes.Find(id);
                if (code == null)
                {
                    continue;
                }

                if (_context.Codes.Remove(code) != null)
                    _isAnyCodeDeleted = true;
            }
            if (_isAnyCodeDeleted)
            {
                _context.SaveChanges();
                return new StatusCodeResult(StatusCodes.Status200OK);
            }
            else
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }

        [HttpPost]
        public IActionResult DeleteAllTableCodes(string R)
        {
            var _codes = _context.Codes.Where(c => c.R == R);
            if (_codes != null)
            {
                _context.Codes.RemoveRange(_codes);
                _context.SaveChanges();
                return new StatusCodeResult(StatusCodes.Status200OK);
            }
            else
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }
    }
}
