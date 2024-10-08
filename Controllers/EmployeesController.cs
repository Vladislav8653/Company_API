﻿using AutoMapper;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.JsonPatch;
using Entities.Models;

namespace CompanyEmployees.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/employees")]
public class EmployeesController : Controller
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    
    public EmployeesController(IRepositoryManager repository, ILoggerManager logger, 
        IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult GetEmployeesForCompany(Guid companyId)
    {
        var company = _repository.Company.GetAllCompanies(trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            return NotFound();
        }
        var employeesFromDb = _repository.Employee.GetEmployees(companyId, trackChanges: false);
        var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
        return Ok(employeesDto);
    }
    
    
    [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
    public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
    {
        var company = _repository.Company.GetCompany(companyId, trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            return NotFound();
        }
        var employeeDb = _repository.Employee.GetEmployee(companyId, id, trackChanges: false);
        if (employeeDb == null)
        {
            _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
            return NotFound();
        }
        var employeesDto = _mapper.Map<EmployeeDto>(employeeDb);
        return Ok(employeesDto);
    }

    [HttpPost]
    public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto? employee)
    {
        if(employee == null)
        {
            _logger.LogError("EmployeeForCreationDto object sent from client is null.");
            return BadRequest("EmployeeForCreationDto object is null");
        }

        var company = _repository.Company.GetCompany(companyId, trackChanges : false);
        if (company == null)
        {
            _logger.LogError($"Company with id: {companyId} doesn't exist in the database.");
            return NotFound();
        }

        var employeeEntity = _mapper.Map<Employee>(employee);
        _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
        _repository.Save();

        var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
        return CreatedAtRoute("GetEmployeeForCompany",
            new { companyId, id = employeeToReturn.Id }, employeeToReturn);

    }

    [HttpDelete("{id}")]
    public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
    {
        var company = _repository.Company.GetCompany(companyId, trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            return NotFound();
        }

        var employeeForCompany = _repository.Employee.GetEmployee(companyId, id, trackChanges: false);
        if (employeeForCompany == null)
        {
            _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
            return NotFound();
        }
        
        _repository.Employee.DeleteEmployee(employeeForCompany);
        _repository.Save();
        return NoContent();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid id, 
        [FromBody] EmployeeForUpdateDto? employee)
    {
        if(employee == null) 
        { 
            _logger.LogError("EmployeeForUpdateDto object sent from client is null."); 
            return BadRequest("EmployeeForUpdateDto object is null"); 
        }

        var company = _repository.Company.GetCompany(companyId, trackChanges: false);
        if(company == null) 
        { 
            _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database."); 
            return NotFound(); 
        }

        var employeeEntity = _repository.Employee.GetEmployee(companyId, id, trackChanges: true);
        if(employeeEntity == null) 
        { 
            _logger.LogInfo($"Employee with id: {id} doesn't exist in the database."); 
            return NotFound(); 
        }

        _mapper.Map(employee, employeeEntity);
        _repository.Save();
        return NoContent();
    }
    
    
    [HttpPatch("{id:guid}")]
    public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, 
        [FromBody] JsonPatchDocument<EmployeeForUpdateDto>? patchDoc)
    {
        if(patchDoc == null)
        {
            _logger.LogError("patchDoc object sent from client is null.");
            return BadRequest("patchDoc object is null");
        }
        var company = _repository.Company.GetCompany(companyId, trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            return NotFound();
        }
        var employeeEntity = _repository.Employee.GetEmployee(companyId, id, trackChanges: 
            true);
        if (employeeEntity == null)
        {
            _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
            return NotFound();
        }
        var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
        patchDoc.ApplyTo(employeeToPatch);
        _mapper.Map(employeeToPatch, employeeEntity);
        _repository.Save();
        return NoContent();
    }
}