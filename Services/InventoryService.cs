using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvWebApp.Data;
using InvWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InvWebApp.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _db;
        public InventoryService(AppDbContext db) => _db = db;

        private async Task<InventoryStock> EnsureStock(int materielId, int locationId)
        {
            var stock = await _db.InventoryStocks
                .FirstOrDefaultAsync(s => s.MaterielId == materielId && s.LocationId == locationId);
            if (stock != null) return stock;
            stock = new InventoryStock { MaterielId = materielId, LocationId = locationId, Quantity = 0 };
            _db.InventoryStocks.Add(stock);
            await _db.SaveChangesAsync();
            return stock;
        }

        public async Task ReceiveAsync(int materielId, int locationId, int quantity, string? batchNo, DateTime? expiry, string user)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            var stock = await EnsureStock(materielId, locationId);

            var batch = new InventoryBatch { InventoryStockId = stock.Id, BatchNumber = batchNo, ExpiryDate = expiry, Quantity = quantity };
            _db.InventoryBatches.Add(batch);

            stock.Quantity += quantity;

            _db.InventoryMovements.Add(new InventoryMovement
            {
                MaterielId = materielId,
                ToLocationId = locationId,
                Batch = batch,
                Type = MovementType.Receive,
                Quantity = quantity,
                PerformedByUserName = user
            });

            await _db.SaveChangesAsync();
        }

        public async Task IssueAsync(int materielId, int locationId, int quantity, string user, string? reason = null)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            var stock = await _db.InventoryStocks.Include(s => s.Materiel)
                .FirstOrDefaultAsync(s => s.MaterielId == materielId && s.LocationId == locationId)
                ?? throw new InvalidOperationException("No stock at this location.");

            if (stock.Quantity < quantity) throw new InvalidOperationException("Not enough stock.");

            // consume from batches by earliest expiry
            var remaining = quantity;
            var batches = await _db.InventoryBatches.Where(b => b.InventoryStockId == stock.Id)
                .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue).ThenBy(b => b.Id).ToListAsync();

            foreach (var b in batches)
            {
                if (remaining <= 0) break;
                var take = Math.Min(b.Quantity, remaining);
                b.Quantity -= take; remaining -= take;

                _db.InventoryMovements.Add(new InventoryMovement
                {
                    MaterielId = materielId,
                    FromLocationId = locationId,
                    BatchId = b.Id,
                    Type = MovementType.Issue,
                    Quantity = take,
                    PerformedByUserName = user,
                    Reason = reason
                });
            }

            if (remaining > 0) throw new InvalidOperationException("Batch mismatch.");
            stock.Quantity -= quantity;
            _db.InventoryBatches.RemoveRange(batches.Where(b => b.Quantity == 0));
            await _db.SaveChangesAsync();
        }

        public async Task TransferAsync(int materielId, int fromLocationId, int toLocationId, int quantity, string user)
        {
            await IssueAsync(materielId, fromLocationId, quantity, user, "Transfer out");
            await ReceiveAsync(materielId, toLocationId, quantity, null, null, user);
            _db.InventoryMovements.Add(new InventoryMovement
            {
                MaterielId = materielId,
                FromLocationId = fromLocationId,
                ToLocationId = toLocationId,
                Type = MovementType.Transfer,
                Quantity = quantity,
                PerformedByUserName = user
            });
            await _db.SaveChangesAsync();
        }

        public async Task AdjustAsync(int materielId, int locationId, int delta, string user, string reason)
        {
            var stock = await EnsureStock(materielId, locationId);
            if (delta == 0) return;
            if (delta < 0 && stock.Quantity < -delta) throw new InvalidOperationException("Not enough stock to reduce.");

            stock.Quantity += delta;
            _db.InventoryMovements.Add(new InventoryMovement
            {
                MaterielId = materielId,
                FromLocationId = delta < 0 ? locationId : null,
                ToLocationId = delta > 0 ? locationId : null,
                Type = MovementType.Adjust,
                Quantity = Math.Abs(delta),
                PerformedByUserName = user,
                Reason = reason
            });
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<InventoryStock>> GetStocksAsync(int? materielId = null, int? locationId = null)
        {
            var q = _db.InventoryStocks.Include(s => s.Materiel).Include(s => s.Location).AsQueryable();
            if (materielId.HasValue) q = q.Where(s => s.MaterielId == materielId.Value);
            if (locationId.HasValue) q = q.Where(s => s.LocationId == locationId.Value);
            return await q.OrderBy(s => s.Materiel.MaterielName).ThenBy(s => s.Location.Name).ToListAsync();
        }
    }
}
