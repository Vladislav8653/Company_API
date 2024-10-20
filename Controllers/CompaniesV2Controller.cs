﻿using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Contracts;

namespace CompanyEmployees.Controllers;


[ApiVersion("2.0")]
[Route("api/{v:apiVersion}/companies")]
[ApiExplorerSettings(GroupName = "v2")]
[ApiController]
public class CompaniesV2Controller : ControllerBase
{
    private readonly IRepositoryManager _repository;
    public CompaniesV2Controller(IRepositoryManager repository)
    {
        _repository = repository;
    }
    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);
        return Ok(companies);
    }

}