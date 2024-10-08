﻿using AutoMapper;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    
    public CompaniesController(IRepositoryManager repository, ILoggerManager loggerManager, IMapper mapper)
    {
        _repository = repository;
        _logger = loggerManager;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult GetCompanies()
    {
        var companies = _repository.Company.GetAllCompanies(trackChanges: false);
        var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
        return Ok(companiesDto);
    }

    
    [HttpGet("{id:guid}", Name = "CompanyById")]
    public IActionResult GetCompany(Guid id)
    {
        var company = _repository.Company.GetCompany(id, trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
            return NotFound();
        }
        else
        {
            var companyDto = _mapper.Map<CompanyDto>(company);
            return Ok(companyDto);
        }
    }

    
    [HttpPost]
    public IActionResult CreateCompany([FromBody] CompanyForCreationDto? company)
    {
        if (company == null)
        {
            _logger.LogError("CompanyForCreationDto object sent from client is null.");
            return BadRequest("CompanyForCreatingDto object is null");
        }

        var companyEntity = _mapper.Map<Company>(company);
        _repository.Company.CreateCompany(companyEntity);
        _repository.Save();
        var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);
        return CreatedAtRoute("CompanyById",
            new { id = companyToReturn.Id }, companyToReturn);
    }

    
    [HttpGet("collection/({ids})", Name = "CompanyCollection")]
    public IActionResult GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]
        IEnumerable<Guid>? ids)
    {
        if (ids == null)
        {
            _logger.LogError("Parameter ids is null");
            return BadRequest("Parameter ids is null");
        }

        var companyEntities = _repository.Company.GetByIds(ids, trackChanges: false);
        if (companyEntities.Count() != ids.Count())
        {
            _logger.LogError("Some ids are not valid in a collection");
            return NotFound();
        }

        var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
        return Ok(companiesToReturn);
    }

    
    [HttpPost("collection")]
    public IActionResult CreateCompanyCollection([FromBody]IEnumerable<CompanyForCreationDto>? companyCollection)
    {
        if (companyCollection == null)
        {
            _logger.LogError("Company collection sent from client is null.");
            return BadRequest("Company collection is null");
        }

        var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
        foreach (var company in companyEntities)
        {
            _repository.Company.CreateCompany(company);
        }
        _repository.Save();
        var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
        var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));
        return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
    }

    
    [HttpPut("{id}")]
    public IActionResult UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto? company)
    {
        if(company == null) 
        { 
            _logger.LogError("CompanyForUpdateDto object sent from client is null."); 
            return BadRequest("CompanyForUpdateDto object is null"); 
        }

        var companyEntity = _repository.Company.GetCompany(id, trackChanges: true);
        if(companyEntity == null) 
        { 
            _logger.LogInfo($"Company with id: {id} doesn't exist in the database."); 
            return NotFound(); 
        }

        _mapper.Map(company, companyEntity);
        _repository.Save();
        return NoContent();
    }
}