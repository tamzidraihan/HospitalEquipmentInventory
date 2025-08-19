using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvWebApp.Services.Inventory
{
    public interface IInventoryService
    {
        Task ReceiveAsync(int materielId, int locationId, int quantity, string? batchNo, DateTime? expiry, string user);
        Task IssueAsync(int materielId, int locationId, int quantity, string user, string? reason = null);
        Task TransferAsync(int materielId, int fromLocationId, int toLocationId, int quantity, string user);
        Task AdjustAsync(int materielId, int locationId, int delta, string user, string reason);
        Task<IReadOnlyList<InvWebApp.Models.InventoryStock>> GetStocksAsync(int? materielId = null, int? locationId = null);
    }
}
