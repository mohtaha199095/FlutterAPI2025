# Warehouse & Inventory — End-to-End Test Checklist

Use this checklist after deploying API changes that touch stock posting, transfers, stock counts, or average cost.

## Automated QA

```
GET /api/ctlInventoryQa/RunInventoryQa?CompanyID={id}&ScanDatabase=true
GET /api/ctlInventoryQa/RunFixturesOnly
```

Expect `allPassed: true`. If `StockBalanceMatchesInvoiceDetails` fails, run:

```
GET /api/ctlStockBalance/RebuildStockBalance?companyId={id}
```

Then re-run QA.

## Prerequisites

1. **Account Settings** → Inventory (and COGS) GL accounts configured.
2. At least one **Branch**, two **Stores**, and stock items (`IsStockItem = true`).
3. User authorized on **Inventory** (`FormId.inventoryPage` / 14).
4. Optional: Technical Info → Seed inventory / movement demo (`DEMO-` documents).

## Test flow

| Step | Action | Expected result |
|------|--------|-----------------|
| 1 | Settings → Store → create Store A and Store B | Stores saved |
| 2 | Settings → Items → stock item, `AllowNegativeStock = false`, set cost | Item saved |
| 3 | Inventory → Good Receipt → receive qty 100 into Store A @ known cost | Posted; on-hand A = 100; AVG cost updated |
| 4 | Inventory → Good Issue → issue qty 10 from Store A | Posted; on-hand A = 90 |
| 5 | Issue more than on-hand (e.g. 999) with negative stock blocked | Save **blocked** with insufficient stock message |
| 6 | Warehouse Transfer → A → B, qty 20 | Atomic GI+GR (`RefNo=WHTRANSFER`); A=70, B=20; company total unchanged; **AVG cost unchanged** |
| 7 | Stock Count on Store A → count 65 (system 70) | GI variance 5 (`RefNo=STOCKCOUNT`); on-hand A = 65 |
| 8 | Stock Count → enter negative counted qty | **Blocked** client/API |
| 9 | Purchase Invoice / Sales Invoice / POS sale | Stock moves by `QTYFactor`; negative guard applies |
| 10 | Sales Offer / Purchase Offer | `QTYFactor=0` — on-hand unchanged |
| 11 | Item Transaction Report / Inventory Report | Matches on-hand math |
| 12 | Serial / Lot / Expiry reports (if tracked items) | History shows movement |
| 13 | Inventory Ops / Valuation dashboards | KPIs load without error |
| 14 | `RebuildStockBalance` then `GetOnHand` | Snapshot matches live on-hand |
| 15 | Run `RunInventoryQa` | All checks pass |

## Document markers

| RefNo | Meaning |
|-------|---------|
| `WHTRANSFER` | Paired Good Issue (source) + Good Receipt (dest) |
| `STOCKCOUNT` | Variance GR (+) / GI (−) from physical count |
| `DEMO-` (Note) | Seeded demo documents |

## Costing rules (critical)

- Weighted average from inbound types: Purchase (2), Good Receipt (8), Financing purchase (22), MO Output (26).
- **Exclude** headers with `RefNo` `WHTRANSFER` or `STOCKCOUNT` from the average-cost base (store moves / counts must not pollute AVG).
- Default inventory GL model is **periodic** (`UsePerpetualInventory = false`). Do not enable perpetual if you post both GR and Purchase Invoice without a GR/IR clearing account.

## Auth

- Inventory hub, transfers, stock counts, and reports currently share `FormId.inventoryPage` (14).
- Store master: forms 45/46.

## API smoke list

- `POST /api/ctlWarehouseTransfer/PostTransfer`
- `POST /api/ctlStockCount/PostStockCount`
- `GET /api/ctlStockBalance/RebuildStockBalance`
- `GET /api/ctlStockBalance/GetOnHand`
- `GET /api/ctlInventoryQa/RunInventoryQa`
- `POST /api/ctlInventoryDemoData/SeedMovementDemo`
- `POST /Main/InsertInvoiceHeader` (types 8 / 9)
- Analytics: `/api/ctlInventoryAnalytics/*`

## Known spelling (do not “fix” casually)

- Enum / DB name: `GoodRecipt` (historical)
- Flutter report file: `inventoyreport.dart`
