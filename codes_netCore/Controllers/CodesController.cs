using codes_netCore.Models;
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
        public IActionResult CreateMulti([Bind("CountryId,NetworkId,R,Values")] Codes codes)
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
                            if (codes.Values.Count() == 1)
                            {
                                // all codes of specific R and network
                                var _codesByNetworkAndR = _context.Codes.Where(c => c.R == codes.R && c.NetworkId == codes.NetworkId);
                                // AB codes of one network
                                var _codesInLineOfTheNetwork = _codesByNetworkAndR.Where(c => c.Value.StartsWith(code.Remove(code.Length - 1)));
                                if (_codesInLineOfTheNetwork.Count() == 9)
                                {
                                    // check if DB contains collapsed codes of specific network. Ex.: 0**, 1** etc
                                    var _collapsedCodesOfTheNet = _codesByNetworkAndR.Where(c=>c.Value.StartsWith(code.Remove(code.Length - 2)));
                                    List<Code> _collapsedCodesOfTheNet_filtered = new List<Code>();
                                    foreach (var _code in _collapsedCodesOfTheNet)
                                    {
                                        if (_code.Value.Length == 1)
                                            _collapsedCodesOfTheNet_filtered.Add(_code);
                                    }
                                    if (_collapsedCodesOfTheNet_filtered.Count() == 9)
                                    {
                                        _context.Codes.RemoveRange(_collapsedCodesOfTheNet_filtered);
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
                                    else
                                    {
                                        _context.Codes.RemoveRange(_codesInLineOfTheNetwork);
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
                            }
                            else if (codes.Values.Count() == 10)
                            {

                            }
                        }

                        if (codes.R.Length > 1)
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

                                    // TODO: triger table update
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
                        }
                        #endregion
                        _context.Codes.Add(new Code() { CountryId = codes.CountryId, NetworkId = codes.NetworkId, R = codes.R, Value = code });

                        _isNewCodeAdded = true;
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
        public IActionResult Delete(int?[] ids)
        {
            ushort _deletedCodes = 0;
            if (ids == null)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            Code code;
            foreach (var id in ids)
            {
                code = _context.Codes.Find(id);
                if (code == null)
                {
                    continue;
                }

                if (_context.Codes.Remove(code) != null)
                    ++_deletedCodes;
            }
            if (_deletedCodes > 0)
            {
                _context.SaveChanges();
                return new StatusCodeResult(StatusCodes.Status200OK);
            }
            else
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }

        private bool CodeExists(int id)
        {
            return _context.Codes.Any(e => e.Id == id);
        }
    }
}
