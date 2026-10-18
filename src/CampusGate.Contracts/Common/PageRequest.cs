// src/CampusGate.Contracts/Common/PageRequest.cs
using System.ComponentModel.DataAnnotations;

namespace CampusGate.Contracts.Common;

/// <summary>Query string paging: ?page=1&amp;pageSize=20. The server caps PageSize.</summary>
public sealed class PageRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, int.MaxValue)]
    public int PageSize { get; init; } = 20;
}