﻿namespace Entities.RequestFeatures;

public class EmployeeParameters : RequestParameters
{
    public uint MinAge { get; set; }
    public uint MaxAge { get; set; } = int.MaxValue;
    public bool ValidAgeRange => MinAge < MaxAge;
    public string SearchTerm { get; set; }
}